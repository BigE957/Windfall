using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windfall.Common.Utils;
public static partial class WindfallUtils
{
    public static LocalizedText GetItemName<T>() where T : ModItem
    {
        return GetTextFromModItem(ModContent.ItemType<T>(), "DisplayName");
    }

    public static LocalizedText GetTextFromModItem(int itemID, string suffix)
    {
        return ItemLoader.GetItem(itemID).GetLocalization(suffix);
    }
}
