namespace Windfall.Content.Items.Fishing;

public class AncientIlmeranBobber : ModProjectile
{
    public new static string LocalizationCategory => "Projectiles.Fishing";
    public override string Texture => "Windfall/Assets/Projectiles/Fishing/AncientIlmeranBobber";
    public override void SetDefaults()
    {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.aiStyle = ProjAIStyleID.Bobber;
        Projectile.bobber = true;
        Projectile.penetrate = -1;
    }
}
