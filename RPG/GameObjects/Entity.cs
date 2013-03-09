using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using RPG.Sprite;
using RPG.Screen;
using RPG.Helpers;

namespace RPG.GameObjects
{
    public enum EntityState { Standing, Jumping, Moving, Ducking, Blocking, Dying, Dead };

    public class Entity
    {
        // Static
        public static float SPEED_PER_MS = 0.11f;
        public static float JUMP_PER_MS = 0.425f;
        public static float GRAVITY_PER_MS = 0.07f;

        public static int JUMP_DELAY_MS = 330;
        public static int ATTACK_DELAY_MS = 250;
        public static int HP_BAR_SHOW_MS = 3000;

        public static Color HIT_BOX_COLOR = Color.Lerp(Color.Red, Color.Transparent, 0.8f);

        // Entity States
        protected EntityStats stats;

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


        public Entity(int x, int y, EntitySprite s) {
            bounds = new EntityBounds(x, y, (int) TileMap.SPRITE_WIDTH, 10, 14, 8, 24);
            state = EntityState.Standing;
            msVel = new Vector2(0, 0);
            facing = Direction.Right;
            jumpDelay = attackDelay = showHpTicks = 0;
            speedMultiplier = 1;
            isOnFloor = true;

            XPValue = 6;

            // Stats
            stats = new EntityStats(30, 1);

            if (s != null)
                this.sprite = s;
            else {
                System.Console.WriteLine("Init Entity with null sprite!");
                Environment.Exit(1);
            }

        }

        public virtual void runAI(TileMap map) {
            // Basic random AI
            int r = ScreenManager.Rand.Next(500);

            if (r < 10) {
                setXSpeedPerMs(SPEED_PER_MS);
            } else if (r < 30) {
                setXSpeedPerMs(msVel.X * -1f);
            } else if (r < 50) {
                setXSpeedPerMs((float) Direction.Stopped);
                attack(map, EntityPart.Body);
            } else if (r < 55) {
                jump(map);
            } else if (r < 60) {
                duck(map);
            } else if (r < 65) {
                block(map);
            }
        }

        public void update(TileMap map, TimeSpan elapsed) {
            tElapsed = elapsed;
            isOnFloor = map.isRectOnFloor(bounds.LegsRect);

            if (attackDelay > 0)
                attackDelay -= elapsed.Milliseconds;
            if (showHpTicks > 0)
                showHpTicks -= elapsed.Milliseconds;

            // ### Update entity state
            if (!Alive) {
                if (state == EntityState.Dead)
                    return;
                else if (state != EntityState.Dying)
                    die(map);
                else if (!isOnFloor)
                    bounds.moveY(2);
                else
                    setState(EntityState.Dead);

                return;
            }

            // ### Run the entities customizable AI
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
                speedMultiplier *= 0.95f;
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

        protected void setXSpeedPerMs(float speedPerMs) {
            msVel.X = speedPerMs;
        }

        protected void jump(TileMap map) {
            if (jumpDelay <= 0 && isOnFloor) {
                msVel.Y = JUMP_PER_MS;
                setState(EntityState.Jumping);
                jumpDelay = JUMP_DELAY_MS; // Won't start decreasing until no longer jumping
            }
        }

        protected void hitGround() {
            msVel.Y = 0;
            setState(EntityState.Standing);
        }

        protected Attack attack(TileMap map, EntityPart part) {
            if (attackDelay <= 0 && canAttack()) {
                Attack a = AttackFactory.FireBall(this, part, map);
                map.addAttack(a);
                attackDelay = ATTACK_DELAY_MS;
                return a;
            }
            return null;
        }
        
        public bool canAttack() {
            return (state != EntityState.Blocking && state != EntityState.Dead);
        }

        public void hitInThe(EntityPart part, int dmg) {
            showHpTicks = HP_BAR_SHOW_MS;
            lastHitPart = part;
            if (part == EntityPart.Legs)
                stats.hurt((int) (dmg * 0.7f));
            else if (part == EntityPart.Body)
                stats.hurt(dmg);
            else if (part == EntityPart.Head)
                stats.hurt((int) (dmg * 1.12f));
        }

        protected void duck(TileMap map) {
            if (isOnFloor) {
                setState(EntityState.Ducking);
                bounds.duck(); // Resets position
            }
        }

        protected void block(TileMap map) {
            if (isOnFloor) {
                setState(EntityState.Blocking);
                bounds.block(facing); // Resets position
            }
        }

        protected void die(TileMap map) {
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
                return sprite.Attack;
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
            Rectangle pRect = getRect();
            pRect.X -= offsetX;
            Texture2D sprite = getSprite(elapsed.Milliseconds);
            if (isFacingForward())
                spriteBatch.Draw(sprite, pRect, Color.White);
            else
                spriteBatch.Draw(sprite, pRect, sprite.Bounds, Color.White, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);

            // Was hit resently, show hp bar
            if (Alive && showHpTicks != 0) {
                Rectangle hpRect = getRect();
                hpRect.X -= offsetX + (int) (hpRect.Width * 0.125); // Offset and a bit over from the absolute left side
                hpRect.Y -= 7;
                hpRect.Height = 3;
                hpRect.Width = (int) Math.Round(hpRect.Width * 0.75 * stats.HpPercent) + 1;

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
        public float AttackPower { get { return stats.AttackPower; } }
        public bool Alive { get { return stats.Hp > 0; } }

        public Point getLocation() { return bounds.Location; }
        public Rectangle getRect() { return bounds.Rect; }
        public EntityBounds getBounds() { return bounds; }
        public bool isFacingForward() { return facing != Direction.Left; }
        public EntityState getState() { return state; }
    }
}
