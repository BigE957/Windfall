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
        this.HideFromBestiary();
        Main.npcFrameCount[Type] = 1;
        NPCID.Sets.NoTownNPCHappiness[Type] = true;
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
        switch (AIState)
        {
        }
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
            ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, $"Watchman/{LunarCultBaseSystem.PlannedActivity}");
            return "";
        }

        if(!spokenTo)
        {
            spokenTo = true;
            Main.LocalPlayer.LunarCult().timesWatchmenTalked++;
        }

        ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "Watchman/Default", Main.LocalPlayer.LunarCult().timesWatchmenTalked >= 3 ? 2 : 0);

        return "";
    }
    public override bool CheckActive() => false;
}