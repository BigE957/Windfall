using System;
using Windfall.Content.Tiles.TileEntities;
using static Windfall.Common.Graphics.Verlet.VerletIntegration;

namespace Windfall.Content.Items.Placeables.Furnature.VerletHangers.Decorations;
public abstract class Decoration : ModItem
{
    public virtual int DecorID => 0;

    public virtual void UpdateDecoration(Vector2[] segmentPositions) { }

    public virtual void DrawAlongVerlet(SpriteBatch spriteBatch, int index, Vector2[] segmentPositions) { }

    public virtual void DrawOnVerletEnd(SpriteBatch spriteBatch, Vector2[] segmentPositions) { }

    public HangerEntity HE = null;
    public int HangIndex = -1;

    public override bool? UseItem(Player player)
    {
        if (HangIndex == -2 && ((Main.MouseWorld - HE.Position.ToWorldCoordinates()).LengthSquared() > 625))
        {
            HE = null;
            HangIndex = -1;
            return true;
        }

        if (HangIndex != -1)
        {
            if (HE != null && HE.DecorationVerlets.ContainsKey(HangIndex))
            {
                HangIndex = -1;
                return true;
            }

            if (HangIndex != -2 && (Main.MouseWorld - HE.MainVerlet[HangIndex].Position).LengthSquared() > 625)
            {
                HE = null;
                HangIndex = -1;
                return true;
            }

            Vector2 startPos = HangIndex == -2 ? HE.Position.ToWorldCoordinates() : HE.MainVerlet[HangIndex].Position;
            VerletObject newVerlet = CreateVerletChain(startPos, startPos + (Vector2.UnitY * 120f), 8, 15);

            if (HangIndex == -2)
            {
                HE.DecorationVerlets.Add(-1, new(newVerlet, DecorID, 8));
            }
            else
                HE.DecorationVerlets.Add(HangIndex, new(newVerlet, DecorID, 8));
            HE.SendSyncPacket();
        }
        return true;
    }
}
