using Terraria.ModLoader.IO;

namespace Windfall.Common.Players
{
    public class LunarCultPlayer : ModPlayer
    {
        public bool SeamstressTalked = false;
        public bool awareOfLunarCoins = false;
        public bool hasRecievedChefMeal = false;
        public int apostleQuestTracker = 0;
        public override void LoadData(TagCompound tag)
        {
            SeamstressTalked = tag.GetBool("SeamstressTalked");
            awareOfLunarCoins = tag.GetBool("awareOfLunarCoins");
            hasRecievedChefMeal = tag.GetBool("hasRecievedChefMeal");
            apostleQuestTracker = tag.GetInt("talkQuestTracker");
        }
        public override void SaveData(TagCompound tag)
        {
            if (SeamstressTalked)
                tag["SeamstressTalked"] = SeamstressTalked;
            if (awareOfLunarCoins)
                tag["awareOfLunarCoins"] = awareOfLunarCoins;
            if (hasRecievedChefMeal)
                tag["hasRecievedChefMeal"] = hasRecievedChefMeal;
            if (apostleQuestTracker != 0)
                tag["talkQuestTracker"] = apostleQuestTracker;
        }
    }
}
