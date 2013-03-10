using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RPG.Screen;
using RPG.Sprite;

namespace RPG.Entities
{
    public class EntityFactory
    {
        public static Entity Wraith(int x, int y, GameScreen screen) {
            return new Entity(x, y, screen.SprEntity[EntitySpriteId.Wraith], EntityAIs.Wraith, 8);
        }

        public static Entity Warlock(int x, int y, GameScreen screen) {
            return new Entity(x, y, screen.SprEntity[EntitySpriteId.Warlock], EntityAIs.Basic);
        }
    }
}
