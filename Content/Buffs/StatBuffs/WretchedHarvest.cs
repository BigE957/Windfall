namespace Windfall.Content.Buffs.StatBuffs
{
    public class WretchedHarvest : ModBuff
    {
        public override string Texture => "CalamityMod/Buffs/Pets/MiniMindBuff";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.Buff().WretchedHarvest = true;
        }
        internal static void DrawEffects(PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            if (Main.rand.NextBool())
            {
                int dustType = Main.rand.NextBool() ? 184 : 18;
                Dust dust = Dust.NewDustPerfect(player.Center + new Vector2(Main.rand.NextFloat(-5f, 5f), -16 + Main.rand.NextFloat(-5f, 5f)), dustType);
                dust.scale = (dustType == 18 ? 0.6f : 1.2f);
                dust.velocity = new Vector2(2, 2).RotatedByRandom(360) * Main.rand.NextFloat(0.3f, 0.7f) + player.velocity;
                dust.noGravity = true;
                dust.alpha = 35;
            }
        }
    }
}
