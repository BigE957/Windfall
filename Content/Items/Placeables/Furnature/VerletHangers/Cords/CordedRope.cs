namespace Windfall.Content.Items.Placeables.Furnature.VerletHangers.Twine;
public class CordedRope : Cord, ILocalizedModType
{
    public override string Texture => "CalamityMod/Items/Pets/BrimstoneJewel";

    public new string LocalizationCategory => "Items.Placeables";

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 9999;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.consumable = true;
        Item.rare = ItemRarityID.Lime;
    }

    public override int CordID => 2;

    public override void DrawRopeSegment(SpriteBatch spriteBatch, int index, Vector2[] segmentPositions)
    {
        Texture2D tex = ModContent.Request<Texture2D>("Windfall/Content/Items/Placeables/Furnature/VerletHangers/Cords/CordedRope").Value;
        //Dust.NewDustPerfect(segmentPositions[index], DustID.LifeDrain).velocity = Vector2.Zero;
        Vector2 line;
        if (index == segmentPositions.Length - 1)
            line = segmentPositions[index - 1] - segmentPositions[index];
        else if(index == 0)
            line = segmentPositions[index + 1] - segmentPositions[index];
        else
            line = segmentPositions[index] - segmentPositions[index + 1];
            
        Color lighting = Lighting.GetColor(segmentPositions[index].ToTileCoordinates());

        spriteBatch.Draw(tex, segmentPositions[index] - Main.screenPosition, null, lighting, line.ToRotation(), new(0, tex.Size().Y * 0.5f), new Vector2(1.33f, 1f), 0, 0);
        //spriteBatch.DrawLineBetter(segmentPositions[index], segmentPositions[index + 1], new Color(199, 175, 84).MultiplyRGB(lighting), 3);
    }
}