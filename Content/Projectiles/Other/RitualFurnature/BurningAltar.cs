
namespace Windfall.Content.Projectiles.Other.RitualFurnature
{
    public class BurningAltar : ModProjectile
    {
        public new string LocalizationCategory => "Projectiles.Other";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 9;
            ProjectileID.Sets.DontAttachHideToAlpha[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = int.MaxValue;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.hide = true;
        }

        public override void AI()
        {
            if (Projectile.ai[0] == 0)
            {
                if (++Projectile.frameCounter > 5)
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame = ++Projectile.frame % (Main.projFrames[Projectile.type] - 1);
                }
                Lighting.AddLight(Projectile.Center, TorchID.Torch);
            }
            else
                Projectile.frame = Main.projFrames[Projectile.type] - 1;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
           behindNPCs.Add(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Vector2 origin = frame.Size() * 0.5f;
            Main.EntitySpriteDraw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, 1f, 0, 0);
            return false;
        }
    }
}
