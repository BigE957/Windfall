using CalamityMod.Items.Accessories;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Items.Placeables.Furniture.Trophies;
using Terraria.ModLoader.IO;
using Windfall.Content.Items.Fishing;
using Windfall.Content.Items.Quests;
using Windfall.Content.Items.Tools;
using Windfall.Content.Items.Weapons.Misc;
using Windfall.Content.NPCs.TravellingNPCs;

namespace Windfall.Common.Systems
{
    public struct QuestItem
    {
        internal int Type;
        internal int Stack = 1;
        internal QuestItem(int type, int stack)
        {
            Type = type;
            Stack = stack;
        }
    }
    public partial class QuestSystem : ModSystem
    {
        public struct Quest
        {
            public string Name;
            public bool Completed = false;
            public bool Active = false;
            public bool Unlocked = false;
            public List<string> Objectives;
            public List<int> ObjectiveRequirements;
            public List<int> ObjectiveProgress;
            public List<QuestItem> QuestGifts;
            public List<QuestItem> QuestRewards;
            public Quest(string name, List<string> objective, bool completed, bool unlocked, List<int> objReq, List<int> objProg, bool active, List<QuestItem> questGifts, List<QuestItem> questRewards)
            {
                Name = name;
                Completed = completed;
                Active = active;
                Unlocked = unlocked;
                Objectives = objective;
                ObjectiveRequirements = objReq;
                ObjectiveProgress = objProg;
                QuestGifts = questGifts;
                QuestRewards = questRewards;
            }
        }
        public static List<Quest> QuestLog = InitializedQuestLog();

        public static readonly List<QuestItem> DungeonQuestItems = new()
        {
            new QuestItem(ItemID.Bone, 50),
            new QuestItem(ItemID.WaterBolt, 1),
            new QuestItem(ModContent.ItemType<DeificInsignia>(), 5),
        };
        public static readonly List<QuestItem> RitualQuestItems = new()
        {
            new QuestItem(ModContent.ItemType<TabletFrags>(), 1),
            new QuestItem(ModContent.ItemType<DraconicBone>(), 1),
            new QuestItem(ModContent.ItemType<PrimalLightShards>(), 1),
        };

        public override void ClearWorld()
        {
            QuestLog = InitializedQuestLog();
            TravellingCultist.QuestArtifact = new(0, 0);
            TravellingCultist.QuestComplete = false;
            TravellingCultist.RitualQuestProgress = 0;
        }
        public override void LoadWorldData(TagCompound tag)
        {
            QuestLog = (List<Quest>)tag.GetList<Quest>("QuestLog");
            EnsureQuestLogUpToDate(InitializedQuestLog());
            TravellingCultist.QuestArtifact = tag.Get<QuestItem>("CultistQuestItem");
            TravellingCultist.QuestComplete = tag.GetBool("CultsitQuestComplete");
            TravellingCultist.RitualQuestProgress = tag.GetInt("RitualQuestProgress");
        }
        public override void SaveWorldData(TagCompound tag)
        {
            tag["QuestLog"] = QuestLog;
            tag["CultistQuestItem"] = TravellingCultist.QuestArtifact;
            tag["CultsitQuestComplete"] = TravellingCultist.QuestComplete;
            tag["RitualQuestProgress"] = TravellingCultist.RitualQuestProgress;
        }
        internal static Quest CreateQuest(string Name, List<string> Objectives, List<int> ObjectiveRequirements, bool Unlocked, List<QuestItem> QuestGifts = null, List<QuestItem> QuestRewards = null)
        {
            List<int> objectiveProgress = new();
            for (int i = 0; i < ObjectiveRequirements.Count; i++)
            {
                objectiveProgress.Add(0);
            }
            return new Quest { Name = Name, Completed = false, Unlocked = Unlocked, Objectives = Objectives, ObjectiveProgress = objectiveProgress, ObjectiveRequirements = ObjectiveRequirements, Active = false, QuestGifts = QuestGifts, QuestRewards = QuestRewards };
        }
        internal static List<Quest> InitializedQuestLog()
        {
            List<Quest> list = new()
            {
                #region Ilmeran Paladin
                CreateQuest
                (
                    "CnidrionHunt",
                    new List<string>{"Pacify 3 Cnidrions"},
                    new List<int>{3},
                    true,
                    new List<QuestItem>{ new QuestItem (ModContent.ItemType<Cnidrisnack>(), 5 ) },
                    new List<QuestItem>{ new QuestItem (ModContent.ItemType<CnidrionBanner>(), 4), new QuestItem ( ModContent.ItemType<AmidiasSpark>(), 1) }
                ),
                CreateQuest
                (
                    "ScoogHunt",
                    new List<string>{"Defeat Desert Scourge"},
                    new List<int>{1},
                    true,
                    new List<QuestItem>{ new QuestItem (ModContent.ItemType<AncientIlmeranRod>(), 1 ), new QuestItem (ModContent.ItemType<Cnidrisnack>(), 5 ) },
                    new List<QuestItem>{ new QuestItem (ItemID.GoldenCrate, 1 ) }
                ),
                CreateQuest
                (
                    "ShuckinClams",
                    new List<string>{"Shuck 8 Clams"},
                    new List<int>{8},
                    true,
                    new List<QuestItem>{ new QuestItem (ModContent.ItemType<IlmeranKnife>(), 1 )},
                    null
                ),
                CreateQuest
                (
                    "ClamHunt",
                    new List<string>{"Defeat the Giant Clam"},
                    new List<int>{1},
                    true,
                    null,
                    new List<QuestItem>{ new QuestItem (ModContent.ItemType<GiantClamTrophy>(), 1 ) }
                ),
                CreateQuest
                (
                    "ScoogHunt2",
                    new List<string>{"Defeat Aquatic Scourge"},
                    new List<int>{1},
                    Main.hardMode,
                    null,
                    new List<QuestItem>{ new QuestItem (ModContent.ItemType<AquaticScourgeTrophy>(), 1 ) }
                ),
                #endregion

                #region Godseeker Knight               
                CreateQuest
                (
                    "PestControl",
                    new List<string>{"Kill 10 Crimera or Eater of Souls"},
                    new List<int>{10},
                    true,
                    null,
                    new List<QuestItem>{ new QuestItem (ItemID.GoldenCrate, 1 ) }
                ),
                CreateQuest
                (
                    "Decontamination",
                    new List<string>{"Use Purification Powder 10 times."},
                    new List<int>{10},
                    true,
                    null,
                    new List<QuestItem>{ new QuestItem (ItemID.GoldenCrate, 1 ) }
                ),
                #endregion

            };
            return list;
        }
        internal static void EnsureQuestLogUpToDate(List<Quest> initList)
        {
            for (int i = 0; i < initList.Count; i++)
            {
                if (i >= QuestLog.Count)
                    QuestLog.Add(initList[i]);
                else
                {
                    //Ensures changed details about pre-existing quests are updated

                    Quest temp = QuestLog[i];
                    if (temp.Name != initList[i].Name)
                        temp.Name = initList[i].Name;
                    if (temp.Unlocked != initList[i].Unlocked)
                        temp.Unlocked = initList[i].Unlocked;
                    if (temp.QuestGifts != initList[i].QuestGifts)
                        temp.QuestGifts = initList[i].QuestGifts;
                    if (temp.ObjectiveRequirements != initList[i].ObjectiveRequirements)
                        temp.ObjectiveRequirements = initList[i].ObjectiveRequirements;
                    if (temp.QuestRewards != initList[i].QuestRewards)
                        temp.QuestRewards = initList[i].QuestRewards;
                    if (temp.Objectives != initList[i].Objectives)
                        temp.Objectives = initList[i].Objectives;
                    for (int j = 0; j < temp.ObjectiveRequirements.Count; j++)
                        if (temp.ObjectiveProgress.Count <= j)
                            temp.ObjectiveProgress.Add(initList[i].ObjectiveRequirements[j]);
                        else if (temp.ObjectiveProgress[j] >= temp.ObjectiveRequirements[j])
                        {
                            temp.ObjectiveProgress[j] = temp.ObjectiveRequirements[j];
                        }
                    QuestLog[i] = temp;
                }
            }
        }
        /// <param name="questIndex">The index within the <see cref="QuestLog"/> of the Quest to be updated.</param>
        /// <param name="questReqIndex">The index within the ObjectiveProgress list within a Quest to be updated.</param>
        /// <returns>
        /// Increments Quest Progress for the given Quest and optionally an specific objective within said Quest.
        /// </returns>
        public static void IncrementQuestProgress(int questIndex, int questReqIndex = 0)
        {
            if (QuestLog[questIndex].ObjectiveProgress[questReqIndex] < QuestLog[questIndex].ObjectiveRequirements[questReqIndex])
            {
                Quest temp = QuestLog[questIndex];
                temp.ObjectiveProgress[questReqIndex]++;
                if (temp.ObjectiveProgress[questReqIndex] >= temp.ObjectiveRequirements[questReqIndex])
                    temp.Completed = true;
                QuestLog[questIndex] = temp;
            }
            else if (!QuestLog[questIndex].Completed)
            {
                Quest temp = QuestLog[questIndex];
                temp.Completed = true;
                QuestLog[questIndex] = temp;
            }
            Main.NewText("Quest Progress: " + QuestLog[questIndex].ObjectiveProgress[0], Color.Yellow);
            if (QuestLog[questIndex].Completed)
            {
                Main.NewText($"Quest Complete!", Color.Yellow);
            }
        }

        /// <param name="questIndex">The index of a Quest within the <see cref="QuestLog"/>d.</param>
        /// <returns>
        /// Toggles whether the given Quest is Active or Inactive.
        /// </returns>
        public static void ToggleQuestActive(int questIndex)
        {
            Quest temp = QuestLog[questIndex];
            if (temp.Active)
                temp.Active = false;
            else
                temp.Active = true;
            QuestLog[questIndex] = temp;
        }

        /// <param name="questIndex">The index of a Quest within the <see cref="QuestLog"/>d.</param>
        /// <returns>
        /// Resets all data for a given Quest.
        /// </returns>
        public static void ResetQuestProgress(int questIndex)
        {
            Quest temp = QuestLog[questIndex];            
            for (int i = 0; i < temp.ObjectiveProgress.Count; i++)
            {
                temp.ObjectiveProgress[i] = 0;
            }
            temp.Completed = false;
            temp.Active = false;
            QuestLog[questIndex] = temp;
        }

        /// <param name="name">The name of a Quest within the <see cref="QuestLog"/>.</param>
        /// <returns>
        /// Returns whether or not the Quest is Active or Inactive.
        /// </returns>
        public static bool IsQuestActive(string name) => QuestLog[QuestLog.FindIndex(quest => quest.Name == name)].Active;
        public class QuestSerializer : TagSerializer<Quest, TagCompound>
        {
            public override TagCompound Serialize(Quest value) => new()
            {
                ["name"] = value.Name,
                ["completion"] = value.Completed,
                ["active"] = value.Active,
                ["unlocked"] = value.Unlocked,
                ["objectives"] = value.Objectives,
                ["objReqs"] = value.ObjectiveRequirements,
                ["objProg"] = value.ObjectiveProgress,
                ["gifts"] = value.QuestGifts,
                ["rewards"] = value.QuestRewards
            };

            public override Quest Deserialize(TagCompound tag) => new(tag.GetString("name"), (List<string>)tag.GetList<string>("objectives"), tag.GetBool("completion"), tag.GetBool("unlocked"), (List<int>)tag.GetList<int>("objReqs"), (List<int>)tag.GetList<int>("objProg"), tag.GetBool("active"), (List<QuestItem>)tag.GetList<QuestItem>("gifts"), (List<QuestItem>)tag.GetList<QuestItem>("rewards"));
        }
        public class QuestItemSerializer : TagSerializer<QuestItem, TagCompound>
        {
            public override TagCompound Serialize(QuestItem value) => new()
            {
                ["type"] = value.Type,
                ["stack"] = value.Stack,

            };

            public override QuestItem Deserialize(TagCompound tag) => new(tag.GetInt("type"), tag.GetInt("stack"));
        }
    }
}
