using Windfall.Content.NPCs.Bosses.Orator;

namespace Windfall.Content.Scenes.MusicScenes
{
    public class OratorMusicScene : ModSceneEffect
    {
        public override int Music => MusicLoader.GetMusicSlot(Windfall.Instance, "Assets/Music/Orator");
        public override bool IsSceneEffectActive(Player player) => NPC.AnyNPCs(ModContent.NPCType<TheOrator>());
        public override SceneEffectPriority Priority => SceneEffectPriority.BossMedium;
    }
}