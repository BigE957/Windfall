namespace Windfall.Content.Items.Quest.Seamstress
{
    public class LunarBishopRobes : ModItem, ILocalizedModType
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
                AddIngredient<DyedRobes>().
                AddIngredient(ItemID.CopperBar, 4).
                AddIngredient(ItemID.FlinxFur, 4).
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
