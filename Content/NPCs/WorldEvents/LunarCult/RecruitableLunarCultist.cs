using Microsoft.Xna.Framework;
using CalamityMod;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Windfall.Common.Utilities.Utilities;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.GameContent;
using CalamityMod.Projectiles.Magic;
using Windfall.Common.Systems.WorldEvents;

namespace Windfall.Content.NPCs.WorldEvents.LunarCult
{
    public class RecruitableLunarCultist : ModNPC
    {
        public override string Texture => "Windfall/Assets/NPCs/TravellingNPCs/TravellingCultist";
        internal static SoundStyle SpawnSound => new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");
        private enum RecruitNames
        {
            Tirith,
            Vivian,
            Tania,
            Doro,
            Skylar,
            Jamie,
        }
        private RecruitNames MyName;
        private enum DialogueState
        {
            //General Use
            Initial,
            End,
            RecruitSuccess,
            RecruitFailed,
            Recruited,
            Unrecruited

            //Tirith

            //Vivian

            //Tania

            //Doro

            //Skylar

            //Jamie

        }
        private DialogueState CurrentDialogue;
        public bool chattable = false;
        private List<dialogueDirections> MyDialogue;
        private bool Recruitable = false;
        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            NPCID.Sets.ActsLikeTownNPC[Type] = true;
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 60;
            NPCID.Sets.AttackType[Type] = 2;
            NPCID.Sets.AttackTime[Type] = 12;
            NPCID.Sets.AttackAverageChance[Type] = 1;
            NPCID.Sets.HatOffsetY[Type] = 4;
            NPCID.Sets.ShimmerTownTransform[Type] = false;
            NPCID.Sets.NoTownNPCHappiness[Type] = true;
            //NPCID.Sets.FaceEmote[Type] = ModContent.EmoteBubbleType<TravellingCultist>();

            // Influences how the NPC looks in the Bestiary
            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new()
            {
                Velocity = 1f,
                Direction = 1
            };
        }
        public override void SetDefaults()
        {
            NPC.friendly = true; // NPC Will not attack player
            NPC.width = 18;
            NPC.height = 40;
            NPC.aiStyle = -1;
            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 400;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 1f;
            NPC.immortal = true;

            AnimationType = NPCID.Stylist;
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.GivenName = MyName.ToString();
            switch (MyName)
            {
                case (RecruitNames.Tirith):
                    MyDialogue = TirithDialogue;
                    break;
                case (RecruitNames.Vivian):
                    MyDialogue = VivianDialogue;
                    break;
                case (RecruitNames.Tania):
                    MyDialogue = TaniaDialogue;
                    break;
                case (RecruitNames.Doro):
                    MyDialogue = DoroDialogue;
                    break;
                case (RecruitNames.Skylar):
                    MyDialogue = SkylarDialogue;
                    break;
                case (RecruitNames.Jamie):
                    MyDialogue = JamieDialogue;
                    break;
            }
            if (chattable == false)
            {
                AnimationType = NPCID.BartenderUnconscious;
                NPC.frame.Y = 0;
            }
        }
        public override void AI()
        {
            if(chattable)
            {
                AnimationType = NPCID.Stylist;
                NPC.aiStyle = 7;
            }
        }
        public override bool CanChat()
        {
            if (!chattable)
                return false;
            else
                return true;
        }
        public override string GetChat()
        {
            return Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.LunarCult.Recruits.{MyName}.{CurrentDialogue}").Value;
        }
        #region DialoguePaths
        private readonly List<dialogueDirections> TirithDialogue = new()
        {
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Initial,
                Button1 = new(){name = "Hey!", heading = (int)DialogueState.End},
                Button2 = new(){name = "Hello...?", heading = (int)DialogueState.End},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.End,
                Button1 = new(){name = "Later!", heading = -1, end = true},
            },
        };
        private readonly List<dialogueDirections> VivianDialogue = new()
        {
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Initial,
                Button1 = new(){name = "Hey!", heading = (int)DialogueState.End},
                Button2 = new(){name = "Hello...?", heading = (int)DialogueState.End},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.End,
                Button1 = new(){name = "Later!", heading = -1, end = true},
            },
        };
        private readonly List<dialogueDirections> TaniaDialogue = new()
        {
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Initial,
                Button1 = new(){name = "Hey!", heading = (int)DialogueState.End},
                Button2 = new(){name = "Hello...?", heading = (int)DialogueState.End},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.End,
                Button1 = new(){name = "Later!", heading = -1, end = true},
            },
        };
        private readonly List<dialogueDirections> DoroDialogue = new()
        {
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Initial,
                Button1 = new(){name = "Hey!", heading = (int)DialogueState.End},
                Button2 = new(){name = "Hello...?", heading = (int)DialogueState.End},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.End,
                Button1 = new(){name = "Later!", heading = -1, end = true},
            },
        };
        private readonly List<dialogueDirections> SkylarDialogue = new()
        {
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Initial,
                Button1 = new(){name = "Hey!", heading = (int)DialogueState.End},
                Button2 = new(){name = "Hello...?", heading = (int)DialogueState.End},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.End,
                Button1 = new(){name = "Later!", heading = -1, end = true},
            },
        };
        private readonly List<dialogueDirections> JamieDialogue = new()
        {
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Initial,
                Button1 = new(){name = "Hey!", heading = (int)DialogueState.End},
                Button2 = new(){name = "Hello...?", heading = (int)DialogueState.End},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.End,
                Button1 = new(){name = "Later!", heading = -1, end = true},
            },
        };
        #endregion
        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            if (CurrentDialogue == DialogueState.End && !firstButton)
            {
                if(Recruitable)
                {
                    CurrentDialogue = DialogueState.RecruitSuccess;
                    if (!CultMeetingSystem.Recruits.Contains((int)MyName))
                        CultMeetingSystem.Recruits.Add((int)MyName);
                }
                else
                    CurrentDialogue = DialogueState.RecruitFailed;
                Main.npcChatText = Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.LunarCult.Recruits.{MyName}.{CurrentDialogue}").Value;
            }
            else if (CurrentDialogue == DialogueState.RecruitFailed || CurrentDialogue == DialogueState.RecruitSuccess || CurrentDialogue == DialogueState.Recruited || CurrentDialogue == DialogueState.Unrecruited)
            {
                if (CurrentDialogue == DialogueState.RecruitFailed)
                    CurrentDialogue = DialogueState.Unrecruited;
                else if (CurrentDialogue == DialogueState.RecruitSuccess)
                    CurrentDialogue = DialogueState.Recruited;
                Main.CloseNPCChatOrSign();
            }
            else
            {
                CurrentDialogue = (DialogueState)GetNPCConversation(MyDialogue, (int)CurrentDialogue, firstButton);

                Main.npcChatText = Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.LunarCult.Recruits.{MyName}.{CurrentDialogue}").Value;
            }
        }
        public override void SetChatButtons(ref string button, ref string button2)
        {
            if (CurrentDialogue == DialogueState.RecruitSuccess)
                button = "Awesome!";
            else if (CurrentDialogue == DialogueState.RecruitFailed)
                button = "Aw man...";
            else if(CurrentDialogue == DialogueState.Recruited)
                button = "Cool!";
            else if (CurrentDialogue == DialogueState.Unrecruited)
                button = "Okay...";
            else
            {
                SetConversationButtons(MyDialogue, (int)CurrentDialogue, ref button, ref button2);
                if (CurrentDialogue == DialogueState.End)
                {
                    button2 = "Recruit";
                }
            }
        }
        public override bool CheckActive()
        {
            return false;
        }
        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = 20;
            knockback = 4f;
        }
        public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
        {
            cooldown = 15;
            randExtraCooldown = 8;
        }
        public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
        {
            projType = ModContent.ProjectileType<PhantasmalFuryProj>();
            attackDelay = 1;
        }
    }
}
