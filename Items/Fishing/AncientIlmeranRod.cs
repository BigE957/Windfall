﻿using CalamityMod.Events;
using CalamityMod.Items;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Windfall.Projectiles.Fishing;
using Windfall.Projectiles.NPCAnimations;
using Windfall.Systems;
using Windfall.NPCs.WanderingNPCs;

namespace Windfall.Items.Fishing
{
    public class AncientIlmeranRod : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Fishing";
        public override string Texture => "CalamityMod/Items/Fishing/FishingRods/WulfrumRod";
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.useAnimation = 8;
            Item.useTime = 8;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item1;
            Item.rare = ItemRarityID.Blue;
            Item.fishingPole = 10;
            Item.shootSpeed = 10f;
            Item.shoot = ModContent.ProjectileType<AncientIlmeranBobber>();
            Item.value = CalamityGlobalItem.Rarity1BuyPrice;
        }
        internal bool isCast = false;
        internal int scoogCounter = 0;
        public override void HoldItem(Player player)
        {
            isCast = false;
            for (int i = 0; i < Main.projectile.Length; i++)
            {
                if (Main.projectile[i].type == ModContent.ProjectileType<AncientIlmeranBobber>())
                {
                    isCast = true;
                    break;
                }
            }
            int scoogWait = 60;
            if(isCast && player.ZoneDesert && !NPC.AnyNPCs(ModContent.NPCType<DesertScourgeHead>()) && !BossRushEvent.BossRushActive)
            {
                
                if(scoogCounter == 0)
                {
                    if (WorldSaveSystem.ScoogFished)
                        scoogWait = Main.rand.Next(20, 40);
                    else
                        scoogWait = Main.rand.Next(50, 70);
                }
                scoogCounter++;

                if (scoogCounter == 60 * 5 && (!NPC.AnyNPCs(ModContent.NPCType<IlmeranPaladin>()) && !NPC.AnyNPCs(ModContent.NPCType<IlmeranPaladinKnocked>())))
                {
                    Projectile.NewProjectile(null, new Vector2(player.Center.X - (80 * player.direction), player.Center.Y + 100), new Vector2(0, -8), ModContent.ProjectileType<IlmeranPaladinDig>(), 0, 0);
                    //Main.NewText("Paladin is approaching!", Color.Yellow);
                }

                if(scoogCounter >= 60 * scoogWait)
                {
                    SoundEngine.PlaySound(SoundID.Roar, player.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<DesertScourgeHead>());
                    else
                        NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, -1, -1, null, player.whoAmI, ModContent.NPCType<DesertScourgeHead>());

                    if (CalamityWorld.revenge)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<DesertNuisanceHead>());
                        else
                            NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, -1, -1, null, player.whoAmI, ModContent.NPCType<DesertNuisanceHead>());

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<DesertNuisanceHead>());
                        else
                            NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, -1, -1, null, player.whoAmI, ModContent.NPCType<DesertNuisanceHead>());
                    }
                }
            }
            else
            {
                scoogCounter = 0;
            }
        }
    }
}
