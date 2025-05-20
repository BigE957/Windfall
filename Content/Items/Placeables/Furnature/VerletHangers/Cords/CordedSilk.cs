using static Windfall.Common.Graphics.Verlet.VerletIntegration;

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
        Item.useTime = Item.useAnimation = 15;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.consumable = true;
        Item.rare = ItemRarityID.Lime;
    }

    public override int cordID => CordID.CordedSilk;

    public override string cordTexturePath => "Windfall/Content/Items/Placeables/Furnature/VerletHangers/Cords/CordedSilkAtlas";

    public override void DrawRopeSegment(SpriteBatch spriteBatch, List<VerletPoint> points, int index)
    {
        VerletPoint p = points[index];

        foreach ((VerletPoint p2, float l) in p.Connections)
        {
            if (l == -1)
                continue;

            float rot = (p2.Position - p.Position).ToRotation();
            Vector2 midPoint = (p.Position + p2.Position) / 2f;

            Color lighting = Lighting.GetColor(midPoint.ToTileCoordinates());
            Rectangle frame = cordTexture.Frame(1, 2);
            Vector2 origin = new(0, frame.Size().Y * 0.5f);

            spriteBatch.Draw(cordTexture.Value, p.Position - Main.screenPosition, frame, lighting, rot, origin, 1f, 0, 0);
        }
    }

    public override void DrawOnRopeEnds(SpriteBatch spriteBatch, Vector2 position, float rotation)
    {
        Rectangle frame = cordTexture.Frame(1, 2, frameY: 1);
        Color lighting = Lighting.GetColor(position.ToTileCoordinates());

        spriteBatch.Draw(cordTexture.Value, position - Main.screenPosition, frame, lighting, rotation + PiOver2, frame.Size() * 0.5f, 1f, 0, 0);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.SilkRopeCoil, 3)
            .AddTile(TileID.Loom)
            .Register();
    }
}
