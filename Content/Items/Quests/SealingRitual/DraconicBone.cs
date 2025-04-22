
using Windfall.Common.Systems;
using Windfall.Common.Systems.WorldEvents;

namespace Windfall.Content.Items.Quests.SealingRitual;

public class DraconicBone : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Quest";
    public override string Texture => "Windfall/Assets/Items/Quest/DraconicBone";
    public override void SetDefaults()
    {
        Item.width = 22;
        Item.height = 24;
        Item.rare = ItemRarityID.Quest;
        Item.maxStack = 1;
    }

    public override bool OnPickup(Player player)
    {
        if (QuestSystem.Quests["DraconicBone"].Active)
            LunarCultBaseSystem.DraconicBoneSequenceActive = true;
        return true;
    }
}
