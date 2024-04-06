
namespace Windfall.Content.Items.Lore
{
    public class OraLore : BaseLoreItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Lore";
        public override string Texture => "Windfall/Assets/Items/Lore/OraLore";
        internal override int Rarity => ItemRarityID.Cyan;
        internal override string Key => "LoreOrator";
        internal override Color LightColor => Color.LightGreen;
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.width = 20;
            Item.height = 20;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.AncientCultistTrophy)
                .AddTile(TileID.Bookcases)
                .Register();
        }
    }
}