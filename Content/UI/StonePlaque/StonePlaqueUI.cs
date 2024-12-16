using Terraria.UI;

namespace Windfall.Content.UI.StonePlaque;
public class StonePlaqueUI : UIState
{
    public MouseBlockingUIPanel Panel;
    public UITextPrompt Prompt;
    public bool IsDark = true;

    public override void OnInitialize()
    {
        Panel = new MouseBlockingUIPanel();
        Panel.Width.Set(400, 0);
        Panel.Height.Set(200, 0);
        Panel.BackgroundColor = Color.DarkGray;
        Panel.HAlign = 0.5f;
        Panel.VAlign = 0.5f;
        Append(Panel);

        Prompt = new((int)(Panel.Width.Pixels - Panel.PaddingRight), (int)(Panel.Height.Pixels - Panel.PaddingBottom), new SoundStyle("Windfall/Assets/Sounds/UI/StoneCut"), Color.DarkGray, Color.Black);
        Panel.Append(Prompt);
    }

    public override void OnActivate()
    {
        Panel.Width.Set(400, 0);
        Panel.Height.Set(200, 0);
        if (IsDark)
            Panel.BackgroundColor = Color.DarkGray;
        else 
            Panel.BackgroundColor = Color.LightGray;
    }
}
