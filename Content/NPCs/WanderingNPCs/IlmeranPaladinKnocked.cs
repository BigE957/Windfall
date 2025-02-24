using CalamityMod.NPCs.DesertScourge;

namespace Windfall.Content.NPCs.WanderingNPCs;

public class IlmeranPaladinKnocked : ModNPC
{
    public override string Texture => "Windfall/Assets/NPCs/WanderingNPCs/IlmeranPaladinKnocked";
    public override void SetStaticDefaults()
    {
        this.HideFromBestiary();
        NPCID.Sets.ActsLikeTownNPC[Type] = true;
        NPCID.Sets.NoTownNPCHappiness[Type] = true;
    }
    public override void SetDefaults()
    {
        NPC.friendly = true; // NPC Will not attack player
        NPC.width = 18;
        NPC.height = 40;
        NPC.aiStyle = -1;
        NPC.damage = 10;
        NPC.defense = 15;
        NPC.lifeMax = 250;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 1f;
        NPC.immortal = true;

        AnimationType = NPCID.BartenderUnconscious;
    }
    public override bool CanChat() => true;

    public override string GetChat()
    {
        Main.CloseNPCChatOrSign();

        NPC.Transform(ModContent.NPCType<IlmeranPaladin>());
        NPC.ai[3] = 1;

        return "";
    }
    public override void OnChatButtonClicked(bool firstButton, ref string shop)
    {
        new IlmeranPaladin().OnChatButtonClicked(firstButton, ref shop);
    }

    public override bool CheckActive() => !NPC.AnyNPCs(ModContent.NPCType<DesertScourgeHead>());
}
