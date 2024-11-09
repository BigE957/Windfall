namespace Windfall.Content.Items.Lore;

public class StatisLore : BaseLoreItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Lore";
    public override string Texture => "Windfall/Assets/Items/Lore/StatisLore";
    internal override int Rarity => ItemRarityID.LightRed;
    internal override string Key => "LoreStatis";
    internal override Color LightColor => Color.Maroon;
    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 20;
    }
    public override void AddRecipes()
    {
        Mod calamity = ModLoader.GetMod("CalamityMod");
        CreateRecipe()
            .AddIngredient(calamity.Find<ModItem>("SlimeGodTrophy").Type)
            .AddTile(TileID.Bookcases)
            .Register();
    }
}