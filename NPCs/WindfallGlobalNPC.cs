﻿using Terraria;
using Terraria.ModLoader;
using Windfall.Systems;

namespace Windfall.NPCs
{
    public class WindfallGlobalNPC : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            Mod calamity = ModLoader.GetMod("CalamityMod");
            if (npc.type == calamity.Find<ModNPC>("Cnidrion").Type)
                DownedNPCSystem.downedCnidrion = true;
            if (npc.type == calamity.Find<ModNPC>("LeviathanStart").Type)
                DownedNPCSystem.downedSirenLure = true;
        }
    }
}