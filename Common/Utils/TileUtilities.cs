namespace Windfall.Common.Utils
{
    public static partial class WindfallUtils
    {
        /// <summary>
        /// Checks if a chest contains a given ItemType and returns that items index if it finds it. Otherwise, returns -1.
        /// </summary>
        public static int ChestContains(Chest chest, int type)
        {
            for (int inventoryIndex = 0; inventoryIndex < 40; inventoryIndex++)
                if (chest.item[inventoryIndex].type == type)
                    return inventoryIndex;
            return -1;
        }
    }
}
