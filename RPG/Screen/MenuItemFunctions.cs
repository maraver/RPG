using System;

namespace RPG.Screen {
    public abstract class MenuItemFunction {
        public abstract void run(ScreenManager sm);
    }

    public class MenuItemFunctionExit : MenuItemFunction {
        public override void run(ScreenManager sm) { Environment.Exit(1); }
    }

    public class MenuItemFunctionSave : MenuItemFunction {
        public override void run(ScreenManager sm) { }
    }

    public class MenuItemFunctionPlay : MenuItemFunction {
        public override void run(ScreenManager sm) {
            Screen game = sm.getScreen("Game");
            Screen pause = sm.getScreen("Pause");
            Screen mainmenu = sm.getScreen("Main Menu");
            mainmenu.DoDraw = mainmenu.DoUpdate = false;
            game.DoDraw = game.DoUpdate = true;
            pause.DoUpdate = true;

        }
    }

    public class MenuItemFunctionClosePause : MenuItemFunction {
        public override void run(ScreenManager sm) {
            PauseScreen pause = (PauseScreen) sm.getScreen("Pause");
            Screen game = sm.getScreen("Game");
            pause.escPressed = false;
            pause.togglePause();

            // If paused draw this and not game
            pause.DoDraw = pause.isPaused();
            game.DoUpdate = !pause.isPaused();
        }
    }

    public class MenuItemFunctionMainMenuHelp : MenuItemFunction {
        public override void run(ScreenManager sm) {
            // Close all
            foreach (Screen s in sm.screens.Values) {
                s.DoDraw = s.DoUpdate = false;
            }

            Screen help = sm.getScreen("MMHelp");
            help.DoDraw = help.DoUpdate = true;
        }
    }

    public class MenuItemFunctionMainMenu : MenuItemFunction {
        public override void run(ScreenManager sm) {
            // Close all
            foreach (Screen s in sm.screens.Values) s.DoDraw = s.DoUpdate = false;

            Screen mainMenu = sm.getScreen("Main Menu");
            mainMenu.DoUpdate = mainMenu.DoDraw = true;
        }
    }
}