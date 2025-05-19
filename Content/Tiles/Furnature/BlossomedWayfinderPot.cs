using Windfall.Content.Items.Placeables.Furnature.Plaques;
using Windfall.Content.Items.Quests.SealingRitual;

namespace Windfall.Content.Tiles.Furnature;
public class BlossomedWayfinderPot : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = false;
        Main.tileNoAttach[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileLavaDeath[Type] = false;
        Main.tileWaterDeath[Type] = false;
        Main.tileFrameImportant[Type] = true;
        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.addTile(Type);

        DustType = DustID.Stone;

        HitSound = SoundID.Dig;

        AddMapEntry(new Color(127, 111, 144));
    }

    public override void MouseOver(int i, int j)
    {
        Tile tile = Main.tile[i, j];
        if (tile.TileFrameX != 0)
            return;

        Tile aboveTile = Main.tile[i, j - 1];
        if (aboveTile.TileType == ModContent.TileType<BlossomedWayfinderPot>())
        { 
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = ModContent.ItemType<BlossomedWayfinder>();
        }
    }

    public override bool RightClick(int i, int j)
    {
        if (Main.LocalPlayer.HeldItem.type != ModContent.ItemType<BlossomedWayfinder>())
            return false;

        Tile tile = Main.tile[i, j];
        if (tile.TileFrameX != 0)
            return false;

        Tile aboveTile = Main.tile[i, j - 1];
        if(aboveTile.TileType == ModContent.TileType<BlossomedWayfinderPot>())
        {
            tile.TileFrameX += 18;
            aboveTile.TileFrameX += 18;

            Vector2 potTop = new Point(i, j).ToWorldCoordinates(autoAddY: 4);
            for (int it = 0; it < 12; it++)
                Dust.NewDustPerfect(potTop + new Vector2(Main.rand.NextFloat(-10, 10), Main.rand.NextFloat(-4, 0)), DustID.Terra, Vector2.UnitY * Main.rand.NextFloat(-3, -1));

            Main.LocalPlayer.HeldItem.stack--;

            return true;
        }
        return false;
    }
}
