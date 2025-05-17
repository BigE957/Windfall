
using Windfall.Common.Systems.WorldEvents;
using Terraria.ID;
using CalamityMod.Particles;

namespace Windfall.Content.Items.Quests.SealingRitual;

public class Wayfinder : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Quest";
    public override string Texture => "CalamityMod/Items/Accessories/AscendantInsignia";
    public override void SetDefaults()
    {
        Item.width = 22;
        Item.height = 24;
        Item.useAnimation = 12;
        Item.useTime = 12;
        Item.autoReuse = false;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.channel = false;
        Item.rare = ItemRarityID.Quest;
        Item.maxStack = 1;
        Item.shoot = ProjectileID.WoodenArrowFriendly; //Does not matter, only here so that Shoot gets called
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if(DraconicRuinsSystem.CutsceneTime == 90 && !DraconicRuinsSystem.AccessGranted)
        {
            //Add cooler stuff Eventually tm
            DraconicRuinsSystem.AccessGranted = true;
        }
        else
        {
            Vector2 toRuins = (DraconicRuinsSystem.RuinsEntrance.ToWorldCoordinates() - player.Center).SafeNormalize(Vector2.Zero);
            Particle particle = new DirectionalPulseRing(player.Center, toRuins * 7f, Color.Cyan, new(1, 0.66f), toRuins.ToRotation(), 0f, 1.5f, 24);
            GeneralParticleHandler.SpawnParticle(particle);
        }

        return false;
    }
}
