using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RPG.Screen;

namespace RPG.GameObjects
{
    class TileBlockEvent
    {
        public static bool Nothing(GameScreen gs) { return true; }

        public static bool NewRoom(GameScreen gs) {
            if (gs.GamePlayer.Alive && gs.KilledEntities == gs.Entities.Count - 1)
                gs.newRoom();

            return true;
        }
    }
}
