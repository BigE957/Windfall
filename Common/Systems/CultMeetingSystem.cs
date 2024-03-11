using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Windfall.Common.Systems
{
    public class CultMeetingSystem : ModSystem
    {
        public static Point SolarHideoutLocation;
        public static Point VortexHideoutLocation;
        public static Point NebulaHideoutLocation;
        public static Point StardustHideoutLocation;

        public static List<int> Recruits = new List<int>();

        public override void ClearWorld()
        {
            SolarHideoutLocation = new(-1, -1);
            VortexHideoutLocation = new(-1, -1);
            NebulaHideoutLocation = new(-1, -1);
            StardustHideoutLocation = new(-1, -1);

            Recruits = new List<int>();
        }
        public override void LoadWorldData(TagCompound tag)
        {
            SolarHideoutLocation = tag.Get<Point>("SolarHideoutLocation");
            VortexHideoutLocation = tag.Get<Point>("VortexHideoutLocation");
            NebulaHideoutLocation = tag.Get<Point>("NebulaHideoutLocation");
            StardustHideoutLocation = tag.Get<Point>("StardustHideoutLocation");

            Recruits = (List<int>)tag.GetList<int>("Recruits");
        }
        public override void SaveWorldData(TagCompound tag)
        {
            tag["SolarHideoutLocation"] = SolarHideoutLocation;
            tag["VortexHideoutLocation"] = VortexHideoutLocation;
            tag["NebulaHideoutLocation"] = NebulaHideoutLocation;
            tag["StardustHideoutLocation"] = StardustHideoutLocation;

            tag["Recruits"] = Recruits;
        }
    }
}
