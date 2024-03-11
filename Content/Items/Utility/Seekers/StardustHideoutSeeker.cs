using CalamityMod.Items;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Windfall.Common.Systems.WorldEvents;

namespace Windfall.Content.Items.Utility
{
    public class StardustHideoutSeeker : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Utility";
        public override string Texture => "CalamityMod/Items/LabFinders/CyanSeekingMechanism";

        public static readonly SoundStyle UseSound = new("Windfall/Assets/Sounds/Items/IlmeranHorn");

        public override void SetDefaults()
        {
            Item.useAnimation = Item.useTime = 100;
            Item.width = 42;
            Item.height = 32;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.value = CalamityGlobalItem.Rarity2BuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.UseSound = UseSound;
            Item.useTurn = true;
        }
        public override bool? UseItem(Player player)
        {
            Main.NewText($"You are {(player.position.X/16) - CultMeetingSystem.StardustHideoutLocation.X}, {(player.position.Y/16) - CultMeetingSystem.StardustHideoutLocation.Y} from the Stardust Hideout.", Color.Yellow);
            return true;
        }        
    }
}
