using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RPG.Screen
{
    public class MenuScreen : Screen
    {
        KeyboardState oldState;
        int curSelection;

        List<MenuItem> menuItems;

        Vector2 selectedItemExtraSize;

        public MenuScreen(ScreenManager screenManager, String[] items, MenuItemFunction[] funcs) : base(screenManager) {
            menuItems = new List<MenuItem>();
            curSelection = 0;
            oldState = Keyboard.GetState();
          
            selectedItemExtraSize = ScreenManager.Font.MeasureString("[  ]");

            for (int i = 0; i < items.GetLength(0); i++)
                menuItems.Add(new MenuItem(this, items[i], funcs[i], MenuItem.MenuItemAlignment.Center));


            if (menuItems.Count == 0) {
                Console.WriteLine("Tried to make a menu screen with no items!");
                Environment.Exit(1);
            }
        }

        public override void Update(GameTime gTime) {
            KeyboardState kb = Keyboard.GetState();

            if (kb.IsKeyDown(Keys.Down) && oldState.IsKeyUp(Keys.Down)) {
                curSelection++;
                updateSelected();
            } else if (kb.IsKeyDown(Keys.Up) && oldState.IsKeyUp(Keys.Up)) {
                curSelection--;
                updateSelected();
            } 

            if (kb.IsKeyDown(Keys.Enter) && oldState.IsKeyUp(Keys.Enter)) {
                menuItems[curSelection].run(screenManager);
            }

            oldState = kb;
        }
        
        protected void updateSelected() {
            curSelection %= menuItems.Count;
            if (curSelection < 0) curSelection = menuItems.Count - 1;
        }

        public override void Draw(GameTime gTime) {
            for (int i=0; i < menuItems.Count; i++) {
                MenuItem item = menuItems[i];

                bool isSelected = (i == curSelection);
                float yPosOffset = -(selectedItemExtraSize.Y * 1.5f * (menuItems.Count - 1) / 2) + (i * selectedItemExtraSize.Y * 1.5f);
                item.draw(isSelected, (int) yPosOffset);
            }
        }

        public override void LoadContent() { }
        public override void UnloadContent() { }
    }
}
