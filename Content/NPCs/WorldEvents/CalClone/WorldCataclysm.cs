using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Items.Placeables.Furniture.Trophies;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.NPCs.CalClone;
using CalamityMod.World;
using Windfall.Common.Systems;
using Windfall.Content.Items.Weapons.Melee;

namespace Windfall.Content.NPCs.WorldEvents.CalClone;

public class WorldCataclysm : ModNPC
{
    public override string Texture => "CalamityMod/NPCs/CalClone/Cataclysm";
    public override string BossHeadTexture => "CalamityMod/NPCs/CalClone/Cataclysm_Head_Boss";
    public override void SetStaticDefaults()
    {
        this.HideFromBestiary();
        Main.npcFrameCount[NPC.type] = 6;
        NPCID.Sets.TrailingMode[NPC.type] = 1;
    }

    public override void SetDefaults()
    {
        NPC.BossBar = Main.BigBossProgressBar.NeverValid;
        NPC.Calamity().canBreakPlayerDefense = true;
        NPC.damage = StatCorrections.ScaleContactDamage(Main.masterMode ? 198 : CalamityWorld.death ? 148 : CalamityWorld.revenge ? 138 : Main.expertMode ? 120 : 60);
        NPC.npcSlots = 5f;
        NPC.width = 120;
        NPC.height = 120;

        if (CalamityWorld.death)
            NPC.scale *= 1.2f;
        NPC.defense = CalamityWorld.death ? 15 : 10;
        NPC.DR_NERD(CalamityWorld.death ? 0.225f : 0.15f);
        NPC.LifeMaxNERB(11000 + 9200, 13200 + 11025, 80000 + 80000);
        double HPBoost = CalamityConfig.Instance.BossHealthBoost * 0.01;
        NPC.lifeMax += (int)(NPC.lifeMax * HPBoost);
        NPC.aiStyle = -1;
        AIType = -1;
        NPC.knockBackResist = 0f;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = SoundID.NPCHit4;
        NPC.DeathSound = SoundID.NPCDeath14;
        NPC.Calamity().VulnerableToHeat = false;
        NPC.Calamity().VulnerableToCold = true;
        NPC.Calamity().VulnerableToWater = true;
    }
    public override void SendExtraAI(BinaryWriter writer)
    {
        for (int i = 0; i < 4; i++)
            writer.Write(NPC.Calamity().newAI[i]);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        for (int i = 0; i < 4; i++)
            NPC.Calamity().newAI[i] = reader.ReadSingle();
    }

    public override void FindFrame(int frameHeight)
    {
        NPC.frameCounter += 0.15f;
        NPC.frameCounter %= Main.npcFrameCount[NPC.type];
        int frame = (int)NPC.frameCounter;
        NPC.frame.Y = frame * frameHeight;
    }

    public override void AI()
    {
        CalamityAIs.CataclysmAI(NPC, Mod);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        SpriteEffects spriteEffects = SpriteEffects.None;
        if (NPC.spriteDirection == 1)
            spriteEffects = SpriteEffects.FlipHorizontally;

        Texture2D texture2D15 = TextureAssets.Npc[NPC.type].Value;
        Vector2 halfSizeTexture = new(TextureAssets.Npc[NPC.type].Value.Width / 2, TextureAssets.Npc[NPC.type].Value.Height / Main.npcFrameCount[NPC.type] / 2);
        int afterimageAmt = 7;

        if (CalamityConfig.Instance.Afterimages)
        {
            for (int i = 1; i < afterimageAmt; i += 2)
            {
                Color afterimageColor = drawColor;
                afterimageColor = Color.Lerp(afterimageColor, Color.White, 0.5f);
                afterimageColor = NPC.GetAlpha(afterimageColor);
                afterimageColor *= (afterimageAmt - i) / 15f;
                Vector2 afterimageDrawPos = NPC.oldPos[i] + new Vector2(NPC.width, NPC.height) / 2f - screenPos;
                afterimageDrawPos -= new Vector2(texture2D15.Width, texture2D15.Height / Main.npcFrameCount[NPC.type]) * NPC.scale / 2f;
                afterimageDrawPos += halfSizeTexture * NPC.scale + new Vector2(0f, NPC.gfxOffY);
                spriteBatch.Draw(texture2D15, afterimageDrawPos, NPC.frame, afterimageColor, NPC.rotation, halfSizeTexture, NPC.scale, spriteEffects, 0f);
            }
        }

        Vector2 drawLocation = NPC.Center - screenPos;
        drawLocation -= new Vector2(texture2D15.Width, texture2D15.Height / Main.npcFrameCount[NPC.type]) * NPC.scale / 2f;
        drawLocation += halfSizeTexture * NPC.scale + new Vector2(0f, NPC.gfxOffY);
        spriteBatch.Draw(texture2D15, drawLocation, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, halfSizeTexture, NPC.scale, spriteEffects, 0f);

        texture2D15 = ModContent.Request<Texture2D>("CalamityMod/NPCs/CalClone/CataclysmGlow").Value;
        Color pinkLerp = Color.Lerp(Color.White, Color.Red, 0.5f);

        if (CalamityConfig.Instance.Afterimages)
        {
            for (int j = 1; j < afterimageAmt; j++)
            {
                Color extraAfterimageColor = pinkLerp;
                extraAfterimageColor = Color.Lerp(extraAfterimageColor, Color.White, 0.5f);
                extraAfterimageColor *= (afterimageAmt - j) / 15f;
                Vector2 extraAfterimageDrawPos = NPC.oldPos[j] + new Vector2(NPC.width, NPC.height) / 2f - screenPos;
                extraAfterimageDrawPos -= new Vector2(texture2D15.Width, texture2D15.Height / Main.npcFrameCount[NPC.type]) * NPC.scale / 2f;
                extraAfterimageDrawPos += halfSizeTexture * NPC.scale + new Vector2(0f, NPC.gfxOffY);
                spriteBatch.Draw(texture2D15, extraAfterimageDrawPos, NPC.frame, extraAfterimageColor, NPC.rotation, halfSizeTexture, NPC.scale, spriteEffects, 0f);
            }
        }

        spriteBatch.Draw(texture2D15, drawLocation, NPC.frame, pinkLerp, NPC.rotation, halfSizeTexture, NPC.scale, spriteEffects, 0f);

        return false;
    }

    public override bool CheckActive() => false;

    public override void OnKill()
    {
        Main.BestiaryTracker.Kills.RegisterKill(ModContent.GetInstance<Cataclysm>().NPC);
        int heartAmt = Main.rand.Next(3) + 3;
        for (int i = 0; i < heartAmt; i++)
            Item.NewItem(NPC.GetSource_Loot(), (int)NPC.position.X, (int)NPC.position.Y, NPC.width, NPC.height, ItemID.Heart);
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ModContent.ItemType<CataclysmTrophy>(), 10);
        npcLoot.Add(ModContent.ItemType<HavocsBreath>(), 4);
        npcLoot.Add(ModContent.ItemType<Boneripper>(), 4);
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
        for (int k = 0; k < 5; k++)
        {
            Dust.NewDust(NPC.position, NPC.width, NPC.height, (int)CalamityDusts.Brimstone, hit.HitDirection, -1f, 0, default, 1f);
        }
        if (NPC.life <= 0)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                Mod calamity = ModLoader.GetMod("CalamityMod");
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, calamity.Find<ModGore>("Cataclysm").Type, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, calamity.Find<ModGore>("Cataclysm2").Type, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, calamity.Find<ModGore>("Cataclysm3").Type, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, calamity.Find<ModGore>("Cataclysm4").Type, NPC.scale);
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, calamity.Find<ModGore>("Cataclysm5").Type, NPC.scale);
            }
            NPC.width = 100;
            NPC.height = 100;
            NPC.position.X = NPC.position.X - NPC.width / 2;
            NPC.position.Y = NPC.position.Y - NPC.height / 2;
            for (int i = 0; i < 40; i++)
            {
                int brimDust = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, (int)CalamityDusts.Brimstone, 0f, 0f, 100, default, 2f);
                Main.dust[brimDust].velocity *= 3f;
                if (Main.rand.NextBool())
                {
                    Main.dust[brimDust].scale = 0.5f;
                    Main.dust[brimDust].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                }
            }
            for (int j = 0; j < 70; j++)
            {
                int brimDust2 = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, (int)CalamityDusts.Brimstone, 0f, 0f, 100, default, 3f);
                Main.dust[brimDust2].noGravity = true;
                Main.dust[brimDust2].velocity *= 5f;
                brimDust2 = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, (int)CalamityDusts.Brimstone, 0f, 0f, 100, default, 2f);
                Main.dust[brimDust2].velocity *= 2f;
            }
        }
    }

    public override bool CanHitPlayer(Player target, ref int cooldownSlot)
    {
        cooldownSlot = ImmunityCooldownID.Bosses;
        return true;
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
    {
        if (hurtInfo.Damage > 0)
            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 120, true);
    }
}
