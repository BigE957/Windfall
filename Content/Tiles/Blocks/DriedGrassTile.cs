using CalamityMod.Dusts;
using CalamityMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.Metadata;
using Windfall.Content.Items.Placeables.Tiles;

namespace Windfall.Content.Tiles.Blocks;
public class DriedGrassTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileBrick[Type] = true;
        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Grass"]);

        CalamityUtils.SetMerge(Type, ModContent.TileType<DriedMudTile>());
        CalamityUtils.SetMerge(Type, ModContent.TileType<NectarbrickTile>());
        /*
        CalamityUtils.SetMerge(Type, TileID.Grass);
        CalamityUtils.SetMerge(Type, TileID.CorruptGrass);
        CalamityUtils.SetMerge(Type, TileID.HallowedGrass);
        CalamityUtils.SetMerge(Type, TileID.CrimsonGrass);
        */

        DustType = DustID.Hay;
        RegisterItemDrop(ModContent.ItemType<DriedMud>());

        AddMapEntry(new Color(133, 109, 140));

        TileID.Sets.Grass[Type] = true;
        TileID.Sets.Conversion.Grass[Type] = true;

        //Grass framing (<3 terraria devs)
        TileID.Sets.NeedsGrassFraming[Type] = true;
        TileID.Sets.NeedsGrassFramingDirt[Type] = ModContent.TileType<DriedMudTile>();
        TileID.Sets.CanBeDugByShovel[Type] = true;
    }
}
