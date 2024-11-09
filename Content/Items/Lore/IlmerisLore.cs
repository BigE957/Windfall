namespace Windfall.Content.Items.Lore;

public class IlmerisLore : BaseLoreItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Lore";
    public override string Texture => "Windfall/Assets/Items/Lore/IlmerisLore";
    internal override int Rarity => ItemRarityID.Blue;
    internal override string Key => "LoreIlmeris";
    internal override Color LightColor => Color.LightCyan;
    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 20;
    }
    public override void AddRecipes()
    {
        Mod calamity = ModLoader.GetMod("CalamityMod");
        CreateRecipe()
            .AddIngredient(calamity.Find<ModItem>("DesertScourgeTrophy").Type)
            .AddTile(TileID.Bookcases)
            .Register();
    }
}