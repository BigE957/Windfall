using Windfall.Common.Systems.WorldEvents;

namespace Windfall.Content.Scenes.DragonicRuinMusicScenes;
public class ChaseMusicScene : ModSceneEffect
{
    public override int Music => ChaseMusic();
    public override bool IsSceneEffectActive(Player player) => DraconicRuinsSystem.State == DraconicRuinsSystem.CutsceneState.CultistFumble && DraconicRuinsSystem.DraconicRuinsArea.Expand(84).Contains(player.Center.ToTileCoordinates());
    public override SceneEffectPriority Priority => SceneEffectPriority.BossLow;

    private static int ChaseMusic()
    {
        if (DraconicRuinsSystem.CutsceneTime > 136)
            return 0;
        return MusicLoader.GetMusicSlot(Windfall.Instance, "Assets/Music/DragonRuinRace");
    }
}
