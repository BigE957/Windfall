using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.Projectiles.Other;
using DialogueHelper.UI.Dialogue;
using Windfall.Content.NPCs.WorldEvents.DragonCult;

namespace Windfall.Content.NPCs.WorldEvents.LunarCult;

public class LunarBishop : ModNPC
{
    private enum States
    {
        Idle,
        SelenicChat,
        Something,
        CafeteriaEvent,
        StaticCharacter,
    }
    private States AIState
    {
        get => (States)NPC.ai[2];
        set => NPC.ai[2] = (float)value;
    }
    public enum Character
    {
        Foodie,
        Speaker
    }
    public Character myCharacter;
    public bool characterSpokenTo = false;

    public override string Texture => "Windfall/Assets/NPCs/WorldEvents/LunarBishop";
    internal static SoundStyle SpawnSound => new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");
    public override void SetStaticDefaults()
    {
        this.HideBestiaryEntry();
        Main.npcFrameCount[Type] = 1;
        NPCID.Sets.NoTownNPCHappiness[Type] = true;
        ModContent.GetInstance<DialogueUISystem>().TreeClose += CloseEffect;
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
            case States.Idle:
                NPC.alpha = 255;
                Vector2 oldPos = NPC.position;
                NPC.position.Y = FindSurfaceBelow(new Point((int)NPC.position.X / 16, (int)NPC.position.Y / 16)).Y * 16 - NPC.height;

                for (int i = 0; i < 2; i++)
                {
                    float altY = (FindSurfaceBelow(new Point((int)(oldPos.X / 16 + i), (int)(oldPos.Y / 16 - 2))).Y - 1) * 16 - NPC.height + 16;
                    if (altY < NPC.position.Y)
                        NPC.position.Y = altY;
                }

                NPC.alpha = 0;
                for (int i = 0; i <= 50; i++)
                {
                    int dustStyle = Main.rand.NextBool() ? 66 : 263;
                    Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                    Dust dust = Dust.NewDustPerfect(NPC.Center, Main.rand.NextBool(3) ? 191 : dustStyle, speed * 3, Scale: Main.rand.NextFloat(1.5f, 2.3f));
                    dust.noGravity = true;
                    dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                }
                SoundEngine.PlaySound(SpawnSound, NPC.Center);
                break;
            case States.CafeteriaEvent:
                NPC.aiStyle = -1;
                NPC.direction = -1;
                NPC.noGravity = true;
                NPC.noTileCollide = true;
                break;
            case States.StaticCharacter:
                NPC.alpha = 0;
                NPC.noGravity = false;
                NPC.aiStyle = -1;
                break;
        }
    }
    public override void AI()
    {
        switch (AIState)
        {
            case States.CafeteriaEvent:
                const int queueGap = 64;
                int queueID = (int)NPC.ai[3];
                if (queueID == -1 || !LunarCultBaseSystem.Active)
                {
                    if (NPC.velocity.X < 1.5f)
                        NPC.velocity.X += 0.05f;
                    else
                        NPC.velocity.X = 1.5f;

                    float goalY = (LunarCultBaseSystem.LunarCultBaseLocation.Y * 16 - 96) - NPC.height;
                    if (NPC.velocity.Y >= 0 && NPC.position.Y >= goalY)
                    {
                        NPC.position.Y = goalY;
                        if (NPC.velocity.Y != 0)
                            NPC.velocity.Y = 0;
                    }
                    if (NPC.position.Y < goalY)
                        NPC.velocity.Y += 0.5f;

                    NPC.direction = 1;
                    if (NPC.Center.X - (LunarCultBaseSystem.LunarCultBaseLocation.X * 16 - (380 * (LunarCultBaseSystem.BaseFacingLeft ? 1 : -1))) > 800)
                    {
                        for (int i = 0; i <= 50; i++)
                        {
                            int dustStyle = Main.rand.NextBool() ? 66 : 263;
                            Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                            Dust dust = Dust.NewDustPerfect(NPC.Center, Main.rand.NextBool(3) ? 191 : dustStyle, speed * 3, Scale: Main.rand.NextFloat(1.5f, 2.3f));
                            dust.noGravity = true;
                            dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                        }
                        SoundEngine.PlaySound(SpawnSound, NPC.Center);
                        NPC.active = false;
                    }
                }
                else
                {
                    int subID = (int)(NPC.ai[3] - Math.Floor(NPC.ai[3])) * 10;

                    if (LunarCultBaseSystem.SeatedTables.Any(t => t.HasValue && t.Value.TableID == queueID)) //Should Be Seated
                    {
                        int tableIndex = LunarCultBaseSystem.SeatedTables.ToList().FindIndex(t => t.HasValue && t.Value.TableID == queueID);
                        Vector2 goalLocation = LunarCultBaseSystem.CafeteriaTables[tableIndex].ToWorldCoordinates();
                        if (subID == 0)
                            goalLocation.X += 32 * (queueID % 2 == 0 ? -1 : 1);
                        else
                            goalLocation.X += 32 * (subID == 2 ? -1 : 1);

                        if (NPC.Center.DistanceSQ(goalLocation) < 256)
                        {
                            NPC.velocity.X = 0;

                            //seated dialogue could be put here, or a seated bool could be used so we arent distance checking all the ding dang time :P
                        }
                        else
                        {
                            if (NPC.Center.X < goalLocation.X)
                            {
                                if (Math.Abs(NPC.velocity.X) < 1.5f)
                                    NPC.velocity.X += 0.05f;
                                else
                                    NPC.velocity.X = 1.5f;
                            }
                            else
                            {
                                if (Math.Abs(NPC.velocity.X) > -1.5f)
                                    NPC.velocity.X -= 0.05f;
                                else
                                    NPC.velocity.X = -1.5f;
                            }
                        }
                    }
                    else //Within Queue
                    {
                        int queueIndex = LunarCultBaseSystem.QueuedTables.FindIndex(t => t.HasValue && t.Value.TableID == queueID);
                        if (queueIndex == -1)
                            Main.NewText("HEY SOMETHINGS GONE HORRIBLY WRONG BRO I CANT FIND MY QUEUE INDEX YOU MOTHERFUCKER!!!");

                        LunarCultBaseSystem.Table myTable = LunarCultBaseSystem.QueuedTables[queueIndex].Value;
                        float goalOffset = 0f;
                        if (subID != 0)
                            goalOffset = queueGap / 3f * (LunarCultBaseSystem.BaseFacingLeft ? 1 : -1) * (subID == 1 ? 1 : -1);
                        Vector2 goalPosition = new(LunarCultBaseSystem.LunarCultBaseLocation.X * 16 - (320 * (LunarCultBaseSystem.BaseFacingLeft ? 1 : -1)) + queueGap * queueIndex + goalOffset, LunarCultBaseSystem.LunarCultBaseLocation.Y * 16 - 96);
                        float angerRatio = (LunarCultBaseSystem.QueuedTables.Count - 4) / ((float)LunarCultBaseSystem.QueueLimit - 4);
                        if (LunarCultBaseSystem.QueuedTables.Count <= 4)
                            angerRatio = 0f;
                        if (NPC.velocity.Y >= 0 && NPC.position.Y >= goalPosition.Y - NPC.height)
                        {
                            NPC.position.Y = goalPosition.Y - NPC.height;
                            if (NPC.velocity.Y != 0)
                                NPC.velocity.Y = 0;
                        }
                        if (NPC.velocity.Y == 0 && NPC.position.Y == goalPosition.Y - NPC.height && Main.rand.NextBool(angerRatio))
                        {
                            if (Main.rand.NextBool(5))
                                CombatText.NewText(NPC.Hitbox, Color.Lerp(Color.White, Color.Red, angerRatio), GetWindfallTextValue("Dialogue.LunarCult.LunarBishop.Cafeteria.Madge." + Main.rand.Next(6)));
                            if (Main.rand.NextBool())
                                NPC.velocity.Y = -4;
                        }
                        if (NPC.position.Y < goalPosition.Y - NPC.height)
                            NPC.velocity.Y += 0.5f;
                        if (queueIndex != 0 && !LunarCultBaseSystem.QueuedTables[queueIndex - 1].HasValue)
                        {
                            goalPosition.X -= queueGap;
                            if (NPC.Center.X - goalPosition.X < queueGap / 2 && subID != 2)
                            {
                                LunarCultBaseSystem.QueuedTables[queueIndex - 1] = LunarCultBaseSystem.QueuedTables[queueIndex];
                                if (queueIndex + 1 == LunarCultBaseSystem.QueuedTables.Count)
                                    LunarCultBaseSystem.QueuedTables.RemoveAt(queueIndex);
                                else
                                    LunarCultBaseSystem.QueuedTables[queueIndex] = null;
                                int currentID = queueID;
                                foreach (NPC npc in Main.npc.Where(n => n.active && n.type == Type && ((int)n.ai[3]) == currentID))
                                    NPC.ai[3] -= 1;
                            }
                        }
                        if (goalPosition.X < NPC.Center.X)
                            if (NPC.velocity.X > -1.5f)
                                NPC.velocity.X -= 0.05f;
                            else
                                NPC.velocity.X = -1.5f;
                        else
                            NPC.velocity.X = 0;
                    }
                }
                break;
            case States.StaticCharacter:

                break;
        }
    }
    public override bool CanChat() => ((AIState == States.SelenicChat || AIState == States.StaticCharacter) && !ModContent.GetInstance<DialogueUISystem>().isDialogueOpen) || (AIState == States.CafeteriaEvent && NPC.ai[3] == 0 && NPC.velocity.X == 0);
    public override string GetChat()
    {
        Main.CloseNPCChatOrSign();

        switch (AIState)
        {
            case States.SelenicChat:
                ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, AIState.ToString(), new(Name, [NPC.whoAmI]));
                break;
            case States.StaticCharacter:
                ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, $"SelenicCultists/{myCharacter}", new(Name, [NPC.whoAmI]), characterSpokenTo ? 1 : 0);
                characterSpokenTo = true;
                break;
        }
        return "Rizz"; //Won't actually be seen.
    }
    public override bool CheckActive() => false;
    private static void CloseEffect(string treeKey, int dialogueID, int buttonID, bool swapped)
    {
        DialogueUISystem uiSystem = ModContent.GetInstance<DialogueUISystem>();
        if (uiSystem.CurrentDialogueContext.Catagory != nameof(LunarBishop))
            return;

        if (treeKey == States.SelenicChat.ToString())
        {
            NPC me = Main.npc[(int)ModContent.GetInstance<DialogueUISystem>().CurrentDialogueContext.Arguments[0]];
            me.As<LunarBishop>().Despawn();
        }
    }
    public void Despawn()
    {
        for (int i = 0; i <= 50; i++)
        {
            int dustStyle = Main.rand.NextBool() ? 66 : 263;
            Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
            Dust dust = Dust.NewDustPerfect(NPC.Center, Main.rand.NextBool(3) ? 191 : dustStyle, speed * 3, Scale: Main.rand.NextFloat(1.5f, 2.3f));
            dust.noGravity = true;
            dust.color = dust.type == dustStyle ? Color.LightGreen : default;
        }
        SoundEngine.PlaySound(SpawnSound, NPC.Center);
        NPC.active = false;
    }
}
