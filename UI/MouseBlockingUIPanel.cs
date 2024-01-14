using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace WindfallAttempt1.UI
{
    public class MouseBlockingUIPanel : UIPanel
    {
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Checking ContainsPoint and then setting mouseInterface to true is very common
            // This causes clicks on this UIElement to not cause the player to use current items
            if (ContainsPoint(Main.MouseScreen))
            {
                Main.LocalPlayer.mouseInterface = true;
            }
        }
    }
}
