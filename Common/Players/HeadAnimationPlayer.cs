using Windfall.Common.Players.DrawLayers;

namespace Windfall.Common.Players;
public class HeadAnimationPlayer : ModPlayer
{
    public int animationFrameNum = 0;

    public override void FrameEffects()
    {
        if (Player.head == -1)
        {
            animationFrameNum = 0;
            return;
        }

        if (EquipLoader.GetEquipTexture(EquipType.Head, Player.head) == null)
        {
            animationFrameNum = 0;
            return;
        }

        ModItem headItem = EquipLoader.GetEquipTexture(EquipType.Head, Player.head).Item;

        if (headItem is IAnimatedHead animated)
        {
            if (Player.miscCounter % animated.AnimationDelay == 0)
            {
                animationFrameNum++;
                if (animationFrameNum >= animated.AnimationLength)
                    animationFrameNum = 0;
            }
        }
        else
            animationFrameNum = 0;
    }
}
