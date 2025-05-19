
using Windfall.Common.Systems.WorldEvents;
using Terraria.ID;
using CalamityMod.Particles;

namespace Windfall.Content.Items.Quests.SealingRitual;

public class BlossomedWayfinder : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Quest";
    public override string Texture => "Windfall/Assets/Items/Quest/BlossomedWayfinder";
    public override void SetDefaults()
    {
        Item.width = 22;
        Item.height = 24;
        Item.useAnimation = 32;
        Item.useTime = 32;
        Item.autoReuse = false;
        Item.useStyle = ItemUseStyleID.RaiseLamp;
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
            for (int i = 1; i < 8; i++)
            {
                Particle particle = new DirectionalPulseRing(player.Center + (toRuins * i * 32), toRuins * 4f, Color.Cyan, new Vector2(0.5f, 1f) * (1 - (float)(i / 8f)), toRuins.ToRotation(), 0f, 1.5f, 12 * (i+1));
                GeneralParticleHandler.SpawnParticle(particle);
            }
        }

        return false;
    }
}
