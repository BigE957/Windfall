using DialogueHelper.UI.Dialogue;
using Terraria;

namespace Windfall.Content.NPCs.WorldEvents.DragonCult;

public class DragonCultist : ModNPC
{
    public override string Texture => "Windfall/Assets/NPCs/WorldEvents/LunarBishop";
    internal static SoundStyle SpawnSound => new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");
    public override void SetStaticDefaults()
    {
        this.HideBestiaryEntry();
        //NPCID.Sets.ActsLikeTownNPC[Type] = true;
        Main.npcFrameCount[Type] = 1;
        NPCID.Sets.NoTownNPCHappiness[Type] = true;
        ModContent.GetInstance<DialogueUISystem>().TreeClose += CloseEffect;

    }
    public override void SetDefaults()
    {
        NPC.friendly = true; // NPC Will not attack player
        NPC.width = 36;
        NPC.height = 58;
        NPC.aiStyle = 0;
        NPC.damage = 0;
        NPC.defense = 0;
        NPC.lifeMax = 500;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 1f;
        NPC.immortal = true;
    }
    public override void OnSpawn(IEntitySource source)
    {
        NPC.GivenName = "???";
    }
    public override bool CanChat() => !ModContent.GetInstance<DialogueUISystem>().isDialogueOpen;
    public override string GetChat()
    {
        Main.CloseNPCChatOrSign();

        ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "SkeletronDefeat", new(Name, [NPC.whoAmI]));

        return base.GetChat();
    }
    private static void CloseEffect(string treeKey, int dialogueID, int buttonID, bool swapped)
    {
        if (treeKey == "SkeletronDefeat")
        {
            NPC me = Main.npc[(int)ModContent.GetInstance<DialogueUISystem>().CurrentDialogueContext.Arguments[0]];
            for (int i = 0; i < 50; i++)
            {
                Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                Dust d = Dust.NewDustPerfect(me.Center, DustID.GoldFlame, speed * 3, Scale: 1.5f);
                d.noGravity = true;
            }
            me.active = false;
        }
    }
}
