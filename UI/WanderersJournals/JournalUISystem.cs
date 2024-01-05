using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace WindfallAttempt1.UI.WanderersJournals
{
    public class JournalUISystem : ModSystem
    {
        internal JournalUIState JournalUIState;
        private UserInterface JournalUI;

        public void ShowMyUI()
        {
            JournalUI?.SetState(JournalUIState);
        }

        public void HideMyUI()
        {
            JournalUI?.SetState(null);
        }
        public override void Load()
        {
            // Create custom interface which can swap between different UIStates
            JournalUI = new UserInterface();
            // Creating custom UIState
            JournalUIState = new JournalUIState();

            // Activate calls Initialize() on the UIState if not initialized, then calls OnActivate and then calls Activate on every child element
            JournalUIState.Activate();
        }
        public override void UpdateUI(GameTime gameTime)
        {
            JournalUI?.Update(gameTime);
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "Windfall: Displays the Wander's Journal Page",
                    delegate
                    {
                        JournalUI.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
}
