using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using RPG.Entities;

namespace RPG.GameObjects
{
    public class Attack
    {
        int xp;
        int dmg;
        bool alive;
        Texture2D sprite;
        Rectangle drawRect, collRect;
        TimeSpan lastElapsed;
        float msSpeed;
        int maxdist, distTraveled;

        public Attack(TileMap map, Texture2D sprite, Rectangle rect, int dmg, float msSpeed, int maxdist) {
            this.xp = 0;
            this.alive = true;
            this.sprite = sprite;
            this.drawRect = rect;
            this.msSpeed = msSpeed;
            this.dmg = dmg;

            if (isFacingForward()) {
                this.collRect = new Rectangle(rect.X, rect.Y, rect.Width - (int) (rect.Width * 0.2), rect.Height);
            } else {
                this.collRect = new Rectangle(rect.X + (int) (rect.Width * 0.2), rect.Y, rect.Width - (int) (rect.Width * 0.2), rect.Height);
            }
            this.maxdist = maxdist;
            this.distTraveled = 0;

            // Initial bounds text with all containing tiles
            int maxX = map.shrinkX(rect.Right, true);
            int maxY = map.shrinkY(rect.Bottom, true);
            for (int x = map.shrinkX(rect.Left, false); x <= maxX; x++) {
                for (int y = map.shrinkY(rect.Top, false); y <= maxY; y++) {
                    Rectangle wallRect = map.getRect(x, y);
                    if (map.checkCollision(rect, wallRect)) {
                        alive = false;
                        break;
                    }
                }

                if (!alive) break;
            }
        }

        public void update(TileMap map, TimeSpan elapsed) {
            if (alive) {
                lastElapsed = elapsed;

                drawRect.X += getRealSpeed();
                collRect.X += getRealSpeed();

                distTraveled += Math.Abs(getRealSpeed());

                if (distTraveled > maxdist || drawRect.Right < 0 || drawRect.Left >= map.getPixelWidth()) {
                    alive = false;
                } else {
                    // Test walls based on direction (left, right)
                    int x;
                    if (isFacingForward())
                        x = map.shrinkX(collRect.Right, false);
                    else
                        x = map.shrinkX(collRect.Left, false);

                    int maxY = map.shrinkY(collRect.Bottom, true);
                    for (int y = map.shrinkY(collRect.Top, false); y <= maxY; y++) {
                        Rectangle wallRect = map.getRect(x, y);

                        if (map.checkCollision(collRect, wallRect)) {
                            alive = false;
                            return;
                        }
                    }

                    // Test collision with entites
                    foreach (Entity e in map.Entities) {
                        if (!e.Alive) 
                            continue;

                        EntityHit eHit;
                        if (isFacingForward())
                            eHit = e.Bounds.collide(collRect, new Point(collRect.Right, collRect.Center.Y));
                        else
                            eHit = e.Bounds.collide(collRect, new Point(collRect.Left, collRect.Center.Y));

                        if (eHit.Part != EntityPart.None) {
                            alive = false;
                            if (eHit.Part != EntityPart.Miss) {
                                float dmgReducer = ((eHit.PercFromCenter < 0.6) ? 1 - eHit.PercFromCenter : 0.4f);
                                int realDmg = e.hitInThe(eHit.Part, dmg, dmgReducer);
                                map.addHitText(e, realDmg);
                                if (!e.Alive)
                                    xp += e.XPValue;
                            }
                            return;
                        }
                    }
                }
            }
        }

        public Texture2D getSprite() {
            if (alive)
                return sprite;
            else
                return null;
        }

        public bool isFacingForward() {
            return msSpeed >= 0;
        }

        protected int getRealSpeed() { return (int) (msSpeed * lastElapsed.Milliseconds); }

        public int Damage { get { return dmg; } }
        public Rectangle Rectangle { get { return drawRect; } }
        public Point Location { get { return drawRect.Location; } }
        public bool Alive { get { return alive; } }
        public bool HasXP { get { return xp > 0; } }

        public int getXP() {
            if (xp > 0) {
                xp--;
                return 1;
            } else {
                return 0;
            }
        }
    }
}
