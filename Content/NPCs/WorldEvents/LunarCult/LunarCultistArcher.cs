using Microsoft.Xna.Framework;
using CalamityMod;
using CalamityMod.Items.Accessories;
using CalamityMod.NPCs.DesertScourge;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Windfall.Common.Utilities;
using Windfall.Content.Items.Fishing;
using Windfall.Content.Items.Utility;
using Windfall.Content.Items.Weapons.Misc;
using System.Linq;

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
            return Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.LunarCult.MechanicShed{CurrentDialogue}").Value;
        }
        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            switch (CurrentDialogue)
            {
                case DialogueState.Initial:
                    if (firstButton)
                    {
                        CurrentDialogue = DialogueState.ExploringHuh;
                    }
                    else
                    {
                        CurrentDialogue = DialogueState.WhoAreWe;
                    }
                    break;
                case DialogueState.ExploringHuh:
                    CurrentDialogue = DialogueState.End;
                    break;
                case DialogueState.WhoAreWe:
                    CurrentDialogue = DialogueState.End;
                    break;
                case DialogueState.End:
                    foreach (NPC npc in Main.npc.Where(n => n.type == NPC.type && n.active))
                    {
                        for (int i = 0; i < 50; i++)
                        {
                            Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                            Dust d = Dust.NewDustPerfect(npc.Center, DustID.GoldFlame, speed * 3, Scale: 1.5f);
                            d.noGravity = true;
                        }
                        npc.active = false;
                    }
                    break;
            }
            Main.npcChatText = Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.LunarCult.MechanicShed{CurrentDialogue}").Value;
        }
        public override void SetChatButtons(ref string button, ref string button2)
        {

            switch (CurrentDialogue)
            {
                case DialogueState.Initial:
                    button = "I'm just exploring.";
                    button2 = "Who are you?";
                    break;
                case DialogueState.WhoAreWe:
                    button = "Sounds interesting.";
                    button2 = "So... a cult?";
                    break;
                case DialogueState.ExploringHuh:
                    button = "Just looking for supplies.";
                    button2 = "What's it matter to you?";
                    break;
                case DialogueState.End:
                    button = "Thank you.";
                    button2 = "Finally...";
                    break;
            }
        }
    }
}
