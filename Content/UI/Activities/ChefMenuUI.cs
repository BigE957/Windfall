using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.UI.WanderersJournals;

namespace Windfall.Content.UI.Activities;
internal class ChefMenuUIState : UIState
{
    public DraggableUIPanel UIPanel;
    public override void OnInitialize()
    {
        DraggableUIPanel UIPanel = new();
        UIPanel.SetPadding(0);
        // We need to place this UIElement in relation to its Parent. Later we will be calling `base.Append(coinCounterPanel);`. 
        // This means that this class, ExampleCoinsUI, will be our Parent. Since ExampleCoinsUI is a UIState, the Left and Top are relative to the top left of the screen.
        // SetRectangle method help us to set the position and size of UIElement
        SetRectangle(UIPanel, left: 200f, top: 100f, width: 400f, height: 518f);
        UIPanel.BackgroundColor = Color.Transparent;
        Append(UIPanel);

        Asset<Texture2D> pageTexture = ModContent.Request<Texture2D>($"{nameof(Windfall)}/Assets/UI/WanderersJournals/JournalPage");
        UIImage page = new(pageTexture);
        SetRectangle(page, left: -55f, top: -25f, width: 400f, height: 518f);
        UIPanel.Append(page);

        for(int i = 0; i < LunarCultBaseSystem.MenuIDs.Count; i++)
        {
            UIItem foodIcon = new(LunarCultBaseSystem.MenuIDs[i]);
            SetRectangle(foodIcon, left: 40f - (foodIcon.Width.Pixels / 2f), top: 45f * (i + 2), width: foodIcon.Width.Pixels, height: foodIcon.Height.Pixels);
            UIPanel.Append(foodIcon);

            UIText foodName = new(foodIcon.Item.Name);
            SetRectangle(foodName, left: 70f, top: 45f * (i + 2), width: foodName.Width.Pixels, height: foodName.Height.Pixels);
            UIPanel.Append(foodName);
        }

        UIText MenuTitle = new("Tonight's Menu", large: true);
        SetRectangle(MenuTitle, left: 0f, top: 30f, width: MenuTitle.Width.Pixels, height: MenuTitle.Height.Pixels);
        MenuTitle.HAlign = 0.5f;
        UIPanel.Append(MenuTitle);
    }
    private static void SetRectangle(UIElement uiElement, float left, float top, float width, float height)
    {
        uiElement.Left.Set(left, 0f);
        uiElement.Top.Set(top, 0f);
        uiElement.Width.Set(width, 0f);
        uiElement.Height.Set(height, 0f);
    }
}

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
    public override void PostSetupContent()
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
