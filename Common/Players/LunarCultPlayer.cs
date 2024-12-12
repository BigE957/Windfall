using CalamityMod.NPCs.NormalNPCs;
using Terraria.ModLoader.IO;

namespace Windfall.Common.Players;

public class LunarCultPlayer : ModPlayer
{
    public bool SeamstressTalked = false;
    public bool awareOfLunarCoins = false;
    public bool hasRecievedChefMeal = false;
    public int apostleQuestTracker = 0;
    public bool spokeToAbandonedChef = false;
    public int timesWatchmenTalked = 0;
    public bool askedWatchmanAboutOrator = false;
    public override void LoadData(TagCompound tag)
    {
        SeamstressTalked = tag.GetBool("SeamstressTalked");
        awareOfLunarCoins = tag.GetBool("awareOfLunarCoins");
        hasRecievedChefMeal = tag.GetBool("hasRecievedChefMeal");
        apostleQuestTracker = tag.GetInt("talkQuestTracker");
        spokeToAbandonedChef = tag.GetBool("spokeToAbandonedChef");
        timesWatchmenTalked = tag.GetInt("timesWatchmenTalked");
        askedWatchmanAboutOrator = tag.GetBool("askedWatchmanAboutOrator");
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
        if (spokeToAbandonedChef)
            tag["spokeToAbandonedChef"] = spokeToAbandonedChef;
        if (timesWatchmenTalked != 0)
            tag["timesWatchmenTalked"] = timesWatchmenTalked;
        if (askedWatchmanAboutOrator)
            tag["askedWatchmanAboutOrator"] = askedWatchmanAboutOrator;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (apostleQuestTracker == 8 && target.type == ModContent.NPCType<Eidolist>() && target.life <= 0)
            Main.LocalPlayer.LunarCult().apostleQuestTracker++;
    }
}
