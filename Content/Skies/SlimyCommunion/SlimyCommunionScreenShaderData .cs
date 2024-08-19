using CalamityMod.NPCs.PlaguebringerGoliath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace Windfall.Content.Skies.ScreenShaders
{
    public class SlimyCommunionScreenShaderData : ScreenShaderData
    {
        private float Opacity = 0f;

        public SlimyCommunionScreenShaderData(string passName)
            : base(passName)
        {
        }       

        public override void Update(GameTime gameTime)
        {
            if (!Main.LocalPlayer.Godly().SlimyCommunion)
            {
                Filters.Scene["Windfall:SlimyCommunion"].Deactivate();
            }
        }

        public override void Apply()
        {
            if (Main.LocalPlayer.Godly().SlimyCommunion)
            {
                UseTargetPosition(Main.LocalPlayer.Center);
            }
            base.Apply();
        }
    }
}
