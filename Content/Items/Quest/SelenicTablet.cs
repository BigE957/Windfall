using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.NPCs.Bosses.TheOrator;
using Windfall.Content.Projectiles.Other;
using Windfall.Content.UI.Selenic;

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
            Item.useTime = Item.useAnimation = 65;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.shoot = ModContent.ProjectileType<HideoutSeeker>();
            Item.shootSpeed = 20f;
        }
        public override bool CanShoot(Player player)
        {
            return !SealingRitualSystem.RitualSequenceSeen && LunarCultActivitySystem.ActiveHideoutCoords != new Point(-1,-1);
        }
        public static string Key = NPC.downedEmpressOfLight || DownedBossSystem.downedRavager ? "Pre-Lunar" : NPC.downedGolemBoss ? "Post-Golem" : "Post-Plant";
        public override bool? UseItem(Player player)
        {
            if (CanShoot(player))
                Item.useStyle = ItemUseStyleID.Shoot;
            else
                Item.useStyle = ItemUseStyleID.HoldUp;
            if (SealingRitualSystem.RitualSequenceSeen)
            {
                SoundEngine.PlaySound(SoundID.Roar, player.Center);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<TheOrator>());
                else
                    NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, -1, -1, null, player.whoAmI, ModContent.NPCType<TheOrator>());
            }
            else if (LunarCultActivitySystem.ActiveHideoutCoords == new Point(-1, -1))
            {
                if (!SelenicTabletUISystem.isUIOpen)
                {
                    if (Main.rand.NextBool(10))
                        Key = "Rare";
                    else
                        Key = NPC.downedEmpressOfLight || DownedBossSystem.downedRavager ? "Pre-Lunar" : NPC.downedGolemBoss ? "Post-Golem" : "Post-Plant";
                    int Topic = Main.rand.Next(0, 3);
                    switch (Key)
                    {
                        case "Post-Plant":
                            switch (Topic)
                            {
                                case 0:
                                    Key += ".Player";
                                    break;
                                case 1:
                                    Key += ".Dungeon";
                                    break;
                                case 2:
                                    Key += ".Change";
                                    break;
                            }
                            break;
                        case "Post-Golem":
                            switch (Topic)
                            {
                                case 0:
                                    Key += ".Weakness";
                                    break;
                                case 1:
                                    Key += ".Faith";
                                    break;
                                case 2:
                                    Key += ".Quiet";
                                    break;
                            }
                            break;
                        case "Pre-Lunar":
                            switch (Topic)
                            {
                                case 0:
                                    Key += ".Beginning";
                                    break;
                                case 1:
                                    Key += ".Orator";
                                    break;
                                case 2:
                                    Key += ".Paradise";
                                    break;
                            }
                            break;
                        case "Rare":
                            switch (Topic)
                            {
                                case 0:
                                    Key += ".Doubt";
                                    break;
                                case 1:
                                    Key += ".Revelation";
                                    break;
                                case 2:
                                    Key += ".You";
                                    break;
                            }
                            break;
                    }
                    SelenicText.Contents = GetWindfallTextValue($"UI.Selenic.{Key}.0");
                    ModContent.GetInstance<SelenicTabletUISystem>().ShowUI();
                }
                else
                    ModContent.GetInstance<SelenicTabletUISystem>().HideUI();
            }
            return true;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            TooltipLine line = tooltips.FirstOrDefault(x => x.Mod == "Terraria" && x.Name == "Tooltip0");
            if (!SealingRitualSystem.RitualSequenceSeen)
            {
                if (line != null)
                    line.Text = GetWindfallTextValue("Items.Quest.SelenicTablet.SeekerTooltip");
                return;
            }
            string tooltip = GetWindfallTextValue("Items.Quest.SelenicTablet.SummonTooltip");
            if (line != null)
                line.Text = tooltip;
        }
    }
}
