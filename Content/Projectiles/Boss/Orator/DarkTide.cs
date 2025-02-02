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

    private float moveCount = 0;
    private int holdCounter = 0;
    private float trueRotation = 0;

    public override void SetDefaults()
    {
        Projectile.height = Projectile.width = 1440;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.Opacity = 1f;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.scale = 1.25f;
        Projectile.netImportant = true;
        CooldownSlot = ImmunityCooldownID.Bosses;
    }
    public override void OnSpawn(IEntitySource source)
    {
        trueRotation = Projectile.velocity.ToRotation();
        Vector2 newPosition = trueRotation.ToRotationVector2().RotatedBy(Pi) * (1800 * Projectile.scale);
        Projectile.Center += newPosition;
        Projectile.velocity = trueRotation.ToRotationVector2() * moveSpeed;

        const int particleCounter = 50;
        for(int i = 0; i < particleCounter; i++)
        {
            Vector2 spawnOffset = (trueRotation.ToRotationVector2() * (Projectile.width / 2.7f)) + (trueRotation.ToRotationVector2().RotatedBy(PiOver2) * ((Projectile.width / 2) - Projectile.width / particleCounter * i));
            SpawnBorderParticle(Projectile, spawnOffset, 0.5f * i, 30, Main.rand.NextFloat(80, 160), 0f, false);
        }
    }

    public override void AI()
    {
        if(Main.netMode == NetmodeID.MultiplayerClient && moveCount == 0 && holdCounter == 0)
        {
            const int particleCounter = 50;
            for (int i = 0; i < particleCounter; i++)
            {
                Vector2 spawnOffset = (trueRotation.ToRotationVector2() * (Projectile.width / 2.7f)) + (trueRotation.ToRotationVector2().RotatedBy(PiOver2) * ((Projectile.width / 2) - Projectile.width / particleCounter * i));
                SpawnBorderParticle(Projectile, spawnOffset, 0.5f * i, 30, Main.rand.NextFloat(80, 160), 0f, false);
            }
        }

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
                if (Projectile.velocity != trueRotation.ToRotationVector2() * -moveSpeed)
                {
                    if (Projectile.velocity == Vector2.Zero)
                        Projectile.velocity = trueRotation.ToRotationVector2() * -0.05f;
                    Projectile.velocity *= 1.05f;
                    if (Projectile.velocity.LengthSquared() >= 16)
                        Projectile.velocity = trueRotation.ToRotationVector2() * -moveSpeed;
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
                    if (Orator.AIState == TheOrator.States.DarkTides && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < 16; i++)
                        {
                            Vector2 position = Projectile.Center + (trueRotation.ToRotationVector2() * (Projectile.width / 2.75f)) + (trueRotation.ToRotationVector2().RotatedBy(PiOver2) * Main.rand.NextFloat(-Projectile.width / 2, Projectile.width / 2));
                            Projectile.NewProjectile(Terraria.Entity.GetSource_NaturalSpawn(), position, trueRotation.ToRotationVector2().RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)), ModContent.ProjectileType<EmpyreanThorn>(), TheOrator.BoltDamage, 0f, ai0: 90, ai1: 36f, ai2: 3f);
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
             if(!isLeft(Projectile.Center + new Vector2(Projectile.width / 2, Projectile.height / 2).RotatedBy(trueRotation), Projectile.Center + new Vector2(Projectile.width / 2, -Projectile.height / 2).RotatedBy(trueRotation), player.Center))
                player.AddBuff(ModContent.BuffType<Entropy>(), 5);
        }
        Vector2 spawnPosition = Projectile.Center + (trueRotation.ToRotationVector2() * (Projectile.width / 2.05f)) + (trueRotation.ToRotationVector2().RotatedBy(PiOver2) * Main.rand.NextFloat(-Projectile.width / 2, Projectile.width / 2));
        Projectile.rotation = trueRotation;
        SpawnDefaultParticle(spawnPosition, trueRotation.ToRotationVector2().RotatedBy(Main.rand.NextFloat(-Pi/2, Pi/2)) * particleVelocity, Main.rand.NextFloat(80f, 120f));
    }
    public override Color? GetAlpha(Color lightColor) => Color.White * Projectile.Opacity;

    private static Color ColorFunction(float completionRatio)
    {
        Color colorA = Color.Lerp(Color.LimeGreen, Color.Orange, BorderLerpValue);
        Color colorB = Color.Lerp(Color.GreenYellow, Color.Goldenrod, BorderLerpValue);

        float fadeToEnd = Lerp(0.65f, 1f, (float)Math.Cos(-Main.GlobalTimeWrappedHourly * 3f) * 0.5f + 0.5f);

        Color endColor = Color.Lerp(colorA, colorB, (float)Math.Sin(completionRatio * Pi * 1.6f - Main.GlobalTimeWrappedHourly * 5f) * 0.5f + 0.5f);
        return Color.Lerp(Color.White, endColor, fadeToEnd);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Main.spriteBatch.UseBlendState(BlendState.Additive);

        Vector2 drawPos = Projectile.Center - Main.screenPosition;
        Color drawColor = ColorFunction(0.5f);
        float scale = Projectile.scale * 1f;
        Vector2 displacement = Vector2.UnitX.RotatedBy(Projectile.rotation - PiOver2) * (512 * scale);
        Texture2D tex; //= ModContent.Request<Texture2D>("CalamityMod/Particles/HighResHollowCircleHardEdge").Value;
        //Main.EntitySpriteDraw(tex, drawPos, null, drawColor * 0.5f, Main.GlobalTimeWrappedHourly / 2f, tex.Size() * 0.5f, scale, SpriteEffects.None, 0);

        tex = ModContent.Request<Texture2D>("Windfall/Assets/Projectiles/Boss/TideNoise1").Value;
        drawColor = ColorFunction(1f);
        Vector2 shiftOffset = Vector2.UnitX.RotatedBy(Projectile.rotation) * ((float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f) * 32f + 770);
        for (int i = -1; i < 3; i++)
        {
            Vector2 scroll = Vector2.UnitY.RotatedBy(Projectile.rotation) * ((Main.GlobalTimeWrappedHourly * 48) % (512 * scale));
            Main.EntitySpriteDraw(tex, drawPos + shiftOffset + (displacement * i) + scroll, null, drawColor * 0.75f, Projectile.rotation + PiOver2, tex.Size() * 0.5f, scale, SpriteEffects.None, 0);
        }
        
        tex = ModContent.Request<Texture2D>("Windfall/Assets/Projectiles/Boss/TideNoise2").Value;
        drawColor = ColorFunction(0f);
        shiftOffset = Vector2.UnitX.RotatedBy(Projectile.rotation) * ((float)Math.Sin(Main.GlobalTimeWrappedHourly) * -32f + 770);
        for (int i = -2; i < 2; i++)
        {
            Vector2 scroll = Vector2.UnitY.RotatedBy(Projectile.rotation) * ((Main.GlobalTimeWrappedHourly * -84) % (512 * scale));
            Main.EntitySpriteDraw(tex, drawPos + shiftOffset + (displacement * i) + scroll, null, drawColor * 0.85f, Projectile.rotation + PiOver2, tex.Size() * 0.5f, scale, SpriteEffects.None, 0);
        }
        
        Main.spriteBatch.UseBlendState(BlendState.AlphaBlend);
        return false;
    }

    public override void PostDraw(Color lightColor)
    {
        DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], Color.White);
    }

    public static bool isLeft(Vector2 a, Vector2 b, Vector2 c) => (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X) > 0;

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(moveCount);
        writer.Write(holdCounter);
        writer.Write(trueRotation);

    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        moveCount = reader.ReadSingle();
        holdCounter = reader.ReadInt32();
        trueRotation = reader.ReadSingle();
    }
}
