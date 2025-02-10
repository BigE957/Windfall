using CalamityMod.Dusts;
using CalamityMod.Projectiles.BaseProjectiles;
using Windfall.Common.Utils;
using Windfall.Content.Buffs.Cooldowns;
using Windfall.Content.Buffs.Weapons;


namespace Windfall.Content.Items.Weapons.Misc;

public class ParryBlade : ModItem
{
    public override string Texture => "CalamityMod/Items/Weapons/Melee/CosmicShiv";
    public override void SetDefaults()
    {
        Item.damage = 8;
        Item.knockBack = 4f;
        Item.useStyle = ItemUseStyleID.Rapier; // Makes the player do the proper arm motion
        Item.useAnimation = Item.useTime = 8;
        Item.width = 32;
        Item.height = 32;
        Item.UseSound = SoundID.Item1;
        Item.DamageType = DamageClass.Default;
        Item.autoReuse = false;
        Item.noUseGraphic = true; // The sword is actually a "projectile", so the item should not be visible when used
        Item.noMelee = true; // The projectile will do the damage and not the item

        Item.rare = ItemRarityID.White;
        Item.value = Item.sellPrice(0, 0, 0, 10);

        Item.shoot = ModContent.ProjectileType<ParryBladeProj>(); // The projectile is what makes a shortsword work
        Item.shootSpeed = 2.1f; // This value bleeds into the behavior of the projectile as velocity, keep that in mind when tweaking values
    }
    public override bool CanUseItem(Player player)
    {
        if (player.HasCooldown(ParryWeapon.ID))
            return false;
        else
            return true;
    }
    public override bool? UseItem(Player player)
    {
        if (!player.Buff().PerfectFlow)
            player.AddCooldown(ParryWeapon.ID, SecondsToFrames(30));
        return true;
    }
}

public class ParryBladeProj : BaseShortswordProjectile
{
    public override string Texture => "CalamityMod/Items/Weapons/Melee/CosmicShiv";

    public override void SetDefaults()
    {
        Projectile.Size = new Vector2(24); // This sets width and height to the same value (important when projectiles can rotate)
        Projectile.aiStyle = -1; // Use our own AI to customize how it behaves, if you don't want that, keep this at ProjAIStyleID.ShortSword. You would still need to use the code in SetVisualOffsets() though
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.scale = 1f;
        Projectile.DamageType = DamageClass.Default;
        Projectile.ownerHitCheck = true; // Prevents hits through tiles. Most melee weapons that use projectiles have this
        Projectile.extraUpdates = 1; // Update 1+extraUpdates times per tick
        Projectile.timeLeft = 360; // This value does not matter since we manually kill it earlier, it just has to be higher than the duration we use in AI
        Projectile.hide = true; // Important when used alongside player.heldProj. "Hidden" projectiles have special draw conditions
        Projectile.timeLeft = 360;
    }

    public override void SetVisualOffsets()
    {
        const int HalfSpriteWidth = 48 / 2;
        const int HalfSpriteHeight = 48 / 2;

        int HalfProjWidth = Projectile.width / 2;
        int HalfProjHeight = Projectile.height / 2;

        DrawOriginOffsetX = 0;
        DrawOffsetX = -(HalfSpriteWidth - HalfProjWidth);
        DrawOriginOffsetY = -(HalfSpriteHeight - HalfProjHeight);
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        Player player = Main.player[Projectile.owner];
        player.AddBuff(ModContent.BuffType<PerfectFlow>(), 120);
    }
}

public class ParryProj : ModProjectile, ILocalizedModType
{
    public override string Texture => "CalamityMod/Projectiles/Summon/SlimePuppet";

    public override void SetDefaults()
    {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Default;
        Projectile.friendly = true;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 6000;
        Projectile.tileCollide = true;
        Projectile.aiStyle = ProjAIStyleID.Arrow;
        AIType = ProjectileID.Bullet;
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {

        Player owner = Main.player[Projectile.owner];
        if (owner.Calamity().cooldowns.ContainsKey(ParryWeapon.ID))
            owner.Calamity().cooldowns[ParryWeapon.ID].timeLeft = 0;
        owner.AddBuff(ModContent.BuffType<PerfectFlow>(), 5 * 60);
    }
    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 75; i++)
        {
            Vector2 speed = Main.rand.NextVector2Circular(2f, 2f);
            Dust d = Dust.NewDustPerfect(Projectile.Center, (int)CalamityDusts.PurpleCosmilite, speed * 5, Scale: 1.5f);
            d.noGravity = true;
        }
    }
}
