using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using RPG.Sprite;

namespace RPG.GameObjects
{
    public class AttackFactory
    {
        public static Attack FireBall(Entity e, EntityPart part, TileMap map) {
            // Static
            int width = 20, height = 8;

            // Changes based on entity state
            int x;
            int y = e.getBounds().getFireFrom(part);
            float speed = 0.175f;
            if (e.isFacingForward()) {
                x = e.getBounds().Right + 1;
            } else {
                speed *= -1;
                x = e.getBounds().Left - width - 1;
            }
            
            return new Attack(map, map.screen.SprAttack[AttackSpriteId.FireBall], 
                        new Rectangle(x, y - (height / 2), width, height), (int) (5 * e.AttackPower), speed, 200);
        }
    }
}
