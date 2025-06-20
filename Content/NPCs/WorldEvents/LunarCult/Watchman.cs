using DialogueHelper.UI.Dialogue;
using MonoMod.Core.Platforms;
using Windfall.Common.Systems;
using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.NPCs.WorldEvents.DragonCult;

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

    public override string Texture => "Windfall/Assets/NPCs/WorldEvents/Watchman";
    internal static SoundStyle SpawnSound => new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");
    public override void SetStaticDefaults()
    {
        this.HideBestiaryEntry();
        Main.npcFrameCount[Type] = 6;
        NPCID.Sets.NoTownNPCHappiness[Type] = true;
        ModContent.GetInstance<DialogueUISystem>().TreeInitialize += ModifyTree;
        ModContent.GetInstance<DialogueUISystem>().ButtonClick += ClickEffect;
    }
    public override void SetDefaults()
    {
        NPC.friendly = true; // NPC Will not attack player
        NPC.width = 36;
        NPC.height = 120;
        NPC.aiStyle = -1;
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

        NPC.direction = LunarCultBaseSystem.BaseFacingLeft ? -1 : 1;

        NPC.spriteDirection = -NPC.direction;
    }

    public override bool CanChat() => !QuestSystem.Quests["DraconicBone"].Complete && !ModContent.GetInstance<DialogueUISystem>().isDialogueOpen && AIState != States.Greeting;
    public override string GetChat()
    {
        Main.CloseNPCChatOrSign();

        if(AIState == States.Idle)
        {
            ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, $"Watchman/{LunarCultBaseSystem.PlannedActivity}", new(Name, [NPC.whoAmI]));
            return "";
        }

        if(!spokenTo)
        {
            spokenTo = true;
            Main.LocalPlayer.LunarCult().timesWatchmenTalked++;
        }

        ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "Watchman/Default", new(Name, [NPC.whoAmI]), Main.LocalPlayer.LunarCult().timesWatchmenTalked >= 3 ? 2 : 0);

        return "";
    }
    public override bool CheckActive() => false;
    private static void ModifyTree(string treeKey, int dialogueID, int buttonID, bool swapped)
    {
        DialogueUISystem uiSystem = ModContent.GetInstance<DialogueUISystem>();
        switch (treeKey)
        {
            case "Watchman/Default":
                uiSystem.CurrentTree.Dialogues[0].Responses[0].Heading = 3;
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
        DialogueUISystem uiSystem = ModContent.GetInstance<DialogueUISystem>();
        if (uiSystem.CurrentDialogueContext.Catagory != nameof(Watchman))
            return;

        if (treeKey == "Watchman/Default" && dialogueID == 11)
            Main.LocalPlayer.LunarCult().askedWatchmanAboutOrator = true;

    }


    public override void FindFrame(int frameHeight)
    {
        NPC.frameCounter += 0.2f;
        NPC.frame.X = 0;
        NPC.frame.Y = frameHeight * ((int)NPC.frameCounter % Main.npcFrameCount[NPC.type]);
    }
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D texture = TextureAssets.Npc[Type].Value;

        spriteBatch.Draw(texture, NPC.Center - screenPos, NPC.frame, drawColor, 0f, NPC.frame.Size() * 0.5f, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : 0, 0f);

        return false;
    }
}