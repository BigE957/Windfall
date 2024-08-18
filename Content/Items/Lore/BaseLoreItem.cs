namespace Windfall.Content.Items.Lore
{
    public abstract class BaseLoreItem : ModItem
    {
        internal virtual string Key => null;
        internal virtual int Rarity => 0;
        internal virtual Color LightColor => Color.White;
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemNoGravity[Type] = true;
        }
        public override void SetDefaults()
        {
            Item.consumable = false;
            Item.rare = Rarity;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            TooltipLine line = tooltips.FirstOrDefault(x => x.Mod == "Terraria" && x.Name == "Tooltip0");
            if (!Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift))
            {
                if (line != null)
                    line.Text = GetTextValue("Items.Lore.ShortTooltip");
                return;
            }
            string tooltip = GetWindfallTextValue($"LoreItems.{Key}");
            if (line != null)
                line.Text = tooltip;
        }
        public override bool CanUseItem(Player player) => false;
        public override void Update(ref float gravity, ref float maxFallSpeed)
        {
            Lighting.AddLight(Item.Center, LightColor.ToVector3());
        }
    }
}
