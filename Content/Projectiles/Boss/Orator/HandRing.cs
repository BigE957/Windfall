using CalamityMod.World;
using Windfall.Common.Graphics.Metaballs;

namespace Windfall.Content.Projectiles.Boss.Orator;

public class HandRing : ModProjectile
{
    public new static string LocalizationCategory => "Projectiles.Boss";
    public override string Texture => "Windfall/Assets/Projectiles/Boss/HandRings";
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }

    public override void SetDefaults()
    {
        Projectile.width = 4;
        Projectile.height = 4;
        Projectile.hostile = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        CooldownSlot = ImmunityCooldownID.Bosses;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.timeLeft = 300;
        Projectile.penetrate = -1;
    }

    public ref float Time => ref Projectile.ai[0];

    public ref float AfterImageOpacity => ref Projectile.ai[1];

    private bool spinDir = false;

    private static Color selenicColor => Color.Lerp(new Color(117, 255, 159), new Color(255, 180, 80), EmpyreanMetaball.BorderLerpValue);
    private Vector2 truePosition = Vector2.Zero;

    public override void OnSpawn(IEntitySource source)
    {
        spinDir = Main.rand.NextBool();
        Projectile.localAI[0] = Main.rand.Next(3);
        Projectile.localAI[1] = Main.rand.Next(4);
        Projectile.netUpdate = true;
    }

    public override void AI()
    {
        //Projectile.velocity *= 0.9925f;
        if (spinDir)
            Projectile.rotation += 0.01f * Projectile.velocity.Length();
        else
         Projectile.rotation -= 0.01f * Projectile.velocity.Length();

        Player target = Main.player[Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height)];

        if (Time < 60)
        {
            Projectile.velocity.Y += 0.25f;
            Projectile.velocity *= (1 - AfterImageOpacity);

            if (Time > 30 && Time < 60)
                AfterImageOpacity = (Time - 30) / 30f;
        }
        else if(Time < 210)
        {
            if(Time == 60)
                Projectile.velocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);

            Projectile.velocity = Projectile.velocity.RotateTowards((target.Center - Projectile.Center).ToRotation(), CalamityWorld.death ? 0.045f : CalamityWorld.revenge ? 0.033f : 0.025f);
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * Clamp(Lerp(0.01f, 16f, CircOutEasing((Time - 60) / 120)), 0.01f, 16f);

            AfterImageOpacity = 1f;
        }
        else
        {
            Projectile.velocity *= 0.97f;
        }

        /*
        if (Projectile.timeLeft > 90)
        {
            truePosition = Projectile.Center;

            if (Projectile.velocity.LengthSquared() < 1f)
            {
                Projectile.tileCollide = true;
                Projectile.timeLeft = 90;
            }
        }
        else if (Projectile.timeLeft <= 90)
        {
            Projectile.Center = truePosition += Projectile.velocity;
            Projectile.Center += Main.rand.NextVector2Circular(4f, 4f);
            Projectile.tileCollide = true;
        }
        */

        Lighting.AddLight(Projectile.Center, selenicColor.ToVector3());
        Time++;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D WhiteOutTexture = ModContent.Request<Texture2D>("Windfall/Assets/Projectiles/Boss/HandRingsWhiteOut" + (Projectile.localAI[1] == 0 ? 0 : 1)).Value;
        Color color = Color.Black;
        switch (Projectile.localAI[0])
        {
            case 0:
                color = new(255, 133, 187);
                break;
            case 1:
                color = new(253, 189, 53); 
                break;
            case 2:
                color = new(220, 216, 155);
                break;
        }
        if(Projectile.timeLeft <= 90)
            color = Color.Lerp(color, selenicColor, (90 - Projectile.timeLeft) / 60f);
        DrawCenteredAfterimages(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], color * AfterImageOpacity, 2, texture: WhiteOutTexture);

        Vector2 drawPosition = Projectile.Center - Main.screenPosition;

        Main.EntitySpriteDraw(WhiteOutTexture, drawPosition, null, color * Projectile.Opacity, Projectile.rotation, WhiteOutTexture.Size() * 0.5f, Projectile.scale * 1.25f * CircOutEasing(AfterImageOpacity), SpriteEffects.None);

        Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
        Rectangle frame = tex.Frame(3, 4, (int)Projectile.ai[1], (int)Projectile.localAI[1]);

        Main.EntitySpriteDraw(tex, drawPosition, frame, Color.White * Projectile.Opacity, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, SpriteEffects.None);

        return false;
    }
    
    public override void PostDraw(Color lightColor)
    {
        if (Projectile.timeLeft > 90)
            return;

        Texture2D WhiteOutTexture = ModContent.Request<Texture2D>("Windfall/Assets/Projectiles/Boss/HandRingsWhiteOut" + (Projectile.localAI[1] == 0 ? 0 : 1)).Value;

        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        float ratio = 0f;
        if (Projectile.timeLeft <= 90)
            ratio = (90 - Projectile.timeLeft) / 60f;
        ratio = Clamp(ratio, 0f, 1f);
        Main.EntitySpriteDraw(WhiteOutTexture, drawPosition, WhiteOutTexture.Frame(), Color.White * AfterImageOpacity, Projectile.rotation, WhiteOutTexture.Frame().Size() * 0.5f, Projectile.scale * ratio, SpriteEffects.None);
    }
    
    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i <= 10; i++)
        {
            EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center, Main.rand.NextVector2Circular(4f, 4f), Main.rand.NextFloat(10f, 20f));
        }
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(spinDir);

        writer.Write(truePosition.X);
        writer.Write(truePosition.Y);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        spinDir = reader.ReadBoolean();

        truePosition = reader.ReadVector2();
    }
}
