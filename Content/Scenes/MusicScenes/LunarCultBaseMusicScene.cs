using Windfall.Common.Systems.WorldEvents;

namespace Windfall.Content.Scenes.MusicScenes;

public class LunarCultBaseMusicScene : ModSceneEffect
{
    public override int Music => MusicLoader.GetMusicSlot(Windfall.Instance, "Assets/Music/Premonition");
    public override bool IsSceneEffectActive(Player player) => new Rectangle((LunarCultBaseSystem.LunarCultBaseLocation.X - 70) * 16, (LunarCultBaseSystem.LunarCultBaseLocation.Y - 120) * 16, 122 * 16, 152 * 16).Contains(player.Hitbox) && !SealingRitualSystem.RitualSequenceSeen;
    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
}
