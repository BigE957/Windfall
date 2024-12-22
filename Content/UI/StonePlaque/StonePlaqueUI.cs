using System;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Windfall.Content.Items.Tools;

namespace Windfall.Content.UI.StonePlaque;
public class StonePlaqueUI : UIState
{
    public MouseBlockingUIPanel Panel;
    public UIImage Background;
    public UIImage ChiselImage;
    public UIText ToggleButton;
    public UIText CloseButton;
    public UITextPrompt Prompt;
    public bool IsDark = true;
    private string StoredText = "";

    public override void OnInitialize()
    {
        Panel = new MouseBlockingUIPanel();
        Panel.Width.Set(365, 0);
        Panel.Height.Set(256, 0);
        Panel.BackgroundColor = Color.DarkGray;
        Panel.HAlign = 0.5f;
        Panel.VAlign = 0.1f;
        Append(Panel);

        Background = new UIImage(ModContent.Request<Texture2D>("Windfall/Assets/UI/StonePlaqueEditor/DarkStonePlaquePrompt"));
        Background.Top.Pixels = -32;
        Background.Left.Pixels = -24;
        Panel.Append(Background);

        ChiselImage = new UIImage(ModContent.Request<Texture2D>("Windfall/Assets/UI/StonePlaqueEditor/HammerAndChiselBig"))
        {
            HAlign = -0.05f,
            VAlign = 0.85f,
            ImageScale = 1.5f,
            NormalizedOrigin = Vector2.One * 0.5f
        };
        Panel.Append(ChiselImage);

        ToggleButton = new("Edit")
        {
            HAlign = 0.15f,
            VAlign = 1f,
            TextColor = Color.White,
            ShadowColor = Color.Black
        };
        ToggleButton.Height.Pixels = 24;
        ToggleButton.OnLeftClick += ToggleState;
        Panel.Append(ToggleButton);

        CloseButton = new("Close")
        {
            HAlign = 0.35f,
            VAlign = 1f,
            TextColor = Color.White,
            ShadowColor = Color.Black
        };
        CloseButton.Height.Pixels = 24;
        CloseButton.OnLeftClick += Close;
        Panel.Append(CloseButton);

        Prompt = new((int)(Panel.Width.Pixels - Panel.PaddingRight), new SoundStyle("Windfall/Assets/Sounds/UI/StoneCut"), Color.DarkGray, Color.Black);
        Panel.Append(Prompt);

        Prompt.OnTyping += OnType;
        Prompt.OnDelete += OnDelete;
    }

    public override void OnActivate()
    {
        ChiselImage.Rotation = (float)Math.Sin(Main.GlobalTimeWrappedHourly) * PiOver4 / 2f;

        ToggleButton.SetText("Edit");
        Prompt.Active = false;

        Main.LocalPlayer.SetTalkNPC(-1);
        Main.playerInventory = false;
        Main.LocalPlayer.sign = -1;

        if (IsDark)
        {
            Background.SetImage(ModContent.Request<Texture2D>("Windfall/Assets/UI/StonePlaqueEditor/DarkStonePlaquePrompt"));
            Panel.BackgroundColor = Color.DarkGray;
            Prompt.SetColor(new(130, 168, 162));
        }
        else
        {
            Background.SetImage(ModContent.Request<Texture2D>("Windfall/Assets/UI/StonePlaqueEditor/WhiteStonePlaquePrompt"));
            Panel.BackgroundColor = Color.LightGray;
            Prompt.SetColor(new(219, 198, 138));
        }
    }

    public override void Update(GameTime gameTime)
    {
        if (Main.LocalPlayer.chest != -1 || Main.LocalPlayer.sign != -1 || Main.LocalPlayer.talkNPC != -1 || Main.playerInventory || Main.InGuideCraftMenu)
        {
            ModContent.GetInstance<StonePlaqueUISystem>().CloseStonePlaqueUI(StoredText);
            return;
        }

        Main.playerInventory = false;
        Main.npcChatText = string.Empty;

        base.Update(gameTime);

        if (ToggleButton.IsMouseHovering)
            ToggleButton.TextColor = Color.Yellow;
        else
            ToggleButton.TextColor = Color.White;

        if (CloseButton.IsMouseHovering)
            CloseButton.TextColor = Color.Yellow;
        else
            CloseButton.TextColor = Color.White;

        ChiselImage.Rotation = (float)Math.Sin(Main.GlobalTimeWrappedHourly) * PiOver4 / 2f;
        if (ChiselImage.ImageScale > 1.51f)
            ChiselImage.ImageScale *= 0.98f;
        else if (ChiselImage.ImageScale < 1.49f)
            ChiselImage.ImageScale *= 1.02f;
        else
            ChiselImage.ImageScale = 1.5f;
    }

    private void ToggleState(UIMouseEvent evt, UIElement listener)
    {
        Prompt.Active = !Prompt.Active;
        if (Prompt.Active)
            ToggleButton.SetText("Save");
        else
        {
            StoredText = Prompt.Text;
            ToggleButton.SetText("Edit");
        }

    }

    private void Close(UIMouseEvent evt, UIElement listener)
    {
        if(Prompt.Active == false)
            ModContent.GetInstance<StonePlaqueUISystem>().CloseStonePlaqueUI(Prompt.Text);
        else
            ModContent.GetInstance<StonePlaqueUISystem>().CloseStonePlaqueUI(StoredText);
    }

    private void OnType() => ChiselImage.ImageScale = 2f;
    private void OnDelete() => ChiselImage.ImageScale = 1f;
}
