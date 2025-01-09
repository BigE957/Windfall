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

    public HangerEntity StartingEntity = null;
    public int HangIndex = -1;

    public override void UseAnimation(Player player)
    {
        if (StartingEntity != null && StartingEntity.DecorationVerlets.ContainsKey(HangIndex))
            HangIndex = -1;
        if (HangIndex != -1)
        {
            List<VerletSegment> newVerlet = [];
            for (int k = 0; k < 16; k++)
            {
                if(HangIndex == -2)
                    newVerlet.Add(new VerletSegment(Vector2.Lerp(StartingEntity.Position.ToWorldCoordinates(), StartingEntity.Position.ToWorldCoordinates() + Vector2.UnitY * 150f, k / 10), Vector2.Zero, false));
                else
                    newVerlet.Add(new VerletSegment(Vector2.Lerp(StartingEntity.MainVerlet[HangIndex].Position, StartingEntity.MainVerlet[HangIndex].Position + Vector2.UnitY * 150f, k / 10), Vector2.Zero, false));
                newVerlet[k].OldPosition = newVerlet[k].Position;
                newVerlet[k].Velocity.Y = 19;
            }
            newVerlet[0].Locked = true;
            if (HangIndex == -2)
            {
                StartingEntity.DecorationVerlets.Add(-1, new(newVerlet, DecorID, 16));
            }
            else
                StartingEntity.DecorationVerlets.Add(HangIndex, new(newVerlet, DecorID, 16));
            return;
        }
    }
}
