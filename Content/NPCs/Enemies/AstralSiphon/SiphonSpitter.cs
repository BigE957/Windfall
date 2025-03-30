using CalamityMod;
using CalamityMod.Dusts;
using CalamityMod.NPCs;
using CalamityMod.NPCs.Astral;
using CalamityMod.Particles;
using CalamityMod.Sounds;
using Terraria;
using Windfall.Content.Projectiles.Enemies;
using Windfall.Content.Projectiles.Props;

namespace Windfall.Content.NPCs.Enemies.AstralSiphon;
public class SiphonSpitter : ModNPC
{
    public override string Texture => "CalamityMod/NPCs/Astral/SightseerSpitter";
    public static Asset<Texture2D> glowmask;
    public override void SetStaticDefaults()
    {
        this.HideBestiaryEntry();
        Main.npcFrameCount[NPC.type] = 4;
        if (!Main.dedServ)
            glowmask = ModContent.Request<Texture2D>("CalamityMod/NPCs/Astral/SightseerSpitterGlow", AssetRequestMode.AsyncLoad);
    }

    public override void SetDefaults()
    {
        NPC.aiStyle = -1;
        NPC.width = 64;
        NPC.height = 56;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.DeathSound = CommonCalamitySounds.AstralNPCDeathSound;

        NPC.damage = 85;
        NPC.defense = 20;
        NPC.knockBackResist = 0.7f;
        NPC.lifeMax = 430;
        NPC.DR_NERD(0.15f);
        Banner = ModContent.NPCType<SightseerSpitter>();

        NPC.HitSound = SoundID.NPCHit12;
        NPC.DeathSound = SoundID.NPCDeath18;
        NPC.Calamity().VulnerableToHeat = true;
        NPC.Calamity().VulnerableToSickness = false;

        CalamityGlobalNPC.AdjustExpertModeStatScaling(NPC);
        CalamityGlobalNPC.AdjustMasterModeStatScaling(NPC);
    }

    int aiCounter = 0;
    int attackCounter = -1;
    Player Target = null;

    public override void AI()
    {
        if(!Main.npc.Any(n => n.active && n.type == ModContent.NPCType<SelenicSiphon>() && n.As<SelenicSiphon>().EventActive))
        {
            NPC.Transform(ModContent.NPCType<SightseerSpitter>());
            return;
        }

        Target ??= Main.player[Player.FindClosest(NPC.position, NPC.width, NPC.height)];

        Vector2 homeInVector = Target.Center - NPC.Center;
        float targetDist = homeInVector.Length();
        homeInVector.Normalize();
        if (targetDist > 450f)
        {
            float velocity = 10f;
            NPC.velocity = (NPC.velocity * 40f + homeInVector * velocity) / 41f;
        }
        else
        {
            if (targetDist < 400f)
            {
                float velocity = -10f;
                NPC.velocity = (NPC.velocity * 40f + homeInVector * velocity) / 41f;
            }
            else
            {
                NPC.velocity *= 0.9f;
            }
        }
        if (NPC.Center.Y > Target.Center.Y)
            NPC.velocity.Y = -4;
        NPC.rotation = (Target.Center - NPC.Center).ToRotation() + Pi;
        NPC.Center += (NPC.rotation + PiOver2).ToRotationVector2() * (float)Math.Cos((aiCounter + attackCounter) / 20f) * (4f / (NPC.velocity.Length() + 1));

        if (attackCounter == -1 && aiCounter % 180 == 0 && Main.rand.NextBool(3))
        {
            attackCounter = 0;
            aiCounter++;
        }

        if (attackCounter != -1)
        {
            Vector2 projectilePos = NPC.Center + (NPC.rotation - Pi).ToRotationVector2() * 56;
            if (attackCounter <= 75)
            {
                float lerpValue = attackCounter / 90f;
                if (attackCounter % 5 == 0 && Main.rand.NextBool(lerpValue))
                {
                    Vector2 spawnPos = projectilePos + Main.rand.NextVector2CircularEdge(4f, 4f);
                    Particle particle = new AltSparkParticle(spawnPos, (projectilePos - spawnPos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(4, 2), false, 200, Main.rand.NextFloat(0.5f, 1f), Main.rand.NextBool() ? Color.Cyan * 1.5f : Color.OrangeRed);
                    GeneralParticleHandler.SpawnParticle(particle);
                }
                if (attackCounter % 30 == 0)
                {
                    Particle particle = new PulseRing(projectilePos, Vector2.Zero, attackCounter % 60 == 0 ? Color.Cyan * 1.5f : Color.OrangeRed, 0f, 0.5f, 30);
                    GeneralParticleHandler.SpawnParticle(particle);
                }
            }
            else
            {
                if (attackCounter == 91)
                {
                    Projectile.NewProjectileDirect(Projectile.GetSource_NaturalSpawn(), projectilePos, (NPC.rotation + Pi).ToRotationVector2() * 6f, ModContent.ProjectileType<AstralBolt>(), 100, 0f);
                }
                if (attackCounter >= 100)
                    attackCounter = -2;
            }
            attackCounter++;
        }
        else
            aiCounter++;

    }

    public override void OnKill()
    {
        Projectile.NewProjectile(Projectile.GetSource_NaturalSpawn(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<AstralEnergy>(), 0, 0f);
    }
    public override void HitEffect(NPC.HitInfo hit)
    {
        if (NPC.soundDelay == 0)
        {
            NPC.soundDelay = 15;
            SoundEngine.PlaySound(CommonCalamitySounds.AstralNPCHitSound, NPC.Center);
        }

        CalamityGlobalNPC.DoHitDust(NPC, hit.HitDirection, Main.rand.Next(0, Math.Max(0, NPC.life)) == 0 ? 5 : ModContent.DustType<AstralEnemy>(), 1f, 4, 22);

        if (NPC.life <= 0)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 5; i++)
                {
                    float rand = Main.rand.NextFloat(-0.18f, 0.18f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.NextFloat(0f, NPC.width), Main.rand.NextFloat(0f, NPC.height)), NPC.velocity * rand, ModContent.GetInstance<CalamityMod.CalamityMod>().Find<ModGore>("SightseerSpitterGore" + i).Type);
                }
            }
        }
    }

    public override void FindFrame(int frameHeight)
    {
        NPC.frameCounter += 0.05f + NPC.velocity.Length() * 0.667f;
        if (NPC.frameCounter >= 8)
        {
            NPC.frameCounter = 0;
            NPC.frame.Y += frameHeight;
            if (NPC.frame.Y > NPC.height * 3)
            {
                NPC.frame.Y = 0;
            }
        }

        //DO DUST
        Dust d = CalamityGlobalNPC.SpawnDustOnNPC(NPC, 118, frameHeight, ModContent.DustType<AstralOrange>(), new Rectangle(70, 18, 48, 18), Vector2.Zero, 0.45f, true);
        if (d != null)
        {
            d.customData = 0.04f;
        }
    }

    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        spriteBatch.Draw(glowmask.Value, NPC.Center - screenPos + new Vector2(0, 4f), NPC.frame, Color.White * 0.75f, NPC.rotation, new Vector2(59f, 28f), NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
    }
}
