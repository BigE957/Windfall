namespace Windfall.Content.Tiles.Blocks;
public class NectarbrickTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileMergeDirt[Type] = false;
        Main.tileBlockLight[Type] = true;
        HitSound = SoundID.Tink;
        AddMapEntry(Color.RosyBrown);
    }

    public override bool CreateDust(int i, int j, ref int type)
    {
        Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.UnusedBrown);
        return false;
    }
}
