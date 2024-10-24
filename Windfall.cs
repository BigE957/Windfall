using CalamityMod.UI;
using System.Reflection;
using Terraria.GameContent.UI;
using Windfall.Content.UI.Events;

namespace Windfall
{
    public class Windfall : Mod
    {
        internal static Windfall Instance;
        public static int LunarCoinCurrencyID;
        public override void Load()
        {
            Instance = this;
            LunarCoinCurrencyID = CustomCurrencyManager.RegisterCurrency(new Content.Currencies.LunarCoinCurrency(ModContent.ItemType<Content.Items.Quest.LunarCoin>(), 99L, "Mods.Windfall.Currencies.LunarCoinCurrency"));
        }
        public override void Unload()
        {
            Instance = null;
        }
    }
}