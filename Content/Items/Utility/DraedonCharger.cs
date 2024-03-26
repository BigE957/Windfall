using CalamityMod.Items;
using CalamityMod.Rarities;

namespace Windfall.Content.Items.Utility
{
    //CalamityGlobalItem
    public class DraedonCharger : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Utility";
        public override string Texture => "Windfall/Assets/Items/Utility/DraedonCharger";

        public override void SetDefaults()
        {
            CalamityGlobalItem thisItem = Item.Calamity();
            Item.width = 26;
            Item.height = 44;
            Item.rare = ModContent.RarityType<DarkOrange>();
            Item.useStyle = ItemUseStyleID.None;
            Item.consumable = false;
            thisItem.UsesCharge = true;
            thisItem.MaxCharge = 500f;
            thisItem.ChargePerUse = 0f;
        }
        public override bool CanRightClick()
        {
            if (!Main.mouseItem.IsAir)
            {
                CalamityGlobalItem itemToCharge = Main.mouseItem.Calamity();
                CalamityGlobalItem thisItem = Item.Calamity();
                if (itemToCharge.UsesCharge)
                {
                    if (itemToCharge.Charge < itemToCharge.MaxCharge && thisItem.Charge > 0f)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public override void RightClick(Player player)
        {
            if (Main.mouseItem.IsAir)
                return;
            CalamityGlobalItem itemToCharge = Main.mouseItem.Calamity();
            CalamityGlobalItem thisItem = Item.Calamity();
            float chargeToFill = itemToCharge.MaxCharge - itemToCharge.Charge;
            if (thisItem.Charge >= chargeToFill)
            {
                itemToCharge.Charge = itemToCharge.MaxCharge;
                thisItem.Charge -= chargeToFill;
            }
            else
            {
                itemToCharge.Charge += thisItem.Charge;
                thisItem.Charge = 0f;
            }
            SoundEngine.PlaySound(SoundID.Item94 with { Volume = SoundID.Item94.Volume * 0.75f }, player.position);
            Item.stack++;
        }
    }
}