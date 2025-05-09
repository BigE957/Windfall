using Terraria;
using Windfall.Common.Systems.WorldEvents;

namespace Windfall.Content.Scenes.MusicScenes;

public class LunarCultBaseMusicScene : ModSceneEffect
{
    public override int Music => MusicLoader.GetMusicSlot(Windfall.Instance, "Assets/Music/ItsRainingSomewhereElse");
    public override bool IsSceneEffectActive(Player player) => (LunarCultBaseSystem.CultBaseWorldArea.Expand(64).Contains(player.Hitbox) || LunarCultBaseSystem.CultBaseBridgeArea.Expand(64).Contains(player.Center.ToTileCoordinates())) && !SealingRitualSystem.RitualSequenceSeen;
    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
}
