using CalamityMod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Windfall.Common.Systems.WorldEvents;
using static Windfall.Common.Systems.WorldEvents.CultMeetingSystem;

namespace Windfall.Content.Projectiles.Other
{
    public class HideoutSeeker : ModProjectile
    {
        public override string Texture => "Windfall/Assets/NPCs/WorldEvents/TheOrator_Head";
        public new static string LocalizationCategory => "Projectiles.Other";
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.penetrate = -1;
            Projectile.alpha = 255;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 200;
        }
        public override void AI() 
        {
            if (ActiveHideoutCoords != new Point(-1, -1))
            {
                Vector2 DistanceVector = new(ActiveHideoutCoords.X - Projectile.Center.X, ActiveHideoutCoords.Y - Projectile.Center.Y);
                Projectile.velocity += DistanceVector.SafeNormalize(Vector2.UnitX);
            }

            int dustType = DustID.GoldFlame;
            if (SolarHideoutLocation.X == ActiveHideoutCoords.X / 16)
                dustType = DustID.SolarFlare;
            else if (VortexHideoutLocation.X == ActiveHideoutCoords.X / 16)
                dustType = DustID.Vortex;
            else if (NebulaHideoutLocation.X == ActiveHideoutCoords.X / 16)
                dustType = DustID.Gastropod;
            else if (StardustHideoutLocation.X == ActiveHideoutCoords.X / 16)
                dustType = DustID.BlueFlare;
            Dust.NewDustPerfect(Projectile.Center, dustType);
        }
    }
}
