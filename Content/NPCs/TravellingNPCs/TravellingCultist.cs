using CalamityMod.Projectiles.Magic;
using DialogueHelper.Content.UI.Dialogue;
using Terraria.GameContent.Bestiary;
using Terraria.Utilities;
using Windfall.Common.Systems;
using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.NPCs.WorldEvents.LunarCult;

namespace Windfall.Content.NPCs.TravellingNPCs
{
    public class TravellingCultist : ModNPC, ILocalizedModType
    {
        public override string Texture => "Windfall/Assets/NPCs/TravellingNPCs/TravellingCultist";
        public override string HeadTexture => "Windfall/Assets/NPCs/TravellingNPCs/TravellingCultist_Head";

        public const double despawnTime = 48600.0;
        public static double spawnTime = double.MaxValue;
        private static Profiles.StackedNPCProfile NPCProfile;

        NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new()
        {
            Velocity = 1f,
            Direction = 1
        };

        public override bool PreAI()
        {
            if (NPC.ai[0] == 0 && (!Main.dayTime || Main.time >= despawnTime) && !IsNpcOnscreen(NPC.Center)) // If it's past the despawn time and the NPC isn't onscreen
            {
                // Here we despawn the NPC and send a message stating that the NPC has despawned
                if(NPC.active)
                    DisplayLocalizedText("The " + DisplayName + " has departed!", new(50, 125, 255));
                NPC.netSkip = -1;
                NPC.active = false;
                return false;
            }
            return true;
        }
        public static void UpdateTravelingMerchant()
        {
            bool travelerIsThere = (NPC.FindFirstNPC(ModContent.NPCType<TravellingCultist>()) != -1);

            if (Main.dayTime && Main.time == 10)
            {
                if (!travelerIsThere && (Main.rand.NextBool(4) || WorldSaveSystem.PlanteraJustDowned))
                {
                    spawnTime = GetRandomSpawnTime(5400, 8100);
                    WorldSaveSystem.PlanteraJustDowned = false;
                }
                else
                {
                    spawnTime = double.MaxValue;
                }
            }
            if (!travelerIsThere && CanSpawnNow())
            {
                int newTraveler = NPC.NewNPC(Terraria.Entity.GetSource_TownSpawn(), Main.spawnTileX * 16, Main.spawnTileY * 16, ModContent.NPCType<TravellingCultist>(), 1); // Spawning at the world spawn
                NPC traveler = Main.npc[newTraveler];
                traveler.homeless = true;
                traveler.direction = Main.spawnTileX >= WorldGen.bestX ? -1 : 1;
                traveler.netUpdate = true;

                // Prevents the traveler from spawning again the same day
                spawnTime = double.MaxValue;

                string key = ("A " + traveler.FullName + " has arrived!");
                Color messageColor = new(50, 125, 255);
                DisplayLocalizedText(key, messageColor);
            }
        }
        private static bool CanSpawnNow()
        {
            //progression locks
            if (!Main.hardMode || !NPC.downedBoss3)// || SealingRitualSystem.RitualSequenceSeen)
                return false;
            // can't spawn if any events are running
            if (Main.eclipse || Main.invasionType > 0 && Main.invasionDelay == 0 && Main.invasionSize > 0)
                return false;

            // can't spawn if the sundial is active
            if (Main.IsFastForwardingTime())
                return false;
            // can spawn if daytime, and between the spawn and despawn times
            return Main.dayTime && Main.time >= spawnTime && Main.time < despawnTime;
        }
        public override void OnSpawn(IEntitySource source)
        {
            if (NPC.ai[0] == 1)
            {
                NPC.aiStyle = -1;
                NPC.direction = 1;
                return; 
            }
            QuestComplete = false;
            QuestArtifact = new(0, 0);
            if (MilestoneMet(CurrentDialogue))
                CurrentDialogue++;
        }
        private static bool IsNpcOnscreen(Vector2 center)
        {
            int w = NPC.sWidth + NPC.safeRangeX * 2;
            int h = NPC.sHeight + NPC.safeRangeY * 2;
            Rectangle npcScreenRect = new((int)center.X - w / 2, (int)center.Y - h / 2, w, h);
            foreach (Player player in Main.player)
                if (player.active && player.getRect().Intersects(npcScreenRect))
                    return true;
            return false;
        }
        public static double GetRandomSpawnTime(double minTime, double maxTime) => (maxTime - minTime) * Main.rand.NextDouble() + minTime;
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
            ModContent.GetInstance<DialogueUISystem>().DialogueClose += CloseEffect;

            // Influences how the NPC looks in the Bestiary
            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new()
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
            NPC.townNPC = true;
            NPC.friendly = true;
            NPC.width = 18;
            NPC.height = 40;
            NPC.aiStyle = 7;
            NPC.damage = 10;
            NPC.defense = 15;
            NPC.lifeMax = 250;
            NPC.HitSound = SoundID.NPCHit55;
            NPC.DeathSound = SoundID.NPCDeath59;
            NPC.knockBackResist = 0.5f;
            AnimationType = NPCID.Stylist;
            TownNPCStayingHomeless = true;
            NPC.immortal = true;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange([
            BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,

            new FlavorTextBestiaryInfoElement(GetWindfallTextValue($"Bestiary.{nameof(TravellingCultist)}")),
        ]);
        }
        public override bool CanTownNPCSpawn(int numTownNPCs) => false;
        public override ITownNPCProfile TownNPCProfile() => NPCProfile;
        public override List<string> SetNPCNameList()
        {
            return [
                "Strange Cultist",
                "Poorly Disguised Cultist",
                "Suspicious Looking Cultist",
            ];
        }
        public static QuestItem QuestArtifact = new(0, 0);
        public static bool QuestComplete = false;
        public static int RitualQuestProgress = 0;
        private enum DialogueState
        {
            SearchForHelp,
            Quests1,
            LunarCultTalk,
            Quests2,
            AllRecruited,
            Quests3,
            RitualTalk,
            QuestsEnd,
        }
        private static DialogueState CurrentDialogue
        {
            get => (DialogueState)WorldSaveSystem.cultistChatState;
            set => WorldSaveSystem.cultistChatState = (int)value;
        }
        public override bool CanChat() => NPC.ai[0] != 1 && !ModContent.GetInstance<DialogueUISystem>().isDialogueOpen;
        public override string GetChat()
        {
            if (!CurrentDialogue.ToString().Contains("Quests"))
            {
                Main.CloseNPCChatOrSign();

                ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "TravellingCultist/" + CurrentDialogue.ToString());

                return base.GetChat();
            }
            WeightedRandom<string> chat = new();

            chat.Add(GetWindfallTextValue("Dialogue.LunarCult.TravellingCultist.Chat.Standard1"));
            chat.Add(GetWindfallTextValue("Dialogue.LunarCult.TravellingCultist.Chat.Standard2"));
            chat.Add(GetWindfallTextValue("Dialogue.LunarCult.TravellingCultist.Chat.Standard3"));
            if (NPC.AnyNPCs(NPCID.Mechanic) || NPC.AnyNPCs(NPCID.Clothier))
            {
                chat.Add(GetWindfallTextValue("Dialogue.LunarCult.TravellingCultist.Chat.AnyDungeonNPC"));
                if (NPC.AnyNPCs(NPCID.Mechanic))
                    chat.Add(GetWindfallTextValue("Dialogue.LunarCult.TravellingCultist.Chat.Mechanic"));
                if (NPC.AnyNPCs(NPCID.Clothier))
                    chat.Add(GetWindfallTextValue("Dialogue.LunarCult.TravellingCultist.Chat.Clothier"));
            }
            if (NPC.downedBoss3)
                if(Main.rand.NextBool(10))
                    chat.Add(GetWindfallTextValue($"Dialogue.LunarCult.TravellingCultist.Chat.SkeletronRare"));
                else
                    chat.Add(GetWindfallTextValue($"Dialogue.LunarCult.TravellingCultist.Chat.Skeletron"));
            for (int i = 0; i < LunarCultBaseSystem.Recruits.Count; i++)
                chat.Add(GetWindfallTextValue($"Dialogue.LunarCult.TravellingCultist.Chat.{(RecruitableLunarCultist.RecruitNames)i}"));
            return chat;
        }

        public override void SetChatButtons(ref string button, ref string button2)
        {
            if (CurrentDialogue.ToString().Contains("Quests"))
                button = Language.GetTextValue("LegacyInterface.64");
            if (CurrentDialogue == DialogueState.Quests2)
                button2 = "Recruits";
        }
        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            if (firstButton && CurrentDialogue.ToString().Contains("Quests"))
            {
                if (CurrentDialogue == DialogueState.Quests3)
                {
                    if (QuestArtifact.Stack == 0)
                        QuestArtifact = QuestSystem.RitualQuestItems[RitualQuestProgress];
                    QuestArtifact = CollectorQuestDialogueHelper(Main.npc[NPC.whoAmI], ref QuestComplete, QuestArtifact, QuestSystem.RitualQuestItems);
                    if (QuestArtifact.Stack == 0)
                        RitualQuestProgress++;
                }
                else
                    QuestArtifact = CollectorQuestDialogueHelper(Main.npc[NPC.whoAmI], ref QuestComplete, QuestArtifact, QuestSystem.DungeonQuestItems);
                return;
            }
            else if (!firstButton && CurrentDialogue == DialogueState.Quests2)
            {
                switch (LunarCultBaseSystem.Recruits.Count)
                {
                    case 0:
                        Main.npcChatText = GetWindfallTextValue("Dialogue.LunarCult.TravellingCultist.Recruits.None");
                        break;
                    case 1:
                        Main.npcChatText = GetWindfallLocalText("Dialogue.LunarCult.TravellingCultist.Recruits.One").Format((RecruitableLunarCultist.RecruitNames)LunarCultBaseSystem.Recruits[0]);
                        break;
                    case 2:
                        Main.npcChatText = GetWindfallLocalText("Dialogue.LunarCult.TravellingCultist.Recruits.Two").Format((RecruitableLunarCultist.RecruitNames)LunarCultBaseSystem.Recruits[0], (RecruitableLunarCultist.RecruitNames)LunarCultBaseSystem.Recruits[1]);
                        break;
                    case 3:
                        Main.npcChatText = GetWindfallLocalText("Dialogue.LunarCult.TravellingCultist.Recruits.Three").Format((RecruitableLunarCultist.RecruitNames)LunarCultBaseSystem.Recruits[0], (RecruitableLunarCultist.RecruitNames)LunarCultBaseSystem.Recruits[1], (RecruitableLunarCultist.RecruitNames)LunarCultBaseSystem.Recruits[2]);
                        break;
                    case 4:
                        Main.npcChatText = GetWindfallTextValue("Dialogue.LunarCult.TravellingCultist.Recruits.Four");
                        break;
                }
            }
        }
        public override void AI()
        {
            NPC.homeless = true;
            if (RitualQuestProgress == 3 && CurrentDialogue >= DialogueState.QuestsEnd)
                RitualQuestProgress++;
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
        private static bool MilestoneMet(DialogueState CurrentDialogue) => (CurrentDialogue == DialogueState.Quests1 && NPC.downedPlantBoss) || (CurrentDialogue == DialogueState.Quests2 && LunarCultBaseSystem.Recruits.Count == 4) || (CurrentDialogue == DialogueState.Quests3 && RitualQuestProgress >= 3) || (CurrentDialogue == DialogueState.QuestsEnd && RitualQuestProgress < 4);
        private void CloseEffect(string treeKey, int dialogueID, int buttonID)
        {
            switch(treeKey)
            {
                case "TravellingCultist/SearchForHelp":
                    CurrentDialogue = DialogueState.Quests1;
                    break;
                case "TravellingCultist/LunarCultTalk":
                    CurrentDialogue = DialogueState.Quests2;
                    break;
                case "TravellingCultist/AllRecruited":
                    CurrentDialogue = DialogueState.Quests3;
                    break;
                case "TravellingCultist/RitualTalk":
                    CurrentDialogue = DialogueState.QuestsEnd;
                    break;
            }
        }
    }
}
