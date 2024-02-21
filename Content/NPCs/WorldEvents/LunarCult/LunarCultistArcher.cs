using CalamityMod;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Windfall.Common.Utilities.Utilities;
using System.Collections.Generic;

namespace Windfall.Content.NPCs.WanderingNPCs
{
    public class LunarCultistArcher : ModNPC
    {
        private enum DialogueState
        {
            Initial,
            WhoAreWe,
            WannaJoin,
            NuhUh,
            ExploringHuh,
            FairEnough,
            SomethingBad,
            End
        }
        private DialogueState CurrentDialogue
        {
            get => (DialogueState)NPC.ai[1];
            set => NPC.ai[1] = (int)value;
        }
        public override string Texture => "Windfall/Assets/NPCs/WorldEvents/LunarCultistArcher";
        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            NPCID.Sets.ActsLikeTownNPC[Type] = true;
            Main.npcFrameCount[Type] = 5;
            NPCID.Sets.NoTownNPCHappiness[Type] = true;
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
        public override bool CanChat() => true;

        public override string GetChat()
        {
            return Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.LunarCult.MechanicShed.{CurrentDialogue}").Value;
        }
        private List<dialogueDirections> MyDialogue = new()
        {
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Initial,
                Button1 = new(){name = "Your guardian?", heading = (int)DialogueState.ExploringHuh},
                Button2 = new(){name = "What issues?", heading = (int)DialogueState.WhoAreWe},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ExploringHuh,
                Button1 = new(){name = "That's a relief.", heading = (int)DialogueState.FairEnough},
                Button2 = new(){name = "You cursed him...?", heading = (int)DialogueState.SomethingBad},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.WhoAreWe,
                Button1 = new(){name = "I'll see what I can do.", heading = (int)DialogueState.WannaJoin},
                Button2 = new(){name = "No promises.", heading = (int)DialogueState.NuhUh},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.End,
                Button1 = new(){name = "Goodbye!", heading = (int)DialogueState.End},
                Button2 = new(){name = "Took long enough...", heading = (int)DialogueState.NuhUh},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.WannaJoin,
                Button1 = new(){name = "I'll keep that in mind!", heading = (int)DialogueState.End},
                Button2 = new(){name = "No thanks...", heading = (int)DialogueState.End},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.NuhUh,
                Button1 = new(){name = "I guess that makes sense.", heading = (int)DialogueState.End},
                Button2 = new(){name = "Surely...", heading = (int)DialogueState.End},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.FairEnough,
                Button1 = new(){name = "Can do.", heading = (int)DialogueState.End},
                Button2 = new(){name = "Fine.", heading = (int)DialogueState.End},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.SomethingBad,
                Button1 = new(){name = "You might be right.", heading = (int)DialogueState.End},
                Button2 = new(){name = "Are you...?", heading = (int)DialogueState.End},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.SomethingBad,
                Button1 = new(){name = "Thank you.", heading = -1},
                Button2 = new(){name = "Finally...", heading = -1},
                end = true
            },
        };
        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            CurrentDialogue = (DialogueState)GetNPCConversation(MyDialogue, (int)CurrentDialogue, firstButton);
            Main.npcChatText = Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.LunarCult.MechanicShed.{CurrentDialogue}").Value;
        }
        public override void SetChatButtons(ref string button, ref string button2)
        {
            SetConversationButtons(MyDialogue, (int)CurrentDialogue, ref button, ref button2);
        }
        public override bool CheckActive()
        {
            return false;
        }
    }
}
