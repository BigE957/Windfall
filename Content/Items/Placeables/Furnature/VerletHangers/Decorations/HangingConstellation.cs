using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windfall.Content.Items.Placeables.Furnature.VerletHangers.Decorations;
public class HangingConstellation : Decoration, ILocalizedModType
{
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
        Item.noUseGraphic = true;
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D texture = TextureAssets.Item[Type].Value;
        Rectangle myFrame = texture.Frame(1, 2, 0, 1);
        Vector2 myOrigin = myFrame.Size() * 0.5f;

        spriteBatch.Draw(texture, position, myFrame, drawColor, 0f, myOrigin, scale * 2, 0, 0);
        return false;
    }

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        Texture2D texture = TextureAssets.Item[Type].Value;
        Rectangle myFrame = texture.Frame(1, 2, 0, 1);
        Vector2 myOrigin = myFrame.Size() * 0.5f;

        spriteBatch.Draw(texture, Item.Center - Main.screenPosition, myFrame, alphaColor, rotation, myOrigin, scale * 2, 0, 0);
        return false;
    }

    public override int DecorID => DecorationID.HangingConstellation;

    public override void UpdateDecoration(Vector2[] segmentPositions)
    {
        Lighting.AddLight(segmentPositions[^1], Color.Yellow.ToVector3() * 0.25f);
    }

    public override void DrawAlongVerlet(SpriteBatch spriteBatch, int index, Vector2[] segmentPositions)
    {
        Texture2D tex = ModContent.Request<Texture2D>("Windfall/Content/Items/Placeables/Furnature/VerletHangers/Decorations/HangingConstellation").Value;
        if (index % 3 == 2 && index != 0 && index != segmentPositions.Length - 1)
        {
            Vector2 line = (segmentPositions[index] - segmentPositions[index + 1]);

            Rectangle frame = tex.Frame(1, 2);
            Vector2 origin = frame.Size() * 0.5f;
            Vector2 drawPos = segmentPositions[index] + (line / 2f) + line.SafeNormalize(Vector2.UnitX).RotatedBy(PiOver2) * ((index / 3) % 2 == 0 ? -3 : 3);
            float rotation = (segmentPositions[index + 1] - segmentPositions[index]).ToRotation();

            spriteBatch.Draw(tex, drawPos - Main.screenPosition, frame, Color.White, rotation - PiOver2 + ((index / 3) % 2 == 0 ? -PiOver4 : PiOver4), origin, 1f, 0, 0);
        }
    }

    public override void DrawOnVerletEnd(SpriteBatch spriteBatch, Vector2[] segmentPositions)
    {
        Texture2D texture = ModContent.Request<Texture2D>("Windfall/Content/Items/Placeables/Furnature/VerletHangers/Decorations/HangingConstellation").Value;

        Vector2 line = segmentPositions[^1] - segmentPositions[^2];

        Rectangle frame = texture.Frame(1, 2, 0, 1);
        Vector2 origin = frame.Size() * 0.5f;
        Vector2 drawPos = segmentPositions[^1];
        drawPos += line.SafeNormalize(Vector2.UnitY) * 4f;
        float rotation = line.ToRotation();

        spriteBatch.Draw(texture, drawPos - Main.screenPosition, frame, Color.White, rotation - PiOver2, origin, 1f, 0, 0);
    }
}