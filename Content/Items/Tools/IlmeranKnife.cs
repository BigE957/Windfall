using CalamityMod.Items.Materials;
using CalamityMod.Items;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using CalamityMod.NPCs.SunkenSea;
using System.Collections.Generic;
using System;
using CalamityMod.Items.Placeables;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Armor.Victide;
using System.Reflection;
using Windfall.Common.Systems;

namespace Windfall.Content.Items.Tools
{
    public class IlmeranKnife : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Tools";
        public override string Texture => "Windfall/Assets/Items/Tools/IlmeranKnife";
        internal struct lootItem
        {
            internal int Type;
            internal int MinStack;
            internal int MaxStack;
        };

        internal List<lootItem> ClamLootPool = new()
        {
            new lootItem {Type = ModContent.ItemType<SeaRemains>(), MinStack = 1, MaxStack = 3},
            new lootItem {Type = ItemID.Seashell, MinStack = 3, MaxStack = 5},
            new lootItem {Type = ItemID.Coral, MinStack = 3, MaxStack = 5},
            new lootItem {Type = ItemID.Starfish, MinStack = 3, MaxStack = 5},
            new lootItem {Type = ItemID.WhitePearl, MinStack = 1, MaxStack = 1},
            new lootItem {Type = ItemID.BlackPearl, MinStack = 1, MaxStack = 1},
            new lootItem {Type = ItemID.PinkPearl, MinStack = 1, MaxStack = 1},
            new lootItem {Type = ModContent.ItemType<PearlShard>(), MinStack = 1, MaxStack = 1},
            new lootItem {Type = ModContent.ItemType<Navystone>(), MinStack = 20, MaxStack = 30},
            new lootItem {Type = ModContent.ItemType<EutrophicSand>(), MinStack = 20, MaxStack = 30},
            new lootItem {Type = ModContent.ItemType<PrismShard>(), MinStack = 10, MaxStack = 15},
            new lootItem {Type = ModContent.ItemType<ShieldoftheOcean>(), MinStack = 1, MaxStack = 1},
            new lootItem {Type = ModContent.ItemType<FishboneBoomerang>(), MinStack = 1, MaxStack = 1},
            new lootItem {Type = ModContent.ItemType<RedtideSpear>(), MinStack = 1, MaxStack = 1},
            new lootItem {Type = ModContent.ItemType<VictideHeadSummon>(), MinStack = 1, MaxStack = 1},
        };
        public override void SetDefaults()
        {
            Item.damage = 1;
            Item.knockBack = 0.5f;
            Item.useAnimation =  Item.useTime = 9;

            Item.DamageType = DamageClass.Melee;
            Item.width = 30;
            Item.height = 32;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.value = CalamityGlobalItem.Rarity4BuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = true;
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if(target.ModNPC is Clam clam && target.type == ModContent.NPCType<Clam>())
            {
                FieldInfo ClamHitAmountFieldInfo = typeof(Clam).GetField("hitAmount", BindingFlags.NonPublic | BindingFlags.Instance);
                if(ClamHitAmountFieldInfo != null)
                    if((int)ClamHitAmountFieldInfo.GetValue(clam) != 3)
                    {
                        int index = Main.rand.Next(0, ClamLootPool.Count - 1);
                        Item.NewItem(Item.GetSource_DropAsItem(), target.Center, target.Size, ClamLootPool[index].Type, Main.rand.Next(ClamLootPool[index].MinStack, ClamLootPool[index].MaxStack));
                        ClamHitAmountFieldInfo.SetValue(clam, 3);
                        int questIndex = QuestSystem.QuestLog.FindIndex(quest => quest.Name == "ShuckinClams");
                        if (questIndex != -1)
                            QuestSystem.IncrementQuestProgress(questIndex, 0);
                    }
            }
            if(target.type == ModContent.NPCType<PrismBack>())
            {
                if(Main.rand.NextBool(3))
                    Item.NewItem(Item.GetSource_DropAsItem(), target.Center, target.Size,ModContent.ItemType<PrismShard>(), Main.rand.Next(3, 5));
            }
        }
    }
}
