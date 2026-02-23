using CalamityMod.UI;
using Windfall.Content.Projectiles.Props;

namespace Windfall.Content.UI.Events;
public class AstralSiphonEventBar : InvasionProgressUI
{
    public override bool IsActive => Main.npc.Any(n => n.active && n.type == ModContent.NPCType<SelenicSiphon>() && (n.ModNPC as SelenicSiphon).EventActive);
    public override float CompletionRatio =>(Main.npc[NPC.FindFirstNPC(ModContent.NPCType<SelenicSiphon>())].ModNPC as SelenicSiphon).FillRatio;
    public override string InvasionName => GetWindfallTextValue("Events.AstralSiphon");
    public override Color InvasionBarColor => Color.OrangeRed;
    public override Texture2D IconTexture => ModContent.Request<Texture2D>("CalamityMod/Items/SummonItems/AstralChunk").Value;
}
