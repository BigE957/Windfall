using Windfall.Common.Systems;
using Windfall.Content.NPCs.Enemies.AstralSiphon;

namespace Windfall.Content.Projectiles.Enemies;
public class AstralBolt : ModNPC, ILocalizedModType
{
    public override string Texture => "Terraria/Images/Projectile_873";
    public new static string LocalizationCategory => "Projectiles.Enemies";
    public override void SetStaticDefaults()
    {
        NPCID.Sets.ImmuneToRegularBuffs[Type] = true;
        NPCID.Sets.TrailingMode[Type] = 7;
        NPCID.Sets.TrailCacheLength[Type] = 20;
    }
    public override void SetDefaults()
    {
        NPC.damage = 100;
        NPC.lifeMax = 1;
        NPC.friendly = false;
        NPC.height = NPC.width = 48;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.immortal = true;
    }
    float initialVelocity = 12f;
    public override void OnSpawn(IEntitySource source)
    {
        NPC.velocity = NPC.ai[0].ToRotationVector2() * 10f;
    }
    public override void AI()
    {
        NPC.rotation = NPC.velocity.ToRotation() + PiOver2;

        if (NPC.friendly && Main.npc.Any(n => n != null && n.active && n.type == ModContent.NPCType<SiphonSpitter>() && n.whoAmI == NPC.ai[1]))
        {
            NPC guardian = Main.npc.First(n => n != null && n.active && n.type == ModContent.NPCType<SiphonSpitter>() && n.whoAmI == NPC.ai[1]);
            if (NPC.Hitbox.Intersects(guardian.Hitbox))
            {
                guardian.immortal = false;
                guardian.StrikeInstantKill();
                NPC.Center = guardian.Center;
                NPC.ModNPC.OnKill();
                NPC.active = false;
            }
            NPC.velocity = NPC.velocity.RotateTowards((Main.npc[(int)NPC.ai[1]].Center - NPC.Center).ToRotation(), 0.1f);
        }
        else if(!NPC.friendly)
            NPC.velocity = NPC.velocity.RotateTowards((Main.player[Player.FindClosest(NPC.position, NPC.width, NPC.height)].Center - NPC.Center).ToRotation(), 0.1f);

        if (NPC.timeLeft <= 60 && !NPC.friendly)
            NPC.velocity = NPC.velocity.SafeNormalize(Vector2.UnitX) * Lerp(0, initialVelocity, NPC.timeLeft / 60f);
    }

    public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
    {  
        NPC.velocity = (NPC.Center - player.Center).SafeNormalize(Vector2.UnitX * player.direction) * (15 + item.knockBack);

        ParryEffect(NPC);
    }

    public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
    {
        if (projectile.velocity == Vector2.Zero)
        {
            Player owner = Main.player[projectile.owner];
            NPC.velocity = (NPC.Center - owner.Center).SafeNormalize(Vector2.UnitX * owner.direction) * (15 + projectile.knockBack);
        }
        else
            NPC.velocity = projectile.velocity.SafeNormalize(Vector2.Zero) * (15 + projectile.knockBack);

        ParryEffect(NPC);    
    }
    
    public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
    {
        NPC.immortal = false;
        NPC.life = 0;
        NPC.ModNPC.OnKill();
    }

    private static void ParryEffect(NPC npc)
    {
        SoundEngine.PlaySound(ContactSystem.Parry, npc.Center);

        npc.friendly = true;

        for (int i = 0; i < 6; i++)
        {
            Color color = Main.rand.NextBool() ? Color.Cyan * 1.5f : Color.OrangeRed;
            Particle particle = new SparkParticle(npc.Center, Main.rand.NextVector2Circular(4f, 4f), false, 32, 1f, color);
            GeneralParticleHandler.SpawnParticle(particle);
        }
        Particle pulse = new PulseRing(npc.Center, Vector2.Zero, Main.rand.NextBool() ? Color.Cyan * 1.5f : Color.OrangeRed, 0f, 0.35f, 16);
        GeneralParticleHandler.SpawnParticle(pulse);
    }

    public override void OnKill()
    {
        SoundEngine.PlaySound(new("CalamityMod/Sounds/Custom/Ravager/RavagerMissileExplosion"), NPC.Center);
        for (int i = 0; i < (NPC.friendly ? 24 : 12); i++)
        {
            Color color = Main.rand.NextBool() ? Color.Cyan * 1.5f : Color.OrangeRed;
            Particle particle = new SparkParticle(NPC.Center, Main.rand.NextVector2Circular(4f, 4f) * (NPC.friendly ? 2 : 1), false, 48, 1f, color);
            GeneralParticleHandler.SpawnParticle(particle);
        }
        Particle pulse = new PulseRing(NPC.Center, Vector2.Zero * 0f, Main.rand.NextBool() ? Color.Cyan * 1.5f : Color.OrangeRed, 0.05f, (NPC.friendly ? 0.9f : 0.5f), 24);
        GeneralParticleHandler.SpawnParticle(pulse);
    }
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

        Vector2 positionToCenterOffset = NPC.Size * 0.5f;
        for (int i = 0; i < NPC.oldPos.Length; i++)
        {
            float interpolent = (NPC.oldPos.Length - i) / (float)NPC.oldPos.Length;
            spriteBatch.Draw(tex, (NPC.oldPos[i] + positionToCenterOffset) - screenPos, null, colorFunc(interpolent), NPC.oldRot[i], tex.Size() * 0.5f, interpolent, 0, 0);
            if (i != NPC.oldPos.Length - 1)
            {
                Vector2 midpoint = new((NPC.oldPos[i].X + NPC.oldPos[i + 1].X) / 2, (NPC.oldPos[i].Y + NPC.oldPos[i + 1].Y) / 2);
                float midangle = (NPC.oldRot[i] + NPC.oldRot[i + 1]) / 2f;
                float midInterp = (NPC.oldPos.Length - i - 0.5f) / NPC.oldPos.Length;
                spriteBatch.Draw(tex, (midpoint + positionToCenterOffset) - screenPos, null, colorFunc(midInterp), midangle, tex.Size() * 0.5f, midInterp, 0, 0);
            }
        }
        spriteBatch.Draw(tex, NPC.Center - screenPos, null, colorFunc(1) * 1.5f, NPC.rotation, tex.Size() * 0.5f, 1.25f, 0, 0);
        return false;
    }
    private static Color colorFunc(float i) => Color.Lerp(Color.Cyan * 1.5f, Color.OrangeRed, (float)Math.Sin((Main.GlobalTimeWrappedHourly + i) * 4f) / 2f + 0.5f) * i;

    private static Color pausedColorFunc(float f, float i) => Color.Lerp(Color.Cyan * 1.5f, Color.OrangeRed, f) * i;

    private static float widthFunc(float i) => (1 - i) * 8;
}
