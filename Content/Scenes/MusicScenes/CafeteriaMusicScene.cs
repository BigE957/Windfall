using Windfall.Common.Systems.WorldEvents;

namespace Windfall.Content.Scenes.MusicScenes
{
    public class CafeteriaMusicScene : ModSceneEffect
    {
        public override int Music => MusicLoader.GetMusicSlot(Windfall.Instance, "Assets/Music/CanYouReallyCallThisAHotelIDidntReceiveAMintOnMyPillowOrAnything");
        public override bool IsSceneEffectActive(Player player) => LunarCultActivitySystem.IsCafeteriaActivityActive();
        public override SceneEffectPriority Priority => SceneEffectPriority.Event;
    }
}