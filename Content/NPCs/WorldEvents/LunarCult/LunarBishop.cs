using CalamityMod;
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
            return Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.LunarCult.DungeonBishop{CurrentDialogue}").Value;
        }
        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            switch (CurrentDialogue)
            {
                case DialogueState.Initial:
                    if (firstButton)
                    {
                        CurrentDialogue = DialogueState.Guardian;
                    }
                    else
                    {
                        CurrentDialogue = DialogueState.Issues;
                    }
                    break;
                case DialogueState.End:
                    Main.CloseNPCChatOrSign();
                    break;
                default:
                    CurrentDialogue = DialogueState.End;
                    break;
            }
            Main.npcChatText = Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.LunarCult.DungeonBishop{CurrentDialogue}").Value;
        }
        public override void SetChatButtons(ref string button, ref string button2)
        {
            switch (CurrentDialogue)
            {
                case DialogueState.Initial:
                    button = "Your guardian?";
                    button2 = "What issues?";
                    break;
                case DialogueState.Guardian:
                    button = "That's a relief.";
                    button2 = "You cursed him...?";
                    break;
                case DialogueState.Issues:
                    button = "I'll see what I can do.";
                    button2 = "No promises.";
                    break;
                case DialogueState.End:
                    button = "Goodbye!";
                    button2 = "Took long enough...";
                    break;
            }
        }
    }
}
