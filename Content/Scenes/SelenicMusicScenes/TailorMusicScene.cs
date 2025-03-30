using Windfall.Common.Systems.WorldEvents;

namespace Windfall.Content.Scenes.MusicScenes;

public class TailorMusicScene : ModSceneEffect
{
    public override int Music => MusicLoader.GetMusicSlot(WindfallMod.Instance, "Assets/Music/DeathByGlamor");
    public override bool IsSceneEffectActive(Player player) => LunarCultBaseSystem.IsTailorActivityActive();
    public override SceneEffectPriority Priority => SceneEffectPriority.Event;
}