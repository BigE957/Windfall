﻿using Microsoft.Xna.Framework.Input;
using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.NPCs.Bosses.TheOrator;
using Windfall.Content.Projectiles.Other;

namespace Windfall.Content.Items.Quest
{
    public class SelenicTablet : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Quest";
        public override string Texture => "CalamityMod/Items/SummonItems/EidolonTablet";
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.rare = ItemRarityID.Cyan;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.shoot = ModContent.ProjectileType<HideoutSeeker>();
            Item.shootSpeed = 20f;
        }
        public override bool CanShoot(Player player)
        {
            return !SealingRitualSystem.RitualSequenceSeen;
        }
        public override bool? UseItem(Player player)
        {
            if(SealingRitualSystem.RitualSequenceSeen)
            {
                SoundEngine.PlaySound(SoundID.Roar, player.Center);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<TheOrator>());
                else
                    NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, -1, -1, null, player.whoAmI, ModContent.NPCType<TheOrator>());
            }
            return true;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            TooltipLine line = tooltips.FirstOrDefault(x => x.Mod == "Terraria" && x.Name == "Tooltip0");
            if (!SealingRitualSystem.RitualSequenceSeen)
            {
                if (line != null)
                    line.Text = GetTextValue("Items.Quest.SelenicTablet.SeekerTooltip");
                return;
            }
            string tooltip = GetWindfallTextValue("Items.Quest.SelenicTablet.SummonTooltip");
            if (line != null)
                line.Text = tooltip;
        }
    }
}