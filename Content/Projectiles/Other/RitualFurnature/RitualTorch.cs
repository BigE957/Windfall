namespace Windfall.Content.Projectiles.Other.RitualFurnature
{
    public class RitualTorch : ModProjectile
    {
        public override string Texture => "Windfall/Content/Projectiles/Other/RitualFurnature/RitualFurnatureAtlas";
        public new string LocalizationCategory => "Projectiles.Other";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            ProjectileID.Sets.DontAttachHideToAlpha[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 55;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = int.MaxValue;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.hide = true;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center - Vector2.UnitY * (Projectile.height / 2f), TorchID.Torch);
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Rectangle frame = new(20, 0, 10, 55);
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Vector2 origin = frame.Size() * 0.5f;
            Main.EntitySpriteDraw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, 1f, 0, 0);
            return false;
        }
    }
}
