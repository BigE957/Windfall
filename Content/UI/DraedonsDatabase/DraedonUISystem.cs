using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Windfall.Content.Systems;

namespace Windfall.Content.UI.DraedonsDatabase
{
    public class DraedonUISystem : ModSystem
    {
        internal UserInterface DraedonUI;
        internal Desktop DesktopState;
        internal DraeDash DraeDashState;
        internal PlutusVault PlutusVaultState;
        internal OrderEnRoute OrderEnRouteState;
        public static Dictionary<string, int> DraeDashOrder = new Dictionary<string, int>();
        public static bool databaseOpen = false;
        public override void PostSetupContent()
        {
            if (!Main.dedServ)
            {
                DraedonUI = new UserInterface();

                DesktopState = new Desktop();
                DesktopState.Activate(); // Activate calls Initialize() on the UIState if not initialized and calls OnActivate, then calls Activate on every child element.

                DraeDashState = new DraeDash();
                DraeDashState.Activate();

                PlutusVaultState = new PlutusVault();
                PlutusVaultState.Activate();

                OrderEnRouteState = new OrderEnRoute();
                OrderEnRouteState.Activate();
            }
        }
        public override void Unload()
        {
            DesktopState = null;
        }
        internal void OpenDraedonDatabase()
        {
            if (WorldSaveSystem.CreditDataNames != null)
            {
                if (!WorldSaveSystem.CreditDataNames.Any(n => n == Main.LocalPlayer.name))
                {
                    WorldSaveSystem.CreditDataNames.Add(Main.LocalPlayer.name);
                    WorldSaveSystem.CreditDataCredits.Add(0);
                }
            }
            else
            {
                WorldSaveSystem.CreditDataNames.Add(Main.LocalPlayer.name);
                WorldSaveSystem.CreditDataCredits.Add(0);
            }
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
            if (app == "DraeDash")
            {
                if (DraeDash.orderEnRoute)
                {
                    DraedonUI?.SetState(OrderEnRouteState);
                }
                else
                {
                    DraedonUI?.SetState(DraeDashState);
                }
            }
            else if (app == "PlutusVault")
                DraedonUI?.SetState(PlutusVaultState);
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
