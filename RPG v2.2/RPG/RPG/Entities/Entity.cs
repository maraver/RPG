﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using RPG.Sprite;
using RPG.Screen;
using RPG.Helpers;
using RPG.GameObjects;

namespace RPG.Entities
{
    public enum EntityState { Standing, Jumping, Moving, Ducking, Blocking, Dying, Dead };

    public class Entity
    {
        // Static
        public static float SPEED_PER_MS = 0.11f;
        public static float JUMP_PER_MS = 0.4f;
        public static float GRAVITY_PER_MS = 0.0675f;

        public static int JUMP_DELAY_MS = 330;
        public static int ATTACK_DELAY_MS = 333;
        public static int HP_BAR_SHOW_MS = 3000;

        public static float BASE_HEAD_MULT = 1.125f;
        public static float BASE_LEGS_MULT = 0.75f;

        public static Color HIT_BOX_COLOR = Color.Lerp(Color.Red, Color.Transparent, 0.8f);

        // Entity States
        public EntityStats Stats;

        public readonly int XPValue;

        // Display States
        private Direction facing;
        protected EntityState state;
        protected bool isOnFloor;

        private float speedMultiplier;
        protected int jumpDelay, attackDelay;

        protected EntityPart lastHitPart;
        protected int showHpTicks;

        protected EntityBounds bounds; // Top Left Tile

        protected TimeSpan tElapsed;
        protected Vector2 msVel;
        protected EntitySprite sprite;

        protected Func<Entity, TileMap, bool> ai;


        public Entity(int x, int y, EntitySprite s, Func<Entity, TileMap, bool> ai, int xp=6) {
            this.ai = ai;
            bounds = new EntityBounds(this, x, y, (int) TileMap.SPRITE_WIDTH, 12, 14, 6, 24);
            state = EntityState.Standing;
            msVel = new Vector2(0, 0);
            facing = Direction.Right;
            jumpDelay = attackDelay = showHpTicks = 0;
            speedMultiplier = 1;
            isOnFloor = true;

            XPValue = xp;

            // Stats
            Stats = new EntityStats(100, 1);

            if (s != null)
                this.sprite = s;
            else {
                System.Console.WriteLine("Init Entity with null sprite!");
                Environment.Exit(1);
            }
        }

        protected virtual void runAI(TileMap map) {
            EntityAIs.Basic(this, map);
        }

        public void update(TileMap map, TimeSpan elapsed) {
            tElapsed = elapsed;
            isOnFloor = map.isRectOnFloor(bounds.StandRect);

            if (attackDelay > 0)
                attackDelay -= elapsed.Milliseconds;
            if (showHpTicks > 0)
                showHpTicks -= elapsed.Milliseconds;

            // ### Update entity state
            if (!Alive) {
                if (state == EntityState.Dead)
                    return;
                else if (state != EntityState.Dying)
                    die();
                else if (!isOnFloor)
                    bounds.moveY(2);
                else {
                    setState(EntityState.Dead);
                    map.killedEntities++;
                }

                return;
            }

            // ### Run the entities customizable AI
            if (ai != null)
                ai(this, map);
            else
                runAI(map);

            // ### Update movement state based on movement
            if (getRealXSpeed() != 0 && state != EntityState.Jumping) {
                setState(EntityState.Moving);
                if (getRealXSpeed() < 0) facing = Direction.Left;
                else facing = Direction.Right;
            } else if (state == EntityState.Moving) { // If state still 'Moving' but not moving, change state
                setState(EntityState.Standing);
            }

            // ### Update X position

            int currXSpeed = (int) (getRealXSpeed() * speedMultiplier);
            if (attackDelay > ATTACK_DELAY_MS * 0.5f && speedMultiplier > 0.25f) // If attacked recently while jumping, move slower
                speedMultiplier *= 0.93f;
            else if (speedMultiplier < 1)
                speedMultiplier += 0.033f;
            else
                speedMultiplier = 1; // Don't overshoot

            bounds.moveX(currXSpeed);
            if (bounds.Right > map.getPixelWidth()) {
                bounds.moveX((map.getPixelWidth() - bounds.Width) - bounds.X);
                msVel.X = 0;
            } else if (bounds.Left <= 0) {
                bounds.moveX(-bounds.X);
                msVel.X = 0;
            } else if (getRealXSpeed() > 0) {
                int newX = map.checkBoundsXRight(bounds.Rect);
                updateBoundsX(map, newX);
            } else if (getRealXSpeed() < 0) {
                int newX = map.checkBoundsXLeft(bounds.Rect);
                updateBoundsX(map, newX);
            }

            // ### Update Y Position

            if (state == EntityState.Jumping) { // Gravity
                msVel.Y -= GRAVITY_PER_MS;
            } else if(jumpDelay > 0) {  // Tick jump delay
                jumpDelay -= elapsed.Milliseconds;
            }

            // Subtract so everything else doesn't have to be switched (0 is top)
            bounds.moveY((int) -getRealYSpeed());
            if (bounds.Top >= map.getPixelHeight() - bounds.Height / 2) {
                bounds.moveY((int) getRealYSpeed()); // Undo the move
                fallWithGravity();
            } else if (bounds.Bottom <= 0) {
                bounds.moveY((int) getRealYSpeed()); // Undo the move
                hitGround();
            } else if (getRealYSpeed() > 0) {
                int newY = map.checkBoundsYUp(bounds.Rect);
                if (newY != bounds.Y) { // Hit something
                    bounds.moveY(newY - bounds.Y); // Move down correct amount (+)
                    fallWithGravity();
                }
            } else if (getRealYSpeed() < 0) {
                int newY = map.checkBoundsYDown(bounds.Rect);
                if (newY != bounds.Y) { // Hit something
                    bounds.moveY(newY - bounds.Y); // Move up correct amount (-)
                    hitGround();
                }
            }
        }

        private void fallWithGravity() {
            msVel.Y = 0;
            setState(EntityState.Jumping);
        }

        private void updateBoundsX(TileMap map, int newX) {
            if (newX == bounds.X && state != EntityState.Jumping && !isOnFloor) {
                fallWithGravity();
            } else if (newX != bounds.X) {
                bounds.moveX(newX - bounds.X);
                msVel.X = 0;
            }
        }

        public void setXSpeedPerMs(float speedPerMs) {
            msVel.X = speedPerMs;
        }

        protected void hitGround() {
            isOnFloor = true;
            msVel.Y = 0;
            setState(EntityState.Standing);
        }

        public Attack attack(TileMap map, EntityPart part, Func<Entity, EntityPart, TileMap, Attack> factoryFunc) {
            if (attackDelay <= 0 && canAttack()) {
                Attack a = factoryFunc(this, part, map);
                map.addAttack(a);
                attackDelay = ATTACK_DELAY_MS;
                return a;
            }
            return null;
        }
        
        public bool canAttack() {
            return (state != EntityState.Blocking && state != EntityState.Dead);
        }

        public void heal(int amnt) {
            Stats.addHp(amnt);
        }

        public int hitInThe(EntityPart part, int dmg, float reducer) {
            showHpTicks = HP_BAR_SHOW_MS;
            lastHitPart = part;

            int realDmg = 0;
            if (part == EntityPart.Legs)
                realDmg = (int)(dmg * Stats.TLegsMultiplier);
            else if (part == EntityPart.Head)
                realDmg = (int)(dmg * Stats.THeadMultiplier);
            else if (part == EntityPart.Body)
                realDmg = (int) (dmg * Stats.TBodyMultiplier);
            realDmg = (int) (realDmg * reducer);

            Stats.addHp(-realDmg);
            return realDmg;
        }

        public void jump() {
            if (jumpDelay <= 0 && isOnFloor) {
                msVel.Y = JUMP_PER_MS;
                setState(EntityState.Jumping);
                jumpDelay = JUMP_DELAY_MS; // Won't start decreasing until no longer jumping
                isOnFloor = false;
            }
        }

        public void duck() {
            if (isOnFloor) {
                setState(EntityState.Ducking);
                bounds.duck(); // Resets position
            }
        }

        public void block() {
            if (isOnFloor) {
                setState(EntityState.Blocking);
                bounds.block(facing); // Resets position
            }
        }

        protected void die() {
            bounds.die();
            msVel.X = msVel.Y = 0;
            state = EntityState.Dying;
        }

        protected void setState(EntityState state) {
            if (Alive || state == EntityState.Dying || state == EntityState.Dead) {
                bounds.resetPositions();  // Resets position
                this.state = state;
            }
        }

        protected Texture2D getSprite(int elapsed) {
            if (attackDelay > ATTACK_DELAY_MS * 0.2) {
                if (state != EntityState.Ducking)
                    return sprite.Attack;
                else
                    return sprite.Duck_Attack;
            } else if (!Alive) { // state == EntityState.Dead || state == EntityState.Dying
                return sprite.Dead;
            } else if (state == EntityState.Moving) {
                sprite.tick(elapsed, 333);

                if (sprite.getTicks() >= 166)
                    return sprite.Move;
                else
                    return sprite.Base;
            } else if (state == EntityState.Ducking) {
                return sprite.Duck;
            } else if (state == EntityState.Blocking) {
                return sprite.Block;
            } else if (state == EntityState.Jumping) {
                // Can attack while jumping, so draw it
                if (attackDelay > ATTACK_DELAY_MS * 0.2) {
                    return sprite.Attack;
                } else {
                    return sprite.Move;
                }
            }

            return sprite.Base;
        }

        public virtual void Draw(SpriteBatch spriteBatch, int offsetX, TimeSpan elapsed) {
            Rectangle pRect = Rect;
            pRect.X -= offsetX;
            Texture2D sprite = getSprite(elapsed.Milliseconds);
            if (isFacingForward())
                spriteBatch.Draw(sprite, pRect, Color.White);
            else
                spriteBatch.Draw(sprite, pRect, sprite.Bounds, Color.White, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);

            // Was hit resently, show hp bar
            if (Alive && showHpTicks != 0) {
                Rectangle hpRect = Rect;
                hpRect.X -= offsetX + (int) (hpRect.Width * 0.125); // Offset and a bit over from the absolute left side
                hpRect.Y -= 7;
                hpRect.Height = 3;
                hpRect.Width = (int) Math.Round(hpRect.Width * 0.75 * Stats.HpPercent) + 1;

                spriteBatch.Draw(ScreenManager.WhiteRect, hpRect, Color.Red);

                Rectangle lastRect = bounds.getRectFromPart(lastHitPart);
                if (showHpTicks > HP_BAR_SHOW_MS / 2 && lastRect.Width != 0) {     
                    lastRect.X += (int) (lastRect.Height * 0.1) - offsetX;
                    lastRect.Width = (int) (lastRect.Width * 0.8);
                    spriteBatch.Draw(ScreenManager.WhiteRect, lastRect, HIT_BOX_COLOR);
                }
            }
        }
        
        protected float getRealXSpeed() { return msVel.X * (tElapsed.Milliseconds); }
        protected float getRealYSpeed() { return msVel.Y * (tElapsed.Milliseconds); }
        public bool Alive { get { return Stats.Hp > 0; } }
        public EntityState State { get { return state; } }
        public EntityBounds Bounds { get { return bounds; } }
        public Point Location { get { return bounds.Location; } }
        public Rectangle Rect { get { return bounds.Rect; } }

        public float getSpeedX() { return msVel.X; }
        public float getSpeedY() { return msVel.Y; }
        public bool isFacingForward() { return facing != Direction.Left; }
    }
}
