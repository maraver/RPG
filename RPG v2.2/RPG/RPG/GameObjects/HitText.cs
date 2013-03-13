using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using RPG.Entities;

namespace RPG.GameObjects
{
    public class HitText
    {
        static float MAX_MS = 500;
        static float X_MS = 1f, Y_MS = -1.5f;

        public bool Alive;
        public string Text;
        public Color TextColor;
        
        private float alpha;
        private Vector2 offset;
        private Entity entity;

        int ms;

        public HitText(Entity e, int dmg) {
            Alive = true;
            Text = dmg.ToString();
            TextColor = Color.Red;
            alpha = 1;
            offset = new Vector2(-2, -8);
            entity = e;
            ms = 0;
        }

        public void update(TimeSpan elapsed) {
            ms += elapsed.Milliseconds;
            if (ms > MAX_MS) Alive = false;
            else {
                offset.X += X_MS; 
                offset.Y += Y_MS; 
                alpha -= ms / MAX_MS; 
                TextColor = Color.Lerp(Color.Red, Color.Transparent, alpha);
            }
        }

        public Vector2 getPosition() {
            return new Vector2(entity.Bounds.Right + offset.X, entity.Bounds.Top + offset.Y);
        }
    }
}
