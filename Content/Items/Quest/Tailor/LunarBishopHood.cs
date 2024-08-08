namespace Windfall.Content.Items.Quest.Seamstress
{
    public class LunarBishopHood : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Quest";
        public override string Texture => "Windfall/Assets/Items/Quest/DeificInsignia";
        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 24;
            Item.rare = ItemRarityID.Quest;
            Item.maxStack = 1;
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<DyedHood>().
                AddIngredient(ItemID.CopperBar, 6).
                AddIngredient(ItemID.FlinxFur, 2).
                AddTile(TileID.Loom).
                Register();
        }
    }
}
