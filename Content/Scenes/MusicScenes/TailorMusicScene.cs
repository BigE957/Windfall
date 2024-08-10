using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.NPCs.Bosses.TheOrator;

namespace Windfall.Content.Scenes.MusicScenes
{
    public class TailorMusicScene : ModSceneEffect
    {
        public override int Music => MusicLoader.GetMusicSlot(Windfall.Instance, "Assets/Music/DeathByGlamor");
        public override bool IsSceneEffectActive(Player player) => LunarCultActivitySystem.IsTailorActivityActive();
        public override SceneEffectPriority Priority => SceneEffectPriority.Event;
    }
}