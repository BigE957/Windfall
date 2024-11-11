using DialogueHelper.UI.Dialogue;
using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.Projectiles.Other;

namespace Windfall.Content.NPCs.WorldEvents.LunarCult;
public class Watchman : ModNPC
{
    private enum States
    {
        Greeting,
        Idle
    }
    private States AIState
    {
        get => (States)NPC.ai[2];
        set => NPC.ai[2] = (float)value;
    }

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

        }
    }
    public override bool CanChat() => LunarCultBaseSystem.State != LunarCultBaseSystem.SystemStates.Yap && LunarCultBaseSystem.State != LunarCultBaseSystem.SystemStates.Waiting;
    public override string GetChat()
    {
        Main.CloseNPCChatOrSign();

        switch (AIState)
        {
        }
        return "Rizz"; //Won't actually be seen.
    }
    public override bool CheckActive() => false;
}