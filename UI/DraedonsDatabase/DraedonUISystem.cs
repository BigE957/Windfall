using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace WindfallAttempt1.UI.DraedonsDatabase
{
    public class DraedonUISystem : ModSystem
    {
        internal UserInterface DraedonUI;
        internal DraedonUI DraedonUIState;
        public static bool databaseOpen = false;
        public override void Load()
        {
            if (!Main.dedServ)
            {
                DraedonUI = new UserInterface();

                DraedonUIState = new DraedonUI();
                DraedonUIState.Activate(); // Activate calls Initialize() on the UIState if not initialized and calls OnActivate, then calls Activate on every child element.
            }
        }
        public override void Unload()
        {
            DraedonUIState = null;
        }
        internal void ShowMyUI()
        {
            databaseOpen = true;
            DraedonUI?.SetState(DraedonUIState);
        }

        internal void HideMyUI()
        {
            databaseOpen = false;
            DraedonUI?.SetState(null);
        }

        private GameTime _lastUpdateUiGameTime;

        public override void UpdateUI(GameTime gameTime)
        {
            _lastUpdateUiGameTime = gameTime;
            if (DraedonUI?.CurrentState != null)
            {
                DraedonUI.Update(gameTime);
            }
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "MyMod: MyInterface",
                    delegate
                    {
                        if (_lastUpdateUiGameTime != null && DraedonUI?.CurrentState != null)
                        {
                            DraedonUI.Draw(Main.spriteBatch, _lastUpdateUiGameTime);
                        }
                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }
    }
}
