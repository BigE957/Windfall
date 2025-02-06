using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.NPCs;
using CalamityMod.NPCs.Astral;
using CalamityMod.Sounds;
using CalamityMod.World;
using Windfall.Content.Projectiles.Props;

namespace Windfall.Content.NPCs.Enemies.AstralSiphon;
public class SiphonCollider : ModNPC
{
    public override string Texture => "CalamityMod/NPCs/Astral/SightseerCollider";

    public static Asset<Texture2D> glowmask;

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[NPC.type] = 4;

        if (!Main.dedServ)
            glowmask = ModContent.Request<Texture2D>("CalamityMod/NPCs/Astral/SightseerColliderGlow", AssetRequestMode.AsyncLoad);
        this.HideFromBestiary();
    }

    public override void SetDefaults()
    {
        NPC.width = 48;
        NPC.height = 40;

        NPC.damage = 58;
        NPC.defense = 26;
        NPC.knockBackResist = 0.48f;
        NPC.lifeMax = 460;
        NPC.DR_NERD(0.15f);

        NPC.DeathSound = CommonCalamitySounds.AstralNPCDeathSound;
        NPC.noGravity = true;
        NPC.value = Item.buyPrice(0, 0, 10, 0);
        NPC.aiStyle = -1;
        Banner = ModContent.NPCType<SightseerCollider>();

        NPC.Calamity().VulnerableToHeat = true;
        NPC.Calamity().VulnerableToSickness = false;

        // Scale stats in Expert and Master
        CalamityGlobalNPC.AdjustExpertModeStatScaling(NPC);
        CalamityGlobalNPC.AdjustMasterModeStatScaling(NPC);
    }

    int aiCounter = 0;
    int attackCounter = -120;
    private NPC Target = null;

    public override void AI()
    {
        if (!Main.npc.Any(n => n.active && n.type == ModContent.NPCType<SelenicSiphon>() && n.As<SelenicSiphon>().EventActive))
        {
            NPC.Transform(ModContent.NPCType<SightseerCollider>());
            return;
        }

        Target ??= Main.npc[NPC.FindFirstNPC(ModContent.NPCType<SelenicSiphon>())];

        if (attackCounter < 0)
        {
            #region Base Movement
            Vector2 homeInVector = Target.Center - NPC.Center;
            float targetDist = homeInVector.Length();
            homeInVector.Normalize();
            if (targetDist > 225f)
            {
                float velocity = 10f;
                NPC.velocity = (NPC.velocity * 40f + homeInVector * velocity) / 41f;
            }
            else
            {
                if (targetDist < 200f)
                {
                    float velocity = -10f;
                    NPC.velocity = (NPC.velocity * 40f + homeInVector * velocity) / 41f;
                }
                else
                {
                    NPC.velocity *= 0.9f;
                }
            }
            NPC.rotation = (Target.Center - NPC.Center).ToRotation() + Pi;
            NPC.Center += (NPC.rotation + PiOver2).ToRotationVector2() * (float)Math.Cos((aiCounter + attackCounter) / 20f) * (4f / (NPC.velocity.Length() + 1));
            #endregion

            aiCounter++;

            if (targetDist < 250f)
                attackCounter++;
            else
                attackCounter = -120;
        }
        else
        {
            if(attackCounter <= 30)
            {
                float reelBackSpeedExponent = 2.6f;
                float reelBackCompletion = Utils.GetLerpValue(0, 30, attackCounter, true);
                float reelBackSpeed = Lerp(2.5f, 16f, MathF.Pow(reelBackCompletion, reelBackSpeedExponent));
                Vector2 reelBackVelocity = NPC.DirectionTo(Target.Center) * -reelBackSpeed;
                NPC.velocity = Vector2.Lerp(NPC.velocity, reelBackVelocity, 0.25f);
            }
            else
            {
                if (attackCounter == 31)
                    NPC.velocity = NPC.DirectionTo(Target.Center) * 24f;
                NPC.velocity *= 0.97f;
                if(NPC.velocity.LengthSquared() < 2f)
                {
                    attackCounter = -120;
                    return;
                }
            }
            attackCounter++;
        }
    }

    public override void FindFrame(int frameHeight)
    {
        NPC.frameCounter += 0.05f + NPC.velocity.Length() * 0.667f;
        if (NPC.frameCounter >= 8)
        {
            NPC.frameCounter = 0;
            NPC.frame.Y += frameHeight;
            if (NPC.frame.Y > NPC.height * 2)
            {
                NPC.frame.Y = 0;
            }
        }
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
        if (NPC.soundDelay == 0)
        {
            NPC.soundDelay = 15;
            SoundEngine.PlaySound(CommonCalamitySounds.AstralNPCHitSound, NPC.Center);
        }

        CalamityGlobalNPC.DoHitDust(NPC, hit.HitDirection, (Main.rand.Next(0, Math.Max(0, NPC.life)) == 0) ? 5 : ModContent.DustType<AstralEnemy>(), 1f, 4, 22);

        //if dead do gores
        if (NPC.life <= 0)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 5; i++)
                {
                    float rand = Main.rand.NextFloat(-0.18f, 0.18f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.NextFloat(0f, NPC.width), Main.rand.NextFloat(0f, NPC.height)), NPC.velocity * rand, ModContent.GetInstance<CalamityMod.CalamityMod>().Find<ModGore>("SightseerColliderGore" + i).Type);
                }
            }
        }
    }

    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        spriteBatch.Draw(glowmask.Value, NPC.Center - screenPos + new Vector2(0, 4f), new Rectangle(0, NPC.frame.Y, 80, NPC.frame.Height), Color.White * 0.75f, NPC.rotation, new Vector2(40f, 20f), NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
    {
        if (hurtInfo.Damage > 0)
            target.AddBuff(ModContent.BuffType<AstralInfectionDebuff>(), 25, true);
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(DropHelper.NormalVsExpertQuantity(ModContent.ItemType<StarblightSoot>(), 1, 1, 2, 1, 3));
    }
}
