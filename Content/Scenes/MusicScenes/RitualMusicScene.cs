using Windfall.Common.Systems.WorldEvents;

namespace Windfall.Content.Scenes.MusicScenes
{
    public class RitualMusicScene : ModSceneEffect
    {
        public override int Music => MusicLoader.GetMusicSlot(Windfall.Instance, "Assets/Music/Amalgam");
        public override bool IsSceneEffectActive(Player player) => LunarCultActivitySystem.IsRitualActivityActive();
        public override SceneEffectPriority Priority => SceneEffectPriority.Event;
    }
}