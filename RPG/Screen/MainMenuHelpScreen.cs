using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace RPG.Screen
{
    class MainMenuHelpScreen : MenuScreen
    {
        public MainMenuHelpScreen(ScreenManager sm) : base(sm, new string[] {"Back"}, new MenuItemFunction[] {new MenuItemFunctionMainMenu()})
        {}

        public override void Update(Microsoft.Xna.Framework.GameTime gTime) {
            base.Update(gTime);
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gTime) {
            SpriteBatch.DrawString(ScreenManager.Font, "Go left => A", new Vector2(160, 33), Color.White);
            SpriteBatch.DrawString(ScreenManager.Font, "Go right => D", new Vector2(150, 63), Color.White);
            SpriteBatch.DrawString(ScreenManager.Font, "Block => W", new Vector2(164, 93), Color.White);

            SpriteBatch.DrawString(ScreenManager.Font, "S <= Duck", new Vector2(380, 33), Color.White);
            SpriteBatch.DrawString(ScreenManager.Font, "Space <= Jump", new Vector2(380, 63), Color.White);
            SpriteBatch.DrawString(ScreenManager.Font, "1-3 <= Attack", new Vector2(380, 93), Color.White);

            base.Draw(gTime);
        }
    }
}
