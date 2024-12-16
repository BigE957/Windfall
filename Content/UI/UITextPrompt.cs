using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Windfall.Content.UI;
public class UITextPrompt : UIElement
{
    public delegate void TypingComplete(string Text);
    public event TypingComplete OnTypingComplete;

    public UIPanel BackPanel = null;
    public string Text = "";
    public bool Active = false;
    public string HintText = "...";

    private Color TextColor = Color.White;
    private Color TextShadow = Color.Black;
    private SoundStyle? TypeSound = null;

    public UITextPrompt(int width, int height, SoundStyle? typeSound = null, Color? textColor = null, Color? shadowColor = null )
    {
        if(textColor.HasValue)
            TextColor = textColor.Value;
        if (shadowColor.HasValue)
            TextShadow = shadowColor.Value;
        TypeSound = typeSound;
        Width.Set(width, 0);
        Height.Set(height, 0);
    }

    public override void OnActivate()
    {
        if (Parent == null)
        {
            BackPanel = new UIPanel();
            BackPanel.Width.Set(Width.Pixels, 0);
            BackPanel.Height.Set(Height.Pixels, 0);
            BackPanel.BackgroundColor = new Color(22, 25, 55);
            BackPanel.PaddingLeft = BackPanel.PaddingRight = BackPanel.PaddingTop = BackPanel.PaddingBottom = 0;
            Append(BackPanel);
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Enter))
        {
            Main.ClosePlayerChat();
            OnTypingComplete.Invoke(Text);
            Active = false;
            return;
        }

        if (Main.mouseLeft && !IsMouseHovering)
            Active = false;
    }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        if(evt.Target == this)
            Active = true;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        if (Active)
        {
            PlayerInput.WritingText = Active;

            Main.instance.HandleIME();

            string newInput = Main.GetInputText(Text);
            if (newInput != Text)
            {
                if (TypeSound.HasValue) 
                {
                    if (newInput.Length > Text.Length)
                        SoundEngine.PlaySound(TypeSound.Value with {Pitch = 0, PitchVariance = 0.5f});
                    else
                        SoundEngine.PlaySound(TypeSound.Value with { Pitch = -0.5f, PitchVariance = 0.5f});
                }
                Text = newInput;
            }
        }

        Vector2 position = GetDimensions().Position() + new Vector2(6, 4);

        if (string.IsNullOrEmpty(Text) && !Active)
        {
            Utils.DrawBorderString(spriteBatch, HintText, position, Color.DarkGray);
            return;
        }
        
        string displayText = Text;
        if (Active && (int)Main.GlobalTimeWrappedHourly % 2 == 0)
            displayText += "|";

        List<string> textLines = [.. Utils.WordwrapString(displayText, FontAssets.MouseText.Value, (int)(Parent.Width.Pixels - Parent.PaddingRight - 10), 250, out _)];
        textLines.RemoveAll(text => string.IsNullOrEmpty(text));

        #region Hexcode Fixes
        string PreviousLineColorHex = "";
        for (int i = 0; i < textLines.Count; i++)
        {
            int LeftBracketCounter = 0;
            int RightBracketCounter = 0;
            bool gettingHex = false;
            for (int j = 0; j < textLines[i].Length; j++)
            {
                if (textLines[i][j] == '[')
                    LeftBracketCounter++;
                if (textLines[i][j] == ']')
                    RightBracketCounter++;
            }
            if (LeftBracketCounter != RightBracketCounter)
            {
                //Main.NewText("Line Imbalance at line: " + i);
                for (int j = 0; j < textLines[i].Length; j++)
                {
                    if (LeftBracketCounter > RightBracketCounter)
                    {
                        if (gettingHex && textLines[i][j] != ':')
                            PreviousLineColorHex += textLines[i][j];
                        if (textLines[i][j] == '/')
                            gettingHex = true;
                        else if (textLines[i][j] == ':')
                            gettingHex = false;
                    }
                }
                if (LeftBracketCounter > RightBracketCounter)
                {
                    //Main.NewText("Closing Bracket needed!");
                    textLines[i] += ']';
                }
                else
                {
                    //Main.NewText("Color Tag needed!");
                    if (PreviousLineColorHex.Length > 6)
                        PreviousLineColorHex = PreviousLineColorHex.Remove(6);
                    string newText = $"[C/{PreviousLineColorHex}:" + textLines[i];
                    //Main.NewText("Added Tag: " + PreviousLineColorHex);
                    textLines[i] = newText;
                }
                //Main.NewText("Updated line: " + textLines[i]);
            }
        }
        #endregion

        List<List<TextSnippet>> dialogLines = [];
        for (int i = 0; i < textLines.Count; i++)
        {
            dialogLines.Add(ChatManager.ParseMessage(textLines[i], Color.White));
        }

        for (int i = 0; i < dialogLines.Count; i++)
            if (dialogLines[i] != null)
            {
                int positionY = (int)(position.Y + (32 * i));
                ChatManager.DrawColorCodedStringShadow(spriteBatch, FontAssets.MouseText.Value, [.. dialogLines[i]], new Vector2(position.X, positionY), TextColor, 0f, Vector2.Zero, Vector2.One, -1, 2f);
                ChatManager.DrawColorCodedString(spriteBatch, FontAssets.MouseText.Value, [.. dialogLines[i]], new Vector2(position.X, positionY), TextShadow, 0f, Vector2.Zero, Vector2.One, out int hoveredSnippet, -1, true);
            }
    }
}
