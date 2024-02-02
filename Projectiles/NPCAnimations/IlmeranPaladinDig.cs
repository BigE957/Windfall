using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Windfall.NPCs.WanderingNPCs;

namespace Windfall.Projectiles.NPCAnimations
{
    public class IlmeranPaladinDig : ModProjectile, ILocalizedModType
    {
        public override string Texture => "Windfall/NPCs/WanderingNPCs/IlmeranPaladin";

        public bool InsideTiles = false;
        public bool PostExitTiles = false;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 25;
        }
        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 40;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = int.MaxValue;
        }
        public override void AI()
        {
            Projectile.velocity *= 0.998f;
            Projectile.velocity.Y += 0.01f;
            InsideTiles = Collision.SolidCollision(Projectile.Center, Projectile.width, Projectile.height);
            if (InsideTiles == false && PostExitTiles == false)
            {
                PostExitTiles = true;
                float numberOfDusts = 20f;
                float rotFactor = 360f / numberOfDusts;
                for (int i = 0; i < numberOfDusts; i++)
                {
                    float rot = MathHelper.ToRadians(i * rotFactor);
                    Vector2 offset = new Vector2(Main.rand.NextFloat(1.5f, 5.5f), 0).RotatedBy(rot * Main.rand.NextFloat(3.1f, 9.1f));
                    Vector2 velOffset = new Vector2(Main.rand.NextFloat(1.5f, 5.5f), 0).RotatedBy(rot * Main.rand.NextFloat(3.1f, 9.1f));
                    MediumMistParticle SandCloud = new MediumMistParticle(Projectile.Center + offset, velOffset * Main.rand.NextFloat(1.5f, 3f), Color.Peru, Color.PeachPuff, Main.rand.NextFloat(0.9f, 1.2f), 160f, Main.rand.NextFloat(0.03f, -0.03f));
                    GeneralParticleHandler.SpawnParticle(SandCloud);
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, Main.rand.NextBool() ? 288 : 207, new Vector2(velOffset.X, velOffset.Y));
                    dust.noGravity = false;
                    dust.velocity = velOffset;
                    dust.scale = Main.rand.NextFloat(1.2f, 1.6f);
                }
            }
            if (Projectile.Center.Y < Main.player[Projectile.owner].Center.Y)
            {
                NPC.NewNPC(null, (int)Projectile.Center.X, (int)Projectile.Center.Y, ModContent.NPCType<IlmeranPaladin>(), 0, Projectile.velocity.Y);
                if (PostExitTiles == false)
                {
                    PostExitTiles = true;
                    float numberOfDusts = 20f;
                    float rotFactor = 360f / numberOfDusts;
                    for (int i = 0; i < numberOfDusts; i++)
                    {
                        float rot = MathHelper.ToRadians(i * rotFactor);
                        Vector2 offset = new Vector2(Main.rand.NextFloat(1.5f, 5.5f), 0).RotatedBy(rot * Main.rand.NextFloat(3.1f, 9.1f));
                        Vector2 velOffset = new Vector2(Main.rand.NextFloat(1.5f, 5.5f), 0).RotatedBy(rot * Main.rand.NextFloat(3.1f, 9.1f));
                        MediumMistParticle SandCloud = new MediumMistParticle(Projectile.Center + offset, velOffset * Main.rand.NextFloat(1.5f, 3f), Color.Peru, Color.PeachPuff, Main.rand.NextFloat(0.9f, 1.2f), 160f, Main.rand.NextFloat(0.03f, -0.03f));
                        GeneralParticleHandler.SpawnParticle(SandCloud);
                        Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, Main.rand.NextBool() ? 288 : 207, new Vector2(velOffset.X, velOffset.Y));
                        dust.noGravity = false;
                        dust.velocity = velOffset;
                        dust.scale = Main.rand.NextFloat(1.2f, 1.6f);
                    }
                }
                Projectile.active = false;
            }
        }
    }
}
