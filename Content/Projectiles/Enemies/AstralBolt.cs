using CalamityMod.NPCs.Astral;
using Windfall.Common.Systems;
using Windfall.Content.Projectiles.Props;

namespace Windfall.Content.Projectiles.Enemies;
public class AstralBolt : ModProjectile, ILocalizedModType
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
        Projectile.damage = 100;
        Projectile.friendly = false;
        Projectile.height = Projectile.width = 32;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 180;
    }

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

        Projectile.rotation = Projectile.velocity.ToRotation() + PiOver2;

        Projectile.velocity = Projectile.velocity.RotateTowards((Main.player[Player.FindClosest(Projectile.position, Projectile.width, Projectile.height)].Center - Projectile.Center).ToRotation(), 0.1f);

        if (Projectile.timeLeft <= 60)
            Projectile.velocity *= 0.97f;
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

    public override bool PreDraw(ref Color drawColor)
    {
        Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

        Vector2 positionToCenterOffset = Projectile.Size * 0.5f;
        for (int i = 0; i < Projectile.oldPos.Length; i++)
        {
            float interpolent = (Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length;
            Main.EntitySpriteDraw(tex, (Projectile.oldPos[i] + positionToCenterOffset) - Main.screenPosition, null, colorFunc(interpolent), Projectile.oldRot[i], tex.Size() * 0.5f, interpolent, 0, 0);
            if (i != Projectile.oldPos.Length - 1)
            {
                Vector2 midpoint = new((Projectile.oldPos[i].X + Projectile.oldPos[i + 1].X) / 2, (Projectile.oldPos[i].Y + Projectile.oldPos[i + 1].Y) / 2);
                float midangle = (Projectile.oldRot[i] + Projectile.oldRot[i + 1]) / 2f;
                float midInterp = (Projectile.oldPos.Length - i - 0.5f) / Projectile.oldPos.Length;
                Main.EntitySpriteDraw(tex, (midpoint + positionToCenterOffset) - Main.screenPosition, null, colorFunc(midInterp), midangle, tex.Size() * 0.5f, midInterp * 0.5f, 0, 0);
            }
        }
        Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, colorFunc(1) * 1.5f, Projectile.rotation, tex.Size() * 0.5f, 0.75f, 0, 0);
        return false;
    }
    private static Color colorFunc(float i) => Color.Lerp(Color.Cyan * 1.5f, Color.OrangeRed, (float)Math.Sin((Main.GlobalTimeWrappedHourly + i) * 4f) / 2f + 0.5f) * i;

    private static Color pausedColorFunc(float f, float i) => Color.Lerp(Color.Cyan * 1.5f, Color.OrangeRed, f) * i;

    private static float widthFunc(float i) => (1 - i) * 8;
}
