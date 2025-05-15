using CalamityMod.Items;
using CalamityMod;
using Windfall.Common.Players.DrawLayers;
using Windfall.Content.Items.Interfaces;

namespace Windfall.Content.Items.Vanity.DevVanities;
public class FuriousCocktail : DevVanity, IHatExtension, IAnimatedHead
{
    public override string Texture => "Windfall/Assets/Items/Vanity/DevVanities/Zarachard/FuriousCocktail";

    public override string DevName => "Zarachard";

    public string ExtensionTexture => $"Windfall/Assets/Items/Vanity/DevVanities/{DevName}/HeadExtension";
    public Vector2 ExtensionSpriteOffset(PlayerDrawSet drawInfo) => new(drawInfo.drawPlayer.direction == 1 ? -20 : 20, -22);

    public int AnimationLength => 4;
    public int AnimationDelay => 10;

    public override void SetDefaults()
    {
        Item.width = 22;
        Item.height = 30;
        Item.accessory = true;
        Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
        Item.rare = ItemRarityID.Green;
        Item.vanity = true;
        Item.Calamity().devItem = true;
    }
}
