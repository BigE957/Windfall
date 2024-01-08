using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace WindfallAttempt1.Utilities
{
    public class WorldSaveSystem : ModSystem
    {
        public static List<bool> JournalsCollected = new(13);
        public static bool CloneRevealed = false;
        public override void ClearWorld()
        {
            CloneRevealed = false;
            for (int i = 0; i < JournalsCollected.Count; i++)
            {
                JournalsCollected[i] = false;
            }
        }
        public override void LoadWorldData(TagCompound tag)
        {
            JournalsCollected = (List<bool>)tag.GetList<bool>("JournalsCollected");
            CloneRevealed = tag.GetBool("CloneRevealed");
        }

        public override void SaveWorldData(TagCompound tag)
        {
            tag["JournalsCollected"] = JournalsCollected;
            tag["CloneRevealed"] = CloneRevealed;
        }
    }
}
