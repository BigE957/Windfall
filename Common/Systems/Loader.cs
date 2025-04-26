using CalamityMod.UI;
using DialogueHelper.UI.Dialogue;
using System.Reflection;
using Terraria.Graphics.Effects;
using Windfall.Content.Items.GlobalItems;
using Windfall.Content.Skies;
using Windfall.Content.Skies.CorruptCommunion;
using Windfall.Content.Skies.CrimsonCommunion;
using Windfall.Content.Skies.SlimyCommunion;
using Windfall.Content.UI.Events;

namespace Windfall.Common.Systems;

public class Loading : ModSystem
{
    public override void Load()
    {
        DialogueUISystem.SubmodulePath = "Windfall/SubModules/";

        #region Custom Skies
        SkyManager.Instance["Windfall:Orator"] = new OratorSky();

        Filters.Scene["Windfall:CorruptCommunion"] = new Filter(new CorruptCommunionScreenShaderData("FilterMiniTower").UseColor(0.6f, 0.2f, 0.6f).UseOpacity(0.5f), EffectPriority.VeryHigh);
        SkyManager.Instance["Windfall:CorruptCommunion"] = new CorruptCommunionSky();

        Filters.Scene["Windfall:CrimsonCommunion"] = new Filter(new CrimsonCommunionScreenShaderData("FilterMiniTower").UseColor(0.6f, 0.2f, 0.2f).UseOpacity(0.5f), EffectPriority.VeryHigh);
        SkyManager.Instance["Windfall:CrimsonCommunion"] = new CrimsonCommunionSky();

        Filters.Scene["Windfall:SlimyCommunion"] = new Filter(new SlimyCommunionScreenShaderData("FilterMiniTower").UseColor(0.4f, 0.4f, 0.6f).UseOpacity(0.5f), EffectPriority.VeryHigh);
        SkyManager.Instance["Windfall:SlimyCommunion"] = new SlimyCommunionSky();
        #endregion
    }
    public override void PostSetupContent()
    {
        #region Event Bars
        ModLoader.GetMod("CalamityMod").Call("RegisterModCooldowns", this);
        FieldInfo InvasionGUIsFieldInfo = typeof(InvasionProgressUIManager).GetField("gUIs", BindingFlags.NonPublic | BindingFlags.Static);
        List<InvasionProgressUI> guis = ((List<InvasionProgressUI>)InvasionGUIsFieldInfo.GetValue(null));
        guis.Add(Activator.CreateInstance(typeof(TailorEventBar)) as InvasionProgressUI);
        guis.Add(Activator.CreateInstance(typeof(CafeteriaEventBar)) as InvasionProgressUI);
        guis.Add(Activator.CreateInstance(typeof(RitualEventBar)) as InvasionProgressUI);
        guis.Add(Activator.CreateInstance(typeof(AstralSiphonEventBar)) as InvasionProgressUI);
        #endregion

        #region Spacial Lock Affected Items
        WindfallGlobalItem.SpacialLockAffectedItems.Add(ItemID.RodofDiscord);
        WindfallGlobalItem.SpacialLockAffectedItems.Add(ItemID.RodOfHarmony);
        WindfallGlobalItem.SpacialLockAffectedItems.Add(ItemID.TeleportationPotion);
        WindfallGlobalItem.SpacialLockAffectedItems.Add(ItemID.RecallPotion);
        WindfallGlobalItem.SpacialLockAffectedItems.Add(ItemID.PotionOfReturn);
        WindfallGlobalItem.SpacialLockAffectedItems.Add(ItemID.MagicMirror);
        WindfallGlobalItem.SpacialLockAffectedItems.Add(ItemID.IceMirror);
        WindfallGlobalItem.SpacialLockAffectedItems.Add(ItemID.CellPhone);
        WindfallGlobalItem.SpacialLockAffectedItems.Add(ItemID.Shellphone);
        WindfallGlobalItem.SpacialLockAffectedItems.Add(ItemID.ShellphoneDummy);
        WindfallGlobalItem.SpacialLockAffectedItems.Add(ItemID.ShellphoneHell);
        WindfallGlobalItem.SpacialLockAffectedItems.Add(ItemID.ShellphoneOcean);
        WindfallGlobalItem.SpacialLockAffectedItems.Add(ItemID.ShellphoneSpawn);
        WindfallGlobalItem.SpacialLockAffectedItems.Add(ItemID.MagicConch);
        WindfallGlobalItem.SpacialLockAffectedItems.Add(ItemID.DemonConch);
        #endregion
    }
}
