using CalamityMod.CalPlayer;
using CalamityMod.Items;
using Microsoft.Xna.Framework.Input;
using Terraria.Chat;
using Windfall.Common.Systems.WorldEvents;
using Terraria.ID;

namespace Windfall.Content.Items.Utility
{
    public class Moonlight : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Utility";
        public override string Texture => "Windfall/Assets/Items/Utility/Moonlight";

        public static readonly SoundStyle UseSound = new("Windfall/Assets/Sounds/Items/IlmeranHorn");

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 28;
            Item.rare = ItemRarityID.Lime;
            Item.useAnimation = 9;
            Item.useTime = 9;
            Item.autoReuse = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.UseSound = SoundID.Item60;
            Item.consumable = false;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = (ContentSamples.CreativeHelper.ItemGroup)CalamityResearchSorting.ToolsOther;
        }

        public override bool CanUseItem(Player player) => !CalamityPlayer.areThereAnyDamnBosses && (int)LunarCultBaseSystem.State <= 2;

        public override bool? UseItem(Player player)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return true;

            if (Main.moonPhase == 7)
                Main.moonPhase = 0;
            else
                Main.moonPhase++;
            string text = GetWindfallTextValue(LocalizationCategory + ".MoonPhaseChanges." + Main.moonPhase);
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                Main.NewText(text, Color.LightSkyBlue);

            }
            else if (Main.netMode == NetmodeID.Server || Main.netMode == NetmodeID.MultiplayerClient)
            {
                ChatHelper.BroadcastChatMessage(NetworkText.From(text), Color.LightSkyBlue);
            }
            return true;
        }
    }
}
