namespace Windfall.Content.Items.Placeables.Furnature.VerletHangers.Twine;
public class TwineID : ModSystem
{
    public static readonly List<int> DecorationIDs = [];

    public override void OnModLoad()
    {
        DecorationIDs.Clear();
        DecorationIDs.Add(-1);
        DecorationIDs.Add(ModContent.ItemType<SelenicTwine>());
    }

    public const byte Twine = 0;
    public const byte SelenicTwine = 1;

    public static Twine GetTwine(int type)
    {
        switch (type)
        {
            case SelenicTwine: return new SelenicTwine();
            default: return new SelenicTwine();
        };
    }
}
