using DialogueHelper.UI.Dialogue;
using DialogueHelper.UI;
using DialogueHelper.UI.Dialogue.DialogueStyles;
using static DialogueHelper.UI.Dialogue.DialogueUIState;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;
using Terraria.UI;
using ReLogic.Content;

namespace Windfall.Content.UI.Dialogue.DialogueStyles;
public class SelenicDialogueStyle : DialogueStyle
{
    public override Vector2 ButtonSize => new(150, 50);
    public override List<Vector2> LineBreakOffsets => 
    [
        new(0,0),
        new(0,32),
        new(8,40),
        new(16,48),
        new(24,56),
        new(32,64),
        new(40,72),
        new(48,80),
        new(56,88),
        new(64,96)
    ];

    public override void OnTextboxCreate(UIPanel textbox, FlippableUIImage speaker, FlippableUIImage subSpeaker)
    {
        bool speakerRight = ModContent.GetInstance<DialogueUISystem>().speakerRight;
        bool spawnBottom = ModContent.GetInstance<DialogueUISystem>().justOpened || ModContent.GetInstance<DialogueUISystem>().styleSwapped;
        bool newSubSpeaker = ModContent.GetInstance<DialogueUISystem>().newSubSpeaker;
        textbox.SetPadding(0);
        textbox.BackgroundColor = Color.Transparent;
        textbox.BorderColor = Color.Transparent;
        SetRectangle(textbox, left: 0, top: spawnBottom ? Main.screenHeight * 1.05f : Main.screenHeight / 1.75f, width: Main.screenWidth / 1.75f, height: Main.screenHeight / 3);
        if (newSubSpeaker && !ModContent.GetInstance<DialogueUISystem>().styleSwapped)
            textbox.Left.Pixels = speakerRight ? Main.screenWidth - textbox.Width.Pixels - Main.screenWidth / 12f : Main.screenWidth / 12f;
        else
            textbox.Left.Pixels = speakerRight ? Main.screenWidth / 12f : Main.screenWidth - textbox.Width.Pixels - Main.screenWidth / 12f;
        textbox.Height.Pixels += 30;
        textbox.Width.Pixels -= 10;
        UIImage boxTexture = new(ModContent.Request<Texture2D>($"{nameof(Windfall)}/Assets/UI/DialogueStyles/SelenicTextbox"));
        SetRectangle(boxTexture, left: -70, top: -100, width: boxTexture.Width.Pixels, height: boxTexture.Height.Pixels);
        textbox.Append(boxTexture);
    }
    public override void OnDialogueTextCreate(DialogueText text)
    {
        text.Top.Pixels = 40;
        text.Left.Pixels = 60;
        text.Width.Pixels -= 50;
    }
    public override void OnResponseButtonCreate(UIPanel button, DialogueHelper.UI.MouseBlockingUIPanel textbox, int responseCount, int buttonCounter)
    {
        ButtonCounters = null;
        button.Width.Set(150, 0);
        button.Height.Set(50, 0);
        button.Left.Pixels = (textbox.Width.Pixels + 200) * (buttonCounter / responseCount);
        button.Top.Set(2000, 0);
        UIImage responseTexture = new(ModContent.Request<Texture2D>($"{nameof(Windfall)}/Assets/UI/DialogueStyles/SelenicResponse"));
        responseTexture.Left.Pixels = -40;
        responseTexture.Top.Pixels = -28;
        button.Append(responseTexture);
    }
    public override void OnResponseTextCreate(UIText text)
    {
        text.HAlign = text.VAlign = 0.5f;
        text.SetText(text.Text, 0.875f, false);
        text.IsWrapped = true;
        text.WrappedTextBottomPadding = -2f;
    }
    public override void OnResponseCostCreate(UIText text, UIPanel costHolder)
    {
        text.VAlign = 0f;
        costHolder.HAlign = 0.5f;
    }
    public override void PostUICreate(int dialogueIndex, UIPanel textbox, FlippableUIImage speaker, FlippableUIImage subSpeaker)
    {
        DialogueHelper.UI.Dialogue.Dialogue CurrentDialogue = ModContent.GetInstance<DialogueUISystem>().CurrentTree.Dialogues[dialogueIndex];
        Character CurrentCharacter = ModContent.GetInstance<DialogueUISystem>().CurrentSpeaker;

        MouseBlockingUIPanel NameBox;
        NameBox = new MouseBlockingUIPanel();
        NameBox.SetPadding(0);
        SetRectangle(NameBox, left: 75f, top: -35f, width: 275f, height: 70f);
        NameBox.BackgroundColor = Color.Transparent;
        NameBox.BorderColor = Color.Transparent;
        textbox.Append(NameBox);

        UIText NameText;
        if (CurrentDialogue.CharacterIndex == -1)
            NameText = new UIText("...");
        else
        {
            if (ModContent.GetInstance<DialogueUISystem>().swappingStyle)
            {
                Character FormerCharacter = ModContent.GetInstance<DialogueUISystem>().SubSpeaker;
                NameText = new UIText(FormerCharacter.Name, 1f, true);
            }
            else
                NameText = new UIText(CurrentCharacter.Name, 1f, true);
        }
        NameText.Width.Pixels = NameBox.Width.Pixels;
        NameText.Left.Pixels -= 16;
        NameText.VAlign = 0.75f;
        NameBox.Append(NameText);
    }

    private static int[] ButtonCounters = null;
    public override void PostUpdateActive(DialogueHelper.UI.MouseBlockingUIPanel textbox, FlippableUIImage speaker, FlippableUIImage subSpeaker)
    {
        if (ModContent.GetInstance<DialogueUISystem>().swappingStyle)
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
                ModContent.GetInstance<DialogueUISystem>().styleSwapped = true;
                ModContent.GetInstance<DialogueUISystem>().swappingStyle = false;
                textbox.RemoveAllChildren();
                textbox.Remove();

                ModContent.GetInstance<DialogueUISystem>().DialogueUIState.SpawnTextBox();
            }
        }
        else
        {
            #region Textbox Updates
            float goalHeight = Main.screenHeight / 1.75f;

            if (textbox.Top.Pixels > goalHeight)
            {
                textbox.Top.Pixels -= (textbox.Top.Pixels - goalHeight) / 10;
                if (textbox.Top.Pixels - goalHeight < 1)
                    textbox.Top.Pixels = goalHeight;

            }
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
            #endregion

            #region Button Updates
            DialogueUIState state = ModContent.GetInstance<DialogueUISystem>().DialogueUIState;
            DialogueText dialogue = (DialogueText)textbox.Children.Where(c => c.GetType() == typeof(DialogueText)).First();
            if (dialogue.Crawling)
                return;
            DialogueUISystem uiSystem = ModContent.GetInstance<DialogueUISystem>();
            if (textbox.Children.Count() == 3 && uiSystem.CurrentTree.Dialogues[uiSystem.CurrentDialogueIndex].Responses.Length == 0)
            {
                SelenicArrow Arrow = new(ModContent.Request<Texture2D>($"{nameof(Windfall)}/Assets/UI/DialogueStyles/SelenicArrow", AssetRequestMode.ImmediateLoad))
                {
                    ImageScale = 0f,
                    NormalizedOrigin = Vector2.One / 2f,
                    HAlign = 0.9f,
                    VAlign = 0.75f
                };
                textbox.Append(Arrow);
                return;
            }
            UIElement[] responseButtons = state.Children.Where(c => c != state.Children.First() && c.GetType() == typeof(UIPanel)).ToArray();

            bool allInPosition = true;
            for (int i = 0; i < responseButtons.Length; i++)
            {
                UIElement button = responseButtons[i];
                if (button.Top.Pixels > 747.1f)
                {
                    allInPosition = false;
                    button.Top.Set(button.Top.Pixels - (button.Top.Pixels - 747) / ((i+1) * 3), 0);
                    button.Left.Pixels = textbox.Width.Pixels / (2 * responseButtons.Length) + (textbox.Width.Pixels * (float)(i / (float)responseButtons.Length) - button.Width.Pixels / 2);
                    button.Left.Pixels += 123.5f;
                }               
            }
            if (allInPosition)
            {
                if (responseButtons.Length > 0)
                {
                    for (int i = 0; i < responseButtons.Length; i++)
                    {
                        UIElement button = responseButtons[i];
                        if (!textbox.HasChild(button))
                        {
                            button.Left.Pixels = textbox.Width.Pixels / (2 * responseButtons.Length) + (textbox.Width.Pixels * (float)(i / (float)responseButtons.Length) - button.Width.Pixels / 2);
                            button.Left.Pixels -= 3;
                            button.Top.Set(260, 0);
                            textbox.AddOrRemoveChild(button, true);
                        }
                    }
                }
                else
                {
                    responseButtons = textbox.Children.Where(c => c.GetType() == typeof(UIPanel)).ToArray();
                    for (int i = 0; i < responseButtons.Length; i++)
                    {
                        ButtonCounters ??= new int[responseButtons.Length];

                        UIElement button = responseButtons[i];
                        
                        UIImage image = (UIImage)button.Children.First(c => c.GetType() == typeof(UIImage));
                        image.AllowResizingDimensions = false;
                        if (button.IsMouseHovering)
                        {
                            image.SetImage(ModContent.Request<Texture2D>($"{nameof(Windfall)}/Assets/UI/DialogueStyles/SelenicResponseSelected"));

                            if (ButtonCounters[i] < 18)
                            {
                                float value = Lerp(ButtonSize.Y, ButtonSize.Y * 1.25f, SineInEasing(ButtonCounters[i] / 18f, 1));
                                button.Width.Pixels = value * 3;
                                button.Height.Pixels = value;
                                ButtonCounters[i]++;
                            }
                            else
                            {
                                ButtonCounters[i] = 18;
                                button.Width.Pixels = ButtonSize.X * 1.25f;
                                button.Height.Pixels = ButtonSize.Y * 1.25f;
                            }
                        }
                        else
                        {
                            image.SetImage(ModContent.Request<Texture2D>($"{nameof(Windfall)}/Assets/UI/DialogueStyles/SelenicResponse"));

                            if (ButtonCounters[i] > 0)
                            {
                                float value = Lerp(ButtonSize.Y * 1.25f, ButtonSize.Y, SineOutEasing(1 - (ButtonCounters[i] / 18f), 1));
                                button.Width.Pixels = value * 3;
                                button.Height.Pixels = value;
                                ButtonCounters[i]--;
                            }
                            else
                            {
                                ButtonCounters[i] = 0;
                                button.Width.Pixels = ButtonSize.X;
                                button.Height.Pixels = ButtonSize.Y;
                            }
                        }
                        
                        UIText text = (UIText)button.Children.First(c => c.GetType() == typeof(UIText));
                        text.SetText(text.Text, 0.875f * button.Width.Pixels / ButtonSize.X, false);
                        text.Top.Pixels = 3;
                        text.Left.Pixels = 2;
                        image.ImageScale = button.Width.Pixels / ButtonSize.X;

                        button.Left.Pixels = textbox.Width.Pixels / (2 * responseButtons.Length) + (textbox.Width.Pixels * (float)(i / (float)responseButtons.Length) - button.Width.Pixels / 2);
                        button.Left.Pixels -= 5;
                        button.Top.Pixels = 257 - ((button.Height.Pixels - ButtonSize.Y) / 2f);

                        image.Top.Pixels = button.Height.Pixels / -2f;
                        image.Top.Pixels += 18 * (((button.Width.Pixels / ButtonSize.X) -1) * 4);
                        image.Left.Pixels = button.Width.Pixels / -4f;
                        image.Left.Pixels += 22 * (((button.Width.Pixels / ButtonSize.X) - 1) * 4);
                        if(button.IsMouseHovering)
                        {
                            image.Top.Pixels -= 2;
                            image.Left.Pixels -= 2;
                        }
                    }
                }
            }
            #endregion
        }
    }
    public override void PostUpdateClosing(DialogueHelper.UI.MouseBlockingUIPanel textbox, FlippableUIImage speaker, FlippableUIImage subSpeaker)
    {
        if (!TextboxOffScreen(textbox))
        {
            DialogueUIState state = ModContent.GetInstance<DialogueUISystem>().DialogueUIState;

            UIElement[] responseButtons = state.Children.Where(c => c != state.Children.First() && c.GetType() == typeof(UIPanel)).ToArray();

            for (int i = 0; i < responseButtons.Length; i++)
            {
                UIElement button = responseButtons[i];

                if (!textbox.HasChild(button))
                {
                    textbox.AddOrRemoveChild(button, true);
                    button.Left.Pixels = textbox.Width.Pixels / (2 * responseButtons.Length) + (textbox.Width.Pixels * (float)(i / (float)responseButtons.Length) - button.Width.Pixels / 2);
                    button.Left.Pixels -= 3;
                    button.Top.Set(260, 0);
                }
            }

            float goalHeight = Main.screenHeight * 1.2f;

            textbox.Top.Pixels += (goalHeight - textbox.Top.Pixels) / 20;
            if (goalHeight - textbox.Top.Pixels < 10)
                textbox.Top.Pixels = goalHeight;
        }

    }
    public override bool TextboxOffScreen(UIPanel textbox)
    {
        return textbox.Top.Pixels >= Main.screenHeight * 1.15f;
    }

    internal class SelenicArrow(Asset<Texture2D> texture) : FlippableUIImage(texture)
    {
        int Counter = 0;
        public override void OnActivate()
        {
            ImageScale = 0f;
            NormalizedOrigin = Vector2.One / 2f;
        }
        public override void Update(GameTime gameTime)
        {
            if (Counter < 24)
            {
                ImageScale = ExpOutEasing(Counter / 24f, 1) / 2f;
                Counter++;
            }
            else
            {
                ImageScale = 0.5f;
                Counter = 24;
            }
            float sineWave = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2);
            Rotation = sineWave * (Pi / 16f);
            Top.Pixels = sineWave * 6;
        }
    }

}
