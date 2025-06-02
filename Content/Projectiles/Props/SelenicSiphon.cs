using CalamityMod.NPCs.Astral;
using CalamityMod.Particles;
using Windfall.Content.NPCs.Enemies.AstralSiphon;
using Windfall.Content.NPCs.WorldEvents.LunarCult;
using Windfall.Content.Projectiles.Enemies;

namespace Windfall.Content.Projectiles.Props;
public class SelenicSiphon : ModNPC
{
    public new string LocalizationCategory => "Projectiles.Props";

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 1;
    }

    public override void SetDefaults()
    {
        NPC.width = 64;
        NPC.height = 128;
        NPC.lifeMax = 1800;
        NPC.friendly = true;
        NPC.behindTiles = true;
        NPC.scale = 1.5f;
        NPC.dontCountMe = true;
        NPC.noTileCollide = true;
        NPC.noGravity = true;
        NPC.knockBackResist = 0f;
    }

    public ref float Time => ref NPC.ai[0];

    public ref float Sign => ref NPC.ai[1];

    public ref float EndTime => ref NPC.ai[2];

    private Vector2 goalPos = Vector2.Zero;
    private Vector2 startPos
    {
        get => goalPos + (Vector2.UnitY * NPC.height);
    }

    public override void OnSpawn(IEntitySource source)
    {
        Sign = Main.rand.NextBool() ? 1 : -1;

        EndTime = -1;

        NPC.rotation = PiOver4 * Sign;

        NPC.Center = NPC.Center.ToTileCoordinates().ToWorldCoordinates();

        goalPos = NPC.Top + Vector2.UnitY * NPC.height * 0.5f;

        NPC.position.Y += NPC.height;

        NPC.netUpdate = true;
    }

    public Rectangle HarvestingHitbox = Rectangle.Empty;
    public float FillRatio = 0f;
    public bool EventActive = false;
    private bool EventFailed = false;

    public override void AI()
    {
        #region There Can Be Only One
        if (Main.npc.Where(n => n.active && n.type == Type).Count() > 1)
            if (Main.npc.First(n => n.active && n.type == Type).whoAmI != NPC.whoAmI)
                NPC.active = false;
        #endregion

        if (EndTime == -1)
        {
            #region Spawn Animation
            if (Time < 135 && Time % 45 == 0)
            {
                Point goalTile = Vector2.Lerp(startPos, goalPos, 0.5f).ToTileCoordinates();
                WorldGen.KillTile(goalTile.X, goalTile.Y, effectOnly: true);
                WorldGen.KillTile(goalTile.X - 1, goalTile.Y, effectOnly: true);
                WorldGen.KillTile(goalTile.X + 1, goalTile.Y, effectOnly: true);
                WorldGen.KillTile(goalTile.X - 2, goalTile.Y, effectOnly: true);
                WorldGen.KillTile(goalTile.X + 2, goalTile.Y, effectOnly: true);
                if (Time > 45)
                {
                    WorldGen.KillTile(goalTile.X - 3, goalTile.Y, effectOnly: true);
                    WorldGen.KillTile(goalTile.X + 3, goalTile.Y, effectOnly: true);
                    WorldGen.KillTile(goalTile.X - 4, goalTile.Y, effectOnly: true);
                    WorldGen.KillTile(goalTile.X + 4, goalTile.Y, effectOnly: true);
                }
            }

            if (Time < 45)
            {
                float lerpVal = Time / 45f;
                lerpVal = ExpOutEasing(lerpVal);

                NPC.rotation = Lerp(PiOver4 * Sign, PiOver4 / 2f * -Sign, lerpVal);
                NPC.Center = new(NPC.Center.X, Lerp(startPos.Y, startPos.Y - (NPC.height / 2f), lerpVal));
            }
            else if (Time < 90)
            {
                float lerpVal = (Time - 45) / 45f;
                lerpVal = ExpOutEasing(lerpVal);

                NPC.rotation = Lerp(PiOver4 / 2f * -Sign, PiOver4 / 3f * Sign, lerpVal);
                NPC.Center = new(NPC.Center.X, Lerp(startPos.Y - (NPC.height / 2f), startPos.Y - (NPC.height / 2f * 1.5f), lerpVal));

            }
            else if (Time < 135)
            {
                EventActive = true;
                float lerpVal = (Time - 90) / 45f;
                lerpVal = ExpOutEasing(lerpVal);

                NPC.rotation = Lerp(PiOver4 / 3f * Sign, 0, lerpVal);
                NPC.Center = new(NPC.Center.X, Lerp(startPos.Y - (NPC.height / 2f * 1.5f), startPos.Y - NPC.height, lerpVal));
            }
            #endregion
            else
            {
                EventActive = true;
                float oldFillRatio = FillRatio;

                if (Time == 135)
                {
                    NPC.Center = goalPos;
                    NPC.rotation = 0;
                }

                HarvestingHitbox = new((int)NPC.position.X, (int)NPC.Top.Y - 8, NPC.width, NPC.width);
                HarvestingHitbox = Main.ReverseGravitySupport(HarvestingHitbox);

                foreach (NPC n in Main.npc.Where(n => n.active && (n.type == ModContent.NPCType<AstralSlime>())))
                {
                    if (HarvestingHitbox.Intersects(n.Hitbox))
                    {
                        Vector2 poofPos = Vector2.Lerp(n.Center, HarvestingHitbox.Center(), 0.66f);
                        for (int i = 0; i < 6; i++)
                        {
                            Color color = Main.rand.NextBool() ? Color.Cyan * 1.5f : Color.OrangeRed;
                            Particle particle = new SparkParticle(poofPos, Main.rand.NextVector2Circular(4f, 4f), false, 32, 1f, color);
                            GeneralParticleHandler.SpawnParticle(particle);
                        }
                        Particle pulse = new PulseRing(poofPos, Vector2.Zero, Main.rand.NextBool() ? Color.Cyan * 1.5f : Color.OrangeRed, 0f, 0.35f, 16);
                        GeneralParticleHandler.SpawnParticle(pulse);

                        n.active = false;

                        FillRatio += 0.05f;
                    }
                }

                foreach (Projectile p in Main.projectile.Where(p => p.active && (p.type == ModContent.ProjectileType<AstralEnergy>() || p.type == ModContent.ProjectileType<AstralBolt>())))
                {
                    if ((p.type == ModContent.ProjectileType<AstralBolt>() || p.ai[0] > 30) && HarvestingHitbox.Intersects(p.Hitbox))
                    {
                        Vector2 poofPos = Vector2.Lerp(p.Center, HarvestingHitbox.Center(), 0.66f);
                        for (int i = 0; i < 6; i++)
                        {
                            Color color = Main.rand.NextBool() ? Color.Cyan * 1.5f : Color.OrangeRed;
                            Particle particle = new SparkParticle(poofPos, Main.rand.NextVector2Circular(4f, 4f), false, 32, 1f, color);
                            GeneralParticleHandler.SpawnParticle(particle);
                        }
                        Particle pulse = new PulseRing(poofPos, Vector2.Zero, Main.rand.NextBool() ? Color.Cyan * 1.5f : Color.OrangeRed, 0f, 0.35f, 16);
                        GeneralParticleHandler.SpawnParticle(pulse);

                        p.active = false;

                        if (p.type == ModContent.ProjectileType<AstralEnergy>())
                            FillRatio += 0.1f;
                        else
                            FillRatio += 0.01f;
                    }
                }

                if (FillRatio > oldFillRatio)
                {
                    Particle pulse = new PulseRing(HarvestingHitbox.Center(), Vector2.Zero, Main.rand.NextBool() ? Color.Cyan * 1.5f : Color.OrangeRed, 0f, 0.75f, 24);
                    GeneralParticleHandler.SpawnParticle(pulse);
                }

                if (FillRatio >= 1f)
                {
                    NPC.dontTakeDamage = true;
                    FillRatio = 1f;
                    EndTime++;
                    return;
                }
            }

            Time++;
        }
        else
        {
            if(EndTime < 135)
            {
                //Event End Animation
                if(EndTime % 15 == 0)
                {
                    Particle pulse = new PulseRing(HarvestingHitbox.Center(), Vector2.Zero, Main.rand.NextBool() ? Color.Cyan * 1.5f : Color.OrangeRed, 1f, Lerp(0.75f, 0, EndTime / 135f), 24);
                    GeneralParticleHandler.SpawnParticle(pulse);
                }
            }
            if (EndTime == 135)
            {
                Particle pulse = new PulseRing(HarvestingHitbox.Center(), Vector2.Zero, Main.rand.NextBool() ? Color.Cyan * 1.5f : Color.OrangeRed, 0f, 1.5f, 24);
                GeneralParticleHandler.SpawnParticle(pulse);

                EventActive = false;
                foreach(Player player in Main.ActivePlayers)
                {
                    if (player.LunarCult().apostleQuestTracker == 13)
                        player.LunarCult().apostleQuestTracker++;
                    int index = NPC.FindFirstNPC(ModContent.NPCType<DisgracedApostleNPC>());
                    if (index != -1)
                        Main.npc[index].ai[0] = 4;
                }
            }
            else
            {
                NPC.position.Y += 4;
            }
            if(EndTime == 270)
                NPC.active = false;

            EndTime++;
        }
    }

    public override bool? CanBeHitByItem(Player player, Item item) => false;

    public override bool? CanBeHitByProjectile(Projectile projectile)
    {
        if(projectile.owner != -1)
            return false;

        return base.CanBeHitByProjectile(projectile);
    }

    public override bool CanBeHitByNPC(NPC attacker) => attacker.type == ModContent.NPCType<SiphonCollider>();

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Vector2 drawCenter = NPC.Center - screenPos;

        int width = 8;
        int height = 16;
        float size = 8f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Rectangle frame = texture.Frame(width, height, x, y);
                Vector2 origin = frame.Size() * 0.5f;
                Vector2 centerOffset = new Vector2(x - (width / 2), y - (height / 2)).RotatedBy(NPC.rotation) * size * NPC.scale;
                centerOffset += origin;
                spriteBatch.Draw(texture, drawCenter + centerOffset, frame, Lighting.GetColor((NPC.Center + centerOffset).ToTileCoordinates()) * NPC.Opacity, NPC.rotation, origin, NPC.scale, 0, 0);
            }
        }

        if (HarvestingHitbox != Rectangle.Empty)
        {
            Rectangle drawnBox = HarvestingHitbox;

            drawnBox.Offset((int)-Main.screenPosition.X, (int)-Main.screenPosition.Y);

            //spriteBatch.Draw(TextureAssets.MagicPixel.Value, drawnBox, Color.Red * 0.6f);
        }

        return false;
    }

    public override void OnKill()
    {
        EventFailed = true;
        NPC.dontTakeDamage = true;
        NPC.life = 1000;
        NPC.active = true;
    }
}
