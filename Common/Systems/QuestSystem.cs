﻿using Terraria.ModLoader.IO;

namespace Windfall.Common.Systems;

public delegate void OnModUnload();
public delegate void OnWorldLoad();
public delegate void OnWorldUnload();
public delegate void SaveWorldData(TagCompound tag);
public delegate void LoadWorldData(TagCompound tag);

public class Quest
{
    public bool Active { get; set; }
    public int Progress { get; set; }
    public void IncrementProgress()
    {
        if(Active)
            Progress++;
        if(AutoDeactivate && Complete)
            Active = false;
    }
    public void ResetQuest()
    {
        Active = false;
        Progress = 0;
    }
    public int AmountToComplete { get; private set; }
    public bool Complete { get => Progress >= AmountToComplete; }
    public bool InProgress { get => Active && !Complete; }
    public float CompletionRatio { get => Progress / (float)AmountToComplete; }

    private readonly bool AutoDeactivate;

    internal Quest(int amountToComplete, bool autoDeactivates, ref OnWorldLoad worldLoad, ref OnWorldUnload worldUnload, ref SaveWorldData saveWorld, ref LoadWorldData loadWorld)
    {
        Active = false;
        Progress = 0;
        AmountToComplete = amountToComplete;
        AutoDeactivate = autoDeactivates;

        worldLoad += ResetQuest;
        worldUnload += ResetQuest;
        saveWorld += SaveWorldData;
        loadWorld += LoadWorldData;
    }

    private void SaveWorldData(TagCompound tag)
    {
        if (Active)
            tag[nameof(Active)] = true;
        if (Progress != 0)
            tag[nameof(Progress)] = Progress;
    }
    private void LoadWorldData(TagCompound tag)
    {
        Active = tag.GetBool(nameof(Active));
        Progress = tag.GetInt(nameof(Progress));
    }
}

public class QuestSystem : ModSystem
{
    public static OnWorldLoad WorldLoadEvent;
    public static OnWorldUnload WorldUnloadEvent;
    public static SaveWorldData SaveWorldEvent;
    public static LoadWorldData LoadWorldEvent;

    public static readonly Dictionary<string, Quest> Quests = [];

    //Quest Initialization
    public override void OnModLoad()
    {
        OnModUnload(); //Resets all data just in case

        AddQuest("CnidrionHunt", 3);
        AddQuest("ScoogHunt");
        AddQuest("ShuckinClams", 8);
        AddQuest("ClamHunt");
        AddQuest("TabletFragment", 2, true);
        AddQuest("ScoogHunt2");
        AddQuest("PrimordialLightShard", autoDeactivate: true);
        AddQuest("Recruitment", 4);
        AddQuest("DraconicBone", autoDeactivate: true);
        AddQuest("SealingRitual", autoDeactivate: true);
    }

    private static void AddQuest(string name, int amountToComplete = 1, bool autoDeactivate = false)
    {
        if (Quests.ContainsKey(name))
        {
            Windfall.Instance.Logger.Warn("Already existing quest of name " + name + " was attempted to be added.");
            return;
        }
        Quests.Add(name, new Quest(amountToComplete, autoDeactivate, ref WorldLoadEvent, ref WorldUnloadEvent, ref SaveWorldEvent, ref LoadWorldEvent));
    }

    #region Saving + Reseting
    public override void OnModUnload()
    {
        WorldLoadEvent = null;
        WorldUnloadEvent = null;
        SaveWorldEvent = null;
        LoadWorldEvent = null;

        Quests.Clear();
    }
    public override void OnWorldLoad()
    {
        WorldLoadEvent.Invoke();
    }
    public override void OnWorldUnload()
    {
        WorldUnloadEvent.Invoke();
    }
    public override void SaveWorldData(TagCompound tag)
    {
        SaveWorldEvent.Invoke(tag);
    }
    public override void LoadWorldData(TagCompound tag)
    {
        LoadWorldEvent.Invoke(tag);
    }
    #endregion
}
