namespace Windfall.Content.Items.Placeables.Furnature.VerletHangers.Twine;
public class TwineID : ModSystem
{
    public static readonly List<int> DecorationIDs = [];

    public override void OnModLoad()
    {
        DecorationIDs.Clear();
        DecorationIDs.Add(-1);
        DecorationIDs.Add(ModContent.ItemType<SelenicTwine>());
        DecorationIDs.Add(ModContent.ItemType<CordedTwine>());
    }

    public const byte None = 0;
    public const byte SelenicTwine = 1;
    public const byte CordedTwine = 2;

    public static Twine GetTwine(int type)
    {
        switch (type)
        {
            case SelenicTwine: return new SelenicTwine();
            case CordedTwine: return new CordedTwine();
            default: return null;
        };
    }
}
