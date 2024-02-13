using CalamityMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.Localization;
using Terraria;
using Terraria.ModLoader;

namespace Windfall.Content.NPCs.WorldEvents.LunarCult
{
    public class LunarBishop : ModNPC
    {
        private enum DialogueState
        {
            Initial,
            SomethingAmazing,
            TheBishop,
            End
        }
        private DialogueState CurrentDialogue
        {
            get => (DialogueState)NPC.ai[1];
            set => NPC.ai[1] = (int)value;
        }
        public override string Texture => "Windfall/Assets/NPCs/WorldEvents/LunarBishop";
        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            NPCID.Sets.ActsLikeTownNPC[Type] = true;
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

            AnimationType = NPCID.BartenderUnconscious;
        }
        public override bool CanChat() => true;

        public override string GetChat()
        {
            return Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.LunarCult.OcularDevotee{CurrentDialogue}").Value;
        }
        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            switch (CurrentDialogue)
            {
                case DialogueState.Initial:
                    if (firstButton)
                    {
                        CurrentDialogue = DialogueState.SomethingAmazing;
                    }
                    else
                    {
                        CurrentDialogue = DialogueState.TheBishop;
                    }
                    break;
                case DialogueState.End:
                    Main.CloseNPCChatOrSign();
                    break;
                default:
                    CurrentDialogue = DialogueState.End;
                    break;
            }
            Main.npcChatText = Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.LunarCult.OcularDevotee{CurrentDialogue}").Value;
        }
        public override void SetChatButtons(ref string button, ref string button2)
        {
            switch (CurrentDialogue)
            {
                case DialogueState.Initial:
                    button = "What are you doing?";
                    button2 = "The Bishop?";
                    break;
                case DialogueState.SomethingAmazing:
                    button = "Interesting.";
                    button2 = "That sounds...";
                    break;
                case DialogueState.TheBishop:
                    button = "I'll keep that in mind!";
                    button2 = "No thanks...";
                    break;
                case DialogueState.End:
                    button = "Alright.";
                    button2 = "If you say so...";
                    break;
            }
        }
        public override bool CheckActive()
        {
            return false;
        }
    }
}
