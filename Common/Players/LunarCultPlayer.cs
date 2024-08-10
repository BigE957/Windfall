using Terraria.ModLoader.IO;

namespace Windfall.Common.Players
{
    public class LunarCultPlayer : ModPlayer
    {
        public bool SeamstressTalked = false;
        public override void LoadData(TagCompound tag)
        {
            SeamstressTalked = tag.GetBool("SeamstressTalked");
        }
        public override void SaveData(TagCompound tag)
        {
            if (SeamstressTalked)
                tag["SeamstressTalked"] = SeamstressTalked;
        }
    }
}
