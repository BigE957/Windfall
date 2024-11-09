using CalamityMod.NPCs.PlaguebringerGoliath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace Windfall.Content.Skies.CorruptCommunion;

public class CorruptCommunionScreenShaderData : ScreenShaderData
{
    public CorruptCommunionScreenShaderData(string passName)
        : base(passName)
    {
    }

    public override void Update(GameTime gameTime)
    {
        if (!Main.LocalPlayer.Godly().CorruptCommunion)
        {
            Filters.Scene["Windfall:CorruptCommunion"].Deactivate();
        }
    }

    public override void Apply()
    {
        if (Main.LocalPlayer.Godly().CorruptCommunion)
        {
            UseTargetPosition(Main.LocalPlayer.Center);
        }
        base.Apply();
    }
}
