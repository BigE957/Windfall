namespace Windfall.Content.Items.Quest.Seamstress
{
    public class Mask : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Quest";
        public override string Texture => "Windfall/Assets/Items/Quest/DeificInsignia";
        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 24;
            Item.rare = ItemRarityID.White;
            Item.maxStack = 1;
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.Bone, 12).
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
