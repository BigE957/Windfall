using System;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Windfall.Common.Utilities;
using CalamityMod;

namespace Windfall.Content.Buffs.Weapons
{
    public class PerfectFlow : ModBuff
    {
        public override string Texture => "Windfall/Assets/Buffs/PerfectFlow";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.Windfall().PerfectFlow = true;
            if (Main.player[Main.myPlayer].buffTime[buffIndex] == 1)
                player.AddCooldown(Cooldowns.ParryWeapon.ID, CalamityUtils.SecondsToFrames(30));
        }
    }
}
