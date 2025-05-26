using Windfall.Common.Systems.WorldEvents;

namespace Windfall.Content.Scenes.MusicScenes;

public class TailorMusicScene : ModSceneEffect
{
    public override int Music => MusicLoader.GetMusicSlot(Windfall.Instance, "Assets/Music/CraftingMinigame");
    public override bool IsSceneEffectActive(Player player) => LunarCultBaseSystem.IsTailorActivityActive();
    public override SceneEffectPriority Priority => SceneEffectPriority.Event;
}