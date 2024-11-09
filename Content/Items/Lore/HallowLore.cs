
namespace Windfall.Content.Items.Lore;

public class HallowLore : BaseLoreItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Lore";
    public override string Texture => "Windfall/Assets/Items/Lore/HallowLore";
    internal override int Rarity => ItemRarityID.Pink;
    internal override string Key => "LoreHallow";
    internal override Color LightColor => Color.Pink;
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.width = 20;
        Item.height = 20;
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.QueenSlimeTrophy)
            .AddTile(TileID.Bookcases)
            .Register();
    }
}