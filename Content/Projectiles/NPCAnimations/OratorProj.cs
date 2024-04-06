using CalamityMod.Dusts;
using Windfall.Content.Items.Quest;
using Windfall.Content.NPCs.WanderingNPCs;
using Windfall.Content.NPCs.WorldEvents.LunarCult;

namespace Windfall.Content.Projectiles.NPCAnimations
{
    public class OratorProj : ProjectileNPC, ILocalizedModType
    {
        public override string Texture => "Windfall/Assets/NPCs/WorldEvents/TheOrator";
        internal override string key => "LunarCult.TheOrator.WorldText.Post-Plant";
        internal override List<float> Delays => new()
        {
            1,
            3,
            3,
            3,
            4,
            3,
            2,
            3,
            3,
            3,
        };
        internal override SoundStyle SpawnSound => new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");
        internal override int NPCType => -1;
        internal override Color TextColor => Color.Cyan;
        internal override void DoOnSpawn()
        {
            for (int i = 0; i < 75; i++)
            {
                Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, (int)CalamityDusts.Ectoplasm, speed * 2, Scale: 1.5f);
                d.noGravity = true;
            }
            SoundEngine.PlaySound(SpawnSound, Projectile.Center);
        }
        internal override void DoBeforeDespawn()
        {
            Item i = Main.item[Item.NewItem(Entity.GetSource_Loot(), Projectile.Center, new Vector2(8, 4), ModContent.ItemType<WFEidolonTablet>())];
            i.velocity = new Vector2(Projectile.direction, 0) * -4;
        }
        internal override void DoOnDespawn()
        {
            foreach (NPC npc in Main.npc.Where(n => (n.type == ModContent.NPCType<LunarBishop>() || n.type == ModContent.NPCType<LunarCultistArcher>() || n.type == ModContent.NPCType<LunarCultistDevotee>()) && n.active))
            {
                for (int i = 0; i < 50; i++)
                {
                    Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                    Dust d = Dust.NewDustPerfect(npc.Center, DustID.GoldFlame, speed * 3, Scale: 1.5f);
                    d.noGravity = true;
                }
                SoundEngine.PlaySound(SpawnSound, npc.Center);
                npc.active = false;
            }
            SoundEngine.PlaySound(SpawnSound, Projectile.Center);
            for (int i = 0; i < 75; i++)
            {
                Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, (int)CalamityDusts.Ectoplasm, speed * 2, Scale: 1.5f);
                d.noGravity = true;
            }
        }
        internal override int DespawnDelay => 60 * 3;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 1;
        }
        public override void SetDefaults()
        {
            Projectile.width = 38;
            Projectile.height = 50;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = int.MaxValue;
            Projectile.alpha = 255;
            Projectile.ai[0] = 0;
        }
    }
}
