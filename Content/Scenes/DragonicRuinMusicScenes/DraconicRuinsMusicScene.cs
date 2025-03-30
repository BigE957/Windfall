using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windfall.Common.Systems.WorldEvents;

namespace Windfall.Content.Scenes.DragonicRuinMusicScenes;
public class DraconicRuinsMusicScene : ModSceneEffect
{
    public override int Music => RuinsMusic();
    public override bool IsSceneEffectActive(Player player) => DraconicRuinsSystem.DraconicRuinsArea.Contains(player.Center.ToTileCoordinates());
    public override SceneEffectPriority Priority => SceneEffectPriority.Event;

    private static int RuinsMusic()
    {
        if (DraconicRuinsSystem.CutsceneTime >= 810 && DraconicRuinsSystem.State != DraconicRuinsSystem.CutsceneState.CultistEnd)
            return 0;
        return MusicLoader.GetMusicSlot(WindfallMod.Instance, "Assets/Music/Cliffs");
    }
}