using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Windfall.Content.NPCs.TravellingNPCs;

namespace Windfall.Common.Systems
{
    public class WorldSaveSystem : ModSystem
    {
        public static List<bool> JournalsCollected = new(13);
        public static bool CloneRevealed = false;
        public static bool ScoogFished = false;
        public static bool MechanicCultistsEncountered = false;

        public static int paladinChats = 0;
        public static int cultistChatState = 0;

        public static List<string> CreditDataNames;
        public static List<int> CreditDataCredits;
        public override void ClearWorld()
        {
            ResetWorldData();
        }
        public override void LoadWorldData(TagCompound tag)
        {
            CloneRevealed = tag.GetBool("CloneRevealed");
            ScoogFished = tag.GetBool("ScoogFished");
            MechanicCultistsEncountered = tag.GetBool("MechanicCultistsEncountered");

            JournalsCollected = (List<bool>)tag.GetList<bool>("JournalsCollected");

            CreditDataNames = (List<string>)tag.GetList<string>("CreditDataNames");
            CreditDataCredits = (List<int>)tag.GetList<int>("CreditDataCredits");

            paladinChats = tag.GetInt("paladinChats");
            cultistChatState = tag.GetInt("TravelCultistChatState");
        }

        public override void SaveWorldData(TagCompound tag)
        {
            tag["CloneRevealed"] = CloneRevealed;
            tag["ScoogFished"] = ScoogFished;
            tag["MechanicCultistsEncountered"] = MechanicCultistsEncountered;

            tag["JournalsCollected"] = JournalsCollected;

            tag["CreditDataNames"] = CreditDataNames;
            tag["CreditDataCredits"] = CreditDataCredits;

            tag["paladinChats"] = paladinChats;
            tag["TravelCultistChatState"] = cultistChatState;
        }
        public static void ResetWorldData()
        {
            CloneRevealed = false;
            ScoogFished = false;
            MechanicCultistsEncountered = false;

            for (int i = 0; i < JournalsCollected.Count; i++)
            {
                JournalsCollected[i] = false;
            }

            CreditDataNames = new List<string>();
            CreditDataCredits = new List<int>();

            paladinChats = 0;
            cultistChatState = 0;
        }
    }
}
