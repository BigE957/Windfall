using Luminance.Core.Graphics;

namespace Windfall.Content.Items.Debug
{
    public class TheZoomer : ModItem, ILocalizedModType
    {
        static bool zooming = false;
        float zoom = 0;
        public new string LocalizationCategory => "Items.Debug";
        public override string Texture => "Windfall/Assets/Items/Debug/TheZoomer";

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.rare = ItemRarityID.Blue;
            Item.useAnimation = Item.useTime = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
        }
        public override bool? UseItem(Player player)
        {
            if (zooming)
            {
                zooming = false;
            }
            else
            {
                zooming = true;
            }
            return true;
        }
        //public override void HoldItem(Player player)
        //{
        //    if (Main.myPlayer == player.whoAmI)
        //    {
        //        Main.LocalPlayer.Windfall_Camera().ScreenFocusPosition = Main.MouseWorld;
        //        Main.LocalPlayer.Windfall_Camera().ScreenFocusInterpolant = Utils.GetLerpValue(0f, 15f, 0.1);
        //    }
        //}
        public override void HoldItem(Player player)
        {
            if (zooming)
            {
                zoom += 0.1f;
                CameraPanSystem.Zoom = zoom;
            }
        }
    }
}
