using CalamityMod.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windfall.Common.Graphics.Metaballs;

namespace Windfall.Content.Buffs.DoT;

public class Entropy : ModBuff
{
    public override string Texture => "CalamityMod/Buffs/StatBuffs/EmpyreanWrath";
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.pvpBuff[Type] = true;
        Main.buffNoSave[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.Buff().Entropy = true;
    }

    public override void Update(NPC npc, ref int buffIndex)
    {
        int DoT = npc.lifeMax / 2;
        npc.Calamity().ApplyDPSDebuff(DoT, DoT / 5, ref npc.lifeRegen, ref npc.damage);
        if (Main.rand.NextBool())
        {
            Vector2 position = npc.Center + new Vector2(Main.rand.NextFloat(-npc.width / 2, npc.width / 2), Main.rand.NextFloat(-npc.height / 2, npc.height / 2));
            EmpyreanMetaball.SpawnDefaultParticle(position, Vector2.UnitY * Main.rand.NextFloat(-2f, 0f), Main.rand.NextFloat(20f, 50f));
        }
    }
    internal static void DrawEffects(PlayerDrawSet drawInfo)
    {
    }

    internal static void DrawEffects(NPC npc, ref Color drawColor)
    {
        if (Main.rand.NextBool())
        {
            Vector2 position = npc.Center + new Vector2(Main.rand.NextFloat(-npc.width / 2, npc.width / 2), Main.rand.NextFloat(-npc.height / 2, npc.height / 2));
            EmpyreanMetaball.SpawnDefaultParticle(position, Vector2.UnitY * Main.rand.NextFloat(-2f, 0f), Main.rand.NextFloat(20f, 50f));
        }
    }
}
