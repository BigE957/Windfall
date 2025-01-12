using Luminance.Common.VerletIntergration;
using System;
using System.Collections.Generic;
using Windfall.Content.Tiles.TileEntities;

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

            List<VerletSegment> newVerlet = [];
            for (int k = 0; k < 8; k++)
            {
                if(HangIndex == -2)
                    newVerlet.Add(new VerletSegment(Vector2.Lerp(HE.Position.ToWorldCoordinates(), HE.Position.ToWorldCoordinates() + Vector2.UnitY * 96f, k / 8f), Vector2.Zero, false));
                else
                    newVerlet.Add(new VerletSegment(Vector2.Lerp(HE.MainVerlet[HangIndex].Position, HE.MainVerlet[HangIndex].Position + Vector2.UnitY * 96f, k / 8f), Vector2.Zero, false));
                newVerlet[k].OldPosition = newVerlet[k].Position;
                newVerlet[k].Velocity.Y = 19;
            }
            newVerlet[0].Locked = true;
            if (HangIndex == -2)
            {
                HE.DecorationVerlets.Add(-1, new(newVerlet, DecorID, 8));
            }
            else
                HE.DecorationVerlets.Add(HangIndex, new(newVerlet, DecorID, 8));
        }
        return true;
    }
}
