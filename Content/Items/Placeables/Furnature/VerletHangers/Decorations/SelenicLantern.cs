﻿namespace Windfall.Content.Items.Placeables.Furnature.VerletHangers.Decorations;
public class SelenicLantern : Decoration, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 9999;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useTime = Item.useAnimation = 15;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.consumable = true;
        Item.rare = ItemRarityID.Lime;
        Item.noUseGraphic = true;
    }

    public override int DecorID => DecorationID.SelenicLantern;

    public override void UpdateDecoration(Vector2[] segmentPositions)
    {
        Lighting.AddLight(segmentPositions[^1], Color.Yellow.ToVector3() * 0.75f);
        if (Math.Floor(Main.GlobalTimeWrappedHourly * 60) % 10 == 0)
            Dust.NewDustPerfect(segmentPositions[^1] - (Vector2.UnitY * 4) + Main.rand.NextVector2Circular(16, 16), DustID.Smoke).velocity = -Vector2.UnitY.RotatedByRandom(PiOver4) * Main.rand.NextFloat(0.5f, 2f);
    }

    public override void DrawOnVerletEnd(SpriteBatch spriteBatch, Vector2[] segmentPositions)
    {
        Texture2D texture = ModContent.Request<Texture2D>("Windfall/Content/Items/Placeables/Furnature/VerletHangers/Decorations/SelenicLantern").Value; ;

        Vector2 line = segmentPositions[^1] - segmentPositions[^2];
        Color lighting = Lighting.GetColor((segmentPositions[^2] + (line / 2f)).ToTileCoordinates());

        Vector2 origin = texture.Size() * 0.5f;
        Vector2 drawPos = segmentPositions[^1];
        float rotation = line.ToRotation();

        spriteBatch.Draw(texture, drawPos - Main.screenPosition, null, lighting, rotation - PiOver2, origin, 1f, 0, 0);
    }
}
