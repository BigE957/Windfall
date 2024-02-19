using Microsoft.Xna.Framework;
using CalamityMod;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using System.Linq;
using CalamityMod.NPCs.DesertScourge;

namespace Windfall.Content.NPCs.WanderingNPCs
{
    public class LunarCultistDevotee : ModNPC
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
        public override string Texture => "Windfall/Assets/NPCs/WorldEvents/LunarCultistDevotee";
        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            NPCID.Sets.ActsLikeTownNPC[Type] = true;
            Main.npcFrameCount[Type] = 6;
            NPCID.Sets.NoTownNPCHappiness[Type] = true;
        }
        public override void SetDefaults()
        {
            NPC.friendly = true; // NPC Will not attack player
            NPC.width = 34;
            NPC.height = 52;
            NPC.aiStyle = 0;
            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 400;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 1f;
            NPC.immortal = true;

            AnimationType = NPCID.CultistDevote;
        }
        public override bool CanChat() => true;

        public override string GetChat()
        {
            return Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.LunarCult.OcularDevotee.{CurrentDialogue}").Value;
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
            Main.npcChatText = Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.LunarCult.OcularDevotee.{CurrentDialogue}").Value;
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
