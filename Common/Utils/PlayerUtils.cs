namespace Windfall.Common.Utils;
public static partial class WindfallUtils
{
    public static bool InventoryHas(this Player player, params int[] items) => player.inventory.Any((Item item) => items.Contains(item.type));

    public static Item HeldItem(this Player player)
    {
        if (!Main.mouseItem.IsAir)
        {
            return Main.mouseItem;
        }

        return player.HeldItem;
    }
}
