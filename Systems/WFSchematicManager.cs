using System.Collections.Generic;
using Terraria.ModLoader;
using CalamityMod.Schematics;
using System.IO;
using static CalamityMod.Schematics.SchematicManager;
using System.Reflection;
using Windfall.Utilities;

namespace Windfall.Systems
{
    public class WFSchematicManager : ModSystem
    {
        internal const string WanderersCabinKey = "Wanderers Cabin";
        internal const string WanderersCabinKeyFilename = "Schematics/WanderersCabin.csch";
        internal static Dictionary<string, SchematicMetaTile[,]> TileMaps =>
            typeof(SchematicManager).GetField("TileMaps", Utilities.Utilities.UniversalBindingFlags).GetValue(null) as Dictionary<string, SchematicMetaTile[,]>;

        internal static Dictionary<string, PilePlacementFunction> PilePlacementMaps;

        internal static readonly MethodInfo ImportSchematicMethod = typeof(CalamitySchematicIO).GetMethod("ImportSchematic", Utilities.Utilities.UniversalBindingFlags);


        public override void OnModLoad()
        {
            TileMaps["Wanderers Cabin"] = LoadWindfallSchematic("Schematics/WanderersCabin.csch");

        }
        public static SchematicMetaTile[,] LoadWindfallSchematic(string filename)
        {
            SchematicMetaTile[,] ret = null;
            using (Stream st = Windfall.Instance.GetFileStream(filename, true))
                ret = (SchematicMetaTile[,])ImportSchematicMethod.Invoke(null, new object[] { st });
            return ret;
        }
    }
}
