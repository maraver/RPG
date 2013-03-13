using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RPG.Screen;
using RPG.Sprite;
using RPG.Entities;

namespace RPG.GameObjects
{
    public enum MapType { Hall };

    public class TileMap {
        public static int SPRITE_WIDTH = 32, SPRITE_HEIGHT = 32;

        public readonly GameScreen Screen;
        public readonly int Width, Height;
        List<TileBlock> tileMap;

        public int killedEntities;
        public List<Entity> Entities;
        public List<Attack> Attacks;
        public List<HitText> HitTexts;

        public TileMap(int width, int height, Player player, MapType type, GameScreen screen) {
            this.Screen = screen;
            this.Width = width;
            this.Height = height;

            // Init
            killedEntities = 0;
            tileMap = new List<TileBlock>();
            Entities = new List<Entity>();
            Attacks = new List<Attack>();
            HitTexts = new List<HitText>();

            // Init entities
            addEntity(player);

            switch (type) {
                case MapType.Hall:
                default: // Hall Way
                    for (int w = 0; w < width; w++)
                        for (int h = 0; h < height; h++) {
                            if (h == height - 2 && w == width - 1)
                                tileMap.Add(TileBlock.DOOR);
                            else if (h == 0 || h == height - 1)
                                tileMap.Add(TileBlock.STONE_WALL);
                            else if (w > 3 && h == height - 2 && ScreenManager.Rand.Next(20) == 0)
                                tileMap.Add(TileBlock.STONE_WALL);
                            else
                            {
                                tileMap.Add(TileBlock.NONE);

                                if (h == height - 2 && ScreenManager.Rand.Next(10) < 1)
                                {
                                    Entity e;
                                    if (ScreenManager.Rand.Next(100) > 50)
                                        e = EntityFactory.Wraith(w * SPRITE_WIDTH, h * SPRITE_HEIGHT, screen);
                                    else
                                        e = EntityFactory.Warlock(w * SPRITE_WIDTH, h * SPRITE_HEIGHT, screen);
                                    Entities.Add(e);
                                }
                            }
                        }
                    break;
            }
        }

        public void addHitText(Entity e, int dmg) {
            HitTexts.Add(new HitText(e, dmg));
        }

        public TileBlock getPixel(int x, int y) {
            return get(shrinkX(x, false), shrinkY(y, false));
        }

        public TileBlock get(int w, int h) {
            if (w < 0 || h < 0 || w >= Width || h >= Height) {
                Console.WriteLine("Tried to get a tile block out of range!");
                Environment.Exit(1);
            }

            return tileMap[h + w * Height];
        }

        public TerrainSpriteId getSpriteId(int w, int h) {
            return get(w, h).getSpriteId();
        }

        public int getPixelWidth() { return Width *  SPRITE_WIDTH; }
        public int getPixelHeight() { return Height *  SPRITE_HEIGHT; }

        public int shrinkX(int x, bool rUp) { 
            // return (int) Math.Round(x / (float)SPRITE_WIDTH);
            if (rUp) return (int) Math.Ceiling(x / (float) SPRITE_WIDTH);
            else     return (int) (x / (float) SPRITE_WIDTH); 
        }
        public int shrinkY(int y, bool rUp) { 
            // return (int) Math.Round(y / (float) SPRITE_HEIGHT);
            if (rUp) return (int) Math.Ceiling(y / (float) SPRITE_HEIGHT);
            else     return (int) (y / (float) SPRITE_HEIGHT);
        }

        /// Get the non-offset bounds
        public Rectangle getRectPixel(int x, int y) {
            int sX = (int) Math.Round(x / (float)SPRITE_WIDTH);
            int sY = (int) Math.Round(y / (float) SPRITE_HEIGHT);

            // Console.WriteLine(" x = " + x + " y = " + y);
            return getRect(sX, sY);
        }

        public Rectangle getRect(int w, int h) {
            if (w < 0 || h < 0 || w >= Width || h >= Height) // Off screen, no collision
                return new Rectangle(0, 0, 0, 0);

            Rectangle bounds = get(w, h).getBounds();

            bounds.X += w *  SPRITE_WIDTH;
            bounds.Y += h *  SPRITE_HEIGHT;

            return bounds;
        }

        public bool checkCollision(Rectangle r1, Rectangle r2) {
            if (r1.Width == 0 || r1.Height == 0 || r2.Width == 0 || r2.Height == 0)
                return false;
            return r1.Intersects(r2);
        }

        public int checkBoundsXRight(Rectangle rect) {
            // Console.WriteLine("Checking");

            Vector2 pos = new Vector2((int) Math.Round(rect.Center.X / (float) SPRITE_WIDTH), (int) Math.Round(rect.Center.Y / (float) SPRITE_HEIGHT));
            int leftmost = rect.Right;
            for (int y = (int) pos.Y - 1; y < pos.Y + 1; y++) {
                // Console.Write("  X-Right: ");
                Rectangle objRect = getRect((int) pos.X, y);

                if (objRect.Left < leftmost && checkCollision(rect, objRect))
                     leftmost = objRect.Left;
            }

            return leftmost - rect.Width; // Move back to the top left corner
        }

        public int checkBoundsXLeft(Rectangle rect) {
            // Console.WriteLine("Checking");

            Vector2 pos = new Vector2((int) (rect.Left / SPRITE_WIDTH), (int) Math.Round(rect.Center.Y / (float) SPRITE_HEIGHT));
            int rightmost = rect.Left;
            for (int y = (int) pos.Y - 1; y < pos.Y + 1; y++) {
                // Console.Write("  X-Left: ");
                Rectangle objRect = getRect((int) pos.X, y);
            
                if (objRect.Right > rightmost && checkCollision(rect, objRect))
                    rightmost = objRect.Right;
            }
            return rightmost;
        }

        public int checkBoundsYDown(Rectangle rect) {
            // Console.WriteLine("Checking");
            
            Vector2 pos = new Vector2((int) Math.Round(rect.Center.X / (float) SPRITE_WIDTH), (int) Math.Round(rect.Center.Y / (float) SPRITE_HEIGHT));
            int topmost = rect.Bottom;
            for (int x = (int) pos.X - 1; x < pos.X + 1; x++) {
                // Console.Write("  Y-Down: ");
                Rectangle objRect = getRect(x, (int) pos.Y);
            
                if (objRect.Top < topmost && checkCollision(rect, objRect))
                    topmost = objRect.Top;
            }

            return topmost - rect.Height; // Move back to the top left corner
        }

        public int checkBoundsYUp(Rectangle rect) {
            // Console.WriteLine("Checking");

            Vector2 pos = new Vector2((int) (rect.Center.X / SPRITE_WIDTH), (int) (rect.Top / SPRITE_HEIGHT));
            int bottommost = rect.Top;
            for (int x = (int) pos.X - 1; x < pos.X + 1; x++) {
                // Console.Write("  Y-Up: ");
                Rectangle objRect = getRect(x, (int) pos.Y);
            
                if (objRect.Bottom > bottommost && checkCollision(rect, objRect))
                    bottommost = objRect.Bottom;
            }

            return bottommost;
        }

        public bool isRectOnFloor(Rectangle rect) {
            // Console.WriteLine("Checking");

            rect.Y += 2;
            Vector2 bottom = new Vector2((int) Math.Round(rect.Center.X / (float) SPRITE_WIDTH), (int) Math.Round(rect.Bottom / (float) SPRITE_HEIGHT));
            for (int x = (int) bottom.X - 1; x < bottom.X + 1; x++) {
                // Console.Write("  On floor:");
                Rectangle objRect = getRect(x, (int) bottom.Y);
                if (checkCollision(rect, objRect))
                    return true;
            }
            return false;
        }

         public void addEntity(Entity e) {
            Entities.Add(e);
        }

        public void addAttack(Attack a) {
            Attacks.Add(a);
        }
    }
}
