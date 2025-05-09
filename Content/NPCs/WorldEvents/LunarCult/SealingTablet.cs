using Windfall.Common.Graphics.Metaballs;
using Windfall.Common.Graphics.Verlet;
using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.Items.Quests.SealingRitual;
using Windfall.Content.NPCs.Bosses.Orator;
using static Windfall.Common.Graphics.Verlet.VerletIntegration;

namespace Windfall.Content.NPCs.WorldEvents.LunarCult;

public class SealingTablet : ModNPC
{
    public override string Texture => "Windfall/Assets/NPCs/WorldEvents/SealingTablet";
    public override void SetStaticDefaults()
    {
        this.HideBestiaryEntry();
        Main.npcFrameCount[Type] = 5;
    }
    public override void SetDefaults()
    {
        NPC.lifeMax = 400;
        NPC.defense = 0;
        NPC.damage = 0;
        NPC.width = NPC.height = 54;
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
    private bool isHovered = false;

    VerletObject box;
    VerletObject LeftChain;
    VerletObject RightChain;

    public override void OnSpawn(IEntitySource source)
    {
        int verletCount = 22;

        box = CreateVerletBox(new((int)NPC.position.X, (int)NPC.position.Y, 18, 18));

        LeftChain = CreateVerletChain(NPC.Bottom + new Vector2(-48, 0), box[0].Position, verletCount, 4);
        RightChain = CreateVerletChain(NPC.Bottom + new Vector2(48, 0), box[2].Position, verletCount, 4);

        ConnectVerlets(LeftChain[^1], box[0], 4);
        ConnectVerlets(RightChain[^1], box[2], 4);
    }

    public override void AI()
    {
        if (!IsNPCOnScreen(NPC.Center))
            return;

        // Orator Summoning
        if (!AnyBossNPCS(true) && !NPC.AnyNPCs(ModContent.NPCType<TheOrator>()) && NPC.ai[0] == 2)
        {
            Lighting.AddLight(NPC.Center, Color.Lerp(new Color(117, 255, 159), new Color(255, 180, 80), (float)(Math.Sin(Main.GlobalTimeWrappedHourly * 1.25f) / 0.5f) + 0.5f).ToVector3() * (summonRatio + 0.25f));
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
                float width = 124f * ExpInEasing((summonRatio - 0.75f) / 0.25f);
                width = Clamp(width, 0f, 96f);
                //Main.NewText(width);
                for (int i = 0; i < 18; i++)
                    EmpyreanMetaball.SpawnDefaultParticle(new Vector2(DungeonCoords.X + Main.rand.NextFloat(-width, width), DungeonCoords.Y + Main.rand.NextFloat(0, 24f)), new Vector2(Main.rand.Next(-2, 2), Main.rand.Next(-5, -1) * SineInEasing((summonRatio - 0.75f) / 0.25f)), Main.rand.NextFloat(15f, 25f) * ((summonRatio - 0.75f) / 0.25f) * 2f);
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

        AffectVerletObject(box, 0.125f, 0.8f);
        AffectVerletObject(LeftChain, 0.125f, 0.8f);
        AffectVerletObject(RightChain, 0.125f, 0.8f);

        //float chainDistance = Vector2.Distance(LeftChain[^1].Position, RightChain[^1].Position);
        //Vector2 chainToChain = (RightChain[^1].Position - LeftChain[^1].Position).SafeNormalize(Vector2.UnitX);

        if (NPC.ai[0] < 4)
        {
            if (!DraconicRuinsSystem.CutsceneActive && NPC.ai[0] == 0)
            {
                if (NPC.Hitbox.Contains((int)Main.MouseWorld.X, (int)Main.MouseWorld.Y))
                {
                    isHovered = true;

                    if (Main.mouseRight)
                    {
                        NPC.ai[0] = 5;
                        NPC.velocity = new(Math.Sign(NPC.Center.X - Main.LocalPlayer.Center.X) * 1.5f, -5.5f);
                        NPC.netUpdate = true;
                        DraconicRuinsSystem.State = DraconicRuinsSystem.CutsceneState.PlayerFumble;
                        DraconicRuinsSystem.StartCutscene();
                        foreach (NPC cultist in Main.npc.Where(n => n.active && n.type == ModContent.NPCType<LunarCultistDevotee>()))
                            cultist.ai[2] = 7;
                    }
                }
                else
                {
                    isHovered = false;
                }
            }

            VerletSimulation(box, 30, gravity: 0.8f, windAffected: false);
            VerletSimulation(LeftChain, 30, gravity: 0.8f, windAffected: false);
            VerletSimulation(RightChain, 30, gravity: 0.8f, windAffected: false);

            NPC.Center = (box[0].Position + box[2].Position) / 2f;
            NPC.rotation = (box[0].Position - box[2].Position).ToRotation() + Pi;
        }
        else
        {
            if(box != null)
            {
                BreakVerletConnection(LeftChain[^1], box[0]);
                BreakVerletConnection(RightChain[^1], box[2]);

                DraconicRuinsSystem.LeftChain = LeftChain;
                DraconicRuinsSystem.RightChain = RightChain;

                box = null;
                LeftChain = null;
                RightChain = null;
            }

            NPC holder = Main.npc[(int)NPC.ai[1]];
            if (NPC.ai[0] == 4)
                NPC.Center = holder.Center + new Vector2(holder.direction * 8, -28);
            else
            {
                NPC.noGravity = false;
                NPC.noTileCollide = false;

                if(NPC.collideX || NPC.collideY)
                {
                    SoundEngine.PlaySound(SoundID.Shatter, NPC.Center);
                    for (int i = 0; i < 5; i++)
                    {
                        Item item = Main.item[Item.NewItem(NPC.GetSource_Loot(), NPC.position, new Vector2(NPC.width, NPC.height), ModContent.ItemType<TabletFragment>())];
                        item.velocity = new(Main.rand.NextFloat(-2f, 5.5f) * Math.Sign(NPC.velocity.X), -3);
                    }
                    NPC.velocity = Vector2.Zero;

                    NPC.active = false;
                }
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
        if(LeftChain != null)
            for (int k = 0; k < LeftChain.Count - 1; k++)
            {
                Vector2 line = LeftChain[k].Position - LeftChain[k + 1].Position;
                Color lighting = Lighting.GetColor((LeftChain[k + 1].Position + (line / 2f)).ToTileCoordinates());

                Main.spriteBatch.DrawLineBetween(LeftChain[k].Position, LeftChain[k + 1].Position, Color.White.MultiplyRGB(lighting), 3);
            }

        if (RightChain != null)
            for (int k = 0; k < RightChain.Count - 1; k++)
            {
                Vector2 line = RightChain[k].Position - RightChain[k + 1].Position;
                Color lighting = Lighting.GetColor((RightChain[k + 1].Position + (line / 2f)).ToTileCoordinates());

                Main.spriteBatch.DrawLineBetween(RightChain[k].Position, RightChain[k + 1].Position, Color.White.MultiplyRGB(lighting), 3);
            }

        Texture2D texture = TextureAssets.Npc[NPC.type].Value;
        Vector2 halfSizeTexture = new(texture.Width / 2, texture.Height / Main.npcFrameCount[NPC.type] / 2);
        Vector2 drawPosition = NPC.Center - screenPos + (Vector2.UnitY * NPC.gfxOffY);
        drawPosition.Y -= 16;
        spriteBatch.Draw(texture, drawPosition, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, halfSizeTexture, NPC.scale, SpriteEffects.None, 0f);
        
        return false;
    }
    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (NPC.ai[0] == 2)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture + "Crack").Value;
            Vector2 drawPosition = NPC.Center - screenPos + (Vector2.UnitY * NPC.gfxOffY);
            drawPosition.Y -= 4;
            Vector2 origin = texture.Size() * 0.5f;
            spriteBatch.Draw(texture, drawPosition, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale, SpriteEffects.None, 0f);
            return;
        }
        if (DraconicRuinsSystem.State == DraconicRuinsSystem.CutsceneState.CultistFumble && NPC.ai[0] == 0 && isHovered)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture + "Outline").Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPosition = NPC.Center - screenPos + (Vector2.UnitY * NPC.gfxOffY);
            drawPosition.Y -= 16;
            spriteBatch.Draw(texture, drawPosition, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale, SpriteEffects.None, 0f);
            return;
        }
        
    }
}
