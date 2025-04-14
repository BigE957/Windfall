using DialogueHelper.UI.Dialogue;
using Windfall.Content.Projectiles.Props;

namespace Windfall.Content.NPCs.WorldEvents.LunarCult;
public class DisgracedApostleNPC : ModNPC
{
    private enum States
    {
        Idle,
        PreSiphon,
        SiphonStart,
        SiphonDuring,
        SiphonAfter
    }
    private States AIState
    {
        get => (States)NPC.ai[0];
        set => NPC.ai[0] = (float)value;
    }
    public override string Texture => "Windfall/Assets/NPCs/WorldEvents/TheOrator_NPC";
    public override void SetStaticDefaults()
    {
        this.HideBestiaryEntry();
        NPCID.Sets.ActsLikeTownNPC[Type] = true;
        Main.npcFrameCount[Type] = 1;
        NPCID.Sets.NoTownNPCHappiness[Type] = true;
        ModContent.GetInstance<DialogueUISystem>().TreeClose += CloseEffect;
    }
    public override void SetDefaults()
    {
        NPC.friendly = true;
        NPC.width = 58;
        NPC.height = 70;
        NPC.aiStyle = 0;
        NPC.damage = 0;
        NPC.defense = 0;
        NPC.lifeMax = 800;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0f;
        NPC.immortal = true;
    }

    public override void AI()
    {
        if(AIState == States.SiphonStart)
        {
            NPC.NewNPC(Terraria.Entity.GetSource_TownSpawn(), (int)NPC.Center.X + 56, (int)NPC.Bottom.Y, ModContent.NPCType<SelenicSiphon>());
            AIState = States.SiphonDuring;
        }
    }

    public override bool CanChat() => (Main.LocalPlayer.LunarCult().apostleQuestTracker == 12 || AIState == States.PreSiphon) && !ModContent.GetInstance<DialogueUISystem>().isDialogueOpen;
    public override string GetChat()
    {
        Main.CloseNPCChatOrSign();
        if(AIState == States.PreSiphon)
            ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "Apostle/AstralSiphonStart", new(Name, [NPC.whoAmI]));
        else
            ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "Apostle/AstralSiphonPrep", new(Name, [NPC.whoAmI]));

        return "";
    }

    private static void CloseEffect(string treeKey, int dialogueID, int buttonID, bool swapped)
    {
        if (treeKey.Contains("Apostle/"))
        {
            NPC apostle = Main.npc[(int)ModContent.GetInstance<DialogueUISystem>().CurrentDialogueContext.Arguments[0]];

            switch (treeKey)
            {
                case "Apostle/AstralSiphonPrep":
                    Main.LocalPlayer.LunarCult().apostleQuestTracker++; //13
                    break;
                case "Apostle/AstralSiphonStart":
                    if (buttonID == 1)
                        apostle.active = false;
                    else
                        apostle.ai[0] = (int)States.SiphonDuring;
                    break;
            }
        }
    }

    public override bool CheckActive() => false;
}
