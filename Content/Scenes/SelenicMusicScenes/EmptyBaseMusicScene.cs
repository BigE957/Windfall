using Windfall.Common.Systems.WorldEvents;

namespace Windfall.Content.Scenes.MusicScenes;

public class EmptyBaseMusicScene : ModSceneEffect
{
    public override int Music => MusicLoader.GetMusicSlot(Windfall.Instance, "Assets/Music/Premonition");
    public override bool IsSceneEffectActive(Player player) => (LunarCultBaseSystem.CultBaseWorldArea.Contains(player.Hitbox) || LunarCultBaseSystem.CultBaseBridgeArea.Contains(player.Center.ToTileCoordinates())) && SealingRitualSystem.RitualSequenceSeen;
    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
}
