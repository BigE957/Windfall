namespace Windfall.Content.Items.Placeables.Furnature.VerletHangers.Cords;
public class SelenicTwine : Cord, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 9999;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useTime = Item.useAnimation = 15;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.consumable = true;
        Item.rare = ItemRarityID.Lime;
    }

    public override int cordID => CordID.SelenicTwine;

    public override void DrawRopeSegment(SpriteBatch spriteBatch, int index, Vector2[] segmentPositions)
    {
        Texture2D tex = ModContent.Request<Texture2D>("Windfall/Content/Items/Placeables/Furnature/VerletHangers/Cords/SelenicTwineAtlas").Value;

        Vector2 line;
        if (index == segmentPositions.Length - 1)
            line = segmentPositions[index - 1] - segmentPositions[index];
        else if (index == 0)
            line = segmentPositions[index + 1] - segmentPositions[index];
        else
            line = segmentPositions[index] - segmentPositions[index + 1];

        Color lighting = Lighting.GetColor(segmentPositions[index].ToTileCoordinates());
        Vector2 origin = new(0, tex.Size().Y * 0.5f);

        spriteBatch.Draw(tex, segmentPositions[index] - Main.screenPosition, null, lighting, line.ToRotation(), origin, 1f, 0, 0);
    }

    public override void DrawDecorationSegment(SpriteBatch spriteBatch, int index, Vector2[] segmentPositions)
    {
        base.DrawDecorationSegment(spriteBatch, index, segmentPositions);
    }
}
