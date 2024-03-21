using System.Collections.Generic;
using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Windfall.Content.NPCs.WanderingNPCs;

namespace Windfall.Content.Projectiles.NPCAnimations
{
    public class SeekerKnightProj : ProjectileNPC, ILocalizedModType
    {
        public override string Texture => "Windfall/Assets/Projectiles/NPCAnimations/LoneRoninTeleport";
        internal override List<dialogue> MyDialogue => new()
        {
            new dialogue {text = "Quite the display..." , delay = 1},
            new dialogue {text = "I'll admit I hadn't thought you'd survive that... thing. Let alone defeat it." , delay = 3},
            new dialogue {text = "Even in such a state, the might of the deities is ever formidable..." , delay = 3},
            new dialogue {text = "Such a thing could have become quite the threat if given the opportunity." , delay = 3},
            new dialogue {text = "Speaking of, there's another of the deities whom I've been hunting." , delay = 3},
            new dialogue {text = "Your help might just prove invaluable in putting a stop to them." , delay = 3},
            new dialogue {text = "If you'd be willing, I'd be ever thankful." , delay = 3},
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
            Projectile.height = 42;
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
