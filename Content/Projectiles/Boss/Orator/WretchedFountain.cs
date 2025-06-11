using CalamityMod;
using CalamityMod.World;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Content.NPCs.Bosses.Orator;

namespace Windfall.Content.Projectiles.Boss.Orator;
public class WretchedFountain : ModProjectile
{
    public override string Texture => "Windfall/Assets/Graphics/Metaballs/BasicCircle";

    public override void SetDefaults()
    {
        upTime = 180;
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.damage = 100;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 250;
        Projectile.tileCollide = false;
        Projectile.scale = 0f;
        Projectile.timeLeft = upTime;

        Projectile.Calamity().DealsDefenseDamage = true;
    }

    int Time = 0;
    static int upTime = 1200;

    public override void OnSpawn(IEntitySource source)
    {
        Point ground = FindSurfaceBelow(Projectile.Center.ToTileCoordinates(), true);
        Projectile.Center = ground.ToWorldCoordinates();
    }

    public override void AI()
    {
        float openness = Clamp(Time / 30f, 0f, 1f);
        int Orator = NPC.FindFirstNPC(ModContent.NPCType<TheOrator>());
        if(Time < (CalamityWorld.revenge ? 60 : 30) && Orator != -1)
        {
            Point ground = FindSurfaceBelow(Main.npc[Orator].Center.ToTileCoordinates(), true);
            Projectile.Center = ground.ToWorldCoordinates();
        }
        float localPlayerY = Main.LocalPlayer.Bottom.Y + Main.screenHeight / 2f;
        for(int i = 0; i < 2; i++)
            EmpyreanMetaball.SpawnDefaultParticle(new(Projectile.Center.X + (Main.rand.NextFloat(-96, 96) * openness), localPlayerY < Projectile.Center.Y ? localPlayerY : Projectile.Center.Y), Vector2.UnitY.RotatedBy(Main.rand.NextFloat(-PiOver4 / 4f, PiOver4 / 4f)) * Main.rand.NextFloat(-14, -4) * (Time > 90 ? 2 : (1 / (2 - openness))), Main.rand.NextFloat(80, 160));

        if (Time > 90 && Time % 4 == 0)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 spawnPos = new(Projectile.Center.X + Main.rand.NextFloat(-72, 72), Projectile.Center.Y + Main.rand.NextFloat(0, 32));
                Projectile.NewProjectileDirect(Terraria.Entity.GetSource_NaturalSpawn(), spawnPos, Vector2.UnitY.RotatedBy(Main.rand.NextFloat(-0.25f, 0.25f)) * Main.rand.NextFloat(-16, -4), ModContent.ProjectileType<DarkGlob>(), 100, 0f, -1, 2, Main.rand.NextFloat(0.5f, 1f));
            }
        }
        Time++;

        if (Time == upTime)
        {
            for (int i = 0; i < 48; i++)
                EmpyreanMetaball.SpawnDefaultParticle(new(Projectile.Center.X + (Main.rand.NextFloat(-96, 96) * openness), Projectile.Center.Y), Vector2.UnitY.RotatedBy(Main.rand.NextFloat(-PiOver2, PiOver2)) * Main.rand.NextFloat(-14, -4), Main.rand.NextFloat(80, 160));
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = TextureAssets.Projectile[Type].Value;
        float sineWave = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4);
        Main.EntitySpriteDraw(tex, Projectile.Bottom - Main.screenPosition + Vector2.UnitY * 16, null, Color.White, 0f, tex.Size() * 0.5f, new Vector2(5f + sineWave / 2f, 1.5f + sineWave / 4f), 0);
        return false;
    }
}
