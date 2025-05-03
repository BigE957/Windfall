using CalamityMod;
using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.NPCs.Bosses.Orator;
using Windfall.Content.Projectiles.Other;

namespace Windfall.Content.Items.Quests;

public class SelenicTablet : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Quest";
    public override string Texture => "Windfall/Assets/Items/Quest/SelenicTablet";
    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 20;
        Item.rare = ItemRarityID.Cyan;
        Item.useTime = Item.useAnimation = 65;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.shoot = ModContent.ProjectileType<HideoutSeeker>();
        Item.shootSpeed = 20f;
    }
    public override bool CanShoot(Player player)
    {
        return !SealingRitualSystem.RitualSequenceSeen && LunarCultBaseSystem.ActivityCoords != new Point(-1,-1);
    }
    public static string Key = NPC.downedEmpressOfLight || DownedBossSystem.downedRavager ? "Pre-Lunar" : NPC.downedGolemBoss ? "Post-Golem" : "Post-Plant";
    public override bool? UseItem(Player player)
    {
        if (CanShoot(player)) //Cult Meeting Active
        {
            return true;

        }
        else //Cult Meeting In-Active
        {
            return false;
            /*
            if (player.LunarCult().apostleQuestTracker == 13 && player.InAstral(1))
            {
                Main.NewText("Start Astral Siphon");
            }
            */
        }
    }
    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        TooltipLine line = tooltips.FirstOrDefault(x => x.Mod == "Terraria" && x.Name == "Tooltip0");
        if (!SealingRitualSystem.RitualSequenceSeen)
        {
            if (line != null)
                line.Text = GetWindfallTextValue("Items.Quest.SelenicTablet.SeekerTooltip");
            return;
        }
        string tooltip = GetWindfallTextValue("Items.Quest.SelenicTablet.SummonTooltip");
        if (line != null)
            line.Text = tooltip;
    }
}
