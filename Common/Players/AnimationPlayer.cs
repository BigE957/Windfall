using Windfall.Content.Items.Interfaces;

namespace Windfall.Common.Players;
public class AnimationPlayer : ModPlayer
{
    public override void FrameEffects()
    {
        if (Player.head == -1)
        {
            Player.bodyFrame.X = 0;
            return;
        }

        if (EquipLoader.GetEquipTexture(EquipType.Head, Player.head) == null)
        {
            Player.bodyFrame.X = 0;
            return;
        }

        ModItem headItem = EquipLoader.GetEquipTexture(EquipType.Head, Player.head).Item;

        if (headItem is IAnimatedHead animated)
        {
            if (Player.miscCounter % animated.AnimationDelay == 0)
            {
                Player.bodyFrame.X += 40;
                if (Player.bodyFrame.X >= 40 * animated.AnimationLength)
                    Player.bodyFrame.X = 0;
            }
        }
        else
            Player.bodyFrame.X = 0;
    }
}
