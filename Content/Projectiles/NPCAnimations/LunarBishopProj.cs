using Windfall.Content.NPCs.WorldEvents.LunarCult;

namespace Windfall.Content.Projectiles.NPCAnimations
{
    public class LunarBishopProj : ProjectileNPC, ILocalizedModType
    {
        public override string Texture => "Windfall/Assets/NPCs/WorldEvents/LunarBishop";
        internal override List<dialogue> MyDialogue => new()
        {
            new dialogue {text = "What is this?" , delay = 1},
            new dialogue {text = "Our guardian... defeated?" , delay = 3},
            new dialogue {text = "Hm..." , delay = 3},
            new dialogue {text = "Your actions have not gone unnoticed." , delay = 3},
            new dialogue {text = "Enter our sanctum." , delay = 3},
            new dialogue {text = "Learn our secrets." , delay = 2},
            new dialogue {text = "Perhaps our paths might cross again." , delay = 2},
        };
        internal override SoundStyle SpawnSound => new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");
        internal override int NPCType => ModContent.NPCType<LunarBishop>();
        internal override Color TextColor => Color.Blue;
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
