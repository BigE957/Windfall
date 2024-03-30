using CalamityMod.Schematics;
using System.Reflection;
using static CalamityMod.Schematics.SchematicManager;

namespace Windfall.Common.Systems
{
    public class WFSchematicManager : ModSystem
    {
        internal const string WanderersCabinKey = "Wanderers Cabin";
        internal const string WanderersCabinKeyFilename = "Content/Schematics/WanderersCabin.csch";
        internal const string SummoningGroundsKey = "Summoning Grounds";
        internal const string SummoningGroundsKeyFilename = "Content/Schematics/SummoningGrounds.csch";
        internal const string SolarCultistKey = "Solar Hideout";
        internal const string SolarCultistKeyFileName = "Content/Schematics/SolarCultistHideout.csch";
        internal static Dictionary<string, SchematicMetaTile[,]> TileMaps =>
            typeof(SchematicManager).GetField("TileMaps", Utilities.WindfallUtils.UniversalBindingFlags).GetValue(null) as Dictionary<string, SchematicMetaTile[,]>;

        internal static Dictionary<string, PilePlacementFunction> PilePlacementMaps;

        internal static readonly MethodInfo ImportSchematicMethod = typeof(CalamitySchematicIO).GetMethod("ImportSchematic", Utilities.WindfallUtils.UniversalBindingFlags);


        public override void OnModLoad()
        {
            TileMaps["Wanderers Cabin"] = LoadWindfallSchematic(WanderersCabinKeyFilename);
            TileMaps["Summoning Grounds"] = LoadWindfallSchematic(SummoningGroundsKeyFilename);
            TileMaps["Solar Hideout"] = LoadWindfallSchematic(SolarCultistKeyFileName);
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
