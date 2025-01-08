using Luminance.Common.VerletIntergration;
using Windfall.Content.Items.Placeables.Furnature.VerletHangers.Decorations;
using Windfall.Content.Items.Placeables.Furnature.VerletHangers.Twine;
using Windfall.Content.Tiles.TileEntities;

namespace Windfall.Common.Graphics.Verlet;
public class VerletHangerDrawing : ModSystem
{
    public static readonly List<Point16> hangers = [];

    public override void OnModLoad()
    {
        On_Main.DrawProjectiles += DrawHangerVerlets;
    }

    public override void ClearWorld()
    {
        hangers.Clear();
    }

    private void DrawHangerVerlets(On_Main.orig_DrawProjectiles orig, Main self)
    {
        float loadedDist = 1000f;

        List<HangerEntity> ActiveTEs = [];
        foreach (var p16 in hangers)
        {
            HangerEntity h = FindTileEntity<HangerEntity>(p16.X, p16.Y, 1, 1);
            if (h == null)
                continue;

            if (h.State != 1 || !h.PartnerLocation.HasValue || !h.RopeID.HasValue)
                continue;

            bool visable = false;
            foreach (Player p in Main.ActivePlayers)
            {
                if ((h.Position.ToWorldCoordinates() - p.Center).Length() < loadedDist || (h.PartnerLocation.Value.ToWorldCoordinates() - p.Center).Length() < loadedDist)
                    visable = true;
            }
            if (!visable)
                continue;

            ActiveTEs.Add(h);
        }

        //Main.NewText(ActiveTEs.Count);
        if (ActiveTEs.Count == 0)
        {
            orig(self);
            return;
        }

        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

        foreach(HangerEntity te in ActiveTEs)
        {
            List<VerletSegment> TwineVerlet = te.MainVerlet;
            Vector2[] segmentPositions = TwineVerlet.Select(x => x.Position).ToArray();
            
            Twine twine = TwineID.GetTwine(te.RopeID.Value);

            for (int k = 0; k < segmentPositions.Length - 1; k++)
                twine.DrawRopeSegment(Main.spriteBatch, k, segmentPositions);

            foreach(Tuple<List<VerletSegment>, int> Decoration in te.DecorationVerlets.Values.Where(v => v.Item1.Count > 0))
            {
                segmentPositions = Decoration.Item1.Select(x => x.Position).ToArray();

                for (int k = 0; k < segmentPositions.Length - 1; k++)
                {
                    twine.DrawDecorationSegment(Main.spriteBatch, k, segmentPositions);

                    DecorationID.GetDecoration(Decoration.Item2).DrawAlongVerlet(Main.spriteBatch, k, segmentPositions);
                }

                DecorationID.GetDecoration(Decoration.Item2).DrawOnVerletEnd(Main.spriteBatch, segmentPositions);
            }
        }

        Main.spriteBatch.End();

        orig(self);
    }
}
