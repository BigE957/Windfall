using Windfall.Content.Projectiles.GodlyAbilities;
using Windfall.Content.Projectiles.Props;

namespace Windfall.Content.Projectiles.Enemies;
public class AstralEnergy : ModProjectile, ILocalizedModType
{
    public override string Texture => "Terraria/Images/Projectile_873";
    public new static string LocalizationCategory => "Projectiles.Enemies";
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Type] = 2;
        ProjectileID.Sets.TrailCacheLength[Type] = 20;
    }
    public override void SetDefaults()
    {
        Projectile.damage = 0;
        Projectile.friendly = false;
        Projectile.height = Projectile.width = 48;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = true;
        Projectile.timeLeft = 720;
    }

    private ref float Time => ref Projectile.ai[0];

    public override void AI()
    {
        if (!Main.npc.Any(n => n.active && n.type == ModContent.NPCType<SelenicSiphon>() && n.As<SelenicSiphon>().EventActive))
        {
            for (int i = 0; i < 6; i++)
            {
                Color color = Main.rand.NextBool() ? Color.Cyan * 1.5f : Color.OrangeRed;
                Particle particle = new SparkParticle(Projectile.Center, Main.rand.NextVector2Circular(4f, 4f), false, 32, 1f, color);
                GeneralParticleHandler.SpawnParticle(particle);
            }
            Particle pulse = new PulseRing(Projectile.Center, Vector2.Zero, Main.rand.NextBool() ? Color.Cyan * 1.5f : Color.OrangeRed, 0f, 0.35f, 16);
            GeneralParticleHandler.SpawnParticle(pulse);
            Projectile.active = false;
            return;
        }

        if (Projectile.velocity.LengthSquared() < 25)
        {
            foreach (Player p in Main.ActivePlayers)
            {
                if (p.dead)
                    continue;
                if (Projectile.Hitbox.Intersects(p.Hitbox) && Projectile.velocity.LengthSquared() < 144)
                {
                    SoundEngine.PlaySound(SoundID.NPCHit1, Projectile.Center);
                    Projectile.velocity = (Projectile.Center - p.Center).SafeNormalize(Vector2.Zero) * 8;
                }
            }
            foreach (Projectile proj in Main.projectile.Where(p => p != null && p.active && p.friendly && p.owner == Projectile.owner && !p.minion && p.whoAmI != Projectile.whoAmI))
            {
                if (Projectile.Hitbox.Intersects(proj.Hitbox))
                {
                    SoundEngine.PlaySound(SoundID.NPCHit1, Projectile.Center);
                    Projectile.velocity = proj.velocity.SafeNormalize(Vector2.Zero) * (2 + proj.knockBack);
                    break;
                }
            }
        }

        if(Projectile.velocity != Vector2.Zero)
        {
            if (Projectile.velocity.LengthSquared() < 1)
                Projectile.velocity = Vector2.Zero;
            else
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + PiOver2;
                Projectile.velocity *= 0.975f;
            }
        }

        if(Projectile.timeLeft <= 120 && Projectile.timeLeft % 30 == 0)
        {
            Particle pulse = new PulseRing(Projectile.Center, Vector2.Zero * 0f, Main.rand.NextBool() ? Color.Cyan * 1.5f : Color.OrangeRed, 0.05f, (Projectile.friendly ? 0.9f : 0.5f), 24);
            GeneralParticleHandler.SpawnParticle(pulse);
        }

        Time++;
    }

    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 12; i++)
        {
            Color color = Main.rand.NextBool() ? Color.Cyan * 1.5f : Color.OrangeRed;
            Particle particle = new SparkParticle(Projectile.Center, Main.rand.NextVector2Circular(4f, 4f) * (Projectile.friendly ? 2 : 1), false, 48, 1f, color);
            GeneralParticleHandler.SpawnParticle(particle);
        }
        Particle pulse = new PulseRing(Projectile.Center, Vector2.Zero * 0f, Main.rand.NextBool() ? Color.Cyan * 1.5f : Color.OrangeRed, 0.05f, (Projectile.friendly ? 0.9f : 0.5f), 24);
        GeneralParticleHandler.SpawnParticle(pulse);
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        if (Projectile.velocity.X != oldVelocity.X)
            Projectile.velocity.X = -oldVelocity.X;
        if (Projectile.velocity.Y != oldVelocity.Y)
            Projectile.velocity.Y = -oldVelocity.Y;
        return false;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

        Vector2 positionToCenterOffset = Projectile.Size * 0.5f;
        for (int i = 0; i < Projectile.oldPos.Length; i++)
        {
            float interpolent = (Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length;
            Main.EntitySpriteDraw(tex, (Projectile.oldPos[i] + positionToCenterOffset) - Main.screenPosition, null, colorFunc(interpolent), Projectile.oldRot[i], tex.Size() * 0.5f, interpolent, 0, 0);
        }
        Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, colorFunc(1) * 1.5f, Projectile.rotation, tex.Size() * 0.5f, 1.25f, 0, 0);
        return false;
    }

    private static Color colorFunc(float i) => Color.Lerp(Color.Cyan * 1.5f, Color.OrangeRed, (float)Math.Sin((Main.GlobalTimeWrappedHourly + i) * 4f) / 2f + 0.5f) * i;

    private static Color pausedColorFunc(float f, float i) => Color.Lerp(Color.Cyan * 1.5f, Color.OrangeRed, f) * i;

    private static float widthFunc(float i) => (1 - i) * 8;
}
