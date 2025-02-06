using DialogueHelper.UI.Dialogue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windfall.Common.Systems.WorldEvents;

namespace Windfall.Content.NPCs.WorldEvents.LunarCult;
public class DisgracedApostle : ModNPC
{
    private enum States
    {
        Idle,
        TutorialChat,
        RitualEvent,
        BetrayalChat,
    }
    private States AIState
    {
        get => (States)NPC.ai[0];
        set => NPC.ai[0] = (float)value;
    }
    public override string Texture => "Windfall/Assets/NPCs/WorldEvents/TheOrator_NPC";
    public override void SetStaticDefaults()
    {
        this.HideFromBestiary();
        NPCID.Sets.ActsLikeTownNPC[Type] = true;
        Main.npcFrameCount[Type] = 1;
        NPCID.Sets.NoTownNPCHappiness[Type] = true;
        ModContent.GetInstance<DialogueUISystem>().DialogueClose += CloseEffect;
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
    public override bool CanChat() => Main.LocalPlayer.LunarCult().apostleQuestTracker == 12;
    public override string GetChat()
    {
        Main.CloseNPCChatOrSign();

        ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "Apostle/AstralSiphonPrep", new(Name, [NPC.whoAmI]));

        return "";
    }

    private static void CloseEffect(string treeKey, int dialogueID, int buttonID)
    {
        if (treeKey.Contains("Apostle/"))
        {
            NPC apostle = Main.npc[(int)ModContent.GetInstance<DialogueUISystem>().CurrentDialogueContext.Arguments[0]];

            switch (treeKey)
            {
                case "Apostle/AstralSiphonPrep":
                    Main.LocalPlayer.LunarCult().apostleQuestTracker++; //13
                    break;
            }
        }
    }

    public override bool CheckActive() => false;
}
