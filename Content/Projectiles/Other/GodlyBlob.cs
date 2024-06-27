
namespace Windfall.Content.Projectiles.Other
{
    public class GodlyBlob : ModProjectile
    {
        public override string Texture => "CalamityMod/NPCs/HiveMind/HiveBlob";
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
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
        private int hitCounter = 0;
        private int aiCounter = 0;
        public override void AI()
        {
            if(hitCounter >= 5)
            {
                for (int k = 0; k < 20; k++)
                {
                    Dust.NewDustPerfect(Projectile.position, DustID.Demonite, Main.rand.NextVector2Circular(10f, 10f));
                }
                Projectile.active = false;
                SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.Center);
                Main.player[Projectile.owner].Godly().Ambrosia++;
                return;
            }
            if (aiCounter >= 30)
            {
                foreach (Projectile proj in Main.projectile.Where(p => p != null && p.active && p.friendly && !p.npcProj && p.owner == Projectile.owner && p.damage > 0 && p.type != ModContent.ProjectileType<GodlyBlob>()))
                {
                    if (Projectile.Hitbox.Intersects(proj.Hitbox))
                    {
                        for (int k = 0; k < 20; k++)
                        {
                            Dust.NewDustPerfect(Projectile.position, DustID.Demonite, Main.rand.NextVector2Circular(10f, 10f));
                        }
                        Projectile.active = false;
                        SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.Center);
                        Main.player[Projectile.owner].Godly().Ambrosia += 5;
                    }
                }
            }
            if(Projectile.velocity.LengthSquared() < 56.25f)
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 7.5f;
            aiCounter++;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            hitCounter++;
        }
        /*
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.spriteDirection == 1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            Texture2D texture = TextureAssets.Npc[NPC.type].Value;
            Vector2 vector = new Vector2(TextureAssets.Npc[NPC.type].Value.Width / 2, TextureAssets.Npc[NPC.type].Value.Height / 2);

            Vector2 vector2 = NPC.Center - screenPos;
            vector2 -= new Vector2(texture.Width, texture.Height) * NPC.scale / 2f;
            vector2 += vector * NPC.scale + new Vector2(0f, NPC.gfxOffY);
            Color color = NPC.GetAlpha(drawColor);

            if (NPC.localAI[1] > ShowTelegraphValue)
                color = Color.Lerp(color, Color.LimeGreen * NPC.Opacity, MathHelper.Clamp((NPC.localAI[1] - ShowTelegraphValue) / TelegraphDuration, 0f, 1f));

            spriteBatch.Draw(texture, vector2, NPC.frame, color, NPC.rotation, vector, NPC.scale, spriteEffects, 0f);

            return false;
        }
        */
    }
}
