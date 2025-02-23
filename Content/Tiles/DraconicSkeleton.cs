using Windfall.Content.Items.Placeables;
using Windfall.Content.Items.Quests.SealingRitual;

namespace Windfall.Content.Tiles;
public class DraconicSkeleton : ModTile
{
    public override string Texture => "Windfall/Assets/Tiles/DraconicSkeleton";

    public override void SetStaticDefaults()
    {
        RegisterItemDrop(ModContent.ItemType<DraconicSkeletonItem>());
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = false;
        Main.tileWaterDeath[Type] = false;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
        TileObjectData.newTile.Width = 16;
        TileObjectData.newTile.Height = 7;

        TileObjectData.newTile.Origin = new Point16(8, 7);
        TileObjectData.newTile.CoordinateHeights = new int[7];
        for (int i = 0; i < 7; i++)
        {
            TileObjectData.newTile.CoordinateHeights[i] = 16;
        }
        //TileObjectData.newTile.CoordinateHeights[22] = 18;
        TileObjectData.newTile.DrawYOffset = 8;

        TileObjectData.newTile.CoordinatePadding = 0;
        //TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;
        //TileObjectData.newTile.StyleWrapLimit = 2;
        //TileObjectData.newTile.StyleMultiplier = 2;
        //TileObjectData.newTile.StyleHorizontal = true;
        //TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        //TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
        //TileObjectData.addAlternate(1);

        TileObjectData.addTile(Type);

        MinPick = 225;
        DustType = DustID.Stone;
        AddMapEntry(new Color(128, 128, 128));
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {

        //effectOnly = true; 
        //noItem = true;
        int boneType = ModContent.ItemType<DraconicBone>();
        if (/*TravellingCultist.QuestArtifact.Type == ModContent.ItemType<DraconicBone>() &&*/ !Main.item.Any(item => item.active && item.type == boneType) && !Main.LocalPlayer.inventory.Any(item => item.active && item.type == boneType))
            Item.NewItem(Entity.GetSource_None(), new Point(i, j).ToWorldCoordinates(), boneType);
    }
}

