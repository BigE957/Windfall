using Terraria.UI;
using Windfall.Common.Systems;
using Windfall.Content.UI.WanderersJournals;

namespace Windfall.Content.UI.Activities;
public class ChefMenuUISystem : ModSystem
{
    public static readonly SoundStyle UseSound = new("Windfall/Assets/Sounds/Items/JournalPageTurn");
    internal ChefMenuUIState ChefMenuUIState;
    private UserInterface ChefMenuUI;
    public static bool isChefMenuOpen = false;

    public void ShowChefMenuUI()
    {
        SoundEngine.PlaySound(UseSound with
        {
            Pitch = -0.25f,
            PitchVariance = 0.5f,
            MaxInstances = 5,
            SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest
        });
        isChefMenuOpen = true;

        ChefMenuUIState = new ChefMenuUIState();
        ChefMenuUIState.Activate();

        ChefMenuUI?.SetState(ChefMenuUIState);
    }

    public void HideChefMenuUI()
    {
        SoundEngine.PlaySound(UseSound with
        {
            Pitch = -0.25f,
            PitchVariance = 0.5f,
            MaxInstances = 5,
            SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest
        });
        isChefMenuOpen = false;
        ChefMenuUI?.SetState(null);
    }
    public override void Load()
    {
        // Create custom interface which can swap between different UIStates
        if (!Main.dedServ)
        {
            ChefMenuUI = new UserInterface();
            // Creating custom UIState

            ChefMenuUIState = new ChefMenuUIState();

            // Activate calls Initialize() on the UIState if not initialized, then calls OnActivate and then calls Activate on every child element
            ChefMenuUIState.Activate();
        }
    }
    private GameTime _lastUpdateUiGameTime;

    public override void UpdateUI(GameTime gameTime)
    {
        _lastUpdateUiGameTime = gameTime;
        if (ChefMenuUI?.CurrentState != null)
        {
            ChefMenuUI?.Update(gameTime);
        }
    }
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Info Accessories Bar"));
        if (mouseTextIndex != -1)
        {
            layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                "Windfall: Displays the Chef's Menu",
                delegate
                {
                    ChefMenuUI.Draw(Main.spriteBatch, new GameTime());
                    return true;
                },
                InterfaceScaleType.UI)
            );
        }
    }
}
