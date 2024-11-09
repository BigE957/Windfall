using CalamityMod.Rarities;

namespace Windfall.Content.Items.Lore;

public class DistortionLore : BaseLoreItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Lore";
    public override string Texture => "Windfall/Assets/Items/Lore/DistortionLore";
    internal override int Rarity => ModContent.RarityType<PureGreen>();
    internal override string Key => "LoreDistortion";
    internal override Color LightColor => Color.Purple;

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.width = 20;
        Item.height = 20;
    }
    public override void AddRecipes()
    {
        Mod calamity = ModLoader.GetMod("CalamityMod");
        CreateRecipe()
            .AddIngredient(calamity.Find<ModItem>("DevourerofGodsTrophy").Type)
            .AddTile(TileID.Bookcases)
            .Register();
    }
}