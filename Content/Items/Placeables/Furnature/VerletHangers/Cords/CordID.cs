using Windfall.Content.Items.Placeables.Furnature.VerletHangers.Cords;

namespace Windfall.Content.Items.Placeables.Furnature.VerletHangers.Cords;
public class CordID : ModSystem
{
    public static readonly List<int> CordTypes = [];

    public override void OnModLoad()
    {
        CordTypes.Clear();
        CordTypes.Add(-1);
        CordTypes.Add(ModContent.ItemType<SelenicTwine>());
        CordTypes.Add(ModContent.ItemType<CordedRope>());
        CordTypes.Add(ModContent.ItemType<CordedSilk>());
        CordTypes.Add(ModContent.ItemType<CordedVines>());
        CordTypes.Add(ModContent.ItemType<CordedWebs>());
    }

    public const byte None = 0;
    public const byte SelenicTwine = 1;
    public const byte CordedRope = 2;
    public const byte CordedSilk = 3;
    public const byte CordedVines = 4;
    public const byte CordedWebs = 5;

    public static Cord GetTwine(int type)
    {
        switch (type)
        {
            case SelenicTwine: return new SelenicTwine();
            case CordedRope: return new CordedRope();
            case CordedSilk: return new CordedSilk();
            case CordedVines: return new CordedVines();
            case CordedWebs: return new CordedWebs();
            default: return null;
        };
    }
}
