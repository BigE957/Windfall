using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Effects;

namespace Windfall.Common.Players
{
    public class MonolithPlayer : ModPlayer
    {
        public bool OratorMonolith = false;
        public bool NearActiveMonolith = false;
        public override void PreUpdate()
        {
            //NearActiveMonolith = false;
            if(!NearActiveMonolith)
                OratorMonolith = false;
        }
        public override void PostUpdate()
        {
            if (SkyManager.Instance["Windfall:Orator"] != null && OratorMonolith != SkyManager.Instance["Windfall:Orator"].IsActive())
            {
                if (OratorMonolith)
                    SkyManager.Instance.Activate("Windfall:Orator");
                else
                    SkyManager.Instance.Deactivate("Windfall:Orator", []);
            }
        }
        public override void ResetEffects()
        {
        }
    }
}
