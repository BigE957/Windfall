using Windfall.Content.Tiles.Furnature.VerletHangers.Hangers;
using Windfall.Content.Tiles.TileEntities;

namespace Windfall.Content.Items.Placeables.Furnature.VerletHangers.Cords;
public abstract class Cord : ModItem
{
    public virtual int cordID => 0;

    public virtual void DrawRopeSegment(SpriteBatch spriteBatch, int index, Vector2[] segmentPositions) { }

    public virtual void DrawDecorationSegment(SpriteBatch spriteBatch, int index, Vector2[] segmentPositions) { DrawRopeSegment(spriteBatch, index, segmentPositions); }

    public virtual void DrawOnRopeEnds(SpriteBatch spriteBatch, Vector2 position, float rotation) { }

    public bool PlacingInProgress = false;
    public HangerEntity StartingEntity = null;

    public override bool AltFunctionUse(Player player) => true;

    public override void UseAnimation(Player player)
    {
        if (PlacingInProgress && player.altFunctionUse == 2)
        {
            PlacingInProgress = false;
            StartingEntity.State = 0;
            StartingEntity = null;
            return;
        }

        Point16 PlacementPoint = Main.MouseWorld.ToTileCoordinates16();
        Tile placementTile = Main.tile[PlacementPoint];
        if (!placementTile.HasTile || Main.tile[PlacementPoint].TileType != ModContent.TileType<HangerTile>())
            return;

        if (PlacingInProgress)
        {
            if (StartingEntity == null || PlacementPoint == StartingEntity.Position)
            {
                PlacingInProgress = false;
                return;
            }
            //TileEntity.PlaceEntityNet(PlacementPoint.X, PlacementPoint.Y, ModContent.TileEntityType<HangerEntity>());
            HangerEntity end = FindTileEntity<HangerEntity>(PlacementPoint.X, PlacementPoint.Y, 1, 1);
            if (end != null)
            {
                end.State = 2;
                end.PartnerLocation = StartingEntity.Position;
                StartingEntity.PartnerLocation = PlacementPoint;
                Vector2 StringStart = StartingEntity.Position.ToWorldCoordinates();
                Vector2 StringEnd = StartingEntity.PartnerLocation.Value.ToWorldCoordinates();
                StartingEntity.Distance = (StringEnd - StringStart).Length() / 8;
                //Main.NewText(TwineID);
                StartingEntity.CordID = (byte?)cordID;
            }
            else
            {
                Main.NewText("WOMPITY WOMP WOMP");
                //return false;
            }
            PlacingInProgress = false;
            StartingEntity = null;
            Item.stack--;
        }
        else
        {
            StartingEntity = FindTileEntity<HangerEntity>(PlacementPoint.X, PlacementPoint.Y, 1, 1);
            PlacingInProgress = true;
            //TileEntity.PlaceEntityNet(StartingPoint.X, StartingPoint.Y, ModContent.TileEntityType<HangerEntity>());
            if (StartingEntity != null)
                StartingEntity.State = 1;
            else
                Main.NewText("WOMPITY WOMP WOMP");
        }
    }

    public override void HoldItem(Player player)
    {
        if (PlacingInProgress)
        {
            Vector2 StringStart = StartingEntity.Position.ToWorldCoordinates();
            Vector2 StringEnd = Main.MouseWorld;

            Particle particle;

            for (int i = 0; i < 8; i++)
            {
                particle = new GlowOrbParticle(Vector2.Lerp(StringStart, StringEnd, i / 8f), Vector2.Zero, false, 4, 0.5f, Color.White);
                GeneralParticleHandler.SpawnParticle(particle);
            }
            particle = new GlowOrbParticle(StringStart, Vector2.Zero, false, 4, 0.5f, Color.White);
            GeneralParticleHandler.SpawnParticle(particle);
            particle = new GlowOrbParticle(StringEnd, Vector2.Zero, false, 4, 0.5f, Color.White);
            GeneralParticleHandler.SpawnParticle(particle);
        }
    }
}
