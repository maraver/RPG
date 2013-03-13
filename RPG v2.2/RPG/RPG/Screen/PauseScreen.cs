using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace RPG.Screen
{
    public class PauseScreen : MenuScreen
    {
        public bool escPressed;
        private bool paused;

        public PauseScreen(ScreenManager screenManager) :
            base(screenManager, new String[] { "Continue", "Start Over", "Exit" }, 
                    new MenuItemFunction[] { new MenuItemFunctionUnpause(), new MenuItemFunctionStartOver(), new MenuItemFunctionExit()})
        { }

        public void togglePause() { paused = !paused; }
        public bool isPaused() { return paused; }

        public override void Update(GameTime gTime) {
            KeyboardState kb = Keyboard.GetState();

            if (kb.IsKeyDown(Keys.Escape))
                escPressed = true;
            else if (escPressed) {
                escPressed = false;
                togglePause();

                // If paused draw this and not game
                DoDraw = paused;
                screenManager.getScreen(ScreenId.Game).DoUpdate = !paused;
            }

            if (isPaused())
                base.Update(gTime);
        }

        public override void Draw(GameTime gTime) {
            SpriteBatch.End(); // End normal, drawing different

            SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.Additive); // Draw additive

            SpriteBatch.Draw(ScreenManager.WhiteRect, new Rectangle(0, 0,
                getScreenManager().GraphicsDevice.Viewport.Width, getScreenManager().GraphicsDevice.Viewport.Height), new Color(.2f, .2f, .2f, .8f));

            base.Draw(gTime); // Draw menu items
            SpriteBatch.End(); // End additive
            SpriteBatch.Begin(); // Restart normal
        }
    }
}
