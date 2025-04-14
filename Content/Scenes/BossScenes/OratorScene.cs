using Terraria.Graphics.Effects;
using Windfall.Content.NPCs.Bosses.Orator;

namespace Windfall.Content.Scenes.BossScenes;

public class OratorScene : ModSceneEffect
{
    public override int Music => MusicLoader.GetMusicSlot(Windfall.Instance, "Assets/Music/Orator");
    
    public override bool IsSceneEffectActive(Player player) => NPC.AnyNPCs(ModContent.NPCType<TheOrator>());
    
    public override SceneEffectPriority Priority => SceneEffectPriority.BossMedium;

    public override void SpecialVisuals(Player player, bool isActive)
    {
        if (SkyManager.Instance["Windfall:Orator"] != null && isActive != SkyManager.Instance["Windfall:Orator"].IsActive())
        {
            if (isActive)
                SkyManager.Instance.Activate("Windfall:Orator", player.Center);
            else
                SkyManager.Instance.Deactivate("Windfall:Orator");
        }
    }
}