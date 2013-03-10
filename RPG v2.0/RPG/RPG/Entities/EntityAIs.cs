using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RPG.GameObjects;
using RPG.Screen;
using RPG.Helpers;

namespace RPG.Entities
{
    public class EntityAIs
    {
        public static bool Wraith(Entity e, TileMap map) {
            int r = ScreenManager.Rand.Next(500);

            if (r < 50) {
                if (r < 25) e.attack(map, EntityPart.Head, AttackFactory.Iceball);
                else if (r < 40) e.attack(map, EntityPart.Body, AttackFactory.Iceball);
                else if (r < 50) e.attack(map, EntityPart.Legs, AttackFactory.Iceball);
            } else if (r < 55) {
                e.setXSpeedPerMs(Entity.SPEED_PER_MS);
            } else if (r < 60) {
                e.setXSpeedPerMs(-Entity.SPEED_PER_MS);
            } else if (r < 80) {
                e.block(map);
            } else if (r < 90) {
                e.jump(map);
            }

            return true;
        }

        public static bool Basic(Entity e, TileMap map) {
            // Basic random AI
            int r = ScreenManager.Rand.Next(500);

            if (r < 10) {
                e.setXSpeedPerMs(Entity.SPEED_PER_MS);
            } else if (r < 30) {
                e.setXSpeedPerMs(e.getSpeedX() * -1f);
            } else if (r < 50) {
                e.setXSpeedPerMs((float) Direction.Stopped);
                e.attack(map, EntityPart.Body, AttackFactory.Scurge_Shot);
            } else if (r < 55) {
                e.jump(map);
            } else if (r < 60) {
                e.duck(map);
            } else if (r < 65) {
                e.block(map);
            }

            return true;
        }
    }
}
