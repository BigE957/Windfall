using CalamityMod.Schematics;
using System.Reflection;
using static CalamityMod.Schematics.SchematicManager;

namespace Windfall.Common.Systems
{
    public class WFSchematicManager : ModSystem
    {
        private const string StructureFilePath = "Content/World/Schematics/";

        internal const string WanderersCabinKey = "Wanderers Cabin";
        internal const string WanderersCabinKeyFilename = StructureFilePath + "WanderersCabin.csch";

        internal const string LunarCultKey = "Lunar Cult Base";
        internal const string LunarCultKeyFileName = StructureFilePath + "LunarCultBase.csch";
        internal static Dictionary<string, SchematicMetaTile[,]> TileMaps =>
            typeof(SchematicManager).GetField("TileMaps", Utilities.UniversalBindingFlags).GetValue(null) as Dictionary<string, SchematicMetaTile[,]>;

        internal static Dictionary<string, PilePlacementFunction> PilePlacementMaps;

        internal static readonly MethodInfo ImportSchematicMethod = typeof(CalamitySchematicIO).GetMethod("ImportSchematic", Utilities.UniversalBindingFlags);


        public override void OnModLoad()
        {
            TileMaps[WanderersCabinKey] = LoadWindfallSchematic(WanderersCabinKeyFilename);
            //TileMaps[SummoningGroundsKey] = LoadWindfallSchematic(SummoningGroundsKeyFilename);
            TileMaps[LunarCultKey] = LoadWindfallSchematic(LunarCultKeyFileName);
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
