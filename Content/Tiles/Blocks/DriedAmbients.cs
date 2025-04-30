using CalamityMod.Dusts;
using CalamityMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.Metadata;
using Windfall.Content.Items.Placeables;
using Windfall.Content.Items.Weapons.Rogue;

namespace Windfall.Content.Tiles.Blocks;
public class DriedAmbients : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileCut[Type] = true;
        Main.tileSolid[Type] = false;
        Main.tileNoAttach[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileWaterDeath[Type] = true;
        Main.tileFrameImportant[Type] = true;
        TileID.Sets.ReplaceTileBreakUp[Type] = true;
        TileID.Sets.SwaysInWindBasic[Type] = true;
        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);

        DustType = DustID.Hay;

        HitSound = SoundID.Grass;

        AddMapEntry(new Color(127, 111, 144));

        FlexibleTileWand.RubblePlacementSmall.AddVariations(ModContent.ItemType<DriedSeeds>(), Type, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);

        base.SetStaticDefaults();
    }

    public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
    {
        offsetY = 2;
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j)
    {
        Vector2 worldPosition = new Vector2(i, j).ToWorldCoordinates();
        Player nearestPlayer = Main.player[Player.FindClosest(worldPosition, 16, 16)];
        if (nearestPlayer.active)
        {
            if (nearestPlayer.ActiveItem().type == ItemID.Sickle)
                yield return new Item(ItemID.Hay, Main.rand.Next(1, 2 + 1));

            if (Main.rand.NextBool(20))
                yield return new Item(ModContent.ItemType<DriedSeeds>());
        }
    }
}
