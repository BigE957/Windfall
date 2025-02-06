using CalamityMod.Dusts;
using Windfall.Content.NPCs.WorldEvents.LunarCult;

namespace Windfall.Content.Projectiles.NPCAnimations;
public class ApostleProj : ProjectileNPC, ILocalizedModType
{
    public override string Texture => "Windfall/Assets/NPCs/WorldEvents/TheOrator_NPC";

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 1;
    }
    public override void SetDefaults()
    {
        Projectile.width = 58;
        Projectile.height = 70;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.netImportant = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = int.MaxValue;
        Projectile.alpha = 255;
        Projectile.ai[0] = 0;
    }
}
