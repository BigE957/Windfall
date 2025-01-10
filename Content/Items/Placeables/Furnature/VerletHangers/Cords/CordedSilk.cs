namespace Windfall.Content.Items.Placeables.Furnature.VerletHangers.Cords;
public class CordedSilk : Cord, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";

    public override void SetDefaults()
    {
        Item.width = 22;
        Item.height = 16;
        Item.maxStack = 9999;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.consumable = true;
        Item.rare = ItemRarityID.Lime;
    }

    public override int cordID => CordID.CordedSilk;

    public override void DrawRopeSegment(SpriteBatch spriteBatch, int index, Vector2[] segmentPositions)
    {
        Texture2D tex = ModContent.Request<Texture2D>("Windfall/Content/Items/Placeables/Furnature/VerletHangers/Cords/CordedSilkAtlas").Value;

        Vector2 line;
        if (index == segmentPositions.Length - 1)
            line = segmentPositions[index - 1] - segmentPositions[index];
        else if (index == 0)
            line = segmentPositions[index + 1] - segmentPositions[index];
        else
            line = segmentPositions[index] - segmentPositions[index + 1];

        Color lighting = Lighting.GetColor(segmentPositions[index].ToTileCoordinates());
        Rectangle frame = tex.Frame(1, 2);
        Vector2 origin = new(0, frame.Size().Y * 0.5f);

        spriteBatch.Draw(tex, segmentPositions[index] - Main.screenPosition, frame, lighting, line.ToRotation(), origin, 1f, 0, 0);
    }

    public override void DrawOnRopeEnds(SpriteBatch spriteBatch, Vector2 position, float rotation)
    {
        Texture2D tex = ModContent.Request<Texture2D>("Windfall/Content/Items/Placeables/Furnature/VerletHangers/Cords/CordedSilkAtlas").Value;
        Rectangle frame = tex.Frame(1, 2, frameY: 1);
        Color lighting = Lighting.GetColor(position.ToTileCoordinates());

        spriteBatch.Draw(tex, position - Main.screenPosition, frame, lighting, rotation + PiOver2, frame.Size() * 0.5f, 1f, 0, 0);
    }
}
