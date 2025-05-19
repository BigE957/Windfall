using Terraria;
using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.NPCs.Bosses.Orator;
using Windfall.Content.Projectiles.Boss.Orator;

namespace Windfall.Common.Players;

public class DeadPlayer : ModPlayer
{
    public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
    {
        if (Player.tongued == true && DraconicRuinsSystem.DraconicRuinsArea.Contains(Player.Center.ToTileCoordinates()))
        {
            damageSource = PlayerDeathReason.ByCustomReason(GetWindfallLocalText("Status.Death.DraconicBarrier." + Main.rand.Next(1, 3 + 1)).ToNetworkText(Player.name));
            Player.tongued = false;
        }

        if ((damage == 10 && hitDirection == 0 && damageSource.SourceOtherIndex == 8) || damageSource.SourceProjectileType == ModContent.ProjectileType<EmpyreanThorn>())
        {
            if(Player.Buff().Entropy)
                damageSource = PlayerDeathReason.ByCustomReason(GetWindfallLocalText("Status.Death.Entropy." + Main.rand.Next(1, 3 + 1)).ToNetworkText(Player.name));
        }
        if ((damageSource.SourceNPCIndex >= 0 && Main.npc[damageSource.SourceNPCIndex].type == ModContent.NPCType<TheOrator>()) || damageSource.SourceProjectileType == ModContent.ProjectileType<DarkBolt>() || damageSource.SourceProjectileType == ModContent.ProjectileType<DarkGlob>() || damageSource.SourceProjectileType == ModContent.ProjectileType<OratorScythe>() || damageSource.SourceProjectileType == ModContent.ProjectileType<OratorJavelin>() || damageSource.SourceProjectileType == ModContent.ProjectileType<FadingStar>() || damageSource.SourceProjectileType == ModContent.ProjectileType<HandRing>())
        {
            damageSource = PlayerDeathReason.ByCustomReason(GetWindfallLocalText("Status.Death.Orator." + Main.rand.Next(1, 3 + 1)).ToNetworkText(Player.name));
        }
        if (damageSource.SourceNPCIndex >= 0 && Main.npc[damageSource.SourceNPCIndex].type == ModContent.NPCType<OratorHand>())
        {
            damageSource = PlayerDeathReason.ByCustomReason(GetWindfallLocalText("Status.Death.Hand." + Main.rand.Next(1, 3 + 1)).ToNetworkText(Player.name));
        }
        if ((damageSource.SourceNPCIndex >= 0 && Main.npc[damageSource.SourceNPCIndex].type == ModContent.NPCType<ShadowHand>()) || damageSource.SourceProjectileType == ModContent.ProjectileType<DarkCoalescence>() || damageSource.SourceProjectileType == ModContent.ProjectileType<UnstableDarkness>() || damageSource.SourceProjectileType == ModContent.ProjectileType<EmpyreanThorn>())
        {
            damageSource = PlayerDeathReason.ByCustomReason(GetWindfallLocalText("Status.Death.BigDark." + Main.rand.Next(1, 3 + 1)).ToNetworkText(Player.name));
        }
        if (damageSource.SourceProjectileType == ModContent.ProjectileType<SelenicIdol>())
        {
            damageSource = PlayerDeathReason.ByCustomReason(GetWindfallLocalText("Status.Death.Idol." + Main.rand.Next(1, 3 + 1)).ToNetworkText(Player.name));
        }
        return true;
    }
}
