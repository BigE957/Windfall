using static Windfall.Common.Graphics.Verlet.VerletIntegration;

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

    public override string cordTexturePath => "Windfall/Content/Items/Placeables/Furnature/VerletHangers/Cords/SelenicTwineAtlas";

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
            Vector2 origin = new(0, cordTexture.Size().Y * 0.5f);

            spriteBatch.Draw(cordTexture.Value, p.Position - Main.screenPosition, null, lighting, rot, origin, 1f, 0, 0);
        }
    }
}
