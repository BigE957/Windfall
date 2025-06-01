using Windfall.Common.Systems.WorldEvents;

namespace Windfall.Content.Projectiles.Other;

public class FoodAlert : ModProjectile
{
    public override string Texture => "CalamityMod/Items/Potions/LavaChickenBroth";
    private int FoodID
    {
        get => (int)Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }
    private enum ObfuscateType
    {
        Darkened,
        Faded,
        Sliced
    }
    private ObfuscateType Obfuscation
    {
        get => (ObfuscateType)Projectile.ai[1];
        set => Projectile.ai[1] = (float)value;
    }
    float ObfuscationRatio = 0f;
    private int NPCIndex
    {
        get => (int)Projectile.ai[2];
        set => Projectile.ai[2] = value;
    }
    private float endFade = 1f;
    public override void SetDefaults()
    {
        Projectile.width = 10;
        Projectile.height = 10;
        Projectile.penetrate = -1;
        Projectile.alpha = 0;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 9999;
    }
    public override void OnSpawn(IEntitySource source)
    {
        if (Obfuscation == ObfuscateType.Faded)
        {
            Projectile.Opacity = 0;
            ObfuscationRatio = -0.1f;
        }
        else
            ObfuscationRatio = -0.25f;
        Projectile.scale = 0f;
    }

    public override void AI()
    {
        if (Projectile.ai[2] != -1 && LunarCultBaseSystem.Active)
        {
            if (Obfuscation == ObfuscateType.Faded && ObfuscationRatio > 0f)
                Projectile.Opacity = ObfuscationRatio;

            if (ObfuscationRatio < 1f)
                ObfuscationRatio += 0.005f;
            else
                ObfuscationRatio = 1f;

            Projectile.velocity *= 0.95f;

            if (Projectile.scale < 1f)
                Projectile.scale += 0.05f;
            else
                Projectile.scale = 1f;
        }
        else
        {
            if(endFade > 0f)
                endFade -= 0.05f;
            Projectile.scale -= 0.05f;
            if (Projectile.scale <= 0f)
                Projectile.active = false;
        }
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>("Windfall/Assets/UI/ThoughtBubble").Value;
        Vector2 position = Projectile.Center - Main.screenPosition;
        SpriteEffects spriteEffects = SpriteEffects.None;
        if (Projectile.spriteDirection == -1)
            spriteEffects = SpriteEffects.FlipHorizontally;
        Vector2 origin = texture.Size() / 2f;
        Main.EntitySpriteDraw(texture, position, new Rectangle(0, 0, texture.Width, texture.Height), lightColor * endFade, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);

        Item item = new(FoodID);
        texture = TextureAssets.Item[FoodID].Value;
        float width = texture.Width;
        if (Obfuscation == ObfuscateType.Sliced)
        {               
            if (ObfuscationRatio > 0f)
                width = texture.Width * ObfuscationRatio;
            else
                width = 0;
            if (width < 1)
                return false;
        }
        Rectangle source = new(0, 0, (int)width, texture.Height / (ItemID.Sets.IsFood[FoodID] ? 3 : 1));
        Texture2D FramedTexture = new(Main.graphics.GraphicsDevice, source.Width, source.Height);
        if (Obfuscation == ObfuscateType.Darkened)
        {
            Color[] data = new Color[FramedTexture.Width * FramedTexture.Height];
            texture.GetData(0, source, data, 0, data.Length);
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].A != 0)
                {
                    byte alpha = data[i].A;
                    if (ObfuscationRatio > 0f)
                        data[i] *= ObfuscationRatio;
                    else
                        data[i] *= 0f;
                    data[i].A = alpha;
                }
            }
            FramedTexture.SetData(data);
        }
        else
        {
            Color[] data = new Color[FramedTexture.Width * FramedTexture.Height];
            texture.GetData(0, source, data, 0, data.Length);
            FramedTexture.SetData(data);                
        }
        
        origin = FramedTexture.Size() / 2f;
        
        Main.EntitySpriteDraw(FramedTexture, position, new Rectangle(0, 0, FramedTexture.Width, FramedTexture.Height), lightColor * Projectile.Opacity * endFade, Projectile.rotation, origin, Projectile.scale, spriteEffects);
        return false;
    }
}
