using Windfall.Content.NPCs.WanderingNPCs;

namespace Windfall.Content.Projectiles.NPCAnimations
{
    public class GodseekerKnightProj : ProjectileNPC, ILocalizedModType
    {
        public override string Texture => "Windfall/Assets/Projectiles/NPCAnimations/GodseekerKnightProj";
        internal override string key => "GodseekerKnight.WorldText";
        internal override List<float> Delays => new()
        {
            1,
            3,
            3,
            3,
            3,
            3,
            3,
        };
        internal override SoundStyle SpawnSound => new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");
        internal override int NPCType => ModContent.NPCType<GodseekerKnight>();
        internal override Color TextColor => Color.Gold;
        internal override void DoOnSpawn()
        {
            for (int i = 0; i < 75; i++)
            {
                Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Pixie, speed * 4, Scale: 1.5f);
                d.noGravity = true;
            }
            SoundEngine.PlaySound(SpawnSound, Projectile.Center);
        }
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 1;
        }
        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 50;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = int.MaxValue;
            Projectile.alpha = 255;
            Projectile.ai[0] = 0;
        }
    }
}
