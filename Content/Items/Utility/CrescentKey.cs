namespace Windfall.Content.Items.Utility;
public class CrescentKey : ModItem
{
    public new string LocalizationCategory => "Items.Utility";
    public override string Texture => "Windfall/Assets/Items/Utility/CrescentKey";
    public override void SetDefaults()
    {
        Item.width = 26;
        Item.height = 22;
        Item.maxStack = 1;
        Item.useStyle = ItemUseStyleID.None;
        Item.consumable = true;
        Item.value = 0;
    }
}
