using Luminance.Common.VerletIntergration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windfall.Content.Tiles.Furnature.VerletHangers.Hangers;
using Windfall.Content.Tiles.TileEntities;

namespace Windfall.Content.Items.Placeables.Furnature.VerletHangers.Decorations;
public abstract class Decoration : ModItem
{
    public virtual int DecorID => 0;
    public virtual string DecorAtlas => "";

    public virtual void DrawAlongVerlet(SpriteBatch spriteBatch, int index, Vector2[] segmentPositions) { }

    public virtual void DrawOnVerletEnd(SpriteBatch spriteBatch, Vector2[] segmentPositions) { }

    public bool PlacingInProgress = false;
    public HangerEntity StartingEntity = null;
    public int HangIndex = -1;

    public override void UseAnimation(Player player)
    {
        if (StartingEntity != null && StartingEntity.DecorationVerlets.ContainsKey(HangIndex))
            HangIndex = -1;
        if (HangIndex != -1)
        {
            List<VerletSegment> newVerlet = [];
            for (int k = 0; k < 20; k++)
                newVerlet.Add(new VerletSegment(Vector2.Lerp(StartingEntity.MainVerlet[HangIndex].Position, StartingEntity.MainVerlet[HangIndex].Position + Vector2.UnitY * 150f, k / 10), Vector2.Zero, false));
            newVerlet[0].Locked = true;
            StartingEntity.DecorationVerlets.Add(HangIndex, new(newVerlet, 1));
            return;
        }
    }
}
