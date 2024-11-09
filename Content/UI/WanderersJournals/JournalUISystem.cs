using Terraria.UI;
using Windfall.Common.Systems;

namespace Windfall.Content.UI.WanderersJournals;

public class JournalUISystem : ModSystem
{
    public static readonly SoundStyle UseSound = new("Windfall/Assets/Sounds/Items/JournalPageTurn");
    internal JournalFullUIState JournalFullUIState;
    private UserInterface JournalUI;
    public static bool isJournalOpen = false;
    public static string whichEvilJournal = "None";

    public void ShowJournalUI()
    {
        SoundEngine.PlaySound(UseSound with
        {
            Pitch = -0.25f,
            PitchVariance = 0.5f,
            MaxInstances = 5,
            SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest
        });
        isJournalOpen = true;
        JournalText.isFullJournal = true;
        JournalUI?.SetState(JournalFullUIState);
    }

    public void HideWandererUI()
    {
        SoundEngine.PlaySound(UseSound with
        {
            Pitch = -0.25f,
            PitchVariance = 0.5f,
            MaxInstances = 5,
            SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest
        });
        isJournalOpen = false;
        JournalUI?.SetState(null);
    }
    public override void Load()
    {
        // Create custom interface which can swap between different UIStates
        if (!Main.dedServ)
        {
            JournalUI = new UserInterface();
            // Creating custom UIState

            JournalFullUIState = new JournalFullUIState();
            for (int i = 0; i < 13; i++)
            {
                WorldSaveSystem.JournalsCollected.Add(false);
            }

            // Activate calls Initialize() on the UIState if not initialized, then calls OnActivate and then calls Activate on every child element
            JournalFullUIState.Activate();
        }
    }
    private GameTime _lastUpdateUiGameTime;

    public override void UpdateUI(GameTime gameTime)
    {
        _lastUpdateUiGameTime = gameTime;
        if (JournalUI?.CurrentState != null)
        {
            JournalUI?.Update(gameTime);
        }
    }
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        if (mouseTextIndex != -1)
        {
            layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                "Windfall: Displays the Wander's Journal Page",
                delegate
                {
                    JournalUI.Draw(Main.spriteBatch, new GameTime());
                    return true;
                },
                InterfaceScaleType.UI)
            );
        }
    }
}
