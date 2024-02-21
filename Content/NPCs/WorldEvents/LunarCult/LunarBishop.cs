using CalamityMod;
using Terraria.ID;
using Terraria.Localization;
using Terraria;
using Terraria.ModLoader;
using static Windfall.Common.Utilities.Utilities;
using static Windfall.Content.NPCs.TravellingNPCs.TravellingCultist;
using System.Collections.Generic;

namespace Windfall.Content.NPCs.WorldEvents.LunarCult
{
    public class LunarBishop : ModNPC
    {
        private enum DialogueState
        {
            Initial,
            Guardian,
            Issues,
            End
        }
        private DialogueState CurrentDialogue = 0;
        public override string Texture => "Windfall/Assets/NPCs/WorldEvents/LunarBishop";
        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            //NPCID.Sets.ActsLikeTownNPC[Type] = true;
            Main.npcFrameCount[Type] = 1;
            NPCID.Sets.NoTownNPCHappiness[Type] = true;
        }
        public override void SetDefaults()
        {
            NPC.friendly = true; // NPC Will not attack player
            NPC.width = 42;
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
        public override bool CanChat() => true;

        public override string GetChat()
        {
            return Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.LunarCult.DungeonBishop.{CurrentDialogue}").Value;
        }
        private List<dialogueDirections> MyDialogue = new()
        {
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Initial,
                Button1 = new(){name = "Your guardian?", heading = (int)DialogueState.Guardian},
                Button2 = new(){name = "What issues?", heading = (int)DialogueState.Issues},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Guardian,
                Button1 = new(){name = "That's a relief.", heading = (int)DialogueState.End},
                Button2 = new(){name = "You cursed him...?", heading = (int)DialogueState.End},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Issues,
                Button1 = new(){name = "I'll see what I can do.", heading = (int)DialogueState.End},
                Button2 = new(){name = "No promises.", heading = (int)DialogueState.End},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.End,
                Button1 = new(){name = "Goodbye!", heading = (int)DialogueState.End},
                Button2 = new(){name = "Took long enough...", heading = (int)DialogueState.End},
                end = true
            },
        };
        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            CurrentDialogue = (DialogueState)GetNPCConversation(MyDialogue, (int)CurrentDialogue, firstButton);
            Main.npcChatText = Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.LunarCult.DungeonBishop.{CurrentDialogue}").Value;
        }
        public override void SetChatButtons(ref string button, ref string button2)
        {
            SetConversationButtons(MyDialogue, (int)CurrentDialogue, ref button, ref button2);
        }
    }
}
