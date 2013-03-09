using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace RPG.Sprite
{
    public enum EntitySpriteId { None, Warrior, Warlock };
    public enum AttackSpriteId { None, FireBall };
    public enum TerrainSpriteId { None, Stone_Wall };

    public class Sprite
    {
        private int ticks;
        public readonly Texture2D Base;

        public Sprite(ContentManager content, String baseName)
        {
            ticks = 0;
            Base = content.Load<Texture2D>(baseName + "/" + baseName);
        }

        public void tick(int elapsed, int max) { ticks = (ticks + elapsed) % max; }
        public int getTicks() { return ticks; }
    }
}
