using DialogueHelper.UI.Dialogue;
using Windfall.Common.Systems.WorldEvents;

namespace Windfall.Content.NPCs.WorldEvents.LunarCult;
public class Watchman : ModNPC
{
    private enum States
    {
        Greeting,
        Idle,
        Talkable
    }
    private States AIState
    {
        get => (States)NPC.ai[2];
        set => NPC.ai[2] = (float)value;
    }

    private bool spokenTo = false;

    public override string Texture => "Windfall/Assets/NPCs/WorldEvents/LunarBishop";
    internal static SoundStyle SpawnSound => new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");
    public override void SetStaticDefaults()
    {
        this.HideBestiaryEntry();
        Main.npcFrameCount[Type] = 1;
        NPCID.Sets.NoTownNPCHappiness[Type] = true;
        ModContent.GetInstance<DialogueUISystem>().TreeInitialize += ModifyTree;
        ModContent.GetInstance<DialogueUISystem>().ButtonClick += ClickEffect;
    }
    public override void SetDefaults()
    {
        NPC.friendly = true; // NPC Will not attack player
        NPC.width = 36;
        NPC.height = 58;
        NPC.aiStyle = 0;
        NPC.damage = 0;
        NPC.defense = 0;
        NPC.lifeMax = 500;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0f;
        NPC.immortal = true;
    }
    public override void OnSpawn(IEntitySource source)
    {
        /*
        switch (AIState)
        {
        }
        */
    }
    public override void AI()
    {
        switch (AIState)
        {
            case States.Greeting:
                spokenTo = false;
                break;
        }
    }
    public override bool CanChat() => AIState != States.Greeting;
    public override string GetChat()
    {
        Main.CloseNPCChatOrSign();

        if(AIState == States.Idle)
        {
            ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(WindfallMod.Instance, $"Watchman/{LunarCultBaseSystem.PlannedActivity}", new(Name, [NPC.whoAmI]));
            return "";
        }

        if(!spokenTo)
        {
            spokenTo = true;
            Main.LocalPlayer.LunarCult().timesWatchmenTalked++;
        }

        ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(WindfallMod.Instance, "Watchman/Default", new(Name, [NPC.whoAmI]), Main.LocalPlayer.LunarCult().timesWatchmenTalked >= 3 ? 2 : 0);

        return "";
    }
    public override bool CheckActive() => false;
    private static void ModifyTree(string treeKey, int dialogueID, int buttonID, bool swapped)
    {
        DialogueUISystem uiSystem = ModContent.GetInstance<DialogueUISystem>();
        switch (treeKey)
        {
            case "Watchman/Default":
                if(Main.LocalPlayer.LunarCult().askedWatchmanAboutOrator)
                {
                    uiSystem.CurrentTree.Dialogues[2].Responses[2].Requirement = false;
                    uiSystem.CurrentTree.Dialogues[4].Responses[2].Requirement = false;
                    uiSystem.CurrentTree.Dialogues[5].Responses[3].Requirement = false;
                }
                break;
        }
    }
    private static void ClickEffect(string treeKey, int dialogueID, int buttonID, bool swapped)
    {
        if(treeKey == "Watchman/Default" && dialogueID == 11)
            Main.LocalPlayer.LunarCult().askedWatchmanAboutOrator = true;

    }
}