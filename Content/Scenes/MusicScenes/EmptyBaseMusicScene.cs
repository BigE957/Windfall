using Windfall.Common.Systems.WorldEvents;

namespace Windfall.Content.Scenes.MusicScenes;

public class EmptyBaseMusicScene : ModSceneEffect
{
    public override int Music => MusicLoader.GetMusicSlot(Windfall.Instance, "Assets/Music/ItsRainingSomewhereElse");
    public override bool IsSceneEffectActive(Player player) => LunarCultBaseSystem.CultBaseWorldArea.Contains(player.Hitbox) && SealingRitualSystem.RitualSequenceSeen;
    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
}
