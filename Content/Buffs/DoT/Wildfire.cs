using CalamityMod.NPCs;
using Terraria;

namespace Windfall.Content.Buffs.DoT;
public class Wildfire : ModBuff
{
    public override string Texture => "Windfall/Assets/Buffs/Wildfire";

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.pvpBuff[Type] = true;
        Main.buffNoSave[Type] = true;

        CalamityLists.debuffList.Add(Type);
        CalamityLists.fireDebuffList.Add(Type);
        CalamityGlobalNPC.moddedDebuffTextureList.Add((Texture, NPC => NPC.Debuff().Wildfire));
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.Buff().Wildfire = true;
    }

    public override void Update(NPC npc, ref int buffIndex)
    {
        npc.Debuff().Wildfire = npc.buffTime[buffIndex] > 0;
    }
}