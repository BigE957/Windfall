using Terraria.GameContent.UI;
using Windfall.Common.Netcode;
using Windfall.Content.Items.GlobalItems;

namespace Windfall;

public class Windfall : Mod
{
    internal static Windfall Instance;
    public static int LunarCoinCurrencyID;

    public override void Load()
    {
        Instance = this;
        LunarCoinCurrencyID = CustomCurrencyManager.RegisterCurrency(new Content.Currencies.LunarCoinCurrency(ModContent.ItemType<Content.Items.Quests.LunarCoin>(), 99L, "Mods.Windfall.Currencies.LunarCoinCurrency"));
    }

    public override void PostSetupContent()
    {
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
    }

    public override void Unload()
    {
        Instance = null;
    }

    public override void HandlePacket(BinaryReader reader, int whoAmI) => WindfallNetcode.HandlePacket(this, reader, whoAmI);
}