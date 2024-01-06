using Terraria.UI;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using System.Collections.Generic;
using System.Linq;

namespace WindfallAttempt1.UI.WanderersJournals
{
    internal class JournalUIState : UIState
    {
        public int i = 0;
        public JournalButton JournalButton;
        public JournalUIPanel UIPanel;
        public JournalText JournalContents;
        public JournalPage Page;

        public override void OnInitialize()
        {
            //UIPanel = new JournalUIPanel();
            JournalUIPanel UIPanel = new();
            UIPanel.SetPadding(0);
            // We need to place this UIElement in relation to its Parent. Later we will be calling `base.Append(coinCounterPanel);`. 
            // This means that this class, ExampleCoinsUI, will be our Parent. Since ExampleCoinsUI is a UIState, the Left and Top are relative to the top left of the screen.
            // SetRectangle method help us to set the position and size of UIElement
            SetRectangle(UIPanel, left: 200f, top: 100f, width: 400f, height: 518f);
            UIPanel.BackgroundColor = new Color(73, 94, 171);
            Append(UIPanel);

            Asset<Texture2D> pageTexture = ModContent.Request<Texture2D>("WindfallAttempt1/UI/WanderersJournals/JournalPage");
            JournalPage page = new (pageTexture);
            SetRectangle(page, left: -55f, top: -25f, width: 400f, height: 518f);
            UIPanel.Append(page);

            Asset<Texture2D> buttonDeleteTexture = ModContent.Request<Texture2D>("Terraria/Images/UI/ButtonDelete");
            JournalButton closeButton = new(buttonDeleteTexture, Language.GetTextValue("LegacyInterface.52")); // Localized text for "Close"
            SetRectangle(closeButton, left: 375f, top: 5f, width: 22f, height: 20f);
            closeButton.OnLeftClick += new MouseEvent(CloseButtonClicked);
            UIPanel.Append(closeButton);

            // UIMoneyDisplay is a fairly complicated custom UIElement. UIMoneyDisplay handles drawing some text and coin textures.
            // Organization is key to managing UI design. Making a contained UIElement like UIMoneyDisplay will make many things easier.
            JournalContents = new JournalText();
            SetRectangle(JournalContents, 15f, 20f, 100f, 40f);
            UIPanel.Append(JournalContents);
        }
        private void SetRectangle(UIElement uiElement, float left, float top, float width, float height)
        {
            uiElement.Left.Set(left, 0f);
            uiElement.Top.Set(top, 0f);
            uiElement.Width.Set(width, 0f);
            uiElement.Height.Set(height, 0f);
        }
        private void CloseButtonClicked(UIMouseEvent evt, UIElement listeningElement)
        {
            SoundEngine.PlaySound(SoundID.MenuClose);
            ModContent.GetInstance<JournalUISystem>().HideMyUI();
        }
    }
    public class JournalText : UIElement
    {
        public static string JournalContents;
        float xResolutionScale = Main.screenWidth / 2560f;
        float yResolutionScale = Main.screenHeight / 1440f;
        readonly float xScale = MathHelper.Lerp(0.004f, 1f, 1);
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle innerDimensions = GetInnerDimensions();
            // Getting top left position of this UIElement
            float xPageTop = innerDimensions.X - 6f;
            float yPageTop = innerDimensions.Y + 12f;
            
            //clammy code
            Texture2D pageTexture = ModContent.Request<Texture2D>("WindfallAttempt1/UI/WanderersJournals/JournalPage").Value;
            int textWidth = (int)((int)(xScale * pageTexture.Width) - 6f);
            textWidth = (int)(textWidth * xResolutionScale);
            List<string> dialogLines = Utils.WordwrapString(JournalContents, FontAssets.MouseText.Value, (int)(textWidth / 0.45f), 250, out _).ToList();
            dialogLines.RemoveAll(text => string.IsNullOrEmpty(text));

            int trimmedTextCharacterCount = string.Concat(dialogLines).Length;
            float yOffsetPerLine = 28f;
            int yScale = (int)(42 * yResolutionScale);
            int yScale2 = (int)(yOffsetPerLine * yResolutionScale);
            for (int i = 0; i < dialogLines.Count; i++)
            {
                if (dialogLines[i] != null)
                {
                    int textDrawPositionY = yScale + i * yScale2 + (int)yPageTop;
                    Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.ItemStack.Value, dialogLines[i], xPageTop, textDrawPositionY, Color.Black, Color.Tan, Vector2.Zero, 0.75f);
                }
            }
            //clammy code

            //Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.ItemStack.Value, JournalContents, shopx - 6f, shopy + 12f, Color.Black, Color.Tan, new Vector2(0.3f), 0.75f);
        }
    }

}