using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace WindfallAttempt1.UI.DraedonsDatabase
{
    public class DraedonUI : UIState
    {
        public override void OnInitialize()
        {
            UIPanel panel = new UIPanel();
            panel.Width.Set(Main.screenWidth, 0);          
            panel.Height.Set(Main.screenHeight, 0);
            panel.HAlign = panel.VAlign = 0.5f;
            Append(panel);

            UIText header = new UIText("Draedon's Database");
            header.HAlign = 0.5f;  // 1
            header.Top.Set(15, 0); // 2
            panel.Append(header);

            UIText text = new UIText("Draedon Gaming!");
            text.HAlign = 0.5f; // 1
            text.VAlign = 0.5f; // 1
            panel.Append(text);
        }
    }
}
