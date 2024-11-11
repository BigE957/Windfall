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
        if (LunarCultBaseSystem.MenuFoodIDs.Count == 0)
        {
            LunarCultBaseSystem.MenuFoodIDs = LunarCultBaseSystem.FoodIDs;
            while (LunarCultBaseSystem.MenuFoodIDs.Count > 5)
                LunarCultBaseSystem.MenuFoodIDs.RemoveAt(Main.rand.Next(LunarCultBaseSystem.MenuFoodIDs.Count));
        }

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

        for(int i = 0; i < LunarCultBaseSystem.MenuFoodIDs.Count; i++)
        {
            UIItem foodIcon = new(LunarCultBaseSystem.MenuFoodIDs[i]);
            SetRectangle(foodIcon, left: 40f - (foodIcon.Width.Pixels / 2f), top: 45f * (i + 2), width: foodIcon.Width.Pixels, height: foodIcon.Height.Pixels);
            UIPanel.Append(foodIcon);

            UIText foodName = new(foodIcon.Item.Name);
            SetRectangle(foodName, left: 70f, top: 45f * (i + 2), width: foodName.Width.Pixels, height: foodName.Height.Pixels);
            UIPanel.Append(foodName);
        }

        UIText MenuTitle = new("Tonight's Menu", large: true);
        SetRectangle(MenuTitle, left: 45f, top: 20f, width: MenuTitle.Width.Pixels, height: MenuTitle.Height.Pixels);
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
