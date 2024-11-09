namespace Windfall.Content.Items.Lore;

public class ProfanedWastesLore : BaseLoreItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Lore";
    public override string Texture => "Windfall/Assets/Items/Lore/ProfanedWastesLore";
    internal override int Rarity => ItemRarityID.Purple;
    internal override string Key => "LoreProfanedWastes";
    internal override Color LightColor => Color.Gold;
    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 20;
    }
    public override void AddRecipes()
    {
        Mod calamity = ModLoader.GetMod("CalamityMod");
        CreateRecipe()
            .AddIngredient(calamity.Find<ModItem>("ProfanedGuardianTrophy").Type)
            .AddTile(TileID.Bookcases)
            .Register();
    }
}