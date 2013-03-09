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
   public class Player : Entity
    {
        int XP_TO_LVL = 100;
        int xp;
        List<Attack> myAttacks;
        
        public Player(int x, int y, EntitySprite s) : base(x, y, s) {
            xp = 0;
            myAttacks = new List<Attack>();
        }

        public override void runAI(TileMap map) {
            foreach (Attack a in myAttacks)
                xp += a.XP;

            // Level up
            if (xp > XP_TO_LVL) {
                xp = 0;
                XP_TO_LVL = (int) (XP_TO_LVL * 1.5f);
                stats.levelUp();
            }

            myAttacks.RemoveAll(new Predicate<Attack>(AttackXpAdded));
        }

        private static bool AttackXpAdded(Attack a) {
            if (!a.Alive && !a.HasXP) {
                return true;
            } else
                return false;
        }

        public override void Draw(SpriteBatch spriteBatch, int offsetX, TimeSpan elapsed) {
            // Draw hp bar
            Rectangle hpRect = new Rectangle(2, 2, 100, 16);
            hpRect.Width = (int) Math.Round(hpRect.Width * stats.HpPercent) + 1;

            spriteBatch.Draw(ScreenManager.WhiteRect, hpRect, Color.Green);

            spriteBatch.DrawString(ScreenManager.Small_Font, "Lvl " + stats.Level, new Vector2(105, 0), Color.White);

            // Draw entity
            Rectangle pRect = getRect();
            pRect.X -= offsetX;
            Texture2D sprite = getSprite(elapsed.Milliseconds);
            if (isFacingForward()) {
                spriteBatch.Draw(sprite, pRect, Color.White);
            } else {
                spriteBatch.Draw(sprite, pRect, sprite.Bounds, Color.White, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
            }
           
            // Draw xp bar
            Rectangle xpBar = new Rectangle(0, spriteBatch.GraphicsDevice.Viewport.Height - 3,
                (int) (xp / (float)XP_TO_LVL * spriteBatch.GraphicsDevice.Viewport.Width), 3);
            spriteBatch.Draw(ScreenManager.WhiteRect, xpBar, Color.LightBlue);

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
            bounds.moveY(3-bounds.Y);
        }

        public void doAttack(TileMap map, EntityPart part) {
            if (Alive) {
                Attack attack = base.attack(map, part);
                if (attack != null) {
                    myAttacks.Add(attack);
                }
            }
        }

        public void doBlock(TileMap map) {
            if (Alive) base.block(map);
        }

        public void doDuck(TileMap map) {
            if (Alive) base.duck(map);
        }

        public void doJump(TileMap map) {
            if (Alive) base.jump(map);
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
