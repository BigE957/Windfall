using System;
using Windfall.Content.UI.Dialogue;

namespace Windfall.Content.Items.Debug;
public class DialogueViewer : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Debug";
    public override string Texture => "CalamityMod/Items/Weapons/Summon/StaffOfNecrosteocytes";
    public override void SetDefaults()
    {
        Item.width = 25;
        Item.height = 29;
        Item.rare = ItemRarityID.Red;
        Item.useAnimation = Item.useTime = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
    }
    public override bool? UseItem(Player player)
    {
        DialogueViewerUISystem uiSystem = ModContent.GetInstance<DialogueViewerUISystem>();
        uiSystem.OpenUI();
        return true;
    }
}
