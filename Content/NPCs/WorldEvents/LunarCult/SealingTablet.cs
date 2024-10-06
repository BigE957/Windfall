using Terraria.ModLoader;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Content.NPCs.Bosses.Orator;

namespace Windfall.Content.NPCs.WorldEvents.LunarCult
{
    public class SealingTablet : ModNPC
    {
        public override string Texture => "Windfall/Assets/NPCs/WorldEvents/SealingTablet";
        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            Main.npcFrameCount[Type] = 5;
        }
        public override void SetDefaults()
        {
            NPC.lifeMax = 400;
            NPC.defense = 0;
            NPC.damage = 0;
            NPC.width = 54;
            NPC.height = 54;
            NPC.aiStyle = -1;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.value = 0f;
            NPC.npcSlots = 0f;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.dontTakeDamage = true;
            NPC.netAlways = true;
            NPC.chaseable = false;
        }
        private float summonRatio = 0f;
        public override void AI()
        {
            if (!AnyBossNPCS(true) && !NPC.AnyNPCs(ModContent.NPCType<TheOrator>()) && NPC.ai[0] == 2)
            {                
                Player closestPlayer = Main.player[Player.FindClosest(NPC.Center, NPC.width, NPC.height)];
                if ((closestPlayer.Center - NPC.Center).Length() < 200f)
                    summonRatio += 0.001f;
                else if (summonRatio > 0f)
                    summonRatio -= 0.001f;
                else
                {
                    summonRatio = 0;
                    return;
                }
                Vector2 spawnOffset = Vector2.One * Main.rand.NextFloat(-16f, 24f);
                Vector2 DungeonCoords = new Vector2(Main.dungeonX - 4, Main.dungeonY).ToWorldCoordinates();
                if (Main.rand.NextBool(summonRatio))
                    EmpyreanMetaball.SpawnDefaultParticle(NPC.Center + spawnOffset, spawnOffset.RotatedBy((Main.rand.NextBool() ? PiOver2 : -PiOver2) + Main.rand.NextFloat(-PiOver4, PiOver4)).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0f, 4f), Main.rand.NextFloat(10, 20));
                if (summonRatio > 0.75f)
                {                    
                    float width = 124f * ExpInEasing((summonRatio - 0.75f) / 0.25f, 1);
                    width = Clamp(width, 0f, 96f);
                    //Main.NewText(width);
                    for (int i = 0; i < 18; i++)
                        EmpyreanMetaball.SpawnDefaultParticle(new Vector2(DungeonCoords.X + Main.rand.NextFloat(-width, width), DungeonCoords.Y + Main.rand.NextFloat(0, 24f)), new Vector2(Main.rand.Next(-2, 2), Main.rand.Next(-5, -1) * SineInEasing((summonRatio - 0.75f) / 0.25f, 1)), Main.rand.NextFloat(15f, 25f) * ((summonRatio - 0.75f) / 0.25f) * 2f);
                }
                if (summonRatio >= 1f && !NPC.AnyNPCs(ModContent.NPCType<TheOrator>()))
                {
                    SoundEngine.PlaySound(SoundID.DD2_EtherianPortalDryadTouch, NPC.Center);
                    Vector2 spawnPos = new(DungeonCoords.X, DungeonCoords.Y - 8);                    
                    NPC boss = NPC.NewNPCDirect(NPC.GetSource_NaturalSpawn(), (int)spawnPos.X, (int)spawnPos.Y, ModContent.NPCType<TheOrator>());
                    for (int i = 0; i < 32; i++)
                        EmpyreanMetaball.SpawnDefaultParticle(boss.Center + new Vector2(Main.rand.NextFloat(-64, 64), 64), Vector2.UnitY * Main.rand.NextFloat(4f, 24f) * -1, Main.rand.NextFloat(110f, 130f));
                }

            }
        }
        public override bool CheckActive() => false;
        public override void FindFrame(int frameHeight)
        {
            if (NPC.ai[0] == 1)
            {
                NPC.frameCounter += 0.2f;
                NPC.frame.Y = frameHeight * (((int)NPC.frameCounter % 4) + 1);
                Lighting.AddLight(NPC.Center, new Vector3(1f, 0.84f, 0f));
            }
            else
                NPC.frame.Y = 0;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[NPC.type].Value;
            Vector2 halfSizeTexture = new(texture.Width / 2, texture.Height / Main.npcFrameCount[NPC.type] / 2);
            Vector2 drawPosition = NPC.Center - screenPos + (Vector2.UnitY * NPC.gfxOffY);
            spriteBatch.Draw(texture, drawPosition, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, halfSizeTexture, NPC.scale, SpriteEffects.None, 0f);
            return false;
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.ai[0] != 2)
                return;
            Texture2D texture = ModContent.Request<Texture2D>("Windfall/Assets/NPCs/WorldEvents/SealingTabletCrack").Value;
            Vector2 drawPosition = NPC.Center - screenPos + (Vector2.UnitY * NPC.gfxOffY);
            drawPosition.Y -= 4;
            Vector2 halfSizeTexture = new(texture.Width / 2, texture.Height / Main.npcFrameCount[NPC.type] / 2);
            spriteBatch.Draw(texture, drawPosition, null, NPC.GetAlpha(drawColor), NPC.rotation, halfSizeTexture, NPC.scale, SpriteEffects.None, 0f);
        }
    }
}
