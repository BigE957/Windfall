using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.NPCs.WorldEvents.LunarCult;
using Windfall.Content.Projectiles.Other;

namespace Windfall.Content.NPCs.WanderingNPCs
{
    public class LunarCultistArcher : ModNPC
    {
        /*
        private enum DialogueState
        {
            Initial,
            WhoAreWe,
            WannaJoin,
            NuhUh,
            ExploringHuh,
            FairEnough,
            SomethingBad,
            End
        }
        private DialogueState CurrentDialogue
        {
            get => (DialogueState)NPC.ai[1];
            set => NPC.ai[1] = (int)value;
        }
        */
        private enum States
        {
            Idle,
            Chatting,
            CafeteriaEvent,
        }
        private States AIState
        {
            get => (States)NPC.ai[2];
            set => NPC.ai[2] = (float)value;
        }
        public override string Texture => "Windfall/Assets/NPCs/WorldEvents/LunarCultistArcher";
        private static SoundStyle SpawnSound => new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");
        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            NPCID.Sets.ActsLikeTownNPC[Type] = true;
            Main.npcFrameCount[Type] = 5;
            NPCID.Sets.NoTownNPCHappiness[Type] = true;
        }
        public override void SetDefaults()
        {
            NPC.friendly = true; // NPC Will not attack player
            NPC.width = 38;
            NPC.height = 52;
            NPC.aiStyle = 0;
            NPC.damage = 45;
            NPC.defense = 14;
            NPC.lifeMax = 210;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 1f;
            NPC.immortal = true;

            AnimationType = NPCID.BartenderUnconscious;
        }    
        public override void OnSpawn(IEntitySource source)
        {
            switch (AIState)
            {
                case States.Idle:
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
                case States.CafeteriaEvent:
                    NPC.ai[3] = LunarCultActivitySystem.CustomerQueue.Count;
                    LunarCultActivitySystem.CustomerQueue.Add(new LunarCultActivitySystem.Customer(NPC, LunarCultActivitySystem.MenuFoodIDs[Main.rand.Next(LunarCultActivitySystem.MenuFoodIDs.Count)]));
                    NPC.aiStyle = -1;
                    NPC.direction = -1;
                    NPC.noGravity = true;
                    NPC.noTileCollide = true;
                    break;
            }
        }
        public override void AI()
        {
            if (AIState == States.CafeteriaEvent)
            {
                const int queueGap = 50;
                int queueIndex = (int)NPC.ai[3];
                if (queueIndex == -1)
                {
                    if (NPC.velocity.X < 1.5f)
                        NPC.velocity.X += 0.05f;
                    else
                        NPC.velocity.X = 1.5f;
                    NPC.direction = 1;
                    NPC.spriteDirection = 1;
                    if (NPC.Center.X - ((LunarCultActivitySystem.LunarCultBaseLocation.X * 16) - 850) > 800)
                        NPC.active = false;
                }
                else
                {
                    Vector2 goalPosition = new((LunarCultActivitySystem.LunarCultBaseLocation.X * 16) - 850 + (queueGap * queueIndex), (LunarCultActivitySystem.LunarCultBaseLocation.Y * 16) - 96);
                    NPC.position.Y = goalPosition.Y - (NPC.height);
                    if (queueIndex != 0 && !LunarCultActivitySystem.CustomerQueue[queueIndex - 1].HasValue)
                    {
                        goalPosition.X -= queueGap;
                        if (NPC.Center.X - goalPosition.X < (queueGap / 2))
                        {
                            LunarCultActivitySystem.CustomerQueue[queueIndex - 1] = LunarCultActivitySystem.CustomerQueue[queueIndex];
                            if (queueIndex + 1 == LunarCultActivitySystem.CustomerQueue.Count)
                                LunarCultActivitySystem.CustomerQueue.RemoveAt(queueIndex);
                            else
                                LunarCultActivitySystem.CustomerQueue[queueIndex] = null;
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
                            Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), NPC.Center, new Vector2(Main.rand.NextFloat(0f, 2f), -2.5f), ModContent.ProjectileType<FoodAlert>(), 0, 0f, ai0: LunarCultActivitySystem.CustomerQueue[queueIndex].Value.OrderID, ai1: Main.rand.Next(3), ai2: NPC.whoAmI);
                    }
                }
            }
        }
        public override bool CheckActive() => false;
        public override bool CanChat() => AIState == States.Chatting || (AIState == States.CafeteriaEvent && NPC.ai[3] == 0 && NPC.velocity.X == 0);
        public override string GetChat()
        {
            if (AIState == States.Chatting)
                return GetWindfallTextValue("Dialogue.LunarCult.MechanicShed.{CurrentDialogue}");
            else
            {
                Main.CloseNPCChatOrSign();

                if (Main.player[Main.myPlayer].HeldItem.type == LunarCultActivitySystem.CustomerQueue[0].Value.OrderID)
                {
                    Main.player[Main.myPlayer].HeldItem.stack--;

                    if (LunarCultActivitySystem.CustomerQueue.Count == 1)
                        LunarCultActivitySystem.CustomerQueue = [];
                    else
                        LunarCultActivitySystem.CustomerQueue[0] = null;
                    if (Main.projectile.Any(p => p.active && p.type == ModContent.ProjectileType<FoodAlert>() && p.ai[2] == NPC.whoAmI))
                        Main.projectile.First(p => p.active && p.type == ModContent.ProjectileType<FoodAlert>() && p.ai[2] == NPC.whoAmI).ai[2] = -1;
                    NPC.ai[3] = -1;
                    CombatText.NewText(NPC.Hitbox, Color.White, GetWindfallTextValue("Dialogue.LunarCult.LunarBishop.Cafeteria.Thanks." + Main.rand.Next(3)));
                    LunarCultActivitySystem.SatisfiedCustomers++;
                    if (LunarCultActivitySystem.SatisfiedCustomers == LunarCultActivitySystem.CustomerGoal)
                    {
                        NPC chef = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TheChef>())];
                        CombatText.NewText(chef.Hitbox, Color.LimeGreen, GetWindfallTextValue("Dialogue.LunarCult.TheChef.Activity.AlmostDone"), true);
                    }
                }
                else
                {
                    CombatText.NewText(NPC.Hitbox, Color.White, GetWindfallTextValue("Dialogue.LunarCult.LunarBishop.Cafeteria.Where." + Main.rand.Next(3)));
                }

                return "Rizz"; //Won't actually be seen.
            }
        }

        /*
        public override string GetChat() => GetWindfallTextValue($"Dialogue.LunarCult.MechanicShed.{CurrentDialogue}");
        
        private readonly List<dialogueDirections> MyDialogue = new()
        {
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Initial,
                Button1 = new(){name = "Your guardian?", heading = (int)DialogueState.ExploringHuh},
                Button2 = new(){name = "What issues?", heading = (int)DialogueState.WhoAreWe},
            },
        };
        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            CurrentDialogue = (DialogueState)GetNPCConversation(MyDialogue, (int)CurrentDialogue, firstButton);
            if (CurrentDialogue == DialogueState.End)
            {
                foreach (NPC npc in Main.npc.Where(n => n.type == NPC.type && n.active))
                {
                    for (int i = 0; i < 50; i++)
                    {
                        Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                        Dust d = Dust.NewDustPerfect(npc.Center, DustID.GoldFlame, speed * 3, Scale: 1.5f);
                        d.noGravity = true;
                    }
                    npc.active = false;
                }
            }
            else
                Main.npcChatText = GetWindfallTextValue($"Dialogue.LunarCult.MechanicShed.{CurrentDialogue}");
        }
        public override void SetChatButtons(ref string button, ref string button2)
        {
            SetConversationButtons(MyDialogue, (int)CurrentDialogue, ref button, ref button2);
        }
        */
    }
}
