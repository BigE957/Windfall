using static Windfall.Common.Graphics.Verlet.VerletIntegration;

namespace Windfall.Content.Items.Placeables.Furnature.VerletHangers.Cords;
public class CordedChains : Cord, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";

    public static Asset<Texture2D> cordTexture;

    public override void SetStaticDefaults()
    {
        if (!Main.dedServ)
            cordTexture = ModContent.Request<Texture2D>("Windfall/Content/Items/Placeables/Furnature/VerletHangers/Cords/CordedChainsAtlas");
    }

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

    public override int cordID => CordID.CordedChains;

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

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.RopeCoil, 3)
            .AddTile(TileID.Loom)
            .Register();
    }
}
