using Terraria.ModLoader.IO;

namespace Windfall.Common.Systems;

public delegate void OnModUnload();
public delegate void OnWorldLoad();
public delegate void OnWorldUnload();
public delegate void SaveWorldData(TagCompound tag);
public delegate void LoadWorldData(TagCompound tag);

public class Quest
{
    #region Fields
    /// <summary>
    /// The Name of the <see cref="Quest"/> as is used in the Quests List
    /// </summary>
    public string Name { get;}

    /// <summary>
    /// Whether or not this <see cref="Quest"/> is currently Active
    /// </summary>
    public bool Active { get; set; }

    /// <summary>
    /// How far this <see cref="Quest"/> has been progressed
    /// </summary>
    public int Progress { get; set; }

    /// <summary>
    /// How much progress must be made on this <see cref="Quest"/> in order for it to be completed
    /// </summary>
    public int AmountToComplete { get; private set; }

    /// <summary>
    /// Whether or not this <see cref="Quest"/> has been Completed
    /// </summary>
    public bool Complete { 
        get 
        {
            bool isComplete = Progress >= AmountToComplete;
            if(AutoDeactivate && isComplete)
                Active = false;
            return isComplete; 
        } 
    }

    /// <summary>
    /// Whether or not this <see cref="Quest"/> is Active but not Complete
    /// </summary>
    public bool InProgress { get => Active && !Complete; }

    /// <summary>
    /// Te ratio of how much progress has been made on this <see cref="Quest"/>
    /// </summary>
    public float CompletionRatio { get => Progress / (float)AmountToComplete; }

    private readonly bool AutoDeactivate;
    #endregion

    /// <summary>
    /// Increments this <see cref="Quest"/>'s progress by one, and handles Auto Deactivation
    /// </summary>
    public void IncrementProgress()
    {
        if (Active)
            Progress++;
        if (AutoDeactivate && Complete)
            Active = false;
    }
    /// <summary>
    /// Resets progress on this <see cref="Quest"/>
    /// </summary>
    public void ResetQuest()
    {
        Active = false;
        Progress = 0;
    }

    internal Quest(string name, int amountToComplete, bool autoDeactivates, ref OnWorldLoad worldLoad, ref OnWorldUnload worldUnload, ref SaveWorldData saveWorld, ref LoadWorldData loadWorld)
    {
        Name = name;
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
            tag[Name + "/" + nameof(Active)] = true;
        if (Progress != 0)
            tag[Name + "/" + nameof(Progress)] = Progress;
    }
    private void LoadWorldData(TagCompound tag)
    {
        Active = tag.GetBool(Name + "/" + nameof(Active));
        Progress = tag.GetInt(Name + "/" + nameof(Progress));
    }
}

public class QuestSystem : ModSystem
{
    public static OnWorldLoad WorldLoadEvent;
    public static OnWorldUnload WorldUnloadEvent;
    public static SaveWorldData SaveWorldDataEvent;
    public static LoadWorldData LoadWorldDataEvent;

    public static readonly Dictionary<string, Quest> Quests = [];

    //Quest Initialization
    public override void OnModLoad()
    {
        OnModUnload(); //Resets all data just in case

        //Ilmeran Paladin Quests
        AddQuest("CnidrionHunt", 3);
        AddQuest("ScoogHunt");
        AddQuest("ShuckinClams", 8);
        AddQuest("ClamHunt");
        AddQuest("ScoogHunt2");

        //Travelling Cultist Quests
        AddQuest("TabletFragment", 2, true);
        AddQuest("PrimordialLightShard", autoDeactivate: true);
        AddQuest("Recruitment", 4);
        AddQuest("DraconicBone", autoDeactivate: true);
        AddQuest("SealingRitual", autoDeactivate: true);
    }

    private static void AddQuest(string name, int amountToComplete = 1, bool autoDeactivate = false)
    {
        if (Quests.ContainsKey(name))
        {
            WindfallMod.Instance.Logger.Warn("Already existing quest of name " + name + " was attempted to be added.");
            return;
        }
        Quests.Add(name, new Quest(name, amountToComplete, autoDeactivate, ref WorldLoadEvent, ref WorldUnloadEvent, ref SaveWorldDataEvent, ref LoadWorldDataEvent));
    }

    #region Saving + Reseting
    public override void OnModUnload()
    {
        WorldLoadEvent = null;
        WorldUnloadEvent = null;
        SaveWorldDataEvent = null;
        LoadWorldDataEvent = null;

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
        SaveWorldDataEvent.Invoke(tag);
    }
    public override void LoadWorldData(TagCompound tag)
    {
        LoadWorldDataEvent.Invoke(tag);
    }
    #endregion
}
