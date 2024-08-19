using CalamityMod.NPCs.PlaguebringerGoliath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace Windfall.Content.Skies.CrimsonCommunion
{
    public class CrimsonCommunionScreenShaderData : ScreenShaderData
    {
        public CrimsonCommunionScreenShaderData(string passName)
            : base(passName)
        {
        }

        public override void Update(GameTime gameTime)
        {
            if (!Main.LocalPlayer.Godly().CrimsonCommunion)
            {
                Filters.Scene["Windfall:CrimsonCommunion"].Deactivate();
            }
        }

        public override void Apply()
        {
            if (Main.LocalPlayer.Godly().CrimsonCommunion)
            {
                UseTargetPosition(Main.LocalPlayer.Center);
            }
            base.Apply();
        }
    }
}
