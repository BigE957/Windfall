using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.NPCs.SlimeGod;
using CalamityMod.Projectiles.Boss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace Windfall.Common.Utils;

public static partial class WindfallUtils
{
    public static void HideBestiaryEntry(this ModNPC n)
    {
        NPCID.Sets.NPCBestiaryDrawModifiers nPCBestiaryDrawModifiers = new()
        {
            Hide = true
        };
        NPCID.Sets.NPCBestiaryDrawModifiers value = nPCBestiaryDrawModifiers;
        NPCID.Sets.NPCBestiaryDrawOffset.Add(n.Type, value);
    }

    public static bool IsValidEnemy(this NPC npc, bool allowStatues = true, bool checkDead = true)
    {
        if (npc == null || (!npc.active && (!checkDead || npc.life > 0)) || npc.townNPC || npc.friendly)
        {
            return false;
        }

        if (!allowStatues && npc.SpawnedFromStatue)
        {
            return false;
        }

        if (npc.lifeMax <= 5 || (npc.defDamage <= 5 && npc.lifeMax <= 3000))
        {
            return false;
        }

        if (npc.type == NPCID.TargetDummy || npc.type == ModContent.NPCType<SuperDummyNPC>() || npc.lifeMax > 25000000)
        {
            return false;
        }

        return true;
    }

    public static bool IsABoss(this NPC npc)
    {
        if (npc == null || !npc.active)
        {
            return false;
        }

        if (npc.boss && npc.type != NPCID.MartianSaucerCore)
        {
            return true;
        }

        if (npc.type == NPCID.EaterofWorldsBody || npc.type == NPCID.EaterofWorldsHead || npc.type == NPCID.EaterofWorldsTail)
        {
            return true;
        }

        if (npc.type != ModContent.NPCType<EbonianPaladin>() && npc.type != ModContent.NPCType<CrimulanPaladin>() && npc.type != ModContent.NPCType<SplitEbonianPaladin>())
        {
            return npc.type == ModContent.NPCType<SplitCrimulanPaladin>();
        }

        return true;
    }

    public static bool AnyBossNPCS(bool checkForMechs = false)
    {
        ActiveEntityIterator<NPC>.Enumerator enumerator = Main.ActiveNPCs.GetEnumerator();
        while (enumerator.MoveNext())
        {
            NPC current = enumerator.Current;
            if (!current.IsABoss())
            {
                continue;
            }

            if (checkForMechs)
            {
                if (current.type != NPCID.TheDestroyer && current.type != NPCID.SkeletronPrime && current.type != NPCID.Spazmatism)
                {
                    return current.type == NPCID.Retinazer;
                }

                return true;
            }

            return true;
        }

        return FindFirstProjectile(ModContent.ProjectileType<DeusRitualDrama>()) != -1;
    }

    public static NPC ClosestNPC(this Vector2 origin, float maxDistanceToCheck, bool ignoreTiles = true, bool bossPriority = false)
    {
        NPC result = null;
        float num = maxDistanceToCheck;
        if (bossPriority)
        {
            bool flag = false;
            for (int i = 0; i < Main.npc.Length; i++)
            {
                if ((flag && !Main.npc[i].boss && Main.npc[i].type != NPCID.WallofFleshEye) || !Main.npc[i].CanBeChasedBy())
                {
                    continue;
                }

                float num2 = Main.npc[i].width / 2 + Main.npc[i].height / 2;
                bool flag2 = true;
                if (num2 < num && !ignoreTiles)
                {
                    flag2 = Collision.CanHit(origin, 1, 1, Main.npc[i].Center, 1, 1);
                }

                if (Vector2.Distance(origin, Main.npc[i].Center) < num && flag2)
                {
                    if (Main.npc[i].boss || Main.npc[i].type == NPCID.WallofFleshEye)
                    {
                        flag = true;
                    }

                    num = Vector2.Distance(origin, Main.npc[i].Center);
                    result = Main.npc[i];
                }
            }
        }
        else
        {
            for (int j = 0; j < Main.npc.Length; j++)
            {
                if (Main.npc[j].CanBeChasedBy())
                {
                    float num3 = Main.npc[j].width / 2 + Main.npc[j].height / 2;
                    bool flag3 = true;
                    if (num3 < num && !ignoreTiles)
                    {
                        flag3 = Collision.CanHit(origin, 1, 1, Main.npc[j].Center, 1, 1);
                    }

                    if (Vector2.Distance(origin, Main.npc[j].Center) < num && flag3)
                    {
                        num = Vector2.Distance(origin, Main.npc[j].Center);
                        result = Main.npc[j];
                    }
                }
            }
        }

        return result;
    }

    public static NPCShop AddWithCustomValue(this NPCShop shop, int itemType, int customValue, params Condition[] conditions)
    {
        Item item = new(itemType)
        {
            shopCustomPrice = customValue
        };
        return shop.Add(item, conditions);
    }

    public static NPCShop AddWithPrice<T>(this NPCShop shop, int customValue, params Condition[] conditions) where T : ModItem => shop.AddWithCustomValue(ModContent.ItemType<T>(), customValue, conditions);

    public static bool IsNPCOnScreen(Vector2 center)
    {
        int w = NPC.sWidth + NPC.safeRangeX * 2;
        int h = NPC.sHeight + NPC.safeRangeY * 2;
        Rectangle npcScreenRect = new((int)center.X - w / 2, (int)center.Y - h / 2, w, h);
        foreach (Player player in Main.player)
            if (player.active && player.getRect().Intersects(npcScreenRect))
                return true;
        return false;
    }

    private static bool IsNPCGrounded(NPC npc, Point standingTilePosition, bool orInWater = false) => 
        (npc.velocity.Y == 0 && npc.oldVelocity.Y == 0.3f) ||
        (orInWater && Main.tile[standingTilePosition + new Point(0, -1)].LiquidAmount > 0.5f);
}
