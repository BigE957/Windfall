using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.UI;

namespace Windfall.Content.Currencies
{
    public  class LunarCoinCurrency : CustomCurrencySingleCoin
    {
        public LunarCoinCurrency(int coinItemID, long currencyCap, string CurrencyTextKey) : base(coinItemID, currencyCap)
        {
            this.CurrencyTextKey = CurrencyTextKey;
            CurrencyTextColor = Color.Cyan;
        }
    }
}
