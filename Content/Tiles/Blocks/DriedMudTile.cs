using CalamityMod;

namespace Windfall.Content.Tiles.Blocks;
internal class DriedMudTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = false;
        Main.tileBlockLight[Type] = true;
        AddMapEntry(Color.Brown);

        CalamityUtils.MergeWithGeneral(Type);
        CalamityUtils.MergeWithOres(Type);
        CalamityUtils.SetMerge(Type, ModContent.TileType<NectarbrickTile>());
    }

    public override bool CreateDust(int i, int j, ref int type)
    {
        Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.Dirt);
        return false;
    }
}
