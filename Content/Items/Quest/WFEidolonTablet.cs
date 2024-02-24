using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace Windfall.Content.Items.Quest
{
    public class WFEidolonTablet : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Quest";
        public override string Texture => "CalamityMod/Items/SummonItems/EidolonTablet";
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.rare = ItemRarityID.Cyan;
            Item.useAnimation = 10;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = false;
        }
    }
}
