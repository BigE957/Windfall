using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windfall.Content.Items.Placeables.Furnature.VerletHangers.Twine;
public class SelenicTwine : Twine, ILocalizedModType
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

    public override int TwineID => 1;

    public override void DrawRopeSegment(SpriteBatch spriteBatch, int index, Vector2[] segmentPositions)
    {
        Vector2 line = (segmentPositions[index] - segmentPositions[index + 1]);
        Vector2 norLine = line.SafeNormalize(Vector2.Zero);
        Color lighting = Lighting.GetColor((segmentPositions[index + 1] + (line / 2f)).ToTileCoordinates());

        spriteBatch.DrawLineBetter(segmentPositions[index] + norLine.RotatedBy(PiOver2), segmentPositions[index + 1] + norLine.RotatedBy(PiOver2), new Color(42, 42, 42).MultiplyRGB(lighting), 6);

        spriteBatch.DrawLineBetter(segmentPositions[index] + norLine.RotatedBy(-PiOver2), segmentPositions[index + 1] + norLine.RotatedBy(-PiOver2), new Color(97, 75, 19).MultiplyRGB(lighting), 6);

        spriteBatch.DrawLineBetter(segmentPositions[index], segmentPositions[index + 1], new Color(199, 175, 84).MultiplyRGB(lighting), 3);
    }

    public override void DrawDecorationSegment(SpriteBatch spriteBatch, int index, Vector2[] segmentPositions)
    {
        Vector2 line = (segmentPositions[index] - segmentPositions[index + 1]);
        Color lighting = Lighting.GetColor((segmentPositions[index + 1] + (line / 2f)).ToTileCoordinates());
        spriteBatch.DrawLineBetter(segmentPositions[index], segmentPositions[index + 1], new Color(97, 75, 19).MultiplyRGB(lighting), 6);

        spriteBatch.DrawLineBetter(segmentPositions[index], segmentPositions[index + 1], new Color(199, 175, 84).MultiplyRGB(lighting), 3);
    }
}
