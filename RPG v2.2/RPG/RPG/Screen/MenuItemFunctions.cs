using System;

namespace RPG.Screen {
    public abstract class MenuItemFunction {
        public abstract void run(ScreenManager sm);
    }

    public class MenuItemFunctionExit : MenuItemFunction {
        public override void run(ScreenManager sm) { Environment.Exit(1); }
    }

    public class MenuItemFunctionStartOver : MenuItemFunction {
        public override void run(ScreenManager sm) {
            GameScreen game = (GameScreen)sm.getScreen(ScreenId.Game);
            game.startOver();
            new MenuItemFunctionUnpause().run(sm);
        }
    }

    public class MenuItemFunctionPlay : MenuItemFunction {
        public override void run(ScreenManager sm) {
            Screen game = sm.getScreen(ScreenId.Game);
            Screen pause = sm.getScreen(ScreenId.Pause);
            Screen mainmenu = sm.getScreen(ScreenId.MainMenu);
            mainmenu.DoDraw = mainmenu.DoUpdate = false;
            game.DoDraw = game.DoUpdate = true;
            pause.DoUpdate = true;
        }
    }

    public class MenuItemFunctionUnpause : MenuItemFunction {
        public override void run(ScreenManager sm) {
            PauseScreen pause = (PauseScreen) sm.getScreen(ScreenId.Pause);
            if (pause.isPaused()) {
                Screen game = sm.getScreen(ScreenId.Game);
                pause.escPressed = false;
                if (pause.isPaused())
                    pause.togglePause();

                // If paused draw this and not game
                pause.DoDraw = false;
                game.DoUpdate = true;
            }
        }
    }

    public class MenuItemFunctionMainMenuHelp : MenuItemFunction {
        public override void run(ScreenManager sm) {
            // Close all
            foreach (Screen s in sm.screens.Values) {
                s.DoDraw = s.DoUpdate = false;
            }

            Screen help = sm.getScreen(ScreenId.MainMenuHelp);
            help.DoDraw = help.DoUpdate = true;
        }
    }

    public class MenuItemFunctionMainMenu : MenuItemFunction {
        public override void run(ScreenManager sm) {
            // Close all
            foreach (Screen s in sm.screens.Values) s.DoDraw = s.DoUpdate = false;

            Screen mainMenu = sm.getScreen(ScreenId.MainMenu);
            mainMenu.DoUpdate = mainMenu.DoDraw = true;
        }
    }
}