using Terraria;
using Windfall.Content.Buffs.DoT;
using Windfall.Content.NPCs.Bosses.Orator;
using static Windfall.Common.Graphics.Metaballs.EmpyreanMetaball;


namespace Windfall.Content.Projectiles.Boss.Orator;

public class DarkTide : ModProjectile
{
    public ref float holdtime => ref Projectile.ai[0];
    public ref float moveDistance => ref Projectile.ai[1];
    public ref float moveSpeed => ref Projectile.ai[2];
    public override void SetDefaults()
    {
        Projectile.height = Projectile.width = 1440;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.Opacity = 1f;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.scale = 1.25f;
        CooldownSlot = ImmunityCooldownID.Bosses;
    }
    public override void OnSpawn(IEntitySource source)
    {
        Projectile.rotation = Projectile.velocity.ToRotation();
        Vector2 newPosition = Projectile.rotation.ToRotationVector2().RotatedBy(Pi) * (1800 * Projectile.scale);
        Projectile.Center += newPosition;
        Projectile.velocity = Projectile.rotation.ToRotationVector2() * moveSpeed;

        int particleCounter = 50;
        for(int i = 0; i < particleCounter; i++)
        {
            Vector2 spawnOffset = (Projectile.rotation.ToRotationVector2() * (Projectile.width / 2.7f)) + (Projectile.rotation.ToRotationVector2().RotatedBy(PiOver2) * ((Projectile.width / 2) - Projectile.width / particleCounter * i));
            SpawnBorderParticle(Projectile, spawnOffset, 0.5f * i, 30, Main.rand.NextFloat(80, 160), 0f, false);
        }
    }
    private float moveCount = 0;
    private int holdCounter = 0;
    public override void AI()
    {
        int holdDuration = (int)holdtime;
        float particleVelocity = Main.rand.NextFloat(6f, 8f);
        if (!NPC.AnyNPCs(ModContent.NPCType<TheOrator>()))
        {
            if(moveCount < moveDistance)
                moveDistance = moveCount;
            if(holdCounter < holdDuration)
                holdCounter = holdDuration;
        }
        if (moveCount < 0)
            Projectile.active = false;
        else if (moveCount >= moveDistance || holdCounter != 0)
        {
            if (holdCounter > holdDuration)
            {
                if (Projectile.velocity != Projectile.rotation.ToRotationVector2() * -moveSpeed)
                {
                    if (Projectile.velocity == Vector2.Zero)
                        Projectile.velocity = Projectile.rotation.ToRotationVector2() * -0.05f;
                    Projectile.velocity *= 1.05f;
                    if (Projectile.velocity.LengthSquared() >= 16)
                        Projectile.velocity = Projectile.rotation.ToRotationVector2() * -moveSpeed;
                }
                if(Projectile.velocity.LengthSquared() >= 1)
                    particleVelocity /= (Projectile.velocity.Length()/2);
                moveCount -= Projectile.velocity.Length();
            }
            else
            {
                if (holdCounter < holdDuration / 2)
                {
                    Projectile.velocity *= 0.95f;
                    moveCount += Projectile.velocity.Length();
                }
                else
                    Projectile.velocity = Vector2.Zero;
                if (holdCounter == holdDuration / 3 && NPC.AnyNPCs(ModContent.NPCType<TheOrator>()))
                {
                    TheOrator Orator = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TheOrator>())].As<TheOrator>();
                    if (Orator.AIState == TheOrator.States.DarkTides)
                    {
                        for (int i = 0; i < 16; i++)
                        {
                            Vector2 position = Projectile.Center + (Projectile.rotation.ToRotationVector2() * (Projectile.width / 2.75f)) + (Projectile.rotation.ToRotationVector2().RotatedBy(PiOver2) * Main.rand.NextFloat(-Projectile.width / 2, Projectile.width / 2));
                            Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), position, Projectile.rotation.ToRotationVector2().RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)), ModContent.ProjectileType<EmpyreanThorn>(), TheOrator.BoltDamage, 0f, ai0: 90, ai1: 36f, ai2: 3f);
                        }
                    }
                }
                particleVelocity = Main.rand.NextFloat(6f, 8f);
                holdCounter++;
            }
        }
        else
        {
            particleVelocity *= (Projectile.velocity.Length()/2.5f);
            moveCount += Projectile.velocity.Length();
        }
        foreach (Player player in Main.player.Where(p => p != null && p.active && !p.dead))
        {
             if(!isLeft(Projectile.Center + new Vector2(Projectile.width / 2, Projectile.height / 2).RotatedBy(Projectile.rotation), Projectile.Center + new Vector2(Projectile.width / 2, -Projectile.height / 2).RotatedBy(Projectile.rotation), player.Center))
                player.AddBuff(ModContent.BuffType<Entropy>(), 5);
        }
        Vector2 spawnPosition = Projectile.Center + (Projectile.rotation.ToRotationVector2() * (Projectile.width / 2.05f)) + (Projectile.rotation.ToRotationVector2().RotatedBy(PiOver2) * Main.rand.NextFloat(-Projectile.width / 2, Projectile.width / 2));
        SpawnDefaultParticle(spawnPosition, Projectile.rotation.ToRotationVector2().RotatedBy(Main.rand.NextFloat(-Pi/2, Pi/2)) * particleVelocity, Main.rand.NextFloat(80f, 120f));
    }
    public override Color? GetAlpha(Color lightColor) => Color.White * Projectile.Opacity;

    public override bool PreDraw(ref Color lightColor)
    {
        Color drawColor = Color.White;
        DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], drawColor);
        return false;
    }
    public bool isLeft(Vector2 a, Vector2 b, Vector2 c) => (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X) > 0;
}
