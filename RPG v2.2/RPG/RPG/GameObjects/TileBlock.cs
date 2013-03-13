using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RPG.Sprite;
using RPG.Screen;

namespace RPG.GameObjects
{
    public class TileBlock {
        public static TileBlock NONE = new TileBlock(TerrainSpriteId.None, new Rectangle(0, 0, 0, 0));
        public static TileBlock STONE_WALL = new TileBlock(TerrainSpriteId.Stone_Wall, new Rectangle(0, 0, TileMap.SPRITE_WIDTH, TileMap.SPRITE_HEIGHT));
        public static TileBlock DOOR = new TileBlock(TerrainSpriteId.Door, new Rectangle(0, 0, 0, 0)).setEvent(TileBlockEvent.NewRoom);

        TerrainSpriteId spriteId;
        Rectangle bounds;
        Func<GameScreen, bool> intEvent;

        private TileBlock(TerrainSpriteId spriteId, Rectangle bounds) {
            this.spriteId = spriteId;
            this.bounds = bounds;
        }

        public TileBlock setEvent(Func<GameScreen, bool> intEvent) {
            this.intEvent = intEvent;
            return this;
        }

        public bool interact(GameScreen gs) {
            if (intEvent != null)
                return intEvent(gs);
            return false;
        }

        public Rectangle getBounds() { return bounds; }
        public TerrainSpriteId getSpriteId() { return spriteId; }
    }
}