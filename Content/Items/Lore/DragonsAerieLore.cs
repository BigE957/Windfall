using CalamityMod.Rarities;

namespace Windfall.Content.Items.Lore;

public class DragonsAerieLore : BaseLoreItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Lore";
    public override string Texture => "Windfall/Assets/Items/Lore/DragonsAerieLore";
    internal override int Rarity => ModContent.RarityType<DarkBlue>();
    internal override string Key => "LoreDragonsAerie";
    internal override Color LightColor => Color.Red;

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
            .AddIngredient(calamity.Find<ModItem>("YharonTrophy").Type)
            .AddTile(TileID.Bookcases)
            .Register();
    }
}