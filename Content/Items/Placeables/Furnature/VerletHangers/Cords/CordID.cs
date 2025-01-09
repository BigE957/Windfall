namespace Windfall.Content.Items.Placeables.Furnature.VerletHangers.Twine;
public class CordID : ModSystem
{
    public static readonly List<int> CordTypes = [];

    public override void OnModLoad()
    {
        CordTypes.Clear();
        CordTypes.Add(-1);
        CordTypes.Add(ModContent.ItemType<SelenicTwine>());
        CordTypes.Add(ModContent.ItemType<CordedRope>());
    }

    public const byte None = 0;
    public const byte SelenicTwine = 1;
    public const byte CordedRope = 2;

    public static Cord GetTwine(int type)
    {
        switch (type)
        {
            case SelenicTwine: return new SelenicTwine();
            case CordedRope: return new CordedRope();
            default: return null;
        };
    }
}
