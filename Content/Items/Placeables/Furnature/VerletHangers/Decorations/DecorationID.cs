namespace Windfall.Content.Items.Placeables.Furnature.VerletHangers.Decorations;
public class DecorationID : ModSystem
{
    public static readonly List<int> DecorationTypes = [];

    public override void OnModLoad()
    {
        DecorationTypes.Clear();
        DecorationTypes.Add(-1);
        DecorationTypes.Add(ModContent.ItemType<SelenicJewelery>());
        DecorationTypes.Add(ModContent.ItemType<SelenicJewelery>());
        DecorationTypes.Add(ModContent.ItemType<SelenicJewelery>());
        DecorationTypes.Add(ModContent.ItemType<JadeCrescent>());
        DecorationTypes.Add(ModContent.ItemType<SelenicLantern>());
        DecorationTypes.Add(ModContent.ItemType<HangingConstellation>());
        DecorationTypes.Add(ModContent.ItemType<HangingCandle>());
        DecorationTypes.Add(ModContent.ItemType<HangingLamp>());
        DecorationTypes.Add(ModContent.ItemType<HangingLantern>());
    }

    public const byte None = 0;
    public const byte SelenicJeweleryRed = 1;
    public const byte SelenicJeweleryGreen = 2;
    public const byte SelenicJewelerySilver = 3;
    public const byte JadeCrescent = 4;
    public const byte SelenicLantern = 5;
    public const byte HangingConstellation = 6;
    public const byte HangingCandle = 7;
    public const byte HangingLamp = 8;
    public const byte HangingLantern = 9;

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
            case JadeCrescent: return new JadeCrescent();
            case SelenicLantern: return new SelenicLantern();
            case HangingConstellation: return new HangingConstellation();
            case HangingCandle: return new HangingCandle();
            case HangingLamp: return new HangingLamp();
            case HangingLantern: return new HangingLantern();
            default: return null;
        };
    }
}
