using System;
using Terraria.UI;
using Windfall.Content.Tiles.TileEntities;

namespace Windfall.Content.UI.StonePlaque;
public class StonePlaqueUISystem : ModSystem
{
    internal StonePlaqueUI StonePlaqueUIState;
    private UserInterface userInterface;
    public bool uiOpen = false;
    private StonePlaqueEntity myTE = null;

    public void OpenWhiteStonePlaqueUI(StonePlaqueEntity TileEntity)
    {
        uiOpen = true;
        myTE = TileEntity;
        StonePlaqueUIState.IsDark = false;
        StonePlaqueUIState.Prompt.Text = TileEntity.PlaqueText;
        userInterface?.SetState(StonePlaqueUIState);
    }

    public void OpenDarkStonePlaqueUI(StonePlaqueEntity TileEntity)
    {
        uiOpen = true;
        myTE = TileEntity;
        StonePlaqueUIState.IsDark = true;
        StonePlaqueUIState.Prompt.Text = TileEntity.PlaqueText;
        userInterface?.SetState(StonePlaqueUIState);
    }

    public void CloseStonePlaqueUI(string EnteredText = null)
    {
        if(EnteredText != null)
            myTE.PlaqueText = EnteredText;
        myTE = null;
        uiOpen = false;
        userInterface?.SetState(null);
    }
    public override void Load()
    {
        // Create custom interface which can swap between different UIStates
        if (!Main.dedServ)
        {
            userInterface = new UserInterface();
            // Creating custom UIState

            StonePlaqueUIState = new StonePlaqueUI();

            // Activate calls Initialize() on the UIState if not initialized, then calls OnActivate and then calls Activate on every child element
            StonePlaqueUIState.Activate();

            StonePlaqueUIState.Prompt.OnTypingComplete += CloseStonePlaqueUI;
        }
    }
    private GameTime _lastUpdateUiGameTime;

    public override void UpdateUI(GameTime gameTime)
    {
        _lastUpdateUiGameTime = gameTime;
        if (userInterface?.CurrentState != null)
        {
            userInterface?.Update(gameTime);
        }
    }
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        if (mouseTextIndex != -1)
        {
            layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                "Windfall: Displays the StonePlaqueUI",
                delegate
                {
                    userInterface.Draw(Main.spriteBatch, new GameTime());
                    return true;
                },
                InterfaceScaleType.UI)
            );
        }
    }
}
