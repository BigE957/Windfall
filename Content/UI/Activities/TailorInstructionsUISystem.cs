using Terraria.UI;

namespace Windfall.Content.UI.Activities;
internal class TailorInstructionsUISystem : ModSystem
{
    public static readonly SoundStyle UseSound = new("Windfall/Assets/Sounds/Items/JournalPageTurn");
    internal TailorInstructionsUIState TailorInstructionsUIState;
    private UserInterface tailorInstructionsUI;
    public static bool IsTailorInstructionsOpen = false;

    public void ShowTailorInstructionsUI()
    {
        SoundEngine.PlaySound(UseSound with
        {
            Pitch = -0.25f,
            PitchVariance = 0.5f,
            MaxInstances = 5,
            SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest
        });
        TailorInstructionsUIState.Pages page = 0;
        Vector2 start = new(200, 100);
        if (!IsTailorInstructionsOpen)
            IsTailorInstructionsOpen = true;
        else
        {
            page = TailorInstructionsUIState.page;
            start = new(TailorInstructionsUIState.Children.First().Left.Pixels, TailorInstructionsUIState.Children.First().Top.Pixels);
        }
        TailorInstructionsUIState = new()
        {
            page = page,
            start = start
        };
        TailorInstructionsUIState.Activate();

        tailorInstructionsUI?.SetState(TailorInstructionsUIState);
    }

    public void HideTailorInstructionsUI()
    {
        SoundEngine.PlaySound(UseSound with
        {
            Pitch = -0.25f,
            PitchVariance = 0.5f,
            MaxInstances = 5,
            SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest
        });
        IsTailorInstructionsOpen = false;
        tailorInstructionsUI?.SetState(null);
    }
    public override void Load()
    {
        // Create custom interface which can swap between different UIStates
        if (!Main.dedServ)
        {
            tailorInstructionsUI = new UserInterface();
            // Creating custom UIState

            TailorInstructionsUIState = new TailorInstructionsUIState();

            // Activate calls Initialize() on the UIState if not initialized, then calls OnActivate and then calls Activate on every child element
            TailorInstructionsUIState.Activate();
        }
    }
    private GameTime _lastUpdateUiGameTime;

    public override void UpdateUI(GameTime gameTime)
    {
        _lastUpdateUiGameTime = gameTime;
        if (tailorInstructionsUI?.CurrentState != null)
        {
            tailorInstructionsUI?.Update(gameTime);
        }
    }
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Info Accessories Bar"));
        if (mouseTextIndex != -1)
        {
            layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                "Windfall: Displays the Tailor's Instructions",
                delegate
                {
                    tailorInstructionsUI.Draw(Main.spriteBatch, new GameTime());
                    return true;
                },
                InterfaceScaleType.UI)
            );
        }
    }
}