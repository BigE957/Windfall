using CalamityMod.Dusts;
using CalamityMod.NPCs.CalClone;
using Terraria.Utilities;
using Windfall.Common.Systems;
using Windfall.Common.Utilities;

namespace Windfall.Content.NPCs.WorldEvents.CalClone
{
    public class WanderingCalClone : ModNPC
    {
        /// <summary>
        /// The main focus of this NPC is to show how to make something similar to the vanilla bone merchant;
        /// which means that the NPC will act like any other town NPC but won't have a happiness button, won't appear on the minimap,
        /// and will spawn like an enemy NPC. If you want a traditional town NPC instead, see <see cref="ExamplePerson"/>.
        /// </summary>
#pragma warning disable CS0649 // Field 'WanderingCalClone.NPCProfile' is never assigned to, and will always have its default value null
        private static readonly Profiles.StackedNPCProfile NPCProfile;
#pragma warning restore CS0649 // Field 'WanderingCalClone.NPCProfile' is never assigned to, and will always have its default value null
        public override string Texture => "Windfall/Assets/NPCs/WorldEvents/WanderingCalClone";

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 6; // The amount of frames the NPC has
            NPCID.Sets.DangerDetectRange[Type] = 700; // The amount of pixels away from the center of the npc that it tries to attack enemies.
            NPCID.Sets.PrettySafe[Type] = 300;
            NPCID.Sets.AttackType[Type] = 1;
            NPCID.Sets.AttackTime[Type] = 60; // The amount of time it takes for the NPC's attack animation to be over once it starts.
            NPCID.Sets.AttackAverageChance[Type] = 30;
            NPCID.Sets.HatOffsetY[Type] = 4; // For when a party is active, the party hat spawns at a Y offset.
            NPCID.Sets.ShimmerTownTransform[NPC.type] = false; // This set says that the Town NPC has a Shimmered form. Otherwise, the Town NPC will become transparent when touching Shimmer like other enemies.

            //This sets entry is the most important part of this NPC. Since it is true, it tells the game that we want this NPC to act like a town NPC without ACTUALLY being one.
            //What that means is: the NPC will have the AI of a town NPC, will attack like a town NPC, and have a shop (or any other additional functionality if you wish) like a town NPC.
            //However, the NPC will not have their head displayed on the map, will de-spawn when no players are nearby or the world is closed, and will spawn like any other NPC.
            NPCID.Sets.ActsLikeTownNPC[Type] = true;

            // This prevents the happiness button
            NPCID.Sets.NoTownNPCHappiness[Type] = true;

            // Connects this NPC with a custom emote.
            // This makes it when the NPC is in the world, other NPCs will "talk about him".
            //NPCID.Sets.FaceEmote[Type] = ModContent.EmoteBubbleType<ExampleBoneMerchantEmote>();

            //The vanilla Bone Merchant cannot interact with doors (open or close them, specifically), but if you want your NPC to be able to interact with them despite this,
            //uncomment this line below.
            NPCID.Sets.AllowDoorInteraction[Type] = true;
        }

        public override void SetDefaults()
        {
            NPC.friendly = true; // NPC Will not attack player
            NPC.townNPC = true;
            NPC.width = 18;
            NPC.height = 40;
            NPC.aiStyle = 7;
            NPC.damage = 10;
            NPC.defense = 15;
            NPC.lifeMax = 250;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.knockBackResist = 0.5f;
        }

        //Make sure to allow your NPC to chat, since being "like a town NPC" doesn't automatically allow for chatting.
        public override bool CanChat() => true;

        public override ITownNPCProfile TownNPCProfile() => NPCProfile;
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            //If any player is underground and has an example item in their inventory, the example bone merchant will have a slight chance to spawn.
            if (spawnInfo.Player.townNPCs > 2f && !DownedBossSystem.downedCalamitasClone && NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3 && WorldSaveSystem.CloneRevealed && !Main.dayTime && !NPC.AnyNPCs(ModContent.NPCType<WanderingPotionSeller>()) && !NPC.AnyNPCs(ModContent.NPCType<WanderingCalClone>()))
                return 0.35f;
            //Else, the example bone merchant will not spawn if the above conditions are not met.
            return 0f;
        }
        public override void OnSpawn(IEntitySource source)
        {
            if (NPC.ai[0] == 1f)
            {
                NPC.aiStyle = 0;
                NPC.noGravity = true;
            }
            else
            {
                string key = "Calamitas is back...";
                Color messageColor = new(50, 125, 255);
                DisplayLocalizedText(key, messageColor);
            }
        }
        public override bool PreAI()
        {
            if (Main.dayTime)
            {
                // Here we despawn the NPC and send a message stating that the NPC has despawned
                // LegacyMisc.35 is {0) has departed!
                for (int i = 0; i < 50; i++)
                {
                    Vector2 speed = Main.rand.NextVector2Circular(0.5f, 1f);
                    Dust d = Dust.NewDustPerfect(NPC.Center, DustID.Blood, speed * 5, Scale: 1.5f);
                    d.noGravity = true;
                }
                SoundEngine.PlaySound(CalCloneTeleport, NPC.Center);
                string key = "Calamitas has left... finally.";
                Color messageColor = new(50, 125, 255);
                DisplayLocalizedText(key, messageColor);
                NPC.active = false;
                NPC.netSkip = -1;
                NPC.life = 0;
                return false;
            }
            return true;
        }
        public override string GetChat()
        {
            WeightedRandom<string> chat = new();

            // These are things that the NPC has a chance of telling you when you talk to it.
            chat.Add(GetWindfallTextValue($"Dialogue.CalPotionSeller.Standard1"));
            chat.Add(GetWindfallTextValue($"Dialogue.CalPotionSeller.Standard2"));
            chat.Add(GetWindfallTextValue($"Dialogue.CalPotionSeller.Standard3"));

            return chat; // chat is implicitly cast to a string.
        }

        public override void SetChatButtons(ref string button, ref string button2)
        {
            button = "Challenge"; 
        }
        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            if (firstButton)
            {
                NPC.aiStyle = 0;
                NPC.noGravity = true;
                Main.CloseNPCChatOrSign();
            }
        }
        public static readonly SoundStyle DashSound = new("CalamityMod/Sounds/Custom/SCalSounds/SCalDash");
        public static readonly SoundStyle Transformation = new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneBigShoot");
        public static readonly SoundStyle CalCloneTeleport = new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");

        internal int aiCounter = 59;
        int i = 20;
        int dialogueCounter = 0;
        float CalCloneHoverY;

        public override void AI()
        {
            NPC.homeless = true;
            if (NPC.aiStyle == 0)
            {
                aiCounter++;
                if (!WorldSaveSystem.CloneRevealed)
                {
                    if (aiCounter < 150)
                    {
                        ZoomSystem.SetZoomEffect(1);
                        Main.LocalPlayer.Windfall_Camera().ScreenFocusPosition = NPC.Center;
                        Main.LocalPlayer.Windfall_Camera().ScreenFocusInterpolant = 1;
                    }
                    string key = "Mods.Windfall.Dialogue.CalPotionSeller.WorldText.Initial.";
                    switch (aiCounter)
                    {
                        case (90):                                                        
                            DisplayLocalizedText(key + dialogueCounter++, Color.Orange);
                            break;
                        case (150):
                            DisplayLocalizedText(key + dialogueCounter++, Color.Orange);
                            break;
                        case (210):
                            DisplayLocalizedText(key + dialogueCounter++, Color.Orange);
                            break;
                        case (270):
                            i = 20;
                            SoundEngine.PlaySound(DashSound, NPC.Center);
                            DisplayLocalizedText(key + dialogueCounter++, Color.Orange);
                            break;
                        case (300):
                            for (int i = 0; i < 40; i++)
                            {
                                int brimDust = Dust.NewDust(new Vector2((int)NPC.Center.X - 20, (int)NPC.position.Y - 10), 100, 100, (int)CalamityDusts.Brimstone, 0f, 0f, 100, default, 2f);
                                Main.dust[brimDust].velocity *= 3f;
                                if (Main.rand.NextBool())
                                {
                                    Main.dust[brimDust].scale = 0.5f;
                                    Main.dust[brimDust].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                                }
                            }
                            for (int j = 0; j < 70; j++)
                            {
                                int brimDust2 = Dust.NewDust(new Vector2((int)NPC.Center.X - 20, (int)NPC.position.Y - 10), 100, 100, (int)CalamityDusts.Brimstone, 0f, 0f, 100, default, 3f);
                                Main.dust[brimDust2].noGravity = true;
                                Main.dust[brimDust2].velocity *= 5f;
                                brimDust2 = Dust.NewDust(new Vector2((int)NPC.Center.X - 20, (int)NPC.position.Y - 10), 100, 100, (int)CalamityDusts.Brimstone, 0f, 0f, 100, default, 2f);
                                Main.dust[brimDust2].velocity *= 2f;
                            }
                            SoundEngine.PlaySound(Transformation, NPC.Center + new Vector2(0, +10));
                            NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X - 20, (int)NPC.Center.Y + 50, ModContent.NPCType<CalamitasClone>());
                            WorldSaveSystem.CloneRevealed = true;
                            NPC.active = false;
                            break;
                    }
                    if (aiCounter >= 270)
                        if (i > 0)
                        {
                            NPC.position.Y -= i-- * 1.5f;
                            CalCloneHoverY = NPC.position.Y;
                        }
                }
                else
                {
                    string key = "Mods.Windfall.Dialogue.CalPotionSeller.WorldText.Subsequent.";
                    switch (aiCounter)
                    {
                        case (60):
                            DisplayLocalizedText(key + dialogueCounter++, Color.Orange);
                            break;
                        case 120:                           
                            DisplayLocalizedText(key + dialogueCounter++, Color.Orange);
                            break;
                        case (160):
                            i = 20;
                            SoundEngine.PlaySound(DashSound, NPC.Center);
                            DisplayLocalizedText(key + dialogueCounter++, Color.Orange);
                            break;
                        case (200):
                            for (int i = 0; i < 40; i++)
                            {
                                int brimDust = Dust.NewDust(new Vector2((int)NPC.Center.X - 20, (int)NPC.position.Y - 10), 100, 100, (int)CalamityDusts.Brimstone, 0f, 0f, 100, default, 2f);
                                Main.dust[brimDust].velocity *= 3f;
                                if (Main.rand.NextBool())
                                {
                                    Main.dust[brimDust].scale = 0.5f;
                                    Main.dust[brimDust].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                                }
                            }
                            for (int j = 0; j < 70; j++)
                            {
                                int brimDust2 = Dust.NewDust(new Vector2((int)NPC.Center.X - 20, (int)NPC.position.Y - 10), 100, 100, (int)CalamityDusts.Brimstone, 0f, 0f, 100, default, 3f);
                                Main.dust[brimDust2].noGravity = true;
                                Main.dust[brimDust2].velocity *= 5f;
                                brimDust2 = Dust.NewDust(new Vector2((int)NPC.Center.X - 20, (int)NPC.position.Y - 10), 100, 100, (int)CalamityDusts.Brimstone, 0f, 0f, 100, default, 2f);
                                Main.dust[brimDust2].velocity *= 2f;
                            }
                            SoundEngine.PlaySound(Transformation, NPC.Center + new Vector2(0, +10));
                            NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X - 20, (int)NPC.Center.Y + 50, ModContent.NPCType<CalamitasClone>());
                            NPC.active = false;
                            break;
                    }
                    if (aiCounter >= 160)
                        if (i > 0)
                        {
                            NPC.position.Y -= i-- * 1.5f;
                            CalCloneHoverY = NPC.position.Y;
                        }
                }
            }
        }
        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = 20;
            knockback = 4f;
        }

        public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
        {
            cooldown = 30;
            randExtraCooldown = 30;
        }

        public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
        {
            Mod calamity = ModLoader.GetMod("CalamityMod");
            projType = calamity.Find<ModProjectile>("SeethingDischargeBrimstoneHellblast").Type;
            attackDelay = 1;
        }

        public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
        {
            multiplier = 2f;
        }
        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 0.2f;
            NPC.frame.Y = frameHeight * ((int)NPC.frameCounter % 6);
            NPC.spriteDirection = NPC.direction;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[NPC.type].Value;
            Vector2 halfSizeTexture = new(TextureAssets.Npc[NPC.type].Value.Width / 2, TextureAssets.Npc[NPC.type].Value.Height / Main.npcFrameCount[NPC.type] / 2);
            Vector2 drawPosition = new Vector2(NPC.Center.X, NPC.Center.Y - 10) - screenPos + Vector2.UnitY * NPC.gfxOffY;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.spriteDirection == -1)
                spriteEffects = SpriteEffects.FlipHorizontally;
            spriteBatch.Draw(texture, drawPosition, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, halfSizeTexture, NPC.scale, spriteEffects, 0f);
            return false;
        }
    }
}