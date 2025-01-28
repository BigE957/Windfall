namespace Windfall.Content.Projectiles.Props.SelenicBaseFurnature;
public class HandStatue : ModProjectile
{
    public new string LocalizationCategory => "Projectiles.Props";

    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 1;
        ProjectileID.Sets.DontAttachHideToAlpha[Projectile.type] = true;
    }

    public override void SetDefaults()
    {
        Projectile.width = 42;
        Projectile.height = 28;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = int.MaxValue;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.hide = true;
        Projectile.scale = 1.33f;
    }

    public override void AI()
    {
        //Main.NewText("Howdy");
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        behindNPCs.Add(index);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, new Vector2(texture.Size().X * 0.5f, texture.Size().Y), Projectile.scale, (SpriteEffects)Projectile.ai[0], 0);
        return false;
    }
}
