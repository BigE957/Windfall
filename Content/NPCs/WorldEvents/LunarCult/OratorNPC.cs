using Windfall.Common.Systems.WorldEvents;
using DialogueHelper.Content.UI.Dialogue;

namespace Windfall.Content.NPCs.WorldEvents.LunarCult
{
    public class OratorNPC : ModNPC
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
            NPC.friendly = true; // NPC Will not attack player
            NPC.width = 58;
            NPC.height = 70;
            NPC.aiStyle = 0;
            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 1000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.immortal = true;
        }
        public override bool CanChat() => NPC.ai[0] != 0 && !ModContent.GetInstance<DialogueUISystem>().isDialogueOpen;
        public override string GetChat()
        {
            Main.CloseNPCChatOrSign();
            Main.NewText(AIState);
            ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "TheOrator/" + AIState.ToString());

            return "In the Cult Base, straight Orating it. And by it i mean, lets just say, my Tablet";
        }
        private void CloseEffect(string treeKey, int dialogueID, int buttonID)
        {
            if (treeKey == "TheOrator/" + States.TutorialChat.ToString())
            {
                NPC orator = Main.npc.First(n => n.active && n.type == ModContent.NPCType<OratorNPC>() && n.ai[0] == (int)States.TutorialChat);
                LunarCultBaseSystem.TutorialComplete = true;
                orator.ai[0] = 0;
                return;
            }
            else if (treeKey == "TheOrator/" + States.RitualEvent.ToString() && buttonID == 1)
            {
                NPC orator = Main.npc.First(n => n.active && n.type == ModContent.NPCType<OratorNPC>() && n.ai[0] == (int)States.RitualEvent);
                LunarCultBaseSystem.State = LunarCultBaseSystem.SystemStates.Ritual;
                LunarCultBaseSystem.Active = true;
                orator.ai[0] = 0;
                return;
            }
            else if (treeKey == "TheOrator/" + States.BetrayalChat.ToString() && buttonID == 1)
            {
                NPC orator = Main.npc.First(n => n.active && n.type == ModContent.NPCType<OratorNPC>() && n.ai[0] == (int)States.TutorialChat);
                LunarCultBaseSystem.BetrayalActive = true;
                orator.ai[0] = 0;
                return;
            }
        }
    }
}
