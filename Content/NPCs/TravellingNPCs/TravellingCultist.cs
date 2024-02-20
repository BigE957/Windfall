using CalamityMod;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;
using Windfall.Common.Systems;
using static Windfall.Common.Utilities.Utilities;

namespace Windfall.Content.NPCs.TravellingNPCs
{
    public class TravellingCultist : ModNPC, ILocalizedModType
    {
        public override string Texture => "Windfall/Assets/NPCs/TravellingNPCs/TravellingCultist";
        public override string HeadTexture => "Windfall/Assets/NPCs/TravellingNPCs/TravellingCultist_Head";

        public const double despawnTime = 48600.0;
        public static double spawnTime = double.MaxValue;
        public static NPC FindNPC(int npcType) => Main.npc.FirstOrDefault(npc => npc.type == npcType && npc.active);

        private static Profiles.StackedNPCProfile NPCProfile;

        NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new()
        {
            Velocity = 1f, // Draws the NPC in the bestiary as if its walking +1 tiles in the x direction
            Direction = 1 // -1 is left and 1 is right. NPCs are drawn facing the left by default but ExamplePerson will be drawn facing the right
        };

        public override bool PreAI()
        {
            if ((!Main.dayTime || Main.time >= despawnTime) && !IsNpcOnscreen(NPC.Center)) // If it's past the despawn time and the NPC isn't onscreen
            {
                // Here we despawn the NPC and send a message stating that the NPC has despawned
                // LegacyMisc.35 is {0) has departed!
                if (Main.netMode == NetmodeID.SinglePlayer) Main.NewText(Language.GetTextValue("LegacyMisc.35", NPC.FullName), 50, 125, 255);
                else ChatHelper.BroadcastChatMessage(NetworkText.FromKey("LegacyMisc.35", NPC.GetFullNetName()), new Color(50, 125, 255));
                NPC.active = false;
                NPC.netSkip = -1;
                NPC.life = 0;
                return false;
            }

            return true;
        }

        public static void UpdateTravelingMerchant()
        {
            bool travelerIsThere = (NPC.FindFirstNPC(ModContent.NPCType<TravellingCultist>()) != -1); // Find a Merchant if there's one spawned in the world

            // Main.time is set to 0 each morning, and only for one update. Sundialling will never skip past time 0 so this is the place for 'on new day' code
            if (Main.dayTime && Main.time == 0)
            {
                // insert code here to change the spawn chance based on other conditions (say, NPCs which have arrived, or milestones the player has passed)
                // You can also add a day counter here to prevent the merchant from possibly spawning multiple days in a row.

                // NPC won't spawn today if it stayed all night
                if (!travelerIsThere && Main.rand.NextBool(4))
                { // 4 = 25% Chance
                  // Here we can make it so the NPC doesn't spawn at the EXACT same time every time it does spawn
                    spawnTime = GetRandomSpawnTime(5400, 8100); // minTime = 6:00am, maxTime = 7:30am
                }
                else
                {
                    spawnTime = double.MaxValue; // no spawn today
                }
            }

            // Spawn the traveler if the spawn conditions are met (time of day, no events, no sundial)
            if (!travelerIsThere && CanSpawnNow())
            {
                int newTraveler = NPC.NewNPC(Terraria.Entity.GetSource_TownSpawn(), Main.spawnTileX * 16, Main.spawnTileY * 16, ModContent.NPCType<TravellingCultist>(), 1); // Spawning at the world spawn
                NPC traveler = Main.npc[newTraveler];
                traveler.homeless = true;
                traveler.direction = Main.spawnTileX >= WorldGen.bestX ? -1 : 1;
                traveler.netUpdate = true;

                // Prevents the traveler from spawning again the same day
                spawnTime = double.MaxValue;

                // Announce that the traveler has spawned in!
                string key = ("A " + traveler.FullName + " has arrived!");
                Color messageColor = new(50, 125, 255);
                CalamityUtils.DisplayLocalizedText(key, messageColor);
            }
        }

        private static bool CanSpawnNow()
        {
            // can't spawn if any events are running
            if (Main.eclipse || Main.invasionType > 0 && Main.invasionDelay == 0 && Main.invasionSize > 0)
                return false;

            // can't spawn if the sundial is active
            if (Main.IsFastForwardingTime())
                return false;

            if (!Main.hardMode || NPC.downedAncientCultist)
                return false;
            // can spawn if daytime, and between the spawn and despawn times
            return Main.dayTime && Main.time >= spawnTime && Main.time < despawnTime;
        }
        public override void OnSpawn(IEntitySource source)
        {
            QuestComplete = false;
            QuestArtifact = new(0, 0);
        }
        private static bool IsNpcOnscreen(Vector2 center)
        {
            int w = NPC.sWidth + NPC.safeRangeX * 2;
            int h = NPC.sHeight + NPC.safeRangeY * 2;
            Rectangle npcScreenRect = new Rectangle((int)center.X - w / 2, (int)center.Y - h / 2, w, h);
            foreach (Player player in Main.player)
            {
                // If any player is close enough to the traveling merchant, it will prevent the npc from despawning
                if (player.active && player.getRect().Intersects(npcScreenRect))
                {
                    return true;
                }
            }
            return false;
        }

        public static double GetRandomSpawnTime(double minTime, double maxTime)
        {
            // A simple formula to get a random time between two chosen times
            return (maxTime - minTime) * Main.rand.NextDouble() + minTime;
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 60;
            NPCID.Sets.AttackType[Type] = 2; // Swings a weapon. This NPC attacks in roughly the same manner as Stylist
            NPCID.Sets.AttackTime[Type] = 12;
            NPCID.Sets.AttackAverageChance[Type] = 1;
            NPCID.Sets.HatOffsetY[Type] = 4;
            NPCID.Sets.ShimmerTownTransform[Type] = false;
            NPCID.Sets.NoTownNPCHappiness[Type] = true; // Prevents the happiness button
            //NPCID.Sets.FaceEmote[Type] = ModContent.EmoteBubbleType<TravellingCultist>();

            // Influences how the NPC looks in the Bestiary
            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                Velocity = 2f, // Draws the NPC in the bestiary as if its walking +2 tiles in the x direction
                Direction = -1 // -1 is left and 1 is right.
            };

            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

            NPCProfile = new Profiles.StackedNPCProfile(
                new Profiles.DefaultNPCProfile(Texture, NPCHeadLoader.GetHeadSlot(HeadTexture), Texture + "_Party")
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
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.5f;
            AnimationType = NPCID.Stylist;
            TownNPCStayingHomeless = true;
            NPC.immortal = true;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface
            });
        }
        public override bool CanTownNPCSpawn(int numTownNPCs)
        {
            return false; // This should always be false, because we spawn in the Traveling Merchant manually
        }

        public override ITownNPCProfile TownNPCProfile()
        {
            return NPCProfile;
        }
        public override List<string> SetNPCNameList()
        {
            return new List<string>() {
                "Strange Cultist",
                "Poorly Disguised Cultist",
                "Suspicious Looking Cultist",
            };
        }
        public override string GetChat()
        {
            WeightedRandom<string> chat = new WeightedRandom<string>();
            chat.Add(Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.LunarCult.TravellingCultist.Standard1").Value);
            if(CurrentDialogueState == DialogueStates.Quests1)
                return chat;
            else
                return Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.LunarCult.TravellingCultist.{CurrentDialogueState}").Value;
        }
   
        public static QuestItem QuestArtifact = new(0,0);

        public static bool QuestComplete = false;

        internal enum DialogueStates
        {
            Initial,
            Weird,
            ThatsYou,
            TheCult,
            HowYouKnow,
            HowToHelp,
            ImIn,
            Quests1,
            PostPlantInitial,
        }

        private DialogueStates CurrentDialogueState
        {
            get => (DialogueStates)WorldSaveSystem.cultistChatState;
            set => WorldSaveSystem.cultistChatState = (int)value;
        }

        public override void SetChatButtons(ref string button, ref string button2)
        {
            if(CurrentDialogueState == DialogueStates.Quests1)
                button = Language.GetTextValue("LegacyInterface.64");
            else
            {
                switch (CurrentDialogueState)
                {
                    case (DialogueStates.Initial):
                        button = "That's me!";
                        button2 = "You look... odd...";
                        break;
                    case (DialogueStates.Weird):
                        button = "That's me!";
                        break;
                    case (DialogueStates.ThatsYou):
                        button = "What's the problem?";
                        break;
                    case (DialogueStates.TheCult):
                        button = "How can I help?";
                        button2 = "How do you know?";
                        break;
                    case (DialogueStates.HowYouKnow):
                        button = "How can I help?";
                        break;
                    case (DialogueStates.HowToHelp):
                        button = "I'll help!";
                        break;
                    case (DialogueStates.ImIn):
                        button = "Will do!";
                        break;
                }
                Main.npcChatText = Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.LunarCult.TravellingCultist.{CurrentDialogueState}").Value;
            }
        }
        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            if (firstButton && CurrentDialogueState == DialogueStates.Quests1)
            {
                QuestArtifact = CollectorQuestDialogueHelper(Main.npc[NPC.whoAmI], ref QuestComplete, QuestArtifact); //will eventually utilize the reach parameter once Post-Plantera quest items are added
                return;
            }
            switch (CurrentDialogueState)
            {
                case (DialogueStates.Initial):
                    if(firstButton)
                        CurrentDialogueState = DialogueStates.ThatsYou;
                    else
                        CurrentDialogueState = DialogueStates.Weird;
                    break;
                case (DialogueStates.Weird):
                    CurrentDialogueState = DialogueStates.ThatsYou;
                    break;
                case (DialogueStates.ThatsYou):
                    CurrentDialogueState = DialogueStates.TheCult;
                    break;
                case (DialogueStates.TheCult):
                    if (firstButton)
                        CurrentDialogueState = DialogueStates.HowToHelp;
                    else
                        CurrentDialogueState = DialogueStates.HowYouKnow;
                    break;
                case (DialogueStates.HowYouKnow):
                    CurrentDialogueState = DialogueStates.HowToHelp;
                    break;
                case (DialogueStates.HowToHelp):
                    CurrentDialogueState = DialogueStates.ImIn;
                    break;
                default:
                    CurrentDialogueState = DialogueStates.Quests1;
                    Main.CloseNPCChatOrSign();
                    break;
            }
        }

        public override void AI()
        {
            NPC.homeless = true; // Make sure it stays homeless
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
