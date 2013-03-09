using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RPG.Sprite;

namespace RPG.GameObjects
{
    public struct TileBlock {
        public static TileBlock NONE = new TileBlock(TerrainSpriteId.None, new Rectangle(0, 0, 0, 0));
        public static TileBlock STONE_WALL = new TileBlock(TerrainSpriteId.Stone_Wall, new Rectangle(0, 0, TileMap.SPRITE_WIDTH, TileMap.SPRITE_HEIGHT));

        TerrainSpriteId spriteId;
        Rectangle bounds;

        private TileBlock(TerrainSpriteId spriteId, Rectangle bounds) {
            this.spriteId = spriteId;
            this.bounds = bounds;
        }

        public Rectangle getBounds() { return bounds; }
        public TerrainSpriteId getSpriteId() { return spriteId; }
    }
}