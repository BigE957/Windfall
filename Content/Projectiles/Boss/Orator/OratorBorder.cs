using Windfall.Content.Buffs.DoT;
using Windfall.Content.NPCs.Bosses.Orator;
using static Windfall.Common.Graphics.Metaballs.EmpyreanMetaball;

namespace Windfall.Content.Projectiles.Boss.Orator;

public class OratorBorder : ModProjectile
{
    public ref float Time => ref Projectile.ai[0];
    public ref float SineCounter => ref Projectile.localAI[0];
    public ref float TrueScale => ref Projectile.ai[1];

    private const float BaseRadius = 250f;
    public float Radius = 1250f;

    public override void SetDefaults()
    {
        Projectile.height = Projectile.width = 900;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.Opacity = 1f;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.scale = 3f;
        Projectile.netImportant = true;
        CooldownSlot = ImmunityCooldownID.Bosses;
        Projectile.timeLeft = 200;
    }
    public override void OnSpawn(IEntitySource source)
    {
        Projectile.scale = 5f;
        Projectile.ai[1] = 5f;
        Radius = BaseRadius * Projectile.scale;
        const int pCount = 250;
        for (int i = 0; i <= pCount; i++)
            SpawnBorderParticle(Projectile, Vector2.Zero, 0.5f * i, 25, Main.rand.NextFloat(80, 160), TwoPi / pCount * i);
        Projectile.netUpdate = true;
    }
    public override void AI()
    {
        #region Particles
        if (Time == 0 && Main.netMode == NetmodeID.MultiplayerClient)
        {
            const int pCount = 250;
            for (int i = 0; i <= pCount; i++)
                SpawnBorderParticle(Projectile, Vector2.Zero, 0.5f * i, 25, Main.rand.NextFloat(80, 160), TwoPi / pCount * i);
        }
        for (int i = 0; i < 5; i++)
        {
            Vector2 spawnPosition = Projectile.Center + Main.rand.NextVector2CircularEdge(Projectile.width * Projectile.scale / 9.65f, Projectile.width * Projectile.scale / 9.65f);
            SpawnDefaultParticle(spawnPosition, ((Projectile.Center - spawnPosition).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-1f, 1f)) * Main.rand.NextFloat(2f, 5f)), Main.rand.NextFloat(60f, 100f));
        }
        #endregion

        if (!NPC.AnyNPCs(ModContent.NPCType<TheOrator>()))
            TrueScale += 0.05f;
        else if(Time > 180)
        {       
            Projectile.timeLeft = 30;
            if (TrueScale > 3f)
            {
                TrueScale -= 0.05f;
                SineCounter = (3 * Pi / 2) * 20;
            }
        }
        else //Spawn in
        {
            if (Time < 90)
                TrueScale = Lerp(5f, 2f, SineOutEasing(Time / 90f, 1));
            else if (Time > 90)
                TrueScale = Lerp(2f, 3f, SineInOutEasing((Time - 90) / 90f, 1));
            else
                TrueScale = 2f;
            SineCounter = (3 * Pi / 2) * 20;
        }
        Projectile.scale = (float)(TrueScale - (Math.Sin(SineCounter / 20f) + 1f) / 8);
        Radius = BaseRadius * Projectile.scale;

        #region Outside Border Debuff
        if (NPC.AnyNPCs(ModContent.NPCType<TheOrator>()))
            foreach(Player target in Main.ActivePlayers)
                if (!target.dead && !target.WithinRange(Projectile.Center, Radius + 16f))
                    target.AddBuff(ModContent.BuffType<Entropy>(), 5);
        #endregion

        SineCounter++;
        Time++;
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
        Texture2D tex = ModContent.Request<Texture2D>("CalamityMod/Particles/HighResHollowCircleHardEdge").Value;
        Main.EntitySpriteDraw(tex, drawPos, null, drawColor * 0.5f, Main.GlobalTimeWrappedHourly / 2f, tex.Size() * 0.5f, Projectile.scale * Lerp(0.275f, 0.2f, 1 - TrueScale / 3f), SpriteEffects.None, 0);

        tex = ModContent.Request<Texture2D>("Windfall/Assets/Projectiles/Boss/BorderAura1").Value;
        drawColor = ColorFunction(1f);
        Main.EntitySpriteDraw(tex, drawPos, null, drawColor * 0.5f, Main.GlobalTimeWrappedHourly / -2f, tex.Size() * 0.5f, Projectile.scale * Lerp(0.7f, 0.5f, 1 - TrueScale / 3f) + ((float)Math.Sin(Main.GlobalTimeWrappedHourly) * 0.05f), SpriteEffects.None, 0);

        tex = ModContent.Request<Texture2D>("Windfall/Assets/Projectiles/Boss/BorderAura2").Value;
        drawColor = ColorFunction(0f);
        Main.EntitySpriteDraw(tex, drawPos, null, drawColor * 0.5f, Main.GlobalTimeWrappedHourly / 2f, tex.Size() * 0.5f, Projectile.scale * Lerp(0.73f, 0.4f, 1 - TrueScale / 3f) + ((float)Math.Sin(Main.GlobalTimeWrappedHourly) * -0.05f), SpriteEffects.None, 0);

        Main.spriteBatch.UseBlendState(BlendState.AlphaBlend);

        return false;
    }

    public override void PostDraw(Color lightColor)
    {
        DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], Color.White);
    }
}
