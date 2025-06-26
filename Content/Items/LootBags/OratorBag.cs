using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Vanity;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Furniture.DevPaintings;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Items.Weapons.Summon;
using CalamityMod.NPCs.Signus;
using CalamityMod;
using Terraria.GameContent.ItemDropRules;
using Windfall.Content.NPCs.Bosses.Orator;
using Windfall.Content.Items.Weapons.Melee;
using Windfall.Content.Items.Weapons.Ranged;
using Windfall.Content.Items.Weapons.Magic;
using Windfall.Content.Items.Weapons.Summon;
using Windfall.Content.Items.Weapons.Rogue;
using Windfall.Content.Items.Vanity.Masks;

namespace Windfall.Content.Items.LootBags;
public class OratorBag : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.TreasureBags";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 3;
        ItemID.Sets.BossBag[Item.type] = true;
    }

    public override void SetDefaults()
    {
        Item.width = 24;
        Item.height = 24;
        Item.maxStack = 9999;
        Item.consumable = true;
        Item.rare = ItemRarityID.Expert;
        Item.expert = true;
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
    {
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossBags;
    }

    public override bool CanRightClick() => true;

    public override Color? GetAlpha(Color lightColor) => Color.Lerp(lightColor, Color.White, 0.4f);

    public override void PostUpdate() => Item.TreasureBagLightAndDust();

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) => CalamityUtils.DrawTreasureBagInWorld(Item, spriteBatch, ref rotation, ref scale, whoAmI);

    public override void ModifyItemLoot(ItemLoot itemLoot)
    {
        // Money
        itemLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(ModContent.NPCType<TheOrator>()));

        // Weapons
        itemLoot.Add(DropHelper.CalamityStyle(DropHelper.BagWeaponDropRateFraction, new int[]
        {
                ModContent.ItemType<Apotelesma>(),
                ModContent.ItemType<FingerGuns>(),
                ModContent.ItemType<Kaimos>(),
                ModContent.ItemType<ShadowHandStaff>(),
                ModContent.ItemType<Prodosia>()
        }));

        // Equipment
        itemLoot.AddRevBagAccessories();

        // Vanity
        itemLoot.Add(ModContent.ItemType<OratorMask>(), 7);
        itemLoot.Add(ModContent.ItemType<ThankYouPainting>(), ThankYouPainting.DropInt);
    }
}
