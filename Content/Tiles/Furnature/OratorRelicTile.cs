using CalamityMod.Tiles.BaseTiles;
using Windfall.Content.Items.Placeables.Furnature;

namespace Windfall.Content.Tiles.Furnature;
public class OratorRelicTile : BaseBossRelic
{
    public override string RelicTextureName => "Windfall/Content/Tiles/Furnature/OratorRelicTile";

    public override int AssociatedItem => ModContent.ItemType<OratorRelic>();
}
