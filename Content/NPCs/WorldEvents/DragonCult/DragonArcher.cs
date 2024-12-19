using DialogueHelper.UI.Dialogue;

namespace Windfall.Content.NPCs.WorldEvents.DragonCult;

public class DragonArcher : ModNPC
{
    public override string Texture => "Windfall/Assets/NPCs/WorldEvents/LunarCultistArcher";
    private static SoundStyle SpawnSound => new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");
    public override void SetStaticDefaults()
    {
        this.HideFromBestiary();
        NPCID.Sets.ActsLikeTownNPC[Type] = true;
        Main.npcFrameCount[Type] = 5;
        NPCID.Sets.NoTownNPCHappiness[Type] = true;
        ModContent.GetInstance<DialogueUISystem>().DialogueClose += CloseEffect;
    }
    public override void SetDefaults()
    {
        NPC.friendly = true; // NPC Will not attack player
        NPC.width = 38;
        NPC.height = 52;
        NPC.aiStyle = 0;
        NPC.damage = 45;
        NPC.defense = 14;
        NPC.lifeMax = 210;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 1f;
        NPC.immortal = true;

        AnimationType = NPCID.BartenderUnconscious;
    }
    public override bool CanChat() => !ModContent.GetInstance<DialogueUISystem>().isDialogueOpen;
    public override string GetChat()
    {
        Main.CloseNPCChatOrSign();

        ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "MechanicShed", new(Name, [NPC.whoAmI]));

        return base.GetChat();
    }
    public override bool CheckActive() => false;
    private static void CloseEffect(string treeKey, int dialogueID, int buttonID)
    {
        if(treeKey == "MechanicShed")
        {
            NPC me = Main.npc[(int)ModContent.GetInstance<DialogueUISystem>().CurrentDialogueContext.Arguments[0]];

            foreach (NPC npc in Main.npc.Where(n => n.type == me.type && n.active))
            {
                for (int i = 0; i < 50; i++)
                {
                    Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                    Dust d = Dust.NewDustPerfect(npc.Center, DustID.GoldFlame, speed * 3, Scale: 1.5f);
                    d.noGravity = true;
                }
                npc.active = false;
            }
        }
    }
}
