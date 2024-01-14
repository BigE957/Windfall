using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.UI;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent.UI.Elements;
using WindfallAttempt1.UI.WanderersJournals;

namespace WindfallAttempt1.UI.DraedonsDatabase
{
    internal class Desktop : UIState
    {
        public MouseBlockingUIPanel panel;
        public override void OnInitialize()
        {
            panel = new();
            panel.Width.Set(Main.screenWidth * 1.5f, 0);          
            panel.Height.Set(Main.screenHeight * 1.5f, 0);
            panel.HAlign = panel.VAlign = 0.5f;
            Append(panel);

            Asset<Texture2D> BackgroundTexture = ModContent.Request<Texture2D>("WindfallAttempt1/UI/DraedonsDatabase/DraedonUIBG"); 
            UIImage Background = new(BackgroundTexture);
            SetRectangle(Background, left: -125f, top: -125f, width: 400f, height: 518f);
            panel.Append(Background);

            Asset<Texture2D> CloseButtonTexture = ModContent.Request<Texture2D>("WindfallAttempt1/UI/DraedonsDatabase/DraedonUIClose");
            UIButton CloseButton = new(CloseButtonTexture, "Close Database");
            SetRectangle(CloseButton, left: 1270f, top: 115f, width: 26f, height: 26f);
            CloseButton.OnLeftClick += new MouseEvent(CloseUI);
            panel.Append(CloseButton);

            Asset<Texture2D> DraeDashIconTexture = ModContent.Request<Texture2D>("WindfallAttempt1/UI/DraedonsDatabase/DraedonUIDraeDashLogo");
            UIButton DraeDashIcon = new(DraeDashIconTexture, "Open Drae-Dash");
            SetRectangle(DraeDashIcon, left: 270f, top: 115f, width: 52f, height: 52f);
            DraeDashIcon.OnLeftClick += new MouseEvent(OpenDraeDash);
            panel.Append(DraeDashIcon);

            UIText header = new("Draedon's Database")
            {
                HAlign = 0.5f  // 1
            };
            header.Top.Set(115, 0); // 2
            panel.Append(header);

            UIText text = new("Draedon Gaming!")
            {
                HAlign = 0.5f, // 1
                VAlign = 0.5f // 1
            };
            panel.Append(text);
        }
        private void SetRectangle(UIElement uiElement, float left, float top, float width, float height)
        {
            uiElement.Left.Set(left, 0f);
            uiElement.Top.Set(top, 0f);
            uiElement.Width.Set(width, 0f);
            uiElement.Height.Set(height, 0f);
        }
        private void CloseUI(UIMouseEvent evt, UIElement listeningElement)
        {
            ModContent.GetInstance<DraedonUISystem>().CloseDraedonDatabase();
        }

        private void OpenDraeDash(UIMouseEvent evt, UIElement listeningElement)
        {
            ModContent.GetInstance<DraedonUISystem>().OpenDraedonApp("DraeDash");
        }
    }
    internal class DraeDash : UIState
    {
        public MouseBlockingUIPanel panel;
        public override void OnInitialize()
        {
            panel = new();
            panel.Width.Set(Main.screenWidth * 1.5f, 0);
            panel.Height.Set(Main.screenHeight * 1.5f, 0);
            panel.HAlign = panel.VAlign = 0.5f;
            Append(panel);

            Asset<Texture2D> BackgroundTexture = ModContent.Request<Texture2D>("WindfallAttempt1/UI/DraedonsDatabase/DraedonUIBG");
            UIImage Background = new(BackgroundTexture);
            SetRectangle(Background, left: -125f, top: -125f, width: 400f, height: 518f);
            panel.Append(Background);

            Asset<Texture2D> CloseButtonTexture = ModContent.Request<Texture2D>("WindfallAttempt1/UI/DraedonsDatabase/DraedonUIClose");
            UIButton CloseButton = new(CloseButtonTexture, "Close Drae-Dash");
            SetRectangle(CloseButton, left: 1270f, top: 115f, width: 56f, height: 26f);
            CloseButton.OnLeftClick += new MouseEvent(CloseDraeDash);
            panel.Append(CloseButton);

            UIText header = new("Drae-Dash")
            {
                HAlign = 0.5f  // 1
            };
            header.Top.Set(115, 0); // 2
            panel.Append(header);

            UIText text = new("Order all your favorite Draedon Enterprise\nproducts all from the comfort of your home!")
            {
                HAlign = 0.5f, // 1
                VAlign = 0.5f // 1
            };
            panel.Append(text);
        }
        private void SetRectangle(UIElement uiElement, float left, float top, float width, float height)
        {
            uiElement.Left.Set(left, 0f);
            uiElement.Top.Set(top, 0f);
            uiElement.Width.Set(width, 0f);
            uiElement.Height.Set(height, 0f);
        }
        private void CloseDraeDash(UIMouseEvent evt, UIElement listeningElement)
        {
            ModContent.GetInstance<DraedonUISystem>().OpenDraedonDatabase();
        }
    }
}
