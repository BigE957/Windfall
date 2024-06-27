namespace Windfall.Content.Projectiles.GodlyAbilities
{
    public class GodlyCrimulanMuck : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/Boss/UnstableCrimulanGlob";
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.penetrate = -1;
            Projectile.Opacity = 0.8f;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 1200;
            Projectile.damage = 0;
            Projectile.scale = 2f;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.scale = 0f;
        }
        public override void AI()
        {
            if (Projectile.scale < 2f)
                Projectile.scale += 0.05f;
            else
            {
                Player owner = Main.player[Projectile.owner];
                foreach (Projectile proj in Main.projectile.Where(p => p != null && p.active && p.whoAmI != Projectile.whoAmI && (p.type == ModContent.ProjectileType<GodlyCrimulanMuck>() || p.type == ModContent.ProjectileType<GodlyEbonianMuck>())))
                {
                    if (Projectile.Hitbox.Intersects(proj.Hitbox) && Projectile.whoAmI < proj.whoAmI)
                    {
                        if (proj.type == ModContent.ProjectileType<GodlyCrimulanMuck>())
                        {
                            owner.Godly().Ambrosia += 10;
                            SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.Center);
                            for (int k = 0; k < 20; k++)
                            {
                                Dust.NewDustPerfect(proj.Center, DustID.Crimslime, Main.rand.NextVector2Circular(5f, 5f));
                            }
                            proj.active = false;
                        }
                        else
                        {
                            owner.Godly().Ambrosia += 20;
                            SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.Center);
                            for (int k = 0; k < 20; k++)
                            {
                                Dust.NewDustPerfect(proj.Center, DustID.Corruption, Main.rand.NextVector2Circular(5f, 5f));
                            }
                            proj.active = false;
                        }
                        SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.Center);
                        for (int k = 0; k < 20; k++)
                        {
                            Dust.NewDustPerfect(Projectile.Center, DustID.Crimslime, Main.rand.NextVector2Circular(5f, 5f));
                        }
                        Projectile.active = false;
                    }
                }
                if (Projectile.Hitbox.Intersects(owner.Hitbox) && Projectile.velocity.LengthSquared() < 144)
                {
                    SoundEngine.PlaySound(SoundID.NPCHit1, Projectile.Center);
                    Projectile.velocity = (Projectile.Center - owner.Center).SafeNormalize(Vector2.Zero) * 15;
                }
                else
                {
                    foreach (Projectile proj in Main.projectile.Where(p => p != null && p.active && p.friendly && p.owner == Projectile.owner && !p.minion && p.whoAmI != Projectile.whoAmI && p.type != ModContent.ProjectileType<GodlyCrimulanMuck>() && p.type != ModContent.ProjectileType<GodlyCrimulanMuck>()))
                    {
                        if (Projectile.Hitbox.Intersects(proj.Hitbox) && proj.Windfall().hasHitMuck == false)
                        {
                            SoundEngine.PlaySound(SoundID.NPCHit1, Projectile.Center);
                            Projectile.velocity = proj.velocity.SafeNormalize(Vector2.Zero) * 5;
                            proj.Windfall().hasHitMuck = true;
                            break;
                        }
                    }
                }
                if (Projectile.velocity.LengthSquared() > 0)
                {
                    if (Projectile.velocity.LengthSquared() < 1)
                        Projectile.velocity = Vector2.Zero;
                    else
                        Projectile.velocity *= 0.95f;
                }
                if (Projectile.timeLeft < 120)
                    Projectile.position += new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f));
                if (Projectile.timeLeft == 1)
                {
                    SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.Center);
                    for (int k = 0; k < 20; k++)
                    {
                        Dust.NewDustPerfect(Projectile.Center, DustID.Crimslime, Main.rand.NextVector2Circular(5f, 5f));
                    }
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
            lightColor.R = (byte)(255 * Projectile.Opacity);
            lightColor.G = (byte)(255 * Projectile.Opacity);
            lightColor.B = (byte)(255 * Projectile.Opacity);
            DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor, 1);
            return false;
        }
    }
}
