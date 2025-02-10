using Luminance.Core.Graphics;
using Terraria.GameContent.Bestiary;
using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.Items.Weapons.Misc;

namespace Windfall.Content.NPCs.WorldEvents.LunarCult;

public class PortalMole : ModNPC
{
    public override string Texture => "CalamityMod/ExtraTextures/SoulVortex";  
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 1;

        NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new()
        {
            Velocity = 1f,
            Direction = 1
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
    }
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.AddRange([
        new FlavorTextBestiaryInfoElement(GetWindfallTextValue($"Bestiary.{nameof(PortalMole)}")),
    ]);
    }
    public override void SetDefaults()
    {
        NPC.width = NPC.height = 75;
        NPC.damage = 0;
        NPC.defense = 100;
        NPC.Calamity().DR = 0.33f;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.lifeMax = 1200;
        NPC.knockBackResist = 0f;
        NPC.scale = 1.25f;
        NPC.HitSound = SoundID.DD2_WitherBeastAuraPulse;
        NPC.DeathSound = SoundID.DD2_EtherianPortalOpen;
        NPC.Calamity().VulnerableToElectricity = true;
        NPC.Calamity().VulnerableToWater = true;
    }
    private int aiCounter = 0;
    private int targetIndex = -1;
    private List<int> dustsIndexs = [];
    public override void OnSpawn(IEntitySource source)
    {
        NPC.scale = 0;
        dustsIndexs = [];
        if(LunarCultBaseSystem.IsRitualActivityActive())
            LunarCultBaseSystem.ActivePortals++;
        SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy with { Volume = 1f }, NPC.Center);
    }
    public override bool PreAI()
    {
        if (LunarCultBaseSystem.PortalsDowned >= LunarCultBaseSystem.RequiredPortalKills)
        {
            Despawn();
            return false;
        }
        return true;
    }
    public override void AI()
    {
        if (LunarCultBaseSystem.Active)
        {
            if (aiCounter > 20)
            {
                if (aiCounter - 20 < 180)
                {
                    if ((aiCounter - 20) % 60 == 0)
                    {
                        Vector2 spawnPosition = NPC.Center;
                        Color color = Color.Lerp(Color.LimeGreen, new(117, 255, 159), Main.rand.NextFloat(0f, 1f));
                        CalamityMod.Particles.Particle particle = new PulseRing(spawnPosition, Vector2.Zero, color, 0.25f, 0.9f, 30);
                        GeneralParticleHandler.SpawnParticle(particle);
                    }
                }
                else
                {
                    if (aiCounter - 20 == 180)
                    {
                        if (Main.npc.Any(n => n != null && n.active && n.type == ModContent.NPCType<LunarCultistDevotee>()))
                        {
                            int length = Main.npc.Where(n => n != null && n.active && n.type == ModContent.NPCType<LunarCultistDevotee>() && n.velocity.LengthSquared() == 0 && n.ai[2] == 3).Count();
                            targetIndex = Main.npc.Where(n => n != null && n.active && n.type == ModContent.NPCType<LunarCultistDevotee>() && n.velocity.LengthSquared() == 0 && n.ai[2] == 3).ElementAt(Main.rand.Next(length)).whoAmI;
                        }
                        else
                            Despawn();
                    }
                    if ((aiCounter - 20) % 30 == 0)
                    {
                        Vector2 spawnPosition = NPC.Center;
                        Color color = Color.Lerp(Color.LimeGreen, new(117, 255, 159), Main.rand.NextFloat(0f, 1f));
                        CalamityMod.Particles.Particle particle = new PulseRing(spawnPosition, Vector2.Zero, color, 1.25f, 0.1f, 100);
                        GeneralParticleHandler.SpawnParticle(particle);
                    }
                    NPC target = Main.npc[targetIndex];
                    if (target == null || !target.active)
                    {
                        Despawn();
                        return;
                    }
                    target.velocity = (NPC.Center - target.Center).SafeNormalize(Vector2.Zero) * 2;
                    if (target.Center.X > NPC.Center.X)
                        target.rotation -= 0.05f;
                    else
                        target.rotation += 0.05f;

                    int dustStyle = Main.rand.NextBool() ? 66 : 263;
                    Dust dust = Dust.NewDustPerfect(target.Center, Main.rand.NextBool(3) ? 191 : dustStyle);
                    dust.scale = Main.rand.NextFloat(1.5f, 2.3f);
                    dust.velocity = Main.rand.NextVector2Circular(10f, 10f);
                    dust.noGravity = true;
                    dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                    dustsIndexs.Add(dust.dustIndex);

                    for (int j = 0; j < dustsIndexs.Count; j++)
                    {
                        int i = dustsIndexs[j];
                        dust = Main.dust[i];
                        if (NPC.Hitbox.Contains((int)dust.position.X, (int)dust.position.Y))
                        {
                            dust.active = false;
                            dustsIndexs[j] = -1;
                        }
                        else
                            dust.velocity = (NPC.Center - dust.position).SafeNormalize(Vector2.Zero) * 12;
                    }
                    dustsIndexs.RemoveAll(i => i == -1 || Main.dust[i] == null || !Main.dust[i].active);
                }
            }
        }
        #region Visuals
        if (aiCounter < 20)
            NPC.scale = SineBumpEasing((float)aiCounter / 40f, 1);
        else
            NPC.scale = (0.5f * (NPC.life / (float)NPC.lifeMax)) + 0.5f;

        if (aiCounter > 20 && aiCounter % 10 == 0)
        {
            Vector2 spawnPosition = NPC.Center + Main.rand.NextVector2CircularEdge(77f * NPC.scale, 77f * NPC.scale);
            Color color = Color.Lerp(Color.LimeGreen, new(117, 255, 159), Main.rand.NextFloat(0f, 1f));
            CalamityMod.Particles.Particle particle = new AltSparkParticle(spawnPosition, (spawnPosition - NPC.Center).SafeNormalize(Vector2.Zero) * (Main.rand.NextFloat(-4, -2f) * NPC.scale), false, 200, Main.rand.NextFloat(0.5f, 1f) * NPC.scale, color);
            GeneralParticleHandler.SpawnParticle(particle);
        }
        Lighting.AddLight(NPC.Center, new Vector3(0.32f, 0.92f, 0.71f));
        NPC.rotation += 0.05f;
        #endregion
        aiCounter++;
    }
    private readonly int[] ValidProjectiles =
    [
        ModContent.ProjectileType<RiftWeaverStab>(),
        ModContent.ProjectileType<RiftWeaverThrow>(),
    ];
    public override bool? CanBeHitByProjectile(Projectile projectile)
    {
        return ValidProjectiles.Contains(projectile.type);
    }
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Particles/LargeBloom").Value;
        Vector2 drawPosition = NPC.Center - Main.screenPosition + (Vector2.UnitY * NPC.gfxOffY);
        Vector2 origin = texture.Size() * 0.5f;
        spriteBatch.Draw(texture, drawPosition, null, NPC.GetAlpha(Color.Black), 0f, origin, NPC.scale / 1.8f, SpriteEffects.None, 0f);

        spriteBatch.UseBlendState(BlendState.AlphaBlend);
        texture = ModContent.Request<Texture2D>("Terraria/Images/Projectile_656").Value;
        origin = texture.Size() * 0.5f;
        spriteBatch.Draw(texture, drawPosition, null, NPC.GetAlpha(new(117, 255, 159)), -NPC.rotation / 2.25f, origin, NPC.scale * (2.25f + (0.5f * ((float)Math.Sin(aiCounter / -20f) / 2f + 0.5f))), SpriteEffects.None, 0f);
        spriteBatch.Draw(texture, drawPosition, null, NPC.GetAlpha(new(117, 255, 159)), NPC.rotation / 1.75f, origin, NPC.scale * (2.25f + (0.5f * ((float)Math.Sin(aiCounter / 20f) / 2f + 0.5f))), SpriteEffects.None, 0f);

        texture = ModContent.Request<Texture2D>("CalamityMod/Particles/LargeBloom").Value;
        drawPosition = NPC.Center - Main.screenPosition + (Vector2.UnitY * NPC.gfxOffY);
        origin = texture.Size() * 0.5f;
        spriteBatch.Draw(texture, drawPosition, null, NPC.GetAlpha(Color.Black), 0f, origin, NPC.scale / 3f, SpriteEffects.None, 0f);

        spriteBatch.UseBlendState(BlendState.Additive);
        texture = TextureAssets.Npc[NPC.type].Value;
        origin = texture.Size() * 0.5f;
        spriteBatch.Draw(texture, drawPosition, null, new(117, 255, 159, 150), NPC.rotation, origin, NPC.scale / 3.5f, SpriteEffects.None, 0f);
        spriteBatch.Draw(texture, drawPosition, null, new(Color.LimeGreen.R, Color.LimeGreen.G, Color.LimeGreen.B, 225), -NPC.rotation, origin, NPC.scale / 3.75f, SpriteEffects.None, 0f);
        spriteBatch.Draw(texture, drawPosition, null, new(Color.Green.R, Color.Green.G, Color.Green.B, 200), NPC.rotation, origin, NPC.scale / 4.5f, SpriteEffects.None, 0f);
        spriteBatch.Draw(texture, drawPosition, null, new(Color.Teal.R, Color.Teal.G, Color.Teal.B, 175), -NPC.rotation, origin, NPC.scale / 5f, SpriteEffects.None, 0f);

        spriteBatch.UseBlendState(BlendState.AlphaBlend);
        return false;
    }
    public override bool CheckDead()
    {
        Despawn();

        LunarCultBaseSystem.PortalsDowned++;
        
        if (LunarCultBaseSystem.PortalsDowned >= LunarCultBaseSystem.RequiredPortalKills)
            LunarCultBaseSystem.ResetTimer();

        return true;
    }
    private void Despawn()
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
        ScreenShakeSystem.StartShake(2f);

        LunarCultBaseSystem.ActivePortals--;

        NPC.active = false;
    }
}
