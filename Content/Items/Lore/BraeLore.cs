using CalamityMod.Rarities;

namespace Windfall.Content.Items.Lore
{
    public class BraeLore : BaseLoreItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Lore";
        public override string Texture => "Windfall/Assets/Items/Lore/BraeLore";
        internal override int Rarity => ModContent.RarityType<Turquoise>();
        internal override string Key => "BraeLore";
        internal override Color LightColor => Color.Green;
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
                .AddIngredient(calamity.Find<ModItem>("ProvidenceTrophy").Type)
                .AddTile(TileID.Bookcases)
                .Register();
        }
    }
}