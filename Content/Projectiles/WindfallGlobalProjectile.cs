
namespace Windfall.Content.Projectiles
{
    public class WindfallGlobalProjectile : GlobalProjectile
    {
        public bool hasHitMuck = false;
        public override void SetDefaults(Projectile entity)
        {
            base.SetDefaults(entity);
            hasHitMuck = false;
        }
        public override bool InstancePerEntity => true;
    }
}
