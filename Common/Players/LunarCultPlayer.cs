using Terraria.ModLoader.IO;

namespace Windfall.Common.Players
{
    public class LunarCultPlayer : ModPlayer
    {
        public bool SeamstressTalked = false;
        public bool awareOfLunarCoins = false;
        public override void LoadData(TagCompound tag)
        {
            SeamstressTalked = tag.GetBool("SeamstressTalked");
            awareOfLunarCoins = tag.GetBool("awareOfLunarCoins");
        }
        public override void SaveData(TagCompound tag)
        {
            if (SeamstressTalked)
                tag["SeamstressTalked"] = SeamstressTalked;
            if (awareOfLunarCoins)
                tag["awareOfLunarCoins"] = awareOfLunarCoins;
        }
    }
}
