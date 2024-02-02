using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Windfall.Systems
{
    public class WorldSaveSystem : ModSystem
    {
        public static List<bool> JournalsCollected = new(13);
        public static bool CloneRevealed = false;
        public static bool ScoogFished = false;

        public static List<string> CreditDataNames;
        public static List<int> CreditDataCredits;
        public override void ClearWorld()
        {
            CloneRevealed = false;
            ScoogFished = false;

            for (int i = 0; i < JournalsCollected.Count; i++)
            {
                JournalsCollected[i] = false;
            }

            CreditDataNames = new List<string>();
            CreditDataCredits = new List<int>();
        }
        public override void LoadWorldData(TagCompound tag)
        {
            CloneRevealed = tag.GetBool("CloneRevealed");
            ScoogFished = tag.GetBool("ScoogFished");

            JournalsCollected = (List<bool>)tag.GetList<bool>("JournalsCollected");
            
            CreditDataNames = (List<string>)tag.GetList<string>("CreditDataNames");
            CreditDataCredits = (List<int>)tag.GetList<int>("CreditDataCredits");
        }

        public override void SaveWorldData(TagCompound tag)
        {
            tag["CloneRevealed"] = CloneRevealed;
            tag["ScoogFished"] = ScoogFished;

            tag["JournalsCollected"] = JournalsCollected;
            
            tag["CreditDataNames"] = CreditDataNames;
            tag["CreditDataCredits"] = CreditDataCredits;
        }
    }
}
