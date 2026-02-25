using Daybreak.Common.Rendering;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Linq;
using System;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Common.Systems;
using Windfall.Content.Buffs.DoT;
using Windfall.Content.NPCs.Bosses.Orator;

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
        //const int pCount = 250;
        //for (int i = 0; i <= pCount; i++)
        //    SpawnBorderParticle(Projectile, Vector2.Zero, 0.5f * i, 25, Main.rand.NextFloat(80, 160), TwoPi / pCount * i);
        Projectile.netUpdate = true;
    }
    public override void AI()
    {
        #region Particles
        for (int i = 0; i < 5; i++)
        {
            Vector2 spawnPosition = Projectile.Center + Main.rand.NextVector2CircularEdge(Projectile.width * Projectile.scale / 9.65f, Projectile.width * Projectile.scale / 9.65f);
            SelenicMetaballParticle.SpawnParticle(spawnPosition, ((Projectile.Center - spawnPosition).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-1f, 1f)) * Main.rand.NextFloat(2f, 5f)), Main.rand.NextFloat(60f, 100f));

            spawnPosition = Projectile.Center + Main.rand.NextVector2CircularEdge(Projectile.width * (Projectile.scale * 0.85f) / 9.65f, Projectile.width * (Projectile.scale * 0.85f) / 9.65f);
            SelenicMetaballParticle.SpawnParticle(spawnPosition, ((Projectile.Center - spawnPosition).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-1f, 1f)) * Main.rand.NextFloat(2f, 5f)), Main.rand.NextFloat(60f, 100f), 0);
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
                TrueScale = Lerp(5f, 2f, SineOutEasing(Time / 90f));
            else if (Time > 90)
                TrueScale = Lerp(2f, 3f, SineInOutEasing((Time - 90) / 90f));
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
        Color colorA = Color.Lerp(Color.LimeGreen, Color.Orange, SelenicMetaball.BorderLerpValue(0));
        Color colorB = Color.Lerp(Color.GreenYellow, Color.Goldenrod, SelenicMetaball.BorderLerpValue(0));

        float fadeToEnd = Lerp(0.65f, 1f, (float)Math.Cos(-Main.GlobalTimeWrappedHourly * 3f) * 0.5f + 0.5f);

        Color endColor = Color.Lerp(colorA, colorB, (float)Math.Sin(completionRatio * Pi * 1.6f - Main.GlobalTimeWrappedHourly * 5f) * 0.5f + 0.5f);
        return Color.Lerp(Color.White, endColor, fadeToEnd);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Main.spriteBatch.End(out var scope);
        var newScope = scope with { BlendState = BlendState.Additive };
        Main.spriteBatch.Begin(newScope);

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

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(scope);

        MetaballSystem.AddMetaballFill<SelenicMetaball>(new(Projectile), 1);
        const int pCount = 250;
        for (int i = 0; i <= pCount; i++)
        {
            float bump = i;
            float Rotation = TwoPi / pCount * i;
            Vector2 Center = Projectile.Center + (new Vector2(Projectile.width / 2 * (Projectile.scale / 5f) * 1.05f, 0).RotatedBy(Rotation + Projectile.rotation));
            Center += (Projectile.Center - Center).SafeNormalize(Vector2.Zero) * SelenicMetaball.SumofSines(0.5f * bump, 25, 1.5f, 2f);
            Center -= Projectile.velocity / 2;
            MetaballSystem.AddMetaballFill<SelenicMetaball>(new(LoadSystem.Circle.Value, Center - Main.screenPosition, null, 0, LoadSystem.Circle.Size() * 0.5f, 2f, 0), 1);
        }

        Projectile.scale *= 0.85f;
        MetaballSystem.AddMetaballFill<SelenicMetaball>(new(Projectile), 0);
        for (int i = 0; i <= pCount; i++)
        {
            float bump = i;
            float Rotation = TwoPi / pCount * i;
            Vector2 Center = Projectile.Center + (new Vector2(Projectile.width / 2 * (Projectile.scale / 5f) * 1.05f, 0).RotatedBy(Rotation + Projectile.rotation));
            Center += (Projectile.Center - Center).SafeNormalize(Vector2.Zero) * SelenicMetaball.SumofSines(0.5f * bump, 25, 1.5f, 2f);
            Center -= Projectile.velocity / 2;

            MetaballSystem.AddMetaballFill<SelenicMetaball>(new(LoadSystem.Circle.Value, Center - Main.screenPosition, null, 0, LoadSystem.Circle.Size() * 0.5f, 2f, 0), 0);
        }
        Projectile.scale /= 0.85f;

        return false;
    }
}
