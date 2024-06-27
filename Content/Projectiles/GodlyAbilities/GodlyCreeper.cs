namespace Windfall.Content.Projectiles.GodlyAbilities
{
    public class GodlyCreeper : ModProjectile
    {
        public override string Texture => "Windfall/Assets/Projectiles/Misc/Creeper";
        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.damage = 25;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90000;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 45;
            Projectile.aiStyle = ProjAIStyleID.Raven;
            Projectile.netImportant = true;
            Projectile.minion = true;
            Projectile.tileCollide = false;
        }
        public int ambrosiaRequirement = 0;
        public override void AI()
        {
            if (Main.player[Projectile.owner].Godly().Ambrosia < ambrosiaRequirement)
            {
                for (int k = 0; k < 20; k++)
                {
                    Dust.NewDustPerfect(Projectile.Center, DustID.Blood, Main.rand.NextVector2Circular(10f, 10f));
                }
                Projectile.active = false;
                SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.Center);
            }
        }
    }
}
