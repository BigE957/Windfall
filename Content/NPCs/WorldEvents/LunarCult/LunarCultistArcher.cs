using Microsoft.Xna.Framework;
using CalamityMod;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Windfall.Common.Utilities.Utilities;
using System.Collections.Generic;
using Terraria.DataStructures;
using System.Linq;
using Terraria.Audio;

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
        internal static SoundStyle SpawnSound => new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");
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
        public override void OnSpawn(IEntitySource source)
        {
            if (NPC.ai[0] == 0)
            {
                NPC.alpha = 255;
                if (AlignNPCWithGround(Main.npc[NPC.whoAmI]))
                {
                    NPC.position.Y -= 6;
                    NPC.alpha = 0;
                    for (int i = 0; i < 50; i++)
                    {
                        Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                        Dust d = Dust.NewDustPerfect(NPC.Center, DustID.GoldFlame, speed * 3, Scale: 1.5f);
                        d.noGravity = true;
                    }
                    SoundEngine.PlaySound(SpawnSound, NPC.Center);
                }
                else
                {
                    NPC.active = false;
                }
            }
        }
        public override bool CanChat()
        {
            if (NPC.ai[0] == 0)
                return false;
            else
                return true;
        }
        public override string GetChat()
        {
            return Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.LunarCult.MechanicShed.{CurrentDialogue}").Value;
        }
        private readonly List<dialogueDirections> MyDialogue = new()
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
                Button1 = new(){name = "Thank you.", heading = -1, end = true},
                Button2 = new(){name = "Finally...", heading = -1, end = true},
            },
        };
        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            CurrentDialogue = (DialogueState)GetNPCConversation(MyDialogue, (int)CurrentDialogue, firstButton);
            if (CurrentDialogue == DialogueState.End)
            {
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
            }
            else
            {
                Main.npcChatText = Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.LunarCult.MechanicShed.{CurrentDialogue}").Value;
            }
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
