using Terraria.ModLoader.IO;

namespace Windfall.Common.Systems.WorldEvents;
public class DraconicRuinsSystem : ModSystem
{
    public static Point DraconicRuinsLocation = new(-1,-1);
    public static bool FacingLeft = false;
    public static Point RuinsEntrance => new(DraconicRuinsLocation.X + (FacingLeft ? -19 : 19), DraconicRuinsLocation.Y - 20);
    public override void ClearWorld()
    {
        DraconicRuinsLocation = new(-1, -1);
        FacingLeft = false;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        tag["DraconicRuinsLocation"] = DraconicRuinsLocation;
        tag["FacingLeft"] = FacingLeft;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        DraconicRuinsLocation = tag.Get<Point>("DraconicRuinsLocation");
        FacingLeft = tag.GetBool("FacingLeft");
    }

    public override void PreUpdateWorld()
    {
        Dust.NewDustPerfect(RuinsEntrance.ToWorldCoordinates(), DustID.Terra, Vector2.Zero);
    }
}
