using CalamityMod.Particles;
using Windfall.Content.Tiles.Furnature.VerletHangers.Hangers;
using Windfall.Content.Tiles.TileEntities;
using static Windfall.Common.Graphics.Verlet.VerletIntegration;

namespace Windfall.Content.Items.Placeables.Furnature.VerletHangers.Cords;
public abstract class Cord : ModItem
{
    public virtual int cordID => 0;

    public virtual string cordTexturePath => "";

    public static Asset<Texture2D> cordTexture;

    public virtual void DrawRopeSegment(SpriteBatch spriteBatch, List<VerletPoint> points, int index) { }

    public virtual void DrawDecorationSegment(SpriteBatch spriteBatch, List<VerletPoint> points, int index) { DrawRopeSegment(spriteBatch, points, index); }

    public virtual void DrawOnRopeEnds(SpriteBatch spriteBatch, Vector2 position, float rotation) { }

    public bool PlacingInProgress = false;
    public HangerEntity StartingEntity = null;

    public override void SetStaticDefaults()
    {
        if(!Main.dedServ)
            cordTexture = ModContent.Request<Texture2D>(cordTexturePath);
    }

    public override bool AltFunctionUse(Player player) => true;

    public override bool? UseItem(Player player)
    {
        if (PlacingInProgress && player.altFunctionUse == 2)
        {
            PlacingInProgress = false;
            StartingEntity.State = 0;
            StartingEntity = null;
            return true;
        }

        Point16 PlacementPoint = Main.MouseWorld.ToTileCoordinates16();
        Tile placementTile = Main.tile[PlacementPoint];
        if (!placementTile.HasTile || Main.tile[PlacementPoint].TileType != ModContent.TileType<HangerTile>())
            return true;

        if (PlacingInProgress)
        {
            if (StartingEntity == null || PlacementPoint == StartingEntity.Position)
            {
                PlacingInProgress = false;
                return true;
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
                StartingEntity.SegmentCount = (int)Math.Ceiling((StringEnd - StringStart).Length() / 15f);
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
        return true;
    }

    public override void HoldItem(Player player)
    {
        if (PlacingInProgress && StartingEntity != null)
        {
            Vector2 StringStart = StartingEntity.Position.ToWorldCoordinates();
            Vector2 StringEnd = Main.MouseWorld;

            Particle particle;

            for (int i = 0; i < 8; i++)
            {
                particle = new GlowOrbParticle(Vector2.Lerp(StringStart, StringEnd, i / 8f), Vector2.Zero, false, 2, 0.5f, Color.White);
                GeneralParticleHandler.SpawnParticle(particle);
            }
            particle = new GlowOrbParticle(StringStart, Vector2.Zero, false, 2, 0.5f, Color.White);
            GeneralParticleHandler.SpawnParticle(particle);
            particle = new GlowOrbParticle(StringEnd, Vector2.Zero, false, 2, 0.5f, Color.White);
            GeneralParticleHandler.SpawnParticle(particle);
        }
    }
}
