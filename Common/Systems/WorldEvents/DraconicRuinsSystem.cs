using Luminance.Core.Graphics;
using Terraria.ModLoader.IO;

namespace Windfall.Common.Systems.WorldEvents;
public class DraconicRuinsSystem : ModSystem
{
    public static Point DraconicRuinsLocation = new(-1,-1);
    public static bool FacingLeft = false;
    public static Point RuinsEntrance => new(DraconicRuinsLocation.X + (FacingLeft ? -19 : 19), DraconicRuinsLocation.Y - 20);
    public static Point TabletRoom => new(DraconicRuinsLocation.X + (FacingLeft ? 9 : -9), DraconicRuinsLocation.Y + 46);

    public static bool CutsceneActive = false;
    public static bool ZoomActive = false;

    private static int CutsceneTime = 0;
    private static int CameraTime = 0;

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
        Dust.NewDustPerfect(TabletRoom.ToWorldCoordinates(), DustID.Shadowflame, Vector2.Zero);

        if (ZoomActive)
        {
            if (CameraTime < 60)
                CameraTime++;
            else
                CameraTime = 60;

            if (CameraTime > 0)
            {
                CameraPanSystem.PanTowards(TabletRoom.ToWorldCoordinates() - new Vector2(0, 120), CircOutEasing(CameraTime / 60f, 1));

                CameraPanSystem.Zoom = CircOutEasing(CameraTime / 60f, 1) / 2f;

                if (CutsceneTime > 240)
                {
                    ZoomActive = false;
                    CutsceneActive = false;
                    CutsceneTime = 0;
                    CameraTime = 0;
                }
            }
        }

        if (CutsceneActive)
            CutsceneTime++;
    }
}
