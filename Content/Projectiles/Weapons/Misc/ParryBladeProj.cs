using CalamityMod.Projectiles.BaseProjectiles;
using Windfall.Content.Buffs.Weapons;

namespace Windfall.Content.Projectiles.Weapons.Misc;

// Shortsword projectiles are handled in a special way with how they draw and damage things
// The "hitbox" itself is closer to the player, the sprite is centered on it
// However the interactions with the world will occur offset from this hitbox, closer to the sword's tip (CutTiles, Colliding)
// Values chosen mostly correspond to Iron Shortsword
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

