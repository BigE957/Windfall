using Windfall.Common.Systems.WorldEvents;

namespace Windfall.Content.Scenes.MusicScenes
{
    public class LunarCultBaseMusicScene : ModSceneEffect
    {
        public override int Music => MusicLoader.GetMusicSlot(Windfall.Instance, "Assets/Music/ItsRainingSomewhereElse");
        public override bool IsSceneEffectActive(Player player) => new Rectangle((LunarCultActivitySystem.LunarCultBaseLocation.X - 70) * 16, (LunarCultActivitySystem.LunarCultBaseLocation.Y - 120) * 16, 122 * 16, 152 * 16).Contains(player.Hitbox);
        public override SceneEffectPriority Priority => SceneEffectPriority.Event;
    }
}
