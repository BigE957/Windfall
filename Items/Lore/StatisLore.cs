using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;


namespace WindfallAttempt1.Items.Lore
{
	public class StatisLore : ModItem
	{



 public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemNoGravity[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.consumable = false;

            Item.rare = ItemRarityID.LightRed;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            TooltipLine line = tooltips.FirstOrDefault(x => x.Mod == "Terraria" && x.Name == "Tooltip0");
            if (!Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift))
            {
                if (line != null)
                    line.Text = Language.GetOrRegister($"Mods.{nameof(WindfallAttempt1)}.LoreItems.LoreGeneric").Value;
                return;
            }

            string tooltip = Language.GetOrRegister($"Mods.{nameof(WindfallAttempt1)}.LoreItems.LoreStatis").Value;

            if (line != null)
                line.Text = tooltip;
        }

        public override bool CanUseItem(Player player) => false;

        public override Color? GetAlpha(Color lightColor) => Color.White;

        public override void AddRecipes()
        {
            Mod calamity = ModLoader.GetMod("CalamityMod");
                CreateRecipe()
                    .AddIngredient(calamity.Find<ModItem>("SlimeGodTrophy").Type)
                    .AddTile(TileID.Bookcases)
                    .Register();
        }
    }
}