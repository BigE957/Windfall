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
        internal Desktop DesktopState;
        internal DraeDash DraeDashState;
        public static bool databaseOpen = false;
        public override void Load()
        {
            if (!Main.dedServ)
            {
                DraedonUI = new UserInterface();

                DesktopState = new Desktop();
                DesktopState.Activate(); // Activate calls Initialize() on the UIState if not initialized and calls OnActivate, then calls Activate on every child element.

                DraeDashState = new DraeDash();
                DraeDashState.Activate();
            }
        }
        public override void Unload()
        {
            DesktopState = null;
        }
        internal void OpenDraedonDatabase()
        {
            databaseOpen = true;
            DraedonUI?.SetState(DesktopState);
        }

        internal void CloseDraedonDatabase()
        {
            databaseOpen = false;
            DraedonUI?.SetState(null);
        }

        internal void OpenDraedonApp(string app)
        {
            DraedonUI?.SetState(DraeDashState);
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
