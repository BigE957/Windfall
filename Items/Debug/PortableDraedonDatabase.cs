using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria;
using Windfall.UI.WanderersJournals;
using Windfall.Utilities;
using CalamityMod.Rarities;
using CalamityMod.Items.Weapons.Melee;
using Windfall.UI.DraedonsDatabase;

namespace Windfall.Items.Debug
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
