using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria;
using WindfallAttempt1.UI.WanderersJournals;
using WindfallAttempt1.Utilities;
using CalamityMod.Rarities;
using CalamityMod.Items.Weapons.Melee;
using WindfallAttempt1.UI.DraedonsDatabase;

namespace WindfallAttempt1.Items.Debug
{
    public class PortableDraedonDatabase : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Debug";
        public override void SetDefaults()
        {
            Item.width = 25;
            Item.height = 29;
            Item.rare = ModContent.RarityType<DarkOrange>();
            Item.useAnimation = Item.useTime = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
        }
        public override bool? UseItem(Player player)
        {
            ModContent.GetInstance<DraedonUISystem>().OpenDraedonDatabase();
            return true;
        }
    }
}
