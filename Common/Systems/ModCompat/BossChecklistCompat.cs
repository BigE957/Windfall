using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windfall.Content.Items.Quest;
using Windfall.Content.Items.Weapons.Summon;
using Windfall.Content.NPCs.Bosses.TheOrator;

namespace Windfall.Common.Systems.ModCompat
{
    public class BossChecklistCompat : ModSystem
    {
        private static readonly string checklistPath = "ModCompat.BossChecklist.";
        public override void PostSetupContent()
        {
            if (!ModLoader.TryGetMod("BossChecklist", out Mod bossChecklistMod))
                return;

            if (bossChecklistMod.Version < new Version(1, 6))
                return;

            string internalName = "TheOrator";
            float weight = 17.0001f;
            Func<bool> downed = () => DownedNPCSystem.downedOrator;
            int bossType = ModContent.NPCType<TheOrator>();
            LocalizedText spawnInfo = GetWindfallLocalText(checklistPath + "Orator.SpawnInfo");
            LocalizedText despawnMessage = GetWindfallLocalText(checklistPath + "Orator.DespawnMessage");
            int spawnItem = ModContent.ItemType<SelenicTablet>();
            List<int> collectibles =
            [
                ModContent.ItemType<ShadowHandStaff>(),
            ];

            bossChecklistMod.Call(
                "LogBoss",
                Mod,
                internalName,
                weight,
                downed,
                bossType,
                new Dictionary<string, object>()
                {
                    ["spawnInfo"] = spawnInfo,
                    ["despawnMessage"] = despawnMessage,
                    ["spawnItems"] = spawnItem,
                    ["collectibles"] = collectibles,
                }
            );
        }
    }
}
