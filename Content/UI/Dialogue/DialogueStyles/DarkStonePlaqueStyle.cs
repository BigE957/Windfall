using DialogueHelper.UI.Dialogue;
using DialogueHelper.UI;
using DialogueHelper.UI.Dialogue.DialogueStyles;
using static DialogueHelper.UI.Dialogue.DialogueUIState;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Windfall.Content.UI.Dialogue.DialogueStyles;
public class DarkStonePlaqueStyle : DialogueStyle
{
    public override Vector2 ButtonSize => new(150, 50);
    public override void OnTextboxCreate(UIPanel textbox, FlippableUIImage speaker, FlippableUIImage subSpeaker)
    {
        DialogueUISystem uiSystem = ModContent.GetInstance<DialogueUISystem>();
        bool speakerRight = uiSystem.speakerRight;
        bool spawnBottom = uiSystem.justOpened || uiSystem.styleSwapped;
        bool newSubSpeaker = uiSystem.newSubSpeaker;
        textbox.SetPadding(0);
        SetRectangle(textbox, left: 0, top: spawnBottom ? Main.screenHeight * 1.05f : Main.screenHeight / 1.75f, width: Main.screenWidth / 2.5f, height: Main.screenHeight / 3);

        if (uiSystem.CurrentTree.Characters.Length == 0)
            textbox.Left.Pixels = (Main.screenWidth / 2f) - (textbox.Width.Pixels / 2f);
        else
        {
            if (newSubSpeaker && !uiSystem.styleSwapped)
                textbox.Left.Pixels = speakerRight ? Main.screenWidth - textbox.Width.Pixels - Main.screenWidth / 12f : Main.screenWidth / 12f;
            else
                textbox.Left.Pixels = speakerRight ? Main.screenWidth / 12f : Main.screenWidth - textbox.Width.Pixels - Main.screenWidth / 12f;
        }

        UIImage boxTexture = new(ModContent.Request<Texture2D>($"{nameof(Windfall)}/Assets/UI/DialogueStyles/DarkStonePlaqueTextbox"));
        SetRectangle(boxTexture, left: -60, top: -30, width: boxTexture.Width.Pixels, height: boxTexture.Height.Pixels);
        textbox.Append(boxTexture);
    }
    public override void OnDialogueTextCreate(DialogueText text)
    {
        text.Top.Pixels = 0;
        text.Left.Pixels = 15;
    }
    public override void OnResponseButtonCreate(UIPanel button, DialogueHelper.UI.MouseBlockingUIPanel textbox, int responseCount, int i)
    {
        button.Width.Set(20, 0);
        button.Height.Set(10, 0);
        button.Left.Pixels = (textbox.Width.Pixels + 200) * (i / responseCount);
        button.Top.Set(2000, 0);
    }
    public override void OnResponseTextCreate(UIText text)
    {
        text.HAlign = text.VAlign = 0.5f;
    }
    public override void OnResponseCostCreate(UIText text, UIPanel costHolder)
    {
        text.VAlign = 0f;
        costHolder.HAlign = 0.5f;
    }
    public override void PostUICreate(int dialogueIndex, UIPanel textbox, FlippableUIImage speaker, FlippableUIImage subSpeaker)
    {
        
    }
    public override void PostUpdateActive(DialogueHelper.UI.MouseBlockingUIPanel textbox, FlippableUIImage speaker, FlippableUIImage subSpeaker)
    {
        DialogueUISystem uiSystem = ModContent.GetInstance<DialogueUISystem>();

        float xResolutionScale = Main.screenWidth / 2560f;
        float yResolutionScale = Main.screenHeight / 1440f;

        if (uiSystem.swappingStyle)
        {
            if (!TextboxOffScreen(textbox))
            {
                float goalHeight = Main.screenHeight * 1.5f;

                textbox.Top.Pixels += (goalHeight - textbox.Top.Pixels) / 20;
                if (goalHeight - textbox.Top.Pixels < 10)
                    textbox.Top.Pixels = goalHeight;
            }
            else
            {
                uiSystem.styleSwapped = true;
                uiSystem.swappingStyle = false;
                textbox.RemoveAllChildren();
                textbox.Remove();

                uiSystem.DialogueUIState.SpawnTextBox();
            }
        }
        else
        {
            float goalHeight = Main.screenHeight / 1.75f;

            if (textbox.Top.Pixels > goalHeight)
            {
                textbox.Top.Pixels -= (textbox.Top.Pixels - goalHeight) / 10;
                if (textbox.Top.Pixels - goalHeight < 1)
                    textbox.Top.Pixels = goalHeight;

            }

            if (uiSystem.CurrentTree.Characters.Length == 0)
                textbox.Left.Pixels = (Main.screenWidth / 2f) - (textbox.Width.Pixels / 2f);
            else
            {
                float goalLeft = Main.screenWidth / 12f;
                float goalright = Main.screenWidth - textbox.Width.Pixels - Main.screenWidth / 12f;

                if (ModContent.GetInstance<DialogueUISystem>().speakerRight && textbox.Left.Pixels > 125f)
                {
                    textbox.Left.Pixels -= (-goalLeft + textbox.Left.Pixels) / 20;
                    if (-goalLeft + textbox.Left.Pixels < 1)
                        textbox.Left.Pixels = goalLeft;
                }
                else if (!ModContent.GetInstance<DialogueUISystem>().speakerRight && textbox.Left.Pixels < 600f)
                {
                    textbox.Left.Pixels += (goalright - textbox.Left.Pixels) / 20;
                    if (goalright - textbox.Left.Pixels < 1)
                        textbox.Left.Pixels = goalright;
                }
            }
            #region Button Updates
            DialogueText dialogue = (DialogueText)textbox.Children.Where(c => c.GetType() == typeof(DialogueText)).First();
            UIElement[] responseButtons;
            if (ModContent.GetInstance<DialogueUISystem>().DialogueUIState.Children.Where(c => c.GetType() == typeof(UIPanel) && c.Children.First().GetType() == typeof(UIText)).Any())
                responseButtons = ModContent.GetInstance<DialogueUISystem>().DialogueUIState.Children.Where(c => c.GetType() == typeof(UIPanel) && c.Children.First().GetType() == typeof(UIText)).ToArray();
            else
                responseButtons = textbox.Children.Where(c => c.GetType() == typeof(UIPanel)).ToArray();
            for (int i = 0; i < responseButtons.Length; i++)
            {
                UIElement button = responseButtons[i];
                if (!dialogue.Crawling && button.Width.Pixels < ButtonSize.X)
                {
                    if (!textbox.HasChild(button))
                        textbox.AddOrRemoveChild(button, true);
                    button.Top.Set(0, 0);
                    button.VAlign = 0.86f;

                    button.Left.Pixels = textbox.Width.Pixels / (2 * responseButtons.Length) + (textbox.Width.Pixels * (float)(i / (float)responseButtons.Length) - button.Width.Pixels / 2);
                    button.Left.Pixels -= 3;

                    button.Width.Pixels = MathHelper.Clamp(button.Width.Pixels + ButtonSize.X / 30, 0f, ButtonSize.X);
                    button.Height.Pixels = MathHelper.Clamp(button.Height.Pixels + ButtonSize.Y / 30, 0f, ButtonSize.Y);

                    button.Top.Pixels += button.Height.Pixels / 2;

                    foreach (UIElement child in button.Children)
                    {
                        if (child.GetType() == typeof(UIText))
                        {
                            if (button.Width.Pixels < ButtonSize.X / 1.5f)
                                continue;
                            //Main.NewText(child.Width.Pixels);
                            UIText textChild = (UIText)child;
                            textChild.SetText(textChild.Text, MathHelper.Clamp(0.875f * ((button.Width.Pixels - ButtonSize.X / 1.5f) / ButtonSize.X) * 3f, 0f, 0.875f), false);
                            textChild.Top.Pixels = button.Top.Pixels - (int)(button.Height.Pixels / 2) + 4;
                            textChild.IsWrapped = true;
                            textChild.WrappedTextBottomPadding = -2f;
                            if (button.Children.Count() > 1)
                                textChild.Top.Pixels = -4;
                        }
                        else
                            child.Top.Pixels = -Main.screenHeight * 1.25f;
                    }
                }
                else if (button.Children.Count() > 1)
                {
                    UIElement child = button.Children.Where(c => c.GetType() == typeof(UIPanel)).First();
                    child.Top.Pixels = child.Parent.Height.Pixels / 4;
                }
            }
            #endregion
        }
    }
    public override void PostUpdateClosing(DialogueHelper.UI.MouseBlockingUIPanel textbox, FlippableUIImage speaker, FlippableUIImage subSpeaker)
    {
        if (!TextboxOffScreen(textbox))
        {
            float goalHeight = Main.screenHeight * 1.1f;

            textbox.Top.Pixels += (goalHeight - textbox.Top.Pixels) / 20;
            if (goalHeight - textbox.Top.Pixels < 10)
                textbox.Top.Pixels = goalHeight;
        }

    }
    public override bool TextboxOffScreen(UIPanel textbox)
    {
        return textbox.Top.Pixels >= Main.screenHeight * 1.05f;
    }
}
