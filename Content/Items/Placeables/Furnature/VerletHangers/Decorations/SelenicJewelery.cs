namespace Windfall.Content.Items.Placeables.Furnature.VerletHangers.Decorations;
public class SelenicJewelery : Decoration, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 9999;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.consumable = true;
        Item.rare = ItemRarityID.Lime;
        Item.noUseGraphic = true;
    }

    internal enum ColorType
    {
        Red,
        Green,
        Silver
    }

    internal ColorType colorType = ColorType.Red;

    public override bool AltFunctionUse(Player player) => true;

    public override void UseAnimation(Player player)
    {
        if(player.altFunctionUse == 2)
        {
            if (colorType == ColorType.Silver)
                colorType = ColorType.Red;
            else
                colorType++;
        }
        else
            base.UseAnimation(player);
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D texture = TextureAssets.Item[Type].Value;
        Rectangle myFrame = texture.Frame(3, 2, (int)colorType, 1);
        Vector2 myOrigin = myFrame.Size() * 0.5f;

        spriteBatch.Draw(texture, position, myFrame, drawColor, 0f, myOrigin, scale * 2, 0, 0);
        return false;
    }

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        Texture2D texture = TextureAssets.Item[Type].Value;
        Rectangle myFrame = texture.Frame(3, 2, (int)colorType, 1);
        Vector2 myOrigin = myFrame.Size() * 0.5f;

        spriteBatch.Draw(texture, Item.Center - Main.screenPosition, myFrame, alphaColor, rotation, myOrigin, scale * 2, 0, 0);
        return false;
    }

    public override int DecorID => (int)(1 + colorType);

    public override void DrawAlongVerlet(SpriteBatch spriteBatch, int index, Vector2[] segmentPositions)
    {
        Texture2D jewelery = ModContent.Request<Texture2D>("Windfall/Content/Items/Placeables/Furnature/VerletHangers/Decorations/SelenicJewelery").Value;
        if (index % 3 == 0 && index != 0 && index != segmentPositions.Length - 1)
        {
            Vector2 line = (segmentPositions[index] - segmentPositions[index + 1]);

            Rectangle frame = jewelery.Frame(3, 2, (int)colorType);
            Vector2 origin = frame.Size() * 0.5f;
            Vector2 drawPos = segmentPositions[index] + (line / 2f) + line.SafeNormalize(Vector2.UnitX).RotatedBy(PiOver2) * ((index / 3) % 2 == 0 ? -3 : 3);
            float rotation = (segmentPositions[index + 1] - segmentPositions[index]).ToRotation();
            
            spriteBatch.Draw(jewelery, drawPos - Main.screenPosition, frame, Color.White, rotation - PiOver2, origin, 1f, 0, 0);
        }
    }

    public override void DrawOnVerletEnd(SpriteBatch spriteBatch, Vector2[] segmentPositions)
    {
        Texture2D jewelery = ModContent.Request<Texture2D>("Windfall/Content/Items/Placeables/Furnature/VerletHangers/Decorations/SelenicJewelery").Value;

        Rectangle bigFrame = jewelery.Frame(3, 2, (int)colorType, 1);
        Vector2 bigOrigin = bigFrame.Size() * 0.5f;
        Vector2 bigDrawPos = segmentPositions[^1];
        float bigRotation = (segmentPositions[^1] - segmentPositions[^2]).ToRotation();
        
        spriteBatch.Draw(jewelery, bigDrawPos - Main.screenPosition, bigFrame, Color.White, bigRotation - PiOver2, bigOrigin, 1f, 0, 0);
    }
}
