using Windfall.Content.Items.Debug;

namespace Windfall.Content.Projectiles.Misc
{
    public class SuperCnidrisnackThrow : ModProjectile
    {
        public override string Texture => "Windfall/Assets/Items/Debug/SuperCnidrisnack";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.DamageType = DamageClass.Default;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 90000;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.aiStyle = 2;
        }
        public override void OnKill(int timeLeft)
        {
            Item.NewItem(Projectile.GetSource_DropAsItem(), (int)Projectile.position.X, (int)Projectile.position.Y, Projectile.width, Projectile.height, ModContent.ItemType<SuperCnidrisnack>());
        }
        public override void PostDraw(Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            int height = texture.Height;
            int drawStart = height * Projectile.frame;
            Vector2 origin = Projectile.Center;
            Main.EntitySpriteDraw(ModContent.Request<Texture2D>("Windfall/Assets/Items/Debug/SuperCnidrisnack_Glow").Value, new Vector2(Projectile.Center.X - Main.screenPosition.X, Projectile.Center.Y - Main.screenPosition.Y), new Microsoft.Xna.Framework.Rectangle?(new Rectangle(0, drawStart, texture.Width, height)), Color.White * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

        }
    }
}
