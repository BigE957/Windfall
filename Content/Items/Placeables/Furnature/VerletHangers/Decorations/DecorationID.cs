namespace Windfall.Content.Items.Placeables.Furnature.VerletHangers.Decorations;
public class DecorationID : ModSystem
{
    public static readonly List<int> DecorationIDs = [];

    public override void OnModLoad()
    {
        DecorationIDs.Clear();
        DecorationIDs.Add(-1);
        DecorationIDs.Add(ModContent.ItemType<SelenicJewelery>());
    }

    public const byte Twine = 0;
    public const byte SelenicJeweleryRed = 1;
    public const byte SelenicJeweleryGreen = 2;
    public const byte SelenicJewelerySilver = 3;

    public static Decoration GetDecoration(int type)
    {
        switch(type)
        {
            case SelenicJeweleryRed:
                SelenicJewelery sj = new()
                {
                    colorType = SelenicJewelery.ColorType.Red
                };
                return sj;
            case SelenicJeweleryGreen:
                sj = new()
                {
                    colorType = SelenicJewelery.ColorType.Green
                };
                return sj;
            case SelenicJewelerySilver:
                sj = new()
                {
                    colorType = SelenicJewelery.ColorType.Silver
                };
                return sj;
            default: return new SelenicJewelery();
        };
    }
}
