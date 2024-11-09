using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using System.Xml.Linq;
using Windfall.Common.Graphics.Metaballs;

namespace Windfall.Content.Projectiles.Boss.Orator;

public class HandRing : ModProjectile
{
    public new static string LocalizationCategory => "Projectiles.Boss";
    public override string Texture => "Windfall/Assets/Projectiles/Boss/HandRings";
    public ref float Time => ref Projectile.ai[0];
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
        Projectile.timeLeft = 420;
        Projectile.penetrate = -1;
    }
    private bool spinDir = false;
    private Color drawColor = Color.Lerp(new Color(117, 255, 159), new Color(255, 180, 80), (float)(Math.Sin(Main.GlobalTimeWrappedHourly * 1.25f) / 0.5f) + 0.5f);
    private Vector2 truePosition = Vector2.Zero;
    public override void OnSpawn(IEntitySource source)
    {
        spinDir = Main.rand.NextBool();
        Projectile.ai[1] = Main.rand.Next(3);
        Projectile.ai[2] = Main.rand.Next(4);
    }
    public override void AI()
    {
        Projectile.velocity *= 0.9925f;
        if (spinDir)
            Projectile.rotation += 0.01f * Projectile.velocity.Length();
        else
         Projectile.rotation -= 0.01f * Projectile.velocity.Length();

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
        Lighting.AddLight(Projectile.Center, drawColor.ToVector3());

    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D WhiteOutTexture = ModContent.Request<Texture2D>("Windfall/Assets/Projectiles/Boss/HandRingsWhiteOut" + (Projectile.ai[2] == 0 ? 0 : 1)).Value;
        Color color = Color.Black;
        switch (Projectile.ai[1])
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
            color = Color.Lerp(color, drawColor, (90 - Projectile.timeLeft) / 60f);
        DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], color, 2, texture: WhiteOutTexture);
        
        Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Rectangle frame = tex.Frame(3, 4, (int)Projectile.ai[1], (int)Projectile.ai[2]);

        Main.EntitySpriteDraw(tex, drawPosition, frame, Color.White * Projectile.Opacity, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, SpriteEffects.None);

        return false;
    }
    
    public override void PostDraw(Color lightColor)
    {
        if (Projectile.timeLeft > 90)
            return;

        Texture2D WhiteOutTexture = ModContent.Request<Texture2D>("Windfall/Assets/Projectiles/Boss/HandRingsWhiteOut" + (Projectile.ai[2] == 0 ? 0 : 1)).Value;

        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        float ratio = 0f;
        if (Projectile.timeLeft <= 90)
            ratio = (90 - Projectile.timeLeft) / 60f;
        ratio = Clamp(ratio, 0f, 1f);
        Main.EntitySpriteDraw(WhiteOutTexture, drawPosition, WhiteOutTexture.Frame(), Color.White, Projectile.rotation, WhiteOutTexture.Frame().Size() * 0.5f, Projectile.scale * ratio, SpriteEffects.None);
    }
    
    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i <= 10; i++)
        {
            EmpyreanMetaball.SpawnDefaultParticle(Projectile.Center, Main.rand.NextVector2Circular(4f, 4f), Main.rand.NextFloat(10f, 20f));
        }
    }
}
