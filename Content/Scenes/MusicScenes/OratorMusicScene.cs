using Windfall.Content.NPCs.Bosses.TheOrator;

namespace Windfall.Content.Scenes.MusicScenes
{
    public class OratorMusicScene : ModSceneEffect
    {
        public override int Music => MusicID.Boss5;
        public override bool IsSceneEffectActive(Player player) => NPC.AnyNPCs(ModContent.NPCType<TheOrator>());
        public override SceneEffectPriority Priority => SceneEffectPriority.BossMedium;
    }
}
