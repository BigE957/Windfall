using static Windfall.Common.Systems.WorldEvents.LunarCultBaseSystem;

namespace Windfall.Content.Items.GlobalItems;

public class LunarCultGlobalItem : GlobalItem
{
    public override bool InstancePerEntity => true;

    public bool madeDuringCafeteriaActivity = false;
    public bool madeDuringTailorActivity = false;

    public override void OnCreated(Item item, ItemCreationContext context)
    {
        madeDuringCafeteriaActivity = IsCafeteriaActivityActive();
        madeDuringTailorActivity = IsTailorActivityActive();
    }

    public override bool CanUseItem(Item item, Player player)
    {            
        bool inCultBase = CultBaseArea.Contains(player.Center.ToTileCoordinates());
        bool illegalCultBaseItem = item.type is ItemID.Sandgun or ItemID.DirtBomb or ItemID.DirtStickyBomb or ItemID.DryBomb;
        if (illegalCultBaseItem && inCultBase)
            return false;

        //CultBaseArea.Inflate(2, 2);
        if ((item.createTile != -1 || item.createWall != -1) && (CultBaseArea.Contains(player.Calamity().mouseWorld.ToTileCoordinates())))
            return false;

        return base.CanUseItem(item, player);
    }
}
