using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using RPG.Sprite;
using RPG.Entities;

namespace RPG.GameObjects
{
    public class AttackFactory
    {
        public static Attack FireBall(Entity e, EntityPart part, TileMap map) {
            // Static
            int width = 20, height = 8;

            // Changes based on entity state
            int x;
            int y = e.Bounds.getFireFrom(part);
            float speed = 0.175f;
            if (e.isFacingForward()) {
                x = e.Bounds.Right + 1;
            } else {
                speed *= -1;
                x = e.Bounds.Left - width - 1;
            }
            
            return new Attack(map, map.Screen.SprAttack[AttackSpriteId.Fireball], 
                        new Rectangle(x, y - (height / 2), width, height), (int) (7 * e.Stats.AttackPower), speed, 200);
        }

        public static Attack Iceball(Entity e, EntityPart part, TileMap map) {
            // Static
            int width = 20, height = 8;

            // Changes based on entity state
            int x;
            int y = e.Bounds.getFireFrom(part);
            float speed = 0.16f;
            if (e.isFacingForward()) {
                x = e.Bounds.Right + 1;
            } else {
                speed *= -1;
                x = e.Bounds.Left - width - 1;
            }
            
            return new Attack(map, map.Screen.SprAttack[AttackSpriteId.Iceball], 
                        new Rectangle(x, y - (height / 2), width, height), (int) (8 * e.Stats.AttackPower), speed, 200);
        }

        public static Attack Scurge_Shot(Entity e, EntityPart part, TileMap map) {
            // Static
            int width = 20, height = 8;

            // Changes based on entity state
            int x;
            int y = e.Bounds.getFireFrom(part);
            float speed = 0.19f;
            if (e.isFacingForward()) {
                x = e.Bounds.Right + 1;
            } else {
                speed *= -1;
                x = e.Bounds.Left - width - 1;
            }
            
            return new Attack(map, map.Screen.SprAttack[AttackSpriteId.Scurge_Shot], 
                        new Rectangle(x, y - (height / 2), width, height), (int) (6 * e.Stats.AttackPower), speed, 200);
        }
    }
}
