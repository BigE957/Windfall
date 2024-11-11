using Terraria.UI;

namespace Windfall.Content.UI;
internal class UIItem : UIElement
{
    public float ImageScale = 1f;
    public float Rotation;
    public bool ScaleToFit;
    public bool AllowResizingDimensions = true;
    public Color Color = Color.White;
    public Vector2 NormalizedOrigin = Vector2.Zero;
    public bool RemoveFloatingPointsFromDrawPosition;
    public SpriteEffects spriteEffects;
    public Item Item = null;

    private Asset<Texture2D> texture;
    private Texture2D nonReloadingTexture;
    bool isHovered = false;

    public UIItem(int ItemID)
    {
        Item = new();
        Item.SetDefaults(ItemID);
        SetImage(TextureAssets.Item[Item.type]);
    }

    protected void SetImage(Asset<Texture2D> texture)
    {
        this.texture = texture;
        nonReloadingTexture = null;
        if (AllowResizingDimensions)
        {
            Width.Set(this.texture.Width(), 0f);
            Height.Set(this.texture.Height(), 0f);
        }
    }

    public override void MouseOver(UIMouseEvent evt)
    {
        base.MouseOver(evt);

        isHovered = true;
    }

    public override void MouseOut(UIMouseEvent evt)
    {
        base.MouseOver(evt);

        isHovered = false;
    }

    public override void Update(GameTime gameTime)
    {
        
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        CalculatedStyle dimensions = GetDimensions();
        Texture2D texture2D = null;
        if (texture != null)
            texture2D = texture.Value;

        if (nonReloadingTexture != null)
            texture2D = nonReloadingTexture;

        if (ScaleToFit)
        {
            spriteBatch.Draw(texture2D, dimensions.ToRectangle(), Color);
            return;
        }

        Vector2 vector = texture2D.Size();
        Vector2 vector2 = dimensions.Position() + vector * (1f - ImageScale) / 2f + vector * NormalizedOrigin;
        if (RemoveFloatingPointsFromDrawPosition)
            vector2 = vector2.Floor();

        spriteBatch.Draw(texture2D, vector2, null, Color, Rotation, vector * NormalizedOrigin, ImageScale, spriteEffects, 0f);

        if (isHovered && !Item.IsAir)
        {
            Main.HoverItem = Item.Clone();

            Main.instance.MouseTextHackZoom(string.Empty);
        }
    }
}
