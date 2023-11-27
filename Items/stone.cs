using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WindfallAttempt1.Items
{
	public class stone : ModItem
	{
        // The Display Name and Tooltip of this item can be edited in the Localization/en-US_Mods.WindfallAttempt1.hjson file.

		public override void SetDefaults()
		{
			Item.damage = 1000000;
			Item.DamageType = DamageClass.Melee;
			Item.width = 200;
			Item.height = 200;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = 1;
			Item.knockBack = 6;
			Item.value = 10000;
			Item.rare = 2;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.DirtBlock, 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
}