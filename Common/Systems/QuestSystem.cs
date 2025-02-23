using Terraria.ModLoader.IO;

namespace Windfall.Common.Systems;
public class Quest
{
    public bool Active { get; set; }
    public int Progress { get; private set; }
    public void IncrementProgress()
    {
        Progress++;
    }
    public void ResetQuest()
    {
        Active = false;
        Progress = 0;
    }
    public int CompletionAmount { get; private set; }
    public bool Complete { get => Progress >= CompletionAmount; }
    public bool InProgress { get => Active && !Complete; }
    public float CompletionRatio { get => Progress / (float)CompletionAmount; }

    internal Quest(int completionAmount)
    {
        Active = false;
        Progress = 0;
        CompletionAmount = completionAmount;
    }

    internal void SaveWorldData(TagCompound tag)
    {
        if (Active)
            tag[nameof(Active)] = true;
        if (Progress != 0)
            tag[nameof(Progress)] = Progress;
    }
    internal void LoadWorldData(TagCompound tag)
    {
        Active = tag.GetBool(nameof(Active));
        Progress = tag.GetInt(nameof(Progress));
    }
}

public class QuestSystem : ModSystem
{
    public static readonly Dictionary<string, Quest> Quests;

    public override void OnModLoad()
    {
        Quests.Add("CnidrionHunt", new Quest(3));
        Quests.Add("ScoogHunt", new Quest(1));
        Quests.Add("ShuckinClams", new Quest(8));
        Quests.Add("ScoogHunt2", new Quest(1));
    }

    public override void OnModUnload()
    {
        Quests.Clear();
    }

    #region Saving + Loading
    public override void OnWorldLoad()
    {
        foreach (Quest quest in Quests.Values)
            quest.ResetQuest();
    }
    public override void OnWorldUnload()
    {
        foreach (Quest quest in Quests.Values)
            quest.ResetQuest();
    }
    public override void SaveWorldData(TagCompound tag)
    {
        foreach (Quest quest in Quests.Values)
            quest.SaveWorldData(tag);
    }
    public override void LoadWorldData(TagCompound tag)
    {
        foreach (Quest quest in Quests.Values)
            quest.LoadWorldData(tag);
    }
    #endregion
}
