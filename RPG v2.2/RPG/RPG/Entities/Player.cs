using System;
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
   public class Player : Entity
    {
        int XP_TO_LVL = 33;
        int xp;
        List<Attack> myAttacks;
        
        public Player(int x, int y, EntitySprite s) : base(x, y, s, null) {
            xp = 0;
            Stats.levelUp();
            myAttacks = new List<Attack>();
        }

        protected override void runAI(TileMap map) {
            foreach (Attack a in myAttacks)
                xp += a.getXP();

            // Level up
            if (xp > XP_TO_LVL) {
                xp = 0;
                XP_TO_LVL = (int) (XP_TO_LVL * 1.5f);
                Stats.levelUp();
            }

            myAttacks.RemoveAll(new Predicate<Attack>(AttackXpAdded));
        }

        private static bool AttackXpAdded(Attack a) {
            return (!a.Alive && !a.HasXP);
        }

        public override void Draw(SpriteBatch spriteBatch, int offsetX, TimeSpan elapsed) {
            // Draw hp bar
            Rectangle hpRect = new Rectangle(2, 2, 100, 16);
            hpRect.Width = (int) Math.Round(hpRect.Width * Stats.HpPercent) + 1;
            spriteBatch.Draw(ScreenManager.WhiteRect, hpRect, Color.Green);

            // Draw level
            spriteBatch.DrawString(ScreenManager.Small_Font, "Lvl " + Stats.Level, new Vector2(105, 0), Color.White);

            // Draw entity
            Rectangle pRect = Rect;
            pRect.X -= offsetX;
            Texture2D sprite = getSprite(elapsed.Milliseconds);
            if (isFacingForward()) {
                spriteBatch.Draw(sprite, pRect, Color.White);
            } else {
                spriteBatch.Draw(sprite, pRect, sprite.Bounds, Color.White, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
            }
           
            // Draw xp bar
            Rectangle xpBar = new Rectangle(0, spriteBatch.GraphicsDevice.Viewport.Height - 3,
                (int) (xp / (float)XP_TO_LVL * spriteBatch.GraphicsDevice.Viewport.Width), 2);
            spriteBatch.Draw(ScreenManager.WhiteRect, xpBar, Color.LightBlue);

            // Draw armour stats
            if (Alive) {
                Viewport vp = spriteBatch.GraphicsDevice.Viewport;
                Rectangle armourImg = new Rectangle(vp.Width - 45, 2, 32, 32);
                Texture2D img;
                switch (state) {
                    case EntityState.Blocking:
                        img = GameScreen.sprGUI[GUISpriteId.Blocking]; break;
                    case EntityState.Ducking:
                        img = GameScreen.sprGUI[GUISpriteId.Ducking]; break;
                    default:
                        img = GameScreen.sprGUI[GUISpriteId.Standing]; break;
                }
                if (isFacingForward())
                    spriteBatch.Draw(img, armourImg, Color.Black);
                else
                    spriteBatch.Draw(img, armourImg, img.Bounds, Color.Black, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
                spriteBatch.DrawString(ScreenManager.Small_Font, Stats.THeadMultiplier.ToString("0.0"), new Vector2(vp.Width - 38, 0), Color.White);
                spriteBatch.DrawString(ScreenManager.Small_Font, Stats.TBodyMultiplier.ToString("0.0"), new Vector2(vp.Width - 38, 11), Color.White);
                spriteBatch.DrawString(ScreenManager.Small_Font, Stats.TLegsMultiplier.ToString("0.0"), new Vector2(vp.Width - 38, 22), Color.White);
            }

            // Draw hit box
            if (Alive && showHpTicks > HP_BAR_SHOW_MS / 2) {
                Rectangle lastRect = bounds.getRectFromPart(lastHitPart);
                if (lastRect.Width != 0) {     
                    lastRect.X += (int) (lastRect.Height * 0.1) - offsetX;
                    lastRect.Width = (int) (lastRect.Width * 0.8);
                    spriteBatch.Draw(ScreenManager.WhiteRect, lastRect, HIT_BOX_COLOR);
                }
            }
        }
        
        public void newMap() {
            bounds.moveX(-bounds.X);
            bounds.moveY(-bounds.Y + TileMap.SPRITE_HEIGHT * 3);
            heal((int) (Stats.MaxHp * .1f));
        }

        public void doAttack(TileMap map, EntityPart part) {
            if (Alive) {
                Attack attack = base.attack(map, part, AttackFactory.FireBall);
                if (attack != null) {
                    myAttacks.Add(attack);
                }
            }
        }

        public void doBlock() {
            if (Alive) base.block();
        }

        public void doDuck() {
            if (Alive) base.duck();
        }

        public void doJump() {
            if (Alive) base.jump();
        }

        public void doMove(Direction dir) {
            if (Alive) 
                base.setXSpeedPerMs(Entity.SPEED_PER_MS * (float) dir);
        }

        public void stand() {
            if (Alive) base.setState(EntityState.Standing);
        }
    }
}
