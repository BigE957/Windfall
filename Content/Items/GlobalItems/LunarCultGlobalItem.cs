using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windfall.Common.Systems.WorldEvents;

namespace Windfall.Content.Items.GlobalItems
{
    public class LunarCultGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;

        public bool madeDuringCafeteriaActivity = false;
        public bool madeDuringTailorActivity = false;

        public override void OnCreated(Item item, ItemCreationContext context)
        {
            madeDuringCafeteriaActivity = LunarCultActivitySystem.IsCafeteriaActivityActive();
            madeDuringTailorActivity = LunarCultActivitySystem.IsCafeteriaActivityActive();
        }
    }
}
