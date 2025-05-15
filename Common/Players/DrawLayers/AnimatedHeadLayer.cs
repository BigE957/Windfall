using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windfall.Common.Players.DrawLayers;
public class AnimatedHeadLayer : PlayerDrawLayer
{
    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        if (drawInfo.drawPlayer.head == -1)
            return false;

        if (EquipLoader.GetEquipTexture(EquipType.Head, drawInfo.drawPlayer.head) == null)
            return false;

        ModItem headItem = EquipLoader.GetEquipTexture(EquipType.Head, drawInfo.drawPlayer.head).Item;

        if (headItem is not IAnimatedHead)
            return false;

        return true;
    }

    public override bool IsHeadLayer => true;

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        Player drawPlayer = drawInfo.drawPlayer;

        ModItem headItem = EquipLoader.GetEquipTexture(EquipType.Head, drawInfo.drawPlayer.head).Item;

        if (headItem is IAnimatedHead animatedHead)
        {
            string equipSlotName = headItem.Name;
            int equipSlot = EquipLoader.GetEquipSlot(Mod, equipSlotName, EquipType.Head);

            if (animatedHead.PreDraw(drawInfo) && !drawInfo.drawPlayer.dead && equipSlot == drawPlayer.head)
            {
                int dyeShader = drawPlayer.dye?[0].dye ?? 0;

                // It is imperative to use drawInfo.Position and not drawInfo.Player.Position, or else the layer will break on the player select & map (in the case of a head layer)
                Vector2 headDrawPosition = drawInfo.Position - Main.screenPosition;

                // Using drawPlayer to get width & height and such is perfectly fine, on the other hand. Just center everything
                headDrawPosition += new Vector2((drawPlayer.width - drawPlayer.bodyFrame.Width) / 2f, drawPlayer.height - drawPlayer.bodyFrame.Height + 4f);

                //Convert to int to remove the jitter.
                headDrawPosition = new Vector2((int)headDrawPosition.X, (int)headDrawPosition.Y);

                //Some dispalcements
                headDrawPosition += drawPlayer.headPosition + drawInfo.headVect;

                //Grab the extension texture
                Texture2D extraPieceTexture = animatedHead.headTexture.Value;

                //Get the frame of the extension based on the players body frame
                Rectangle frame = extraPieceTexture.Frame(animatedHead.AnimationLength, 20, drawPlayer.HeadAnimationPlayer().animationFrameNum, drawPlayer.bodyFrame.Y / drawPlayer.bodyFrame.Height);
                
                DrawData pieceDrawData = new(extraPieceTexture, headDrawPosition, frame, drawInfo.colorArmorHead, drawPlayer.headRotation, drawInfo.headVect, 1f, drawInfo.playerEffect, 0)
                {
                    shader = dyeShader
                };

                drawInfo.DrawDataCache.Add(pieceDrawData);

            }
        }
    }
}
