using CalamityMod.Items.Weapons.Rogue;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Windfall.Content.Items.Weapons.Misc;

namespace Windfall.Content.Projectiles.Other
{
    public class CnidrisnackThrow : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 6;
        }
        public override string Texture => "Windfall/Assets/Projectiles/Other/CnidrisnackThrow";
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
        public override void AI()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 6)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
        }
        public override void OnKill(int timeLeft)
        {
            Item.NewItem(Projectile.GetSource_DropAsItem(), (int)Projectile.position.X, (int)Projectile.position.Y, Projectile.width, Projectile.height, ModContent.ItemType<Cnidrisnack>());
        }
        public override void PostDraw(Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            int height = texture.Height / Main.projFrames[Projectile.type];
            int drawStart = height * Projectile.frame;
            Vector2 origin = Projectile.Size / 2;
            Main.EntitySpriteDraw(ModContent.Request<Texture2D>("Windfall/Assets/Projectiles/Other/CnidrisnackThrowGlow").Value, Projectile.Center - Main.screenPosition, new Microsoft.Xna.Framework.Rectangle?(new Rectangle(0, drawStart, texture.Width, height)), Color.White * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
        }
    }
}
