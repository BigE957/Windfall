using Windfall.Content.Projectiles.GodlyAbilities;

namespace Windfall.Content.Projectiles.Enemies;
public class AstralEnergy : ModProjectile, ILocalizedModType
{
    public override string Texture => "Terraria/Images/Projectile_873";
    public new static string LocalizationCategory => "Projectiles.Enemies";
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Type] = 0;
        ProjectileID.Sets.TrailCacheLength[Type] = 20;
    }
    public override void SetDefaults()
    {
        Projectile.damage = 0;
        Projectile.friendly = false;
        Projectile.height = Projectile.width = 48;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = true;
    }

    public override void AI()
    {
        if (Projectile.velocity == Vector2.Zero)
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
            foreach (Projectile proj in Main.projectile.Where(p => p != null && p.active && p.friendly && p.owner == Projectile.owner && !p.minion && p.whoAmI != Projectile.whoAmI && p.type != ModContent.ProjectileType<GodlyCrimulanMuck>() && p.type != ModContent.ProjectileType<GodlyCrimulanMuck>()))
            {
                if (Projectile.Hitbox.Intersects(proj.Hitbox) && proj.Windfall().hasHitMuck == false)
                {
                    SoundEngine.PlaySound(SoundID.NPCHit1, Projectile.Center);
                    Projectile.velocity = proj.velocity.SafeNormalize(Vector2.Zero) * (2 + proj.knockBack);
                    proj.Windfall().hasHitMuck = true;
                    break;
                }
            }
        }
        else
        {
            if (Projectile.velocity.LengthSquared() < 1)
                Projectile.velocity = Vector2.Zero;
            else
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + PiOver2;
                Projectile.velocity *= 0.95f;
            }
        }
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
            if (false)//i != Projectile.oldPos.Length - 1)
            {
                Vector2 midpoint = new((Projectile.oldPos[i].X + Projectile.oldPos[i + 1].X) / 2, (Projectile.oldPos[i].Y + Projectile.oldPos[i + 1].Y) / 2);
                float midangle = (Projectile.oldRot[i] + Projectile.oldRot[i + 1]) / 2f;
                float midInterp = (Projectile.oldPos.Length - i - 0.5f) / Projectile.oldPos.Length;
                Main.EntitySpriteDraw(tex, (midpoint + positionToCenterOffset) - Main.screenPosition, null, colorFunc(midInterp), midangle, tex.Size() * 0.5f, midInterp, 0, 0);
            }
        }
        Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, colorFunc(1) * 1.5f, Projectile.rotation, tex.Size() * 0.5f, 1.25f, 0, 0);
        return false;
    }

    private static Color colorFunc(float i) => Color.Lerp(Color.Cyan * 1.5f, Color.OrangeRed, (float)Math.Sin((Main.GlobalTimeWrappedHourly + i) * 4f) / 2f + 0.5f) * i;

    private static Color pausedColorFunc(float f, float i) => Color.Lerp(Color.Cyan * 1.5f, Color.OrangeRed, f) * i;

    private static float widthFunc(float i) => (1 - i) * 8;
}
