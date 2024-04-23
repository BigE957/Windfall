using CalamityMod.Items.Weapons.Summon;
using CalamityMod.World;
using Terraria.Graphics.Shaders;
using Terraria;
using Windfall.Common.Systems;
using Windfall.Content.Projectiles.Boss.Orator;
using CalamityMod.NPCs.SupremeCalamitas;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Items.LoreItems;
using Windfall.Content.Items.Lore;
using CalamityMod.Items.Placeables.Furniture.Trophies;
using CalamityMod;
using CalamityMod.Items.Armor.Vanity;
using CalamityMod.Items.TreasureBags;
using Terraria.GameContent.ItemDropRules;
using Windfall.Common.Utils;
using CalamityMod.Items.Weapons.Magic;

namespace Windfall.Content.NPCs.Bosses.TheOrator
{
    public class TheOrator : ModNPC
    {
        public override string Texture => "Windfall/Assets/NPCs/WorldEvents/TheOrator";
        public static readonly SoundStyle Dash = new("CalamityMod/Sounds/Item/DeadSunShot") { PitchVariance = 0.35f, Volume = 0.5f };
        public static readonly SoundStyle DashWarn = new("CalamityMod/Sounds/Item/DeadSunRicochet") { Volume = 0.5f };
        public static readonly SoundStyle HurtSound = new("CalamityMod/Sounds/NPCHit/ShieldHit", 3);
        public override void SetDefaults()
        {
            NPC.friendly = false;
            NPC.boss = true;
            NPC.width = NPC.height = 44;
            NPC.Size = new Vector2(150, 150);
            NPC.aiStyle = -1;
            //Values gotten from Lunatic Cultist. Subject to change.
            NPC.DR_NERD(0.10f);
            NPC.LifeMaxNERB(Main.masterMode ? 153000 : Main.expertMode ? 120000 : 80000, 144000);
            NPC.npcSlots = 5f;
            NPC.defense = 50;
            NPC.HitSound = HurtSound;
            NPC.DeathSound = SoundID.NPCDeath59;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.dontTakeDamage = true;
        }
        private float forcefieldOpacity = 0f;
        private int hitTimer = 0;
        private float forcefieldScale = 0f;

        private static readonly int MonsterDamage = StatCorrections.ScaleProjectileDamage(Main.masterMode ? 360 : CalamityWorld.death ? 280 : CalamityWorld.revenge ? 268 : Main.expertMode ? 240 : 120);
        internal static readonly int GlobDamage = StatCorrections.ScaleProjectileDamage(Main.masterMode ? 260 : CalamityWorld.death ? 220 : CalamityWorld.revenge ? 180 : Main.expertMode ? 140 : 100);
        internal static readonly int BoltDamage = StatCorrections.ScaleProjectileDamage(Main.masterMode ? 264 : CalamityWorld.death ? 188 : CalamityWorld.revenge ? 176 : Main.expertMode ? 152 : 90);
        //Will eventually be used for a special drop if the player kills every Dark Spawn the Orator summons and doesn't let any escape.
        public static bool noSpawnsEscape = true;
        public override void OnSpawn(IEntitySource source)
        {
            noSpawnsEscape = true;
        }
        private enum States
        {
            Spawning,
            DarkMonster,
            DarkSpawn,
            DarkOrbit,
            DarkSlice,
            DarkStorm,
        }
        private States AIState
        {
            get => (States)NPC.ai[0];
            set => NPC.ai[0] = (float)value;
        }
        int aiCounter = 0;
        int attackCounter = 0;
        bool dashing = false;
        Vector2 VectorToTarget = Vector2.Zero;
        public override bool PreAI()
        {

            return true;
        }
        public override void AI()
        {
            Player target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
            if (target.active == false || target.dead)
                NPC.active = false;
            if (NPC.Center.X < target.Center.X)
                NPC.direction = 1;
            else
                NPC.direction = -1;
            NPC.spriteDirection = NPC.direction;
            Lighting.AddLight(NPC.Center, new Vector3(0.32f, 0.92f, 0.71f));
            switch (AIState)
            {
                case States.Spawning:
                    target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
                    if (NPC.Center.Y < target.Center.Y - 250)
                        NPC.velocity.Y++;
                    else
                        NPC.velocity.Y--;
                    if (NPC.Center.X < target.Center.X)
                        NPC.velocity.X++;
                    else
                        NPC.velocity.X--;
                    if (NPC.velocity.Length() > 15)
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * 15;

                    if (aiCounter > 150)
                    {
                        forcefieldOpacity = Lerp(forcefieldOpacity, 1f, 0.08f);
                        forcefieldScale = Lerp(forcefieldScale, 1f, 0.08f);
                        if (forcefieldScale >= 0.99f)
                        {
                            NPC.dontTakeDamage = false;
                            forcefieldOpacity = 1f;
                            forcefieldScale = 1f;
                        }
                    }

                    if (aiCounter == 240)
                    {
                        aiCounter = -30;
                        SoundEngine.PlaySound(DashWarn);
                        attackCounter = 0;
                        NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * -5;
                        AIState = States.DarkSlice;
                        return;
                    }
                    break;
                case States.DarkMonster:
                    if (aiCounter == 1)
                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, Vector2.Zero * 20, ModContent.ProjectileType<DarkMonster>(), MonsterDamage, 0f);
                    VectorToTarget = target.Center - NPC.Center;
                    if (target.velocity.Length() < 1)
                    {
                        if (VectorToTarget.Length() < 300)
                            NPC.velocity -= VectorToTarget.SafeNormalize(Vector2.Zero);
                        else
                            NPC.velocity += VectorToTarget.SafeNormalize(Vector2.Zero);
                        if (NPC.velocity.Length() > 5)
                            NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * 5;
                    }
                    else
                    {
                        NPC.velocity = (target.Center + target.velocity * 70 - NPC.position) / 20;
                        VectorToTarget = target.Center - NPC.Center;
                        if (VectorToTarget.Length() < 200)
                        {
                            if (NPC.Center.Y > target.Center.Y)
                                NPC.velocity.Y += 10;
                            else
                                NPC.velocity.Y -= 10;

                            if (NPC.Center.X > target.Center.X)
                                NPC.velocity.X += 10;
                            else
                                NPC.velocity.X -= 10;
                        }
                    }
                    VectorToTarget = target.Center - NPC.Center;
                    int AttackFrequency = CalamityWorld.death ? 30 : CalamityWorld.revenge ? 34 : Main.expertMode ? 36 : 40;
                    if (aiCounter % AttackFrequency == 0)
                    {                       
                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, VectorToTarget.SafeNormalize(Vector2.UnitX) * 10, ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 0, 0.5f);
                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, VectorToTarget.RotatedBy(Pi / 2f).SafeNormalize(Vector2.UnitX) * 10, ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 0, 0.5f);
                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, VectorToTarget.RotatedBy(Pi / -2f).SafeNormalize(Vector2.UnitX) * 10, ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 0, 0.5f);
                        Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 0, 0, 0.5f);
                    }
                    if (aiCounter % 90 == 0)
                    {
                        Vector2 MyVector = VectorToTarget.RotatedBy(Pi / 2);
                        for (int i = 0; i < 18; i++)
                        {
                            Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, MyVector.SafeNormalize(Vector2.UnitX) * 20, ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 0, 1f);
                            MyVector = MyVector.RotatedBy(Pi / 18);
                        }
                    }
                    if (aiCounter == 900)
                    {
                        aiCounter = 0;
                        AIState = States.DarkSpawn;
                        Main.projectile[FindFirstProjectile(ModContent.ProjectileType<DarkMonster>())].ai[0] = 1;
                        return;
                    }
                    break;
                case States.DarkSpawn:
                    
                    #region Movement
                    target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];                   

                    if (NPC.Center.Y < target.Center.Y - 250)
                        NPC.velocity.Y++;
                    else
                        NPC.velocity.Y--;
                    if (NPC.Center.X < target.Center.X)
                        NPC.velocity.X++;
                    else
                        NPC.velocity.X--;
                    if (NPC.velocity.Length() > 15)
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * 15;
                    #endregion

                    const int EndTime = 1500;
                    if (aiCounter == 0)
                        attackCounter = 0;

                    if (aiCounter % 15 == 0 && attackCounter < 8 && aiCounter < 150)
                    {
                        NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<DarkSpawn>());
                        attackCounter++;
                    }
                    
                    if(aiCounter > 150)
                    {
                        attackCounter = 0;
                        foreach(NPC spawn in Main.npc.Where(n => n.type == ModContent.NPCType<DarkSpawn>() && n.active))
                        {
                            if(spawn.ModNPC is DarkSpawn darkSpawn && darkSpawn.CurrentAI != DarkSpawn.AIState.OnBoss)
                                attackCounter++;
                        }
                        if(attackCounter < 2 && aiCounter < EndTime)
                            foreach (NPC spawn in Main.npc.Where(n => n.type == ModContent.NPCType<DarkSpawn>() && n.active))
                            {
                                if (attackCounter >= 2)
                                    break;
                                if (spawn.ModNPC is DarkSpawn darkSpawn && darkSpawn.CurrentAI == DarkSpawn.AIState.OnBoss)
                                {
                                    Vector2 ToTarget = (target.Center - spawn.Center);
                                    spawn.velocity = ToTarget.SafeNormalize(Vector2.Zero) * -10;
                                    darkSpawn.CurrentAI = DarkSpawn.AIState.Dashing;
                                    spawn.rotation = ToTarget.ToRotation() + Pi;
                                    attackCounter++;
                                }
                            }
                        if (aiCounter > EndTime && NPC.AnyNPCs(ModContent.NPCType<DarkSpawn>()))
                        {
                            foreach (NPC spawn in Main.npc.Where(n => n.type == ModContent.NPCType<DarkSpawn>() && n.active))
                            {
                                if (spawn.ModNPC is DarkSpawn darkSpawn)
                                    darkSpawn.CurrentAI = DarkSpawn.AIState.Sacrifice;
                            }
                        }
                    }
                    
                    if (!NPC.AnyNPCs(ModContent.NPCType<DarkSpawn>()) && aiCounter < EndTime)
                        aiCounter = EndTime;                    
                    if (aiCounter >= EndTime + 90)
                    {
                        aiCounter = -30;
                        SoundEngine.PlaySound(DashWarn);
                        attackCounter = 0;
                        NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * -5;
                        AIState = States.DarkSlice;
                        return;
                    }
                    break;
                case States.DarkOrbit:                  
                    if (aiCounter >= 0)
                    {
                        target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
                        if (aiCounter == 0)
                        {
                            NPC.position.X = target.position.X;
                            NPC.position.Y = target.position.Y - 400;
                            NPC.velocity = Vector2.Zero;
                            NPC.ai[3] = Main.rand.Next(500, 600);
                        }
                        if (aiCounter < NPC.ai[3] - 45)
                        {
                            VectorToTarget = target.Center - NPC.Center;
                            NPC.velocity.X = (float)(20 * Math.Cos((double)aiCounter / 25));
                            NPC.velocity.Y = (float)(20 * Math.Sin((double)aiCounter / 25));
                            NPC.position += target.velocity;
                            if (aiCounter % 10 == 0 && aiCounter > 30)
                            {                               
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, VectorToTarget.SafeNormalize(Vector2.UnitX) * 20, ModContent.ProjectileType<DarkBolt>(), BoltDamage, 0);
                            }
                            
                            if(aiCounter % 120 == 0 && aiCounter > 30)
                            {
                                for(int i = 1; i < 6; i++)
                                {
                                    Projectile proj = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, NPC.velocity.RotatedBy(PiOver2).SafeNormalize(Vector2.UnitX) * (5 * i), ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 0, 0.5f);
                                    proj.timeLeft = (int)(proj.timeLeft / 1.5f);

                                    proj = Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, NPC.velocity.RotatedBy(PiOver2).SafeNormalize(Vector2.UnitX) * (5 * -i), ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 0, 0.5f);
                                    proj.timeLeft = (int)(proj.timeLeft / 1.5f);
                                }
                            }
                            
                            if (Main.expertMode && aiCounter < NPC.ai[3] - 90)
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, VectorToTarget.SafeNormalize(Vector2.UnitX) * -15, ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 0, 1f);
                        }
                        else if(aiCounter < NPC.ai[3])
                        {
                            if (aiCounter == NPC.ai[3] - 30)
                            {
                                SoundEngine.PlaySound(DashWarn);
                                NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * -4;
                            }
                        }
                        else
                        {
                            if (aiCounter == NPC.ai[3])
                            {
                                VectorToTarget = NPC.velocity.SafeNormalize(Vector2.UnitX) * -50;
                                SoundEngine.PlaySound(Dash);
                                //values gotten from Astrum Deus' contact damage. Subject to change.
                                NPC.damage = StatCorrections.ScaleContactDamage(Main.masterMode ? 360 : CalamityWorld.death ? 280 : CalamityWorld.revenge ? 268 : Main.expertMode ? 240 : 120);
                            }
                            NPC.velocity = VectorToTarget;
                            VectorToTarget -= VectorToTarget.SafeNormalize(Vector2.UnitX);

                            #region Dash Projectiles
                            if (aiCounter % 10 == 0 && Main.expertMode)
                            {
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, VectorToTarget.RotatedBy(Pi / -2f).SafeNormalize(Vector2.UnitX) * 20, ModContent.ProjectileType<DarkBolt>(), BoltDamage, 0f, -1, 0, -20);
                                if(CalamityWorld.revenge)
                                    Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * 20, ModContent.ProjectileType<DarkBolt>(), BoltDamage, 0f, -1, 0, 20);
                            }
                            else if (aiCounter % 5 == 0 && Main.expertMode)
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, VectorToTarget.RotatedBy(Pi / 2f).SafeNormalize(Vector2.UnitX) * 20, ModContent.ProjectileType<DarkBolt>(), BoltDamage, 0f, -1, 0, -20);
                            #endregion

                            if (VectorToTarget.Length() <= 1)
                            {
                                NPC.damage = 0;
                                if (++attackCounter == 3 || !CalamityWorld.death)
                                {
                                    aiCounter = 0;
                                    AIState = States.DarkMonster;
                                }
                                else
                                    aiCounter = (int)NPC.ai[3];
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (NPC.Center.Y < target.Center.Y - 250)
                            NPC.velocity.Y++;
                        else
                            NPC.velocity.Y--;
                        if (NPC.Center.X < target.Center.X)
                            NPC.velocity.X++;
                        else
                            NPC.velocity.X--;
                        if (NPC.velocity.Length() > 15)
                            NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * 15;
                    }
                    break;
                case States.DarkSlice:
                    if (aiCounter == 0)
                    {
                        SoundEngine.PlaySound(Dash);
                        VectorToTarget = NPC.velocity.SafeNormalize(Vector2.UnitX) * -60;                       
                        //values gotten from Astrum Deus' contact damage. Subject to change.
                        NPC.damage = StatCorrections.ScaleContactDamage(Main.masterMode ? 360 : CalamityWorld.death ? 280 : CalamityWorld.revenge ? 268 : Main.expertMode ? 240 : 120);
                    }
                    if (aiCounter > 0)
                    {
                        NPC.velocity = VectorToTarget;
                        VectorToTarget -= VectorToTarget.SafeNormalize(Vector2.UnitX);

                        #region Dash Projectiles
                        if((CalamityWorld.death && aiCounter % 5 == 0) || aiCounter % 10 == 0)
                            Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 0, 1f);
                        if (aiCounter % 10 == 0 && Main.expertMode)
                        {
                            Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, VectorToTarget.RotatedBy(Pi / -2f).SafeNormalize(Vector2.UnitX) * 20, ModContent.ProjectileType<DarkBolt>(), BoltDamage, 0f, -1, 0, -20);
                            if (CalamityWorld.revenge)
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * 20, ModContent.ProjectileType<DarkBolt>(), BoltDamage, 0f, -1, 0, 20);
                        }
                        else if (aiCounter % 5 == 0)
                        {
                            if (Main.expertMode)
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, VectorToTarget.RotatedBy(Pi / 2f).SafeNormalize(Vector2.UnitX) * 20, ModContent.ProjectileType<DarkBolt>(), BoltDamage, 0f, -1, 0, -20);
                        }
                        #endregion

                        if (VectorToTarget.Length() <= 1)
                        {
                            NPC.damage = 0;
                            aiCounter = -30;
                            if (++attackCounter == 3)
                            {
                                aiCounter = 0;
                                AIState = States.DarkStorm;
                            }
                            else
                            {
                                SoundEngine.PlaySound(DashWarn);
                                NPC.velocity = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * -5;
                            }
                            return;
                        }
                    }
                    break;
                case States.DarkStorm:
                    AttackFrequency = CalamityWorld.death ? 10 : CalamityWorld.revenge ? 12 : Main.expertMode ? 14 : 16;
                    target = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
                    if (NPC.Center.Y < target.Center.Y - 250)
                        NPC.velocity.Y++;
                    else
                        NPC.velocity.Y--;
                    if (NPC.Center.X < target.Center.X)
                        NPC.velocity.X++;
                    else
                        NPC.velocity.X--;
                    if (NPC.velocity.Length() > 15)
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * 15;
                    if(aiCounter > 120 && NPC.Center.Y < target.Center.Y - 50)
                    {
                        if (aiCounter % 5 == 0)
                        {
                            //The Anti-Cheesers
                            Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), new Vector2(target.Center.X - 400, target.Center.Y - 600), new Vector2(Main.rand.NextFloat(-3f, -1), 0), ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 1, 1f);
                            Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), new Vector2(target.Center.X + 400, target.Center.Y - 600), new Vector2(Main.rand.NextFloat(1, 3f), 0), ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 1, 1f);  
                        }
                        if (aiCounter % AttackFrequency == 0)
                        {
                            Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, new Vector2((float)(10 * Math.Sin(aiCounter)), -10), ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 1, 0.5f);
                            Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, new Vector2((float)(-10 * Math.Sin(aiCounter)), -10), ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 1, 0.5f);
                            if (CalamityWorld.revenge && aiCounter % 20 == 0)
                                Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), NPC.Center, new Vector2(0, -10), ModContent.ProjectileType<DarkGlob>(), GlobDamage, 0f, -1, 1, 0.5f);
                        }

                    }
                    if(aiCounter > 720)
                    {
                        aiCounter = -60;
                        attackCounter = 0;
                        AIState = States.DarkOrbit;
                    }
                    break;
            }
            aiCounter++;
            if (hitTimer > 0)
                hitTimer--;
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                hitTimer = 35;
                NPC.netUpdate = true;
            }
        }
        public override void OnKill()
        {
            for (int i = 0; i <= 50; i++)
            {
                int dustStyle = Main.rand.NextBool() ? 66 : 263;
                Dust dust = Dust.NewDustPerfect(NPC.Center, Main.rand.NextBool(3) ? 191 : dustStyle);
                dust.scale = Main.rand.NextFloat(1.5f, 2.3f);
                dust.velocity = Main.rand.NextVector2Circular(10f, 10f);
                dust.noGravity = true;
                dust.color = dust.type == dustStyle ? Color.LightGreen : default;
            }
            NPC.downedAncientCultist = true;
            DownedNPCSystem.downedOrator = true;
            WorldGen.TriggerLunarApocalypse();
        }
        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.GreaterHealingPotion;
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemID.LunarCraftingStation);

            //Boss Bag
            npcLoot.Add(ItemDropRule.BossBag(ItemID.CultistBossBag));

            //Normal Only
            var normalOnly = npcLoot.DefineNormalOnlyDropSet();
            {
                normalOnly.Add(ItemID.BossMaskCultist, 7);
            }

            // Test Drop for not letting Orator heal
            npcLoot.DefineConditionalDropSet(WindfallConditions.OratorNeverHeal).Add(ModContent.ItemType<TomeofFates>());

            // Trophy
            npcLoot.Add(ItemID.AncientCultistTrophy, 10);

            // Relic
            npcLoot.DefineConditionalDropSet(DropHelper.RevAndMaster).Add(ItemID.LunaticCultistMasterTrophy);

            //Lore
            npcLoot.AddConditionalPerPlayer(() => !DownedNPCSystem.downedOrator, ModContent.ItemType<OraLore>(), desc: DropHelper.FirstKillText);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            
            Texture2D texture = TextureAssets.Npc[NPC.type].Value;
            Vector2 halfSizeTexture = new(TextureAssets.Npc[NPC.type].Value.Width / 2, TextureAssets.Npc[NPC.type].Value.Height / Main.npcFrameCount[NPC.type] / 2);
            Vector2 drawPosition = new Vector2(NPC.Center.X, NPC.Center.Y) - screenPos;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.spriteDirection == 1)
                spriteEffects = SpriteEffects.FlipHorizontally;
            spriteBatch.Draw(texture, drawPosition, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, halfSizeTexture, NPC.scale, spriteEffects, 0f);
            DrawForcefield(spriteBatch);
            return false;
        }

        public void DrawForcefield(SpriteBatch spriteBatch)
        {
            spriteBatch.EnterShaderRegion();
            float intensity = 35f / 35f;

            if (NPC.dontTakeDamage)
                intensity = 0.75f + Math.Abs((float)Math.Cos(Main.GlobalTimeWrappedHourly * 1.7f)) * 0.1f;

            float lifeRatio = NPC.life / (float)NPC.lifeMax;
            float flickerPower = 0f;
            if (lifeRatio < 0.6f)
                flickerPower += 0.1f;
            if (lifeRatio < 0.3f)
                flickerPower += 0.15f;
            if (lifeRatio < 0.1f)
                flickerPower += 0.2f;
            if (lifeRatio < 0.05f)
                flickerPower += 0.1f;
            float opacity = forcefieldOpacity;
            opacity *= Lerp(1f, Max(1f - flickerPower, 0.56f), (float)Math.Pow(Math.Cos(Main.GlobalTimeWrappedHourly * Lerp(3f, 5f, flickerPower)), 24D));

            if (!NPC.dontTakeDamage && dashing)
                intensity = 1.1f;

            intensity *= 0.75f;
            opacity *= 0.75f;

            Texture2D forcefieldTexture = ModContent.Request<Texture2D>("CalamityMod/NPCs/SupremeCalamitas/ForcefieldTexture").Value;
            GameShaders.Misc["CalamityMod:SupremeShield"].UseImage1("Images/Misc/Perlin");

            Color forcefieldColor = Color.Black;
            Color secondaryForcefieldColor = Color.PaleGreen;

            if (!NPC.dontTakeDamage && dashing)
            {
                forcefieldColor *= 0.25f;
                secondaryForcefieldColor = Color.Lerp(secondaryForcefieldColor, Color.Black, 0.7f);
            }

            forcefieldColor *= opacity;
            secondaryForcefieldColor *= opacity;

            GameShaders.Misc["CalamityMod:SupremeShield"].UseSecondaryColor(secondaryForcefieldColor);
            GameShaders.Misc["CalamityMod:SupremeShield"].UseColor(forcefieldColor);
            GameShaders.Misc["CalamityMod:SupremeShield"].UseSaturation(intensity);
            GameShaders.Misc["CalamityMod:SupremeShield"].UseOpacity(opacity);
            GameShaders.Misc["CalamityMod:SupremeShield"].Apply();

            spriteBatch.Draw(forcefieldTexture, NPC.Center - Main.screenPosition, null, Color.White * opacity, 0f, forcefieldTexture.Size() * 0.5f, forcefieldScale * 3f, SpriteEffects.None, 0f);

            spriteBatch.ExitShaderRegion();
        }
    }
}
