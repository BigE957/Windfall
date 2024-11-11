using DialogueHelper.UI.Dialogue;
using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.Projectiles.Other;

namespace Windfall.Content.NPCs.WorldEvents.LunarCult;

public class LunarCultistDevotee : ModNPC
{
    private enum States
    {
        Idle,
        Chatting,
        CafeteriaEvent,
        RitualEvent,
        StaticCharacter,
    }
    private States AIState
    {
        get => (States)NPC.ai[2];
        set => NPC.ai[2] = (float)value;
    }
    public enum Character
    {
        NewClothes,
        Eeper
    }
    public Character myCharacter;
    public bool characterSpokenTo = false;

    public Vector2 goalPosition = Vector2.Zero;
    public override string Texture => "Windfall/Assets/NPCs/WorldEvents/LunarCultistDevotee";
    internal static SoundStyle SpawnSound => new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");
    public override void SetStaticDefaults()
    {
        this.HideFromBestiary();
        Main.npcFrameCount[Type] = 6;
        NPCID.Sets.NoTownNPCHappiness[Type] = true;
        NPCID.Sets.AllowDoorInteraction[Type] = true;
    }
    public override void SetDefaults()
    {
        NPC.friendly = true; // NPC Will not attack player
        NPC.width = 34;
        NPC.height = 48;
        NPC.aiStyle = 0;
        NPC.damage = 0;
        NPC.defense = 0;
        NPC.lifeMax = 400;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0f;
        NPC.immortal = true;

        AnimationType = NPCID.CultistDevote;
    }
    public override void OnSpawn(IEntitySource source)
    {
        if(AIState == States.CafeteriaEvent || AIState == States.RitualEvent || AIState == States.StaticCharacter)
            NPC.aiStyle = -1;
        switch (AIState)
        {
            case States.CafeteriaEvent:
                NPC.ai[3] = LunarCultBaseSystem.CustomerQueue.Count;
                LunarCultBaseSystem.CustomerQueue.Add(new LunarCultBaseSystem.Customer(NPC, LunarCultBaseSystem.MenuFoodIDs[Main.rand.Next(LunarCultBaseSystem.MenuFoodIDs.Count)]));                    
                NPC.direction = -1;
                NPC.noGravity = true;
                NPC.noTileCollide = true;
                AnimationType = NPCID.BartenderUnconscious;
                NPC.frame.X = 3;
                break;
            case States.StaticCharacter:
                NPC.alpha = 0;
                NPC.noGravity = false;
                NPC.aiStyle = -1;
                break;
            default:
                NPC.alpha = 255;
                Vector2 oldPos = NPC.position;
                NPC.position.Y = FindSurfaceBelow(new Point((int)NPC.position.X / 16, (int)NPC.position.Y / 16)).Y * 16 - NPC.height;

                float altY = 0;
                for (int i = 0; i < 2; i++)
                {
                    altY = (FindSurfaceBelow(new Point((int)(oldPos.X / 16 + i), (int)(oldPos.Y / 16 - 2))).Y - 1) * 16 - NPC.height + 16;
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
        }
    }
    public override void AI()
    {       
        //AIState = States.RitualEvent;
        switch (AIState)
        {
            case States.CafeteriaEvent:
                const int queueGap = 50;
                int queueIndex = (int)NPC.ai[3];
                if (queueIndex == -1 || !LunarCultBaseSystem.Active)
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
                    NPC.spriteDirection = 1;
                    if (NPC.Center.X - (LunarCultBaseSystem.LunarCultBaseLocation.X * 16 - 850) > 800)
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
                    Vector2 goalPosition = new(LunarCultBaseSystem.LunarCultBaseLocation.X * 16 - 850 + queueGap * queueIndex, LunarCultBaseSystem.LunarCultBaseLocation.Y * 16 - 96);
                    float angerRatio = (LunarCultBaseSystem.CustomerQueue.Where(c => c.HasValue).Count() - 4) / ((float)LunarCultBaseSystem.CustomerLimit - 4);
                    if (LunarCultBaseSystem.CustomerQueue.Where(c => c.HasValue).Count() <= 4)
                        angerRatio = 0f;
                    if (NPC.velocity.Y >= 0 && NPC.position.Y >= goalPosition.Y - NPC.height)
                    {
                        NPC.position.Y = goalPosition.Y - NPC.height;
                        if (NPC.velocity.Y != 0)
                            NPC.velocity.Y = 0;
                    }
                    if (NPC.velocity.Y == 0 && NPC.position.Y == goalPosition.Y - NPC.height && Main.rand.NextBool(angerRatio))
                    {
                        if(Main.rand.NextBool(5))
                            CombatText.NewText(NPC.Hitbox, Color.Lerp(Color.White, Color.Red, angerRatio), GetWindfallTextValue("Dialogue.LunarCult.LunarBishop.Cafeteria.Madge." + Main.rand.Next(6)));
                        if (Main.rand.NextBool())
                            NPC.velocity.Y = -4;
                    }
                    if (NPC.position.Y < goalPosition.Y - NPC.height)
                        NPC.velocity.Y += 0.5f;
                    if (queueIndex != 0 && !LunarCultBaseSystem.CustomerQueue[queueIndex - 1].HasValue)
                    {
                        goalPosition.X -= queueGap;
                        if (NPC.Center.X - goalPosition.X < queueGap / 2)
                        {
                            LunarCultBaseSystem.CustomerQueue[queueIndex - 1] = LunarCultBaseSystem.CustomerQueue[queueIndex];
                            if (queueIndex + 1 == LunarCultBaseSystem.CustomerQueue.Count)
                                LunarCultBaseSystem.CustomerQueue.RemoveAt(queueIndex);
                            else
                                LunarCultBaseSystem.CustomerQueue[queueIndex] = null;
                            NPC.ai[3] -= 1;
                        }
                    }
                    if (goalPosition.X < NPC.Center.X)
                        if (NPC.velocity.X > -1.5f)
                            NPC.velocity.X -= 0.05f;
                        else
                            NPC.velocity.X = -1.5f;
                    else
                    {
                        NPC.velocity.X = 0;
                        if (queueIndex == 0 && !Main.projectile.Any(p => p.active && p.type == ModContent.ProjectileType<FoodAlert>() && p.ai[2] == NPC.whoAmI))
                            Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, new Vector2(Main.rand.NextFloat(0f, 2f), -2.5f), ModContent.ProjectileType<FoodAlert>(), 0, 0f, ai0: LunarCultBaseSystem.CustomerQueue[queueIndex].Value.OrderID, ai1: Main.rand.Next(3), ai2: NPC.whoAmI);
                    }
                }
                break;
            case States.RitualEvent:
                if (NPC.velocity.Y != 0)
                {
                    bool touchingPortal = false;
                    foreach (NPC portal in Main.npc.Where(n => n != null && n.active && n.type == ModContent.NPCType<PortalMole>()))
                    {
                        if (portal.Hitbox.Contains((int)NPC.Center.X, (int)NPC.Center.Y))
                            touchingPortal = true;
                    }
                    if (touchingPortal)
                    {
                        NPC.scale -= 0.0175f;
                        if (NPC.scale < 0.5f)
                        {
                            NPC.Opacity -= 0.05f;
                            if (NPC.scale < 0.1f)
                            {
                                LunarCultBaseSystem.RemainingCultists--;
                                if (LunarCultBaseSystem.RemainingCultists <= 0)
                                    LunarCultBaseSystem.ResetTimer();
                                NPC.active = false;
                            }
                        }
                    }
                    else
                    {
                        if (NPC.oldRot[0] == NPC.rotation)
                        {
                            NPC.velocity.X *= 0.95f;
                            if (NPC.velocity.X < 0.1f)
                                NPC.velocity.X = 0f;
                        }
                        if (NPC.velocity.Y == 0)
                            NPC.rotation = 0f;
                        if (NPC.scale < 1f)
                            NPC.scale += 0.05f;
                        else
                            NPC.scale = 1f;

                        if (NPC.Opacity < 1f)
                            NPC.Opacity += 0.05f;
                        else
                            NPC.Opacity = 1f;
                    }
                }
                else if (Math.Abs(goalPosition.X - NPC.Center.X) > 4)
                {
                    NPC.rotation = 0f;
                    if (NPC.Center.X > goalPosition.X)
                    {
                        NPC.velocity.X = -2;
                        NPC.spriteDirection = -1;
                    }
                    else
                    {
                        NPC.velocity.X = 2;
                        NPC.spriteDirection = 1;
                    }
                }
                else
                {
                    NPC.velocity.X = 0;
                    if(NPC.Center.X > LunarCultBaseSystem.ActivityCoords.X)
                        NPC.spriteDirection = -1;
                    else
                        NPC.spriteDirection = 1;
                }

                break;
            case States.StaticCharacter:
                if (myCharacter == Character.Eeper)
                {
                    if (!characterSpokenTo && (Main.GlobalTimeWrappedHourly - (int)Main.GlobalTimeWrappedHourly) < 0.015)
                    {
                        CombatText z = Main.combatText[CombatText.NewText(new((int)NPC.Center.X, (int)NPC.Bottom.Y, 1, 1), Color.LimeGreen, "Z", true)];
                        z.lifeTime /= 2;
                    }
                }
                break;
        }
    }
    public override bool CanChat() => AIState == States.StaticCharacter || AIState == States.CafeteriaEvent && NPC.ai[3] == 0 && NPC.velocity.X == 0;
    public override string GetChat()
    {
        Main.CloseNPCChatOrSign();

        switch(AIState)
        {
            case States.StaticCharacter:
                ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, $"SelenicBase/{myCharacter}", characterSpokenTo ? 1 : 0);
                characterSpokenTo = true;
                break;
            case States.CafeteriaEvent:
                if (Main.player[Main.myPlayer].HeldItem.type == LunarCultBaseSystem.CustomerQueue[0].Value.OrderID)
                {
                    Main.player[Main.myPlayer].HeldItem.stack--;

                    if (LunarCultBaseSystem.CustomerQueue.Count == 1)
                        LunarCultBaseSystem.CustomerQueue = [];
                    else
                        LunarCultBaseSystem.CustomerQueue[0] = null;
                    if (Main.projectile.Any(p => p.active && p.type == ModContent.ProjectileType<FoodAlert>() && p.ai[2] == NPC.whoAmI))
                        Main.projectile.First(p => p.active && p.type == ModContent.ProjectileType<FoodAlert>() && p.ai[2] == NPC.whoAmI).ai[2] = -1;
                    NPC.ai[3] = -1;
                    CombatText.NewText(NPC.Hitbox, Color.White, GetWindfallTextValue("Dialogue.LunarCult.LunarBishop.Cafeteria.Thanks." + Main.rand.Next(3)));
                    LunarCultBaseSystem.SatisfiedCustomers++;
                    if (LunarCultBaseSystem.SatisfiedCustomers == LunarCultBaseSystem.CustomerGoal)
                    {
                        NPC chef = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TheChef>())];
                        CombatText.NewText(chef.Hitbox, Color.LimeGreen, GetWindfallTextValue("Dialogue.LunarCult.TheChef.Activity.AlmostDone"), true);
                    }
                }
                else
                    CombatText.NewText(NPC.Hitbox, Color.White, GetWindfallTextValue("Dialogue.LunarCult.LunarBishop.Cafeteria.Where." + Main.rand.Next(3)));
                break;
        }

        return "Rizz"; //Won't actually be seen.
    }

    public override bool CheckActive() => false;
}
