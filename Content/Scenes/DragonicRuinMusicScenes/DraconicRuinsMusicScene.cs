using Windfall.Common.Systems.WorldEvents;

namespace Windfall.Content.Scenes.DragonicRuinMusicScenes;
public class DraconicRuinsMusicScene : ModSceneEffect
{
    public override int Music => RuinsMusic();
    public override bool IsSceneEffectActive(Player player) => DraconicRuinsSystem.DraconicRuinsArea.Contains(player.Center.ToTileCoordinates());
    public override SceneEffectPriority Priority => SceneEffectPriority.Event;

    private static int RuinsMusic()
    {
        if (DraconicRuinsSystem.CutsceneTime >= 150 && DraconicRuinsSystem.State != DraconicRuinsSystem.CutsceneState.Arrival)
            return 0;
        return MusicLoader.GetMusicSlot(Windfall.Instance, "Assets/Music/DragonRuin");
    }
}