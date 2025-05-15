namespace Windfall.Common.Players.DrawLayers;
public class HatExtensionLayer : PlayerDrawLayer
{
    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => drawInfo.shadow == 0f || !drawInfo.drawPlayer.dead;

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        Player drawPlayer = drawInfo.drawPlayer;

        if (drawPlayer.head == -1)
            return;

        if (EquipLoader.GetEquipTexture(EquipType.Head, drawPlayer.head) == null)
            return;

        ModItem headItem = EquipLoader.GetEquipTexture(EquipType.Head, drawPlayer.head).Item;

        if (headItem is IHatExtension hatExtension)
        {
            string equipSlotName = hatExtension.EquipSlotName(drawPlayer) != "" ? hatExtension.EquipSlotName(drawPlayer) : headItem.Name;
            int equipSlot = EquipLoader.GetEquipSlot(Mod, equipSlotName, EquipType.Head);

            if (hatExtension.PreDrawExtension(drawInfo) && !drawInfo.drawPlayer.dead && equipSlot == drawPlayer.head)
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

                //Apply our custom head position offset
                headDrawPosition += hatExtension.ExtensionSpriteOffset(drawInfo);

                //Grab the extension texture
                Texture2D extraPieceTexture = hatExtension.extensionTexture.Value;

                //Get the frame of the extension based on the players body frame
                Rectangle frame = extraPieceTexture.Frame((headItem is IAnimatedHead animated ? animated.AnimationLength : 1), 20, 0, drawPlayer.bodyFrame.Y / drawPlayer.bodyFrame.Height);

                if (headItem is IAnimatedHead)
                    frame.X = drawPlayer.HeadAnimationPlayer().animationFrameNum * 40;

                DrawData pieceDrawData = new(extraPieceTexture, headDrawPosition, frame, drawInfo.colorArmorHead, drawPlayer.headRotation, drawInfo.headVect, 1f, drawInfo.playerEffect, 0)
                {
                    shader = dyeShader,
                };
                drawInfo.DrawDataCache.Add(pieceDrawData);

            }
        }
    }
}
