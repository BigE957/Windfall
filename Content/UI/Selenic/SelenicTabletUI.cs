using Terraria.UI;
using Windfall.Content.Items.Quests;

namespace Windfall.Content.UI.Selenic;

internal class SelenicTabletUIState : UIState
{
    public int i = 0;
    public MouseBlockingUIPanel SelenicUIPanel;
    public SelenicText SelenicContents;
    public UIButton nextButton;
    public static int TextState = 0;

    internal const float UIWidth = 400f;
    internal const float UIHeight = 150f;

    public override void OnInitialize()
    {
        SelenicUIPanel = new();
        SelenicUIPanel.SetPadding(0);
        // We need to place this UIElement in relation to its Parent. Later we will be calling `base.Append(coinCounterPanel);`. 
        // This means that this class, ExampleCoinsUI, will be our Parent. Since ExampleCoinsUI is a UIState, the Left and Top are relative to the top left of the screen.
        // SetRectangle method help us to set the position and size of UIElement
        SetRectangle(SelenicUIPanel, left: 500f, top: Main.screenHeight + UIHeight, width: UIWidth, height: UIHeight);
        SelenicUIPanel.BackgroundColor = new Color(103, 207, 193);
        SelenicUIPanel.BorderColor = new Color(132, 225, 211);
        Append(SelenicUIPanel);

        Asset<Texture2D> ArrowRightTexture = ModContent.Request<Texture2D>($"{nameof(WindfallMod)}/Assets/UI/DraedonsDatabase/DraedonUIArrowRight");
        nextButton = new(ArrowRightTexture, "Continue...");
        SetRectangle(nextButton, left: 280f, top: 100f, width: 56f, height: 26f);
        nextButton.OnLeftClick += new MouseEvent(NextPage);
        SelenicUIPanel.Append(nextButton);

        SelenicContents = new SelenicText();
        SetRectangle(SelenicContents, left: 15f, top: 5f, width: 100f, height: 40f);
        SelenicUIPanel.Append(SelenicContents);
    }
    private static void SetRectangle(UIElement uiElement, float left, float top, float width, float height)
    {
        uiElement.Left.Set(left, 0f);
        uiElement.Top.Set(top, 0f);
        uiElement.Width.Set(width, 0f);
        uiElement.Height.Set(height, 0f);
    }
    public int textCounter = 0;
    private void NextPage(UIMouseEvent evt, UIElement listeningElement)
    {
        textCounter++;
        LoadPage();
    }
    private void LoadPage()
    {
        SelenicText.Contents = GetWindfallTextValue($"UI.Selenic.{SelenicTablet.Key}.{textCounter}");
        if (SelenicText.Contents.Contains("Mods.Windfall"))
        {
            SelenicText.Contents = GetWindfallTextValue($"UI.Selenic.{SelenicTablet.Key}.{textCounter - 1}");
            ModContent.GetInstance<SelenicTabletUISystem>().HideUI();
        }
        else
            ModContent.GetInstance<SelenicTabletUISystem>().UpdateUI();
    }
}
public class SelenicText : UIElement
{
    public static bool isFullJournal;
    public static string Contents;
    readonly float xResolutionScale = Main.screenWidth / 2560f;
    readonly float yResolutionScale = Main.screenHeight / 1440f;
    readonly float xScale = Lerp(0.004f, 1f, 1);
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        CalculatedStyle innerDimensions = GetInnerDimensions();
        // Getting top left position of this UIElement
        float xPageTop = innerDimensions.X - 6f;
        float yPageTop = innerDimensions.Y + 12f;

        int textWidth = (int)((int)(xScale * SelenicTabletUIState.UIWidth) - 6f);
        textWidth = (int)(textWidth * xResolutionScale);
        List<string> dialogLines = Utils.WordwrapString(Contents, FontAssets.MouseText.Value, (int)(textWidth / 0.85f), 250, out _).ToList();
        dialogLines.RemoveAll(text => string.IsNullOrEmpty(text));

        int trimmedTextCharacterCount = string.Concat(dialogLines).Length;
        float yOffsetPerLine = 48f * (this.Width.Pixels / 100f);
        int yScale = (int)(42 * yResolutionScale * (this.Width.Pixels / 100f));
        int yScale2 = (int)(yOffsetPerLine * yResolutionScale);
        for (int i = 0; i < dialogLines.Count; i++)
            if (dialogLines[i] != null)
            {
                int textDrawPositionY = yScale + i * yScale2 + (int)yPageTop;
                Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.ItemStack.Value, dialogLines[i], xPageTop, textDrawPositionY, Color.Black, new(103, 207, 193), Vector2.Zero, 1.5f * (this.Width.Pixels / 100f));
            }
    }
}