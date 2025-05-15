using CalamityMod.Items;
using CalamityMod;
using Windfall.Common.Players.DrawLayers;
using CalamityMod.Particles;

namespace Windfall.Content.Items.Vanity.DevVanities;
public class FuriousCocktail : DevVanity, IHatExtension, IAnimatedHead
{
    public override string Texture => "Windfall/Assets/Items/Vanity/DevVanities/Zarachard/FuriousCocktail";

    public override string DevName => "Zarachard";

    public Asset<Texture2D> extensionTexture { get; set; }
    public Vector2 ExtensionSpriteOffset(PlayerDrawSet drawInfo) => new(drawInfo.drawPlayer.direction == 1 ? -16 : 16, -24);

    public int AnimationLength => 5;
    public int AnimationDelay => 5;

    public Asset<Texture2D> headTexture { get; set; }

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        extensionTexture = ModContent.Request<Texture2D>("Windfall/Assets/Items/Vanity/DevVanities/Zarachard/HeadExtension");
        headTexture = ModContent.Request<Texture2D>("Windfall/Assets/Items/Vanity/DevVanities/Zarachard/AnimatedHead");
    }

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

    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
    {
        drawInfo.colorArmorHead = Color.White;
    }

    public override void UpdateVanity(Player player)
    {
        Vector2 headPos = player.Center - Vector2.UnitY * 8;
        Lighting.AddLight(headPos + player.velocity * 6, new Color(255, 0, 72).ToVector3() * 0.75f);
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        if (!hideVisual)
        {
            Vector2 headPos = player.Center - Vector2.UnitY * 8;
            Lighting.AddLight(headPos + player.velocity * 6, new Color(255, 0, 72).ToVector3() * 0.75f);
        }
    }
}
