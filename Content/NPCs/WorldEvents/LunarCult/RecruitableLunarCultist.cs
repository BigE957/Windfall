using CalamityMod.Projectiles.Ranged;
using CalamityMod.Projectiles.Rogue;
using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.NPCs.Bosses.Orator;

namespace Windfall.Content.NPCs.WorldEvents.LunarCult
{
    public class RecruitableLunarCultist : ModNPC
    {
        public override string Texture => "Windfall/Assets/NPCs/WorldEvents/Recruits/Recruits_Cultist";
        internal static SoundStyle SpawnSound => new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");
        public enum RecruitNames
        {
            Tirith,
            Vivian,
            Tania,
            Doro,
            Skylar,
            Jamie,
        }
        public RecruitNames MyName;
        private enum DialogueState
        {
            #region General Use
            CurrentEventsInitial,
            CurrentEventsGoodEnd,
            CurrentEventsBadEnd,
            ParadiseInitial,
            ParadiseGoodEnd,
            ParadiseBadEnd,


            RecruitSuccess,
            Recruited,
            Recruitable,
            Unrecruited,
            #endregion

            #region Tirith
            CurrentEventsScary,
            CurrentEventsReassurance,
            CurrentEventsWhyBother,
            CurrentEventsExciting,
            CurrentEventsTrue,
            CurrentEventsHappy,

            ParadiseBefore,
            ParadiseSorryToHear,
            ParadiseThemis,
            ParadiseDisscusion,
            ParadiseWhy,
            ParadiseInteresting,
            #endregion

            #region Vivian
            CurrentEventsSorry,
            CurrentEventsAzafure,
            CurrentEventsNow,
            CurrentEventsWhatHappened,
            CurrentEventsKnowledge,
            CurrentEventsExample,
            CurrentEventsFuture,
            ParadiseTrolled,
            ParadiseSorry,
            ParadiseIKR,
            ParadiseTraining,
            ParadiseNatural,
            ParadiseCalamitas,
            ParadiseSoundsLikeIt,
            ParadiseSurelyNot,
            ParadiseWorthIt,
            ParadiseItsPossible,
            ParadiseDoubtIt,
            #endregion

            #region Tania
            ParadiseConfusing,
            ParadiseHowKind,
            ParadiseSoundsWrong,
            ParadiseManipulative,
            ParadiseChangeHow,
            ParadiseThatsGood,
            ParadiseIsThatGood,
            #endregion

            #region Doro
            CurrentEventsNotSure,
            CurrentEventsNoClue,
            CurrentEventsFaith,
            CurrentEventsTellUs,
            CurrentEventsThatsOkay,
            CurrentEventsNotNeeded,
            #endregion

            #region Skylar
            ParadiseClimax,
            ParadiseSessions,
            ParadiseJoined,
            ParadiseStory,
            ParadiseRules,
            ParadiseMeta,
            #endregion

            #region Jamie
            CurrentEventsMetHim,
            CurrentEventsVivian,
            CurrentEventsSpeeches,
            CurrentEventsWho,
            CurrentEventsThing,
            CurrentEventsComeFrom,
            #endregion
        }
        private DialogueState CurrentDialogue;
        public bool Chattable = false;
        private List<dialogueDirections> MyDialogue;
        public bool Recruitable = false;
        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            NPCID.Sets.ActsLikeTownNPC[Type] = true;
            NPCID.Sets.AllowDoorInteraction[Type] = true;
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
            //NPCID.Sets.FaceEmote[Type] = ModContent.EmoteBubbleType<RecruitableLunarCultist>();

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
            NPC.height = 46;
            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 400;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.immortal = true;

            TownNPCStayingHomeless = true;
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
            if (Chattable == false)
            {
                AnimationType = NPCID.BartenderUnconscious;
                NPC.frame.Y = 0;

                if (NPC.FindFirstNPC(ModContent.NPCType<LunarBishop>()) != -1)
                {
                    NPC Bishop = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<LunarBishop>())];
                    if (Bishop != null)
                        if (NPC.position.X < Bishop.position.X)
                            NPC.direction = 1;
                        else
                            NPC.direction = -1;
                }
            }
            switch(LunarCultBaseSystem.CurrentMeetingTopic)
            {
                case (LunarCultBaseSystem.MeetingTopic.CurrentEvents):
                    CurrentDialogue = DialogueState.CurrentEventsInitial;
                    break;
                case (LunarCultBaseSystem.MeetingTopic.Paradise):
                    CurrentDialogue = DialogueState.ParadiseInitial;
                    break;
            }
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
        }
        public override bool PreAI()
        {            
            if (Chattable)
            {
                AnimationType = NPCID.Stylist;
                NPC.aiStyle = NPCAIStyleID.Passive;
                AIType = NPCID.SkeletonMerchant;
            }
            else
            {               
                if (NPC.FindFirstNPC(ModContent.NPCType<LunarBishop>()) != -1)
                {
                    NPC Bishop = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<LunarBishop>())];
                    if (Bishop != null)
                        if (NPC.position.X < Bishop.position.X)
                            NPC.direction = 1;
                        else
                            NPC.direction = -1;
                }
            }
            return true;
        }
        public override bool CanChat()
        {
            if (!Chattable || NPC.FindFirstNPC(ModContent.NPCType<TheOrator>()) != -1)
                return false;
            else
                return true;
        }
        public override string GetChat()
        {
            if (CurrentDialogue != DialogueState.Unrecruited && CurrentDialogue != DialogueState.RecruitSuccess && CurrentDialogue != DialogueState.Recruitable && CurrentDialogue != DialogueState.Recruited)
                return GetWindfallTextValue($"Dialogue.LunarCult.Recruits.{MyName}.{LunarCultBaseSystem.CurrentMeetingTopic}.{CurrentDialogue.ToString().Remove(0, LunarCultBaseSystem.CurrentMeetingTopic.ToString().Length)}");
            else
                return GetWindfallTextValue($"Dialogue.LunarCult.Recruits.{MyName}.{CurrentDialogue}");
        }

        #region Dialogue Paths
        private readonly List<dialogueDirections> TirithDialogue =
        [
            #region Reused Dialogue
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Recruitable,
                Button1 = new(){name = "Happy to help!", heading = (int)DialogueState.Recruitable, end = true},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.RecruitSuccess,
                Button1 = new(){name = "Sounds good!", heading = (int)DialogueState.Recruited, end = true},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Recruited,
                Button1 = new(){name = "Right-", heading = (int)DialogueState.Recruited, end = true},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Unrecruited,
                Button1 = new(){name = "Your welcome.", heading = (int)DialogueState.Unrecruited, end = true},
            },            
            #endregion

            #region Current Events
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsInitial,
                Button1 = new(){name = "Kinda scary...", heading = (int)DialogueState.CurrentEventsScary},
                Button2 = new(){name = "Quite exciting!", heading = (int)DialogueState.CurrentEventsExciting},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsScary,
                Button1 = new(){name = "It'll be okay.", heading = (int)DialogueState.CurrentEventsReassurance},
                Button2 = new(){name = "Why bother then?", heading = (int)DialogueState.CurrentEventsWhyBother},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsExciting,
                Button1 = new(){name = "That's true.", heading = (int)DialogueState.CurrentEventsTrue},
                Button2 = new(){name = "You should be happy!", heading = (int)DialogueState.CurrentEventsHappy},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsReassurance,
                Button1 = new(){name = "Of course!", heading = (int)DialogueState.CurrentEventsBadEnd},
                Button2 = new(){name = "I dont know...", heading = (int)DialogueState.CurrentEventsGoodEnd},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsWhyBother,
                Button1 = new(){name = "Yeah!", heading = (int)DialogueState.CurrentEventsGoodEnd},
                Button2 = new(){name = "That's up to you.", heading = (int)DialogueState.CurrentEventsGoodEnd},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsTrue,
                Button1 = new(){name = "That's understandable.", heading = (int)DialogueState.CurrentEventsGoodEnd},
                Button2 = new(){name = "Be strong.", heading = (int)DialogueState.CurrentEventsBadEnd},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsHappy,
                Button1 = new(){name = "That's the spirit!", heading = (int)DialogueState.CurrentEventsBadEnd},
                Button2 = new(){name = "Hang on-", heading = (int)DialogueState.CurrentEventsBadEnd},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsGoodEnd,
                Button1 = new(){name = "Hell yeah!", heading = (int)DialogueState.Recruitable, end = true},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsBadEnd,
                Button1 = new(){name = "Your welcome.", heading = (int)DialogueState.Unrecruited, end = true},
            },
            #endregion

            #region Paradise
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseInitial,
                Button1 = new(){name = "Before?", heading = (int)DialogueState.ParadiseBefore},
                Button2 = new(){name = "Our discussion...", heading = (int)DialogueState.ParadiseDisscusion},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseBefore,
                Button1 = new(){name = "Sorry to hear...", heading = (int)DialogueState.ParadiseSorryToHear},
                Button2 = new(){name = "Who's Themis?", heading = (int)DialogueState.ParadiseThemis},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseSorryToHear,
                Button1 = new(){name = "Glad to hear!", heading = (int)DialogueState.CurrentEventsBadEnd},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseThemis,
                Button1 = new(){name = "Quite interesting!", heading = (int)DialogueState.ParadiseBadEnd},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseDisscusion,
                Button1 = new(){name = "Why?", heading = (int)DialogueState.ParadiseWhy},
                Button2 = new(){name = "How intriguing...", heading = (int)DialogueState.ParadiseInteresting},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseWhy,
                Button1 = new(){name = "How wonderful!", heading = (int)DialogueState.ParadiseBadEnd},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseInteresting,
                Button1 = new(){name = "Will do!", heading = (int)DialogueState.ParadiseBadEnd},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseBadEnd,
                Button1 = new(){name = "Later!", heading = (int)DialogueState.Unrecruited, end = true},
            },
            #endregion
        ];
        private readonly List<dialogueDirections> VivianDialogue =
        [
            #region Reused Dialogue
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Recruitable,
                Button1 = new(){name = "Same.", heading = (int)DialogueState.Recruitable, end = true},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.RecruitSuccess,
                Button1 = new(){name = "Wonderful!", heading = (int)DialogueState.Recruited, end = true},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Recruited,
                Button1 = new(){name = "Of course.", heading = (int)DialogueState.Recruited, end = true},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Unrecruited,
                Button1 = new(){name = "See ya!", heading = (int)DialogueState.Unrecruited, end = true},
            },
            #endregion

            #region Current Events
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsInitial,
                Button1 = new(){name = "Oh, sorry.", heading = (int)DialogueState.CurrentEventsSorry},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsSorry,
                Button1 = new(){name = "Are you from Azafure?", heading = (int)DialogueState.CurrentEventsAzafure},
                Button2 = new(){name = "Knowledge?", heading = (int)DialogueState.CurrentEventsKnowledge},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsAzafure,
                Button1 = new(){name = "How about now?", heading = (int)DialogueState.CurrentEventsNow},
                Button2 = new(){name = "What happened to Azafure?", heading = (int)DialogueState.CurrentEventsWhatHappened},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsKnowledge,
                Button1 = new(){name = "Such as...?", heading = (int)DialogueState.CurrentEventsExample},
                Button2 = new(){name = "What's going to happen now?", heading = (int)DialogueState.CurrentEventsFuture},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsNow,
                Button1 = new(){name = "That's nice to hear!", heading = (int)DialogueState.CurrentEventsBadEnd},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsWhatHappened,
                Button1 = new(){name = "Wow...", heading = (int)DialogueState.CurrentEventsBadEnd},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsExample,
                Button1 = new(){name = "Sorry to hear that...", heading = (int)DialogueState.CurrentEventsBadEnd},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsFuture,
                Button1 = new(){name = "I see...", heading = (int)DialogueState.CurrentEventsBadEnd},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsBadEnd,
                Button1 = new(){name = "Later!", heading = (int)DialogueState.Unrecruited, end = true},
            },
            #endregion

            #region Paradise
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseInitial,
                Button1 = new(){name = "I know, right?", heading = (int)DialogueState.ParadiseIKR},
                Button2 = new(){name = "Paradise?", heading = (int)DialogueState.ParadiseTrolled}
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseTrolled,
                Button1 = new(){name = "Sorry!", heading = (int)DialogueState.ParadiseSorry},
                Button2 = new(){name = "Sorry!", heading = (int)DialogueState.ParadiseSorry}
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseSorry,
                Button1 = new(){name = "I know, right?", heading = (int)DialogueState.ParadiseIKR},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseIKR,
                Button1 = new(){name = "Training?", heading = (int)DialogueState.ParadiseTraining},
                Button2 = new(){name = "Is it worth it?", heading = (int)DialogueState.ParadiseWorthIt}
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseTraining,
                Button1 = new(){name = "That's natural.", heading = (int)DialogueState.ParadiseNatural},
                Button2 = new(){name = "Calamitas...?", heading = (int)DialogueState.ParadiseCalamitas}
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseNatural,
                Button1 = new(){name = "Likewise...", heading = (int)DialogueState.ParadiseBadEnd},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseCalamitas,
                Button1 = new(){name = "Sounds like it.", heading = (int)DialogueState.ParadiseSoundsLikeIt},
                Button2 = new(){name = "Surely not.", heading = (int)DialogueState.ParadiseSurelyNot}
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseSoundsLikeIt,
                Button1 = new(){name = "Hell yeah!", heading = (int)DialogueState.ParadiseGoodEnd},
                Button2 = new(){name = "Eek!", heading = (int)DialogueState.ParadiseGoodEnd}
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseSurelyNot,
                Button1 = new(){name = "Probably...", heading = (int)DialogueState.ParadiseBadEnd},
                Button2 = new(){name = "Wait-", heading = (int)DialogueState.ParadiseBadEnd}
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseWorthIt,
                Button1 = new(){name = "Training?", heading = (int)DialogueState.ParadiseTraining},
                Button2 = new(){name = "Is it worth it?", heading = (int)DialogueState.ParadiseWorthIt}
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseItsPossible,
                Button1 = new(){name = "True!", heading = (int)DialogueState.ParadiseGoodEnd},
                Button2 = new(){name = "For sure!", heading = (int)DialogueState.ParadiseGoodEnd}
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseDoubtIt,
                Button1 = new(){name = "You got this.", heading = (int)DialogueState.ParadiseBadEnd},
                Button2 = new(){name = "Good luck.", heading = (int)DialogueState.ParadiseBadEnd}
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseGoodEnd,
                Button1 = new(){name = "It's okay.", heading = (int)DialogueState.Recruitable, end = true},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseBadEnd,
                Button1 = new(){name = "Bye.", heading = (int)DialogueState.Unrecruited, end = true},
            },
            #endregion
        ];
        private readonly List<dialogueDirections> TaniaDialogue =
        [
            #region Reused Dialogue
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Recruitable,
                Button1 = new(){name = "...", heading = (int)DialogueState.Recruitable, end = true},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.RecruitSuccess,
                Button1 = new(){name = "...", heading = (int)DialogueState.Recruited, end = true},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Recruited,
                Button1 = new(){name = "...", heading = (int)DialogueState.Recruited, end = true},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Unrecruited,
                Button1 = new(){name = "...", heading = (int)DialogueState.Unrecruited, end = true},
            }, 
            #endregion

            #region Paradise
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseInitial,
                Button1 = new(){name = "Kinda confusing...", heading = (int)DialogueState.ParadiseConfusing},
                Button2 = new(){name = "Change how?", heading = (int)DialogueState.ParadiseChangeHow},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseConfusing,
                Button1 = new(){name = "How Kind!", heading = (int)DialogueState.ParadiseHowKind},
                Button2 = new(){name = "That sounds... wrong.", heading = (int)DialogueState.ParadiseSoundsWrong},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseHowKind,
                Button1 = new(){name = "Glad they helped.", heading = (int)DialogueState.ParadiseBadEnd},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseSoundsWrong,
                Button1 = new(){name = "Sounds manipulative.", heading = (int)DialogueState.ParadiseManipulative},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseManipulative,
                Button1 = new(){name = "Yeah...", heading = (int)DialogueState.ParadiseGoodEnd},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseChangeHow,
                Button1 = new(){name = "That's good!", heading = (int)DialogueState.ParadiseIsThatGood},
                Button2 = new(){name = "...is that good?", heading = (int)DialogueState.ParadiseIsThatGood},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseThatsGood,
                Button1 = new(){name = "Hopefully!", heading = (int)DialogueState.ParadiseBadEnd},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseIsThatGood,
                Button1 = new(){name = "Agreed.", heading = (int)DialogueState.ParadiseGoodEnd},
                Button2 = new(){name = "Maybe.", heading = (int)DialogueState.ParadiseGoodEnd},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseGoodEnd,
                Button1 = new(){name = "Will do!", heading = (int)DialogueState.Recruitable, end = true},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseBadEnd,
                Button1 = new(){name = "Later!", heading = (int)DialogueState.Unrecruited, end = true},
            },
            #endregion
        ];
        private readonly List<dialogueDirections> DoroDialogue =
        [
            #region Reused Dialogue
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Recruitable,
                Button1 = new(){name = "Glad to hear!", heading = (int)DialogueState.Recruitable, end = true},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.RecruitSuccess,
                Button1 = new(){name = "Thank you.", heading = (int)DialogueState.Recruited, end = true},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Recruited,
                Button1 = new(){name = "Fair...", heading = (int)DialogueState.Recruited, end = true},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Unrecruited,
                Button1 = new(){name = "For sure!", heading = (int)DialogueState.Unrecruited, end = true},
            },            
            #endregion
            
            #region Current Events
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsInitial,
                Button1 = new(){name = "I'm not sure...", heading = (int)DialogueState.CurrentEventsNotSure},
                Button2 = new(){name = "They'll tell us.", heading = (int)DialogueState.CurrentEventsTellUs},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsNotSure,
                Button1 = new(){name = "No clue...", heading = (int)DialogueState.CurrentEventsNoClue},
                Button2 = new(){name = "Faith?", heading = (int)DialogueState.CurrentEventsFaith},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsNoClue,
                Button1 = new(){name = "That's true.", heading = (int)DialogueState.CurrentEventsGoodEnd},
                Button2 = new(){name = "You should be happy!", heading = (int)DialogueState.CurrentEventsGoodEnd},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsFaith,
                Button1 = new(){name = "Uh...", heading = (int)DialogueState.CurrentEventsBadEnd},
                Button2 = new(){name = "Yeah.", heading = (int)DialogueState.CurrentEventsBadEnd},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsTellUs,
                Button1 = new(){name = "That's okay.", heading = (int)DialogueState.CurrentEventsThatsOkay},
                Button2 = new(){name = "Maybe they don't need you?", heading = (int)DialogueState.CurrentEventsNotNeeded},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsThatsOkay,
                Button1 = new(){name = "That's understandable.", heading = (int)DialogueState.CurrentEventsGoodEnd},
                Button2 = new(){name = "Be strong.", heading = (int)DialogueState.CurrentEventsBadEnd},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsNotNeeded,
                Button1 = new(){name = "It'll do you good.", heading = (int)DialogueState.CurrentEventsGoodEnd},
                Button2 = new(){name = "Up to you.", heading = (int)DialogueState.CurrentEventsGoodEnd},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsGoodEnd,
                Button1 = new(){name = "Hell yeah!", heading = (int)DialogueState.Recruitable, end = true},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsBadEnd,
                Button1 = new(){name = "Same!", heading = (int)DialogueState.Unrecruited, end = true},
            },
            #endregion
        ];
        private readonly List<dialogueDirections> SkylarDialogue =
        [
            #region Reused Dialogue
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Recruitable,
                Button1 = new(){name = "Glad to hear!", heading = (int)DialogueState.Recruitable, end = true},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.RecruitSuccess,
                Button1 = new(){name = "Thank you.", heading = (int)DialogueState.Recruited, end = true},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Recruited,
                Button1 = new(){name = "Fair...", heading = (int)DialogueState.Recruited, end = true},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Unrecruited,
                Button1 = new(){name = "For sure!", heading = (int)DialogueState.Unrecruited, end = true},
            }, 
            #endregion

            #region Paradise
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseInitial,
                Button1 = new(){name = "Climax?", heading = (int)DialogueState.ParadiseClimax},
                Button2 = new(){name = "Story?", heading = (int)DialogueState.ParadiseStory},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseClimax,
                Button1 = new(){name = "Sessions?", heading = (int)DialogueState.ParadiseSessions},
                Button2 = new(){name = "How'd you join?", heading = (int)DialogueState.ParadiseJoined},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseSessions,
                Button1 = new(){name = "Sounds like it!", heading = (int)DialogueState.ParadiseBadEnd},
                Button2 = new(){name = "Informative indeed!", heading = (int)DialogueState.ParadiseBadEnd},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseJoined,
                Button1 = new(){name = "Same!", heading = (int)DialogueState.ParadiseBadEnd},
                Button2 = new(){name = "Sounds cool!", heading = (int)DialogueState.ParadiseBadEnd},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseStory,
                Button1 = new(){name = "What rules?", heading = (int)DialogueState.ParadiseRules},
                Button2 = new(){name = "...Meta?", heading = (int)DialogueState.ParadiseMeta},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseRules,
                Button1 = new(){name = "Oh, gotcha.", heading = (int)DialogueState.ParadiseBadEnd},
                Button2 = new(){name = "Hmm...", heading = (int)DialogueState.ParadiseBadEnd},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseMeta,
                Button1 = new(){name = "Magic... Yeah...", heading = (int)DialogueState.ParadiseBadEnd},
                Button2 = new(){name = "How... arcane.", heading = (int)DialogueState.ParadiseBadEnd},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ParadiseBadEnd,
                Button1 = new(){name = "Later!", heading = (int)DialogueState.Unrecruited, end = true},
            },
            #endregion
        ];
        private readonly List<dialogueDirections> JamieDialogue =
        [
            #region Reused Dialogue
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Unrecruited,
                Button1 = new(){name = "See ya!", heading = (int)DialogueState.Unrecruited, end = true},
            },
            #endregion

            #region Current Events
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsInitial,
                Button1 = new(){name = "Yeah, I met him.", heading = (int)DialogueState.CurrentEventsMetHim},
                Button2 = new(){name = "Who?", heading = (int)DialogueState.CurrentEventsWho},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsMetHim,
                Button1 = new(){name = "Vivian?", heading = (int)DialogueState.CurrentEventsVivian},
                Button2 = new(){name = "He gives speeches?", heading = (int)DialogueState.CurrentEventsSpeeches},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsWho,
                Button1 = new(){name = "Thing?", heading = (int)DialogueState.CurrentEventsThing},
                Button2 = new(){name = "Where does he come from?", heading = (int)DialogueState.CurrentEventsComeFrom},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsVivian,
                Button1 = new(){name = "Interesting.", heading = (int)DialogueState.CurrentEventsBadEnd},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsSpeeches,
                Button1 = new(){name = "Yeah.", heading = (int)DialogueState.CurrentEventsBadEnd},
                Button2 = new(){name = "Really?", heading = (int)DialogueState.CurrentEventsBadEnd},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsThing,
                Button1 = new(){name = "I see...", heading = (int)DialogueState.CurrentEventsBadEnd},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsComeFrom,
                Button1 = new(){name = "Gotcha...", heading = (int)DialogueState.CurrentEventsBadEnd},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.CurrentEventsBadEnd,
                Button1 = new(){name = "Thanks!", heading = (int)DialogueState.Unrecruited, end = true},
            },
            #endregion
        ];
        #endregion
        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            if ((CurrentDialogue.ToString().Contains("GoodEnd") || CurrentDialogue == DialogueState.Recruitable) && !firstButton && Recruitable)
            {
                if (!LunarCultBaseSystem.Recruits.Contains((int)MyName))
                    LunarCultBaseSystem.Recruits.Add((int)MyName);
                CurrentDialogue = DialogueState.RecruitSuccess;
                LunarCultBaseSystem.RecruitmentsSkipped = 0;
                foreach(NPC npc in Main.npc.Where(n => n.active && n.type == ModContent.NPCType<RecruitableLunarCultist>()))
                {
                    if (npc.ModNPC is RecruitableLunarCultist recruit)
                    {
                        recruit.Recruitable = false;
                        recruit.CurrentDialogue = DialogueState.Unrecruited;
                    }
                }
            }
            else
            {
                CurrentDialogue = (DialogueState)GetNPCConversation(MyDialogue, (int)CurrentDialogue, firstButton);
            }
            if (CurrentDialogue != DialogueState.Unrecruited && CurrentDialogue != DialogueState.RecruitSuccess && CurrentDialogue != DialogueState.Recruitable && CurrentDialogue != DialogueState.Recruited)
                Main.npcChatText = GetWindfallTextValue($"Dialogue.LunarCult.Recruits.{MyName}.{LunarCultBaseSystem.CurrentMeetingTopic}.{CurrentDialogue.ToString().Remove(0, LunarCultBaseSystem.CurrentMeetingTopic.ToString().Length)}");
            else
                Main.npcChatText = GetWindfallTextValue($"Dialogue.LunarCult.Recruits.{MyName}.{CurrentDialogue}");
        }
        public override void SetChatButtons(ref string button, ref string button2)
        {
            SetConversationButtons(MyDialogue, (int)CurrentDialogue, ref button, ref button2);
            if (CurrentDialogue.ToString().Contains("GoodEnd") || CurrentDialogue == DialogueState.Recruitable)
                button2 = "Recruit";
        }
        public override bool CheckActive() => Chattable;
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
            switch (MyName)
            {
                case (RecruitNames.Tirith):
                    projType = ProjectileID.LostSoulFriendly;
                    break;
                case (RecruitNames.Vivian):
                    projType = ModContent.ProjectileType<BrimstoneBolt>();
                    break;
                case (RecruitNames.Tania):
                    projType = ProjectileID.Typhoon;
                    break;
                case (RecruitNames.Doro):
                    projType = ModContent.ProjectileType<Brick>();
                    break;
                case (RecruitNames.Skylar):
                    projType = ProjectileID.FrostBoltSword;
                    break;
                case (RecruitNames.Jamie):
                    projType = ProjectileID.ThrowingKnife;
                    break;
            }
            attackDelay = 1;
        }
        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Width = 37;
            NPC.frame.Height = frameHeight;
            NPC.frame.X = NPC.frame.Width * (int)MyName;
            if (!Chattable)
                NPC.spriteDirection = NPC.direction;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SpriteEffects direction = SpriteEffects.None;
            if (NPC.spriteDirection == -1)
                direction = SpriteEffects.FlipHorizontally;
            Texture2D texture = ModContent.Request<Texture2D>("Windfall/Assets/NPCs/WorldEvents/Recruits/Recruits_Cultist").Value;
            Vector2 drawPosition = NPC.Center - Main.screenPosition + Vector2.UnitY * NPC.gfxOffY;
            Vector2 origin = NPC.frame.Size() * 0.5f;

            Main.spriteBatch.Draw(texture, drawPosition, NPC.frame, drawColor * NPC.Opacity, NPC.rotation, origin, NPC.scale, direction, 0f);
            return false;
        }
    }
}
