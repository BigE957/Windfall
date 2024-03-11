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
using Windfall.Common.Systems;

namespace Windfall.Content.NPCs.WorldEvents.LunarCult
{
    public class RecruitableLunarCultist : ModNPC
    {
        public override string Texture => "Windfall/Assets/NPCs/TravellingNPCs/TravellingCultist";
        internal static SoundStyle SpawnSound => new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");
        private static Profiles.StackedNPCProfile NPCProfile;
        private enum RecruitNames
        {
            Tirith,
            Vivian,
            Tania,
            Doro,
            Skylar,
            Jamie,
        }
        private RecruitNames MyName
        {
            get => (RecruitNames)NPC.ai[0];
            set => NPC.ai[0] = (int)value;
        }
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
        private DialogueState CurrentDialogue
        {
            get => (DialogueState)NPC.ai[1];
            set => NPC.ai[1] = (int)value;
        }
        private enum AiState
        {
            Listening,
            Chattable,
        }
        private AiState MyAiState
        {
            get => (AiState)NPC.ai[2];
            set => NPC.ai[2] = (int)value;
        }
        private List<dialogueDirections> MyDialogue;
        private bool Recruitable = false;
        public override void SetStaticDefaults()
        {
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
            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                Velocity = 1f,
                Direction = 1
            };

            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

            NPCProfile = new Profiles.StackedNPCProfile(
                new Profiles.DefaultNPCProfile(Texture, NPCHeadLoader.GetHeadSlot(HeadTexture))
            );
        }
        public override void SetDefaults()
        {
            NPC.friendly = true; // NPC Will not attack player
            NPC.width = 18;
            NPC.height = 40;
            NPC.aiStyle = 0;
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
            MyAiState = AiState.Chattable;
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
            if (MyAiState == 0)
            {
                AnimationType = NPCID.BartenderUnconscious;
                NPC.frame.Y = 0;
            }
        }
        public override bool CanChat()
        {
            if (MyAiState == 0)
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
            else if (CurrentDialogue == DialogueState.RecruitFailed || CurrentDialogue == DialogueState.RecruitSuccess)
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
    }
}
