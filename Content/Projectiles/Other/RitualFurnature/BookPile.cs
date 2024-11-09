using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windfall.Common.Systems.WorldEvents;

namespace Windfall.Content.Projectiles.Other.RitualFurnature;

public class BookPile : ModProjectile
{
    public override string Texture => "Windfall/Content/Projectiles/Other/RitualFurnature/RitualFurnatureAtlas";
    public new string LocalizationCategory => "Projectiles.Other";

    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 1;
        ProjectileID.Sets.DontAttachHideToAlpha[Projectile.type] = true;
    }

    public override void SetDefaults()
    {
        Projectile.width = 42;
        Projectile.height = 28;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = int.MaxValue;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.hide = true;
    }

    public override void AI()
    {
        if(SealingRitualSystem.RitualSequenceSeen)
        {
            Player closestPlayer = Main.player[Player.FindClosest(Projectile.Center, Projectile.width, Projectile.height)];
            if (CalamityUtils.ManhattanDistance(Projectile.Center, closestPlayer.Center) > 800f)
                Projectile.active = false;
        }
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        behindNPCs.Add(index);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Rectangle frame = Projectile.ai[0] == 0 ? new(38, 36, 48, 19) : new(92, 40, 40, 15);
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Vector2 origin = frame.Size() * 0.5f;
        Main.EntitySpriteDraw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, 1f, 0, 0);
        return false;
    }
}
