using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Windfall.Common.Systems;

namespace Windfall.Content.UI.WanderersJournals
{
    internal class JournalFullUIState : UIState
    {
        public int i = 0;
#pragma warning disable CS0649 // Field 'JournalFullUIState.UIPanel' is never assigned to, and will always have its default value null
        public DraggableUIPanel UIPanel;
#pragma warning restore CS0649 // Field 'JournalFullUIState.UIPanel' is never assigned to, and will always have its default value null
        public JournalText JournalContents;
#pragma warning disable CS0649 // Field 'JournalFullUIState.Page' is never assigned to, and will always have its default value null
        public UIImage Page;
#pragma warning restore CS0649 // Field 'JournalFullUIState.Page' is never assigned to, and will always have its default value null
        public static int PageNumber = 0;
        public enum JournalTypes
        {
            Forest,
            Tundra,
            Desert,
            Ilmeris,
            Evil,
            Jungle,
            Ocean,
            Dungeon,
            Sulphur,
            Aerie1,
            Aerie2,
            Aerie3,
            Aerie4,
        }

        public override void OnInitialize()
        {
            DraggableUIPanel UIPanel = new();
            UIPanel.SetPadding(0);
            // We need to place this UIElement in relation to its Parent. Later we will be calling `base.Append(coinCounterPanel);`. 
            // This means that this class, ExampleCoinsUI, will be our Parent. Since ExampleCoinsUI is a UIState, the Left and Top are relative to the top left of the screen.
            // SetRectangle method help us to set the position and size of UIElement
            SetRectangle(UIPanel, left: 200f, top: 100f, width: 400f, height: 518f);
            UIPanel.BackgroundColor = new Color(73, 94, 171);
            Append(UIPanel);

            Asset<Texture2D> pageTexture = ModContent.Request<Texture2D>($"{nameof(Windfall)}/Assets/UI/WanderersJournals/JournalPage");
            UIImage page = new(pageTexture);
            SetRectangle(page, left: -55f, top: -25f, width: 400f, height: 518f);
            UIPanel.Append(page);

            Asset<Texture2D> journalArrowRightTexture = ModContent.Request<Texture2D>($"{nameof(Windfall)}/Assets/UI/WanderersJournals/JournalArrowRight");
            UIButton nextPageButton = new(journalArrowRightTexture, "Next Page");
            SetRectangle(nextPageButton, left: 325f, top: 500f, width: 56f, height: 26f);
            nextPageButton.OnLeftClick += new MouseEvent(NextPage);
            UIPanel.Append(nextPageButton);

            Asset<Texture2D> journalArrowLeftTexture = ModContent.Request<Texture2D>($"{nameof(Windfall)}/Assets/UI/WanderersJournals/JournalArrowLeft");
            UIButton previousPageButton = new(journalArrowLeftTexture, "Previous Page");
            SetRectangle(previousPageButton, left: 25f, top: 500f, width: 56f, height: 26f);
            previousPageButton.OnLeftClick += new MouseEvent(PreviousPage);
            UIPanel.Append(previousPageButton);

            JournalContents = new JournalText();
            SetRectangle(JournalContents, left: 15f, top: 20f, width: 100f, height: 40f);
            UIPanel.Append(JournalContents);
        }
        private static void SetRectangle(UIElement uiElement, float left, float top, float width, float height)
        {
            uiElement.Left.Set(left, 0f);
            uiElement.Top.Set(top, 0f);
            uiElement.Width.Set(width, 0f);
            uiElement.Height.Set(height, 0f);
        }
        private void NextPage(UIMouseEvent evt, UIElement listeningElement)
        {

            if (PageNumber != 12)
            {
                PageNumber++;
                LoadPage();
            }
        }
        private void PreviousPage(UIMouseEvent evt, UIElement listeningElement)
        {
            if (PageNumber != 0)
            {
                PageNumber--;
                LoadPage();
            }
        }
        private void LoadPage()
        {
            if (WorldSaveSystem.JournalsCollected[PageNumber])
                if ((JournalTypes)PageNumber == JournalTypes.Evil)
                    if (JournalUISystem.whichEvilJournal == "Crimson")
                        JournalText.JournalContents = GetWindfallTextValue($"JournalContents.Crimson");
                    else if (JournalUISystem.whichEvilJournal == "Corruption")
                        JournalText.JournalContents = GetWindfallTextValue($"JournalContents.Corruption");
                    else
                        JournalText.JournalContents = "";
                else
                    JournalText.JournalContents = GetWindfallTextValue($"JournalContents.{(JournalTypes)PageNumber}");
            else
                JournalText.JournalContents = "";
            ModContent.GetInstance<JournalUISystem>().ShowJournalUI();
        }
    }
    public class JournalText : UIElement
    {
        public static bool isFullJournal;
        public static string JournalContents;
        readonly float xResolutionScale = Main.screenWidth / 2560f;
        readonly float yResolutionScale = Main.screenHeight / 1440f;
        readonly float xScale = MathHelper.Lerp(0.004f, 1f, 1);
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle innerDimensions = GetInnerDimensions();
            // Getting top left position of this UIElement
            float xPageTop = innerDimensions.X - 6f;
            float yPageTop = innerDimensions.Y + 12f;

            Texture2D pageTexture = ModContent.Request<Texture2D>($"{nameof(Windfall)}/Assets/UI/WanderersJournals/JournalPage").Value;
            int textWidth = (int)((int)(xScale * pageTexture.Width) - 6f);
            textWidth = (int)(textWidth * xResolutionScale);
            List<string> dialogLines = Utils.WordwrapString(JournalContents, FontAssets.MouseText.Value, (int)(textWidth / 0.45f), 250, out _).ToList();
            dialogLines.RemoveAll(text => string.IsNullOrEmpty(text));

            int trimmedTextCharacterCount = string.Concat(dialogLines).Length;
            float yOffsetPerLine = 28f;
            int yScale = (int)(42 * yResolutionScale);
            int yScale2 = (int)(yOffsetPerLine * yResolutionScale);
            for (int i = 0; i < dialogLines.Count; i++)
                if (dialogLines[i] != null)
                {
                    int textDrawPositionY = yScale + i * yScale2 + (int)yPageTop;
                    Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.ItemStack.Value, dialogLines[i], xPageTop, textDrawPositionY, Color.Black, Color.Tan, Vector2.Zero, 0.75f);
                }
            if (isFullJournal)
            {
                string pgNumStr = Convert.ToString(JournalFullUIState.PageNumber + 1);
                Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.ItemStack.Value, pgNumStr, xPageTop + 185f, yPageTop + 470f, Color.Black, Color.Tan, Vector2.Zero, 1.5f);
            }
        }
    }
}