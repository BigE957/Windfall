using Windfall.Content.NPCs.WorldEvents.DragonCult;
using Windfall.Content.NPCs.WorldEvents.LunarCult;

namespace Windfall.Content.Projectiles.NPCAnimations
{
    public class LunaticCultistProj : ProjectileNPC, ILocalizedModType
    {
        public override string Texture => "Windfall/Assets/NPCs/WorldEvents/LunarBishop";
        internal override string key => "DragonCult.LunaticCultist.WorldText";
        internal override List<float> Delays =>
        [
            1,
            3,
            3,
            3,
            3,
            2,
            2,
        ];
        internal override SoundStyle SpawnSound => new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");
        internal override int NPCType => ModContent.NPCType<DragonCultist>();
        internal override Color TextColor => Color.Cyan;
        internal override bool SpawnConditions(Player player)
        {
            Vector2 vectorFromNpcToPlayer = player.Center - Projectile.Center;
            float distance = vectorFromNpcToPlayer.Length();
            return distance < 250f;
        }
        internal override void DoOnSpawn()
        {
            for (int i = 0; i < 75; i++)
            {
                Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame, speed * 3, Scale: 1.5f);
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
            Projectile.width = 40;
            Projectile.height = 56;
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
