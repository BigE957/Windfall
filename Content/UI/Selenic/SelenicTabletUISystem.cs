using Terraria.UI;

namespace Windfall.Content.UI.Selenic
{
    public class SelenicTabletUISystem : ModSystem
    {
        public static readonly SoundStyle UseSound = SoundID.DD2_DarkMageCastHeal;
        internal SelenicTabletUIState SelenicTabletUIState;
        private UserInterface SelenicTabletUI;
        public static bool isUIOpen = false;
        readonly float xResolutionScale = Main.screenWidth / 2560f;
        readonly float yResolutionScale = Main.screenHeight / 1440f;

        public void ShowUI()
        {
            isUIOpen = true;
            JustOpened = true;
            SelenicTabletUIState.textCounter = 0;
            SoundEngine.PlaySound(UseSound with
            {
                Pitch = -0.25f,
                PitchVariance = 0.5f,
                MaxInstances = 5,
                SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest
            });
            SelenicTabletUIState.SelenicUIPanel.Top.Set(Main.screenHeight + SelenicTabletUIState.UIHeight, 0f);
            SelenicTabletUI?.SetState(SelenicTabletUIState);
        }

        public void UpdateUI()
        {
            SelenicTabletUI?.SetState(SelenicTabletUIState);
        }

        public void HideUI()
        {
            isUIOpen = false;
            SelenicTabletUIState.textCounter = 0;
            SoundEngine.PlaySound(UseSound with
            {
                Pitch = -0.25f,
                PitchVariance = 0.5f,
                MaxInstances = 5,
                SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest
            });           
        }
        public override void Load()
        {
            // Create custom interface which can swap between different UIStates
            if (!Main.dedServ)
            {
                SelenicTabletUI = new UserInterface();
                // Creating custom UIState

                SelenicTabletUIState = new SelenicTabletUIState();

                // Activate calls Initialize() on the UIState if not initialized, then calls OnActivate and then calls Activate on every child element
                SelenicTabletUIState.Activate();
            }
        }
        private GameTime _lastUpdateUiGameTime;

        public override void UpdateUI(GameTime gameTime)
        {
            _lastUpdateUiGameTime = gameTime;
            if (SelenicTabletUI?.CurrentState != null)
            {
                SelenicTabletUI?.Update(gameTime);
            }            
        }
        private bool JustOpened = false;
        internal bool UIStill = false;
        public override void PostUpdateInput()
        {
            float xResolutionScale = Main.screenWidth / 2560f;
            float yResolutionScale = Main.screenHeight / 1440f;
            if (SelenicTabletUIState.SelenicUIPanel.Width.Pixels / SelenicTabletUIState.UIWidth >= 0.9f)
                SelenicTabletUIState.nextButton.SetVisibility(0.75f, 0.5f);
            else
                SelenicTabletUIState.nextButton.SetVisibility(0f, 0f);
            if (isUIOpen)
            {
                if (JustOpened)
                {
                    SelenicTabletUIState.SelenicUIPanel.Width.Set(0f, 0f);
                    SelenicTabletUIState.SelenicUIPanel.Height.Set(0f, 0f);
                    SelenicTabletUIState.SelenicUIPanel.Top.Set(Main.screenHeight / 2 + SelenicTabletUIState.UIHeight, 0f);
                    JustOpened = false;
                }
                if(SelenicTabletUIState.SelenicUIPanel.Top.Pixels > 200f)
                    SelenicTabletUIState.SelenicUIPanel.Top.Set(SelenicTabletUIState.SelenicUIPanel.Top.Pixels - 15f, 0f);

                if (SelenicTabletUIState.SelenicUIPanel.Height.Pixels < SelenicTabletUIState.UIHeight)
                    SelenicTabletUIState.SelenicUIPanel.Height.Set(SelenicTabletUIState.SelenicUIPanel.Height.Pixels + (SelenicTabletUIState.UIHeight / ((70 / 1.5f) * yResolutionScale)), 0f);
                if (SelenicTabletUIState.SelenicUIPanel.Width.Pixels < SelenicTabletUIState.UIWidth)
                    SelenicTabletUIState.SelenicUIPanel.Width.Set(SelenicTabletUIState.SelenicUIPanel.Width.Pixels + (SelenicTabletUIState.UIWidth / ((70 / 1.5f) * xResolutionScale)), 0f);
                
            }
            else if (SelenicTabletUI.CurrentState == SelenicTabletUIState)
            {
                if (SelenicTabletUIState.SelenicUIPanel.Top.Pixels < Main.screenHeight / 2 + SelenicTabletUIState.UIHeight)
                    SelenicTabletUIState.SelenicUIPanel.Top.Set(SelenicTabletUIState.SelenicUIPanel.Top.Pixels + 15f, 0f);
                else
                {
                    SelenicTabletUI?.SetState(null);
                    return;
                }

                if (SelenicTabletUIState.SelenicUIPanel.Height.Pixels > 0f)
                    SelenicTabletUIState.SelenicUIPanel.Height.Set(SelenicTabletUIState.SelenicUIPanel.Height.Pixels - (SelenicTabletUIState.UIHeight / ((70 / 1.5f) * yResolutionScale)), 0f);
                if (SelenicTabletUIState.SelenicUIPanel.Width.Pixels > 0f)
                    SelenicTabletUIState.SelenicUIPanel.Width.Set(SelenicTabletUIState.SelenicUIPanel.Width.Pixels - (SelenicTabletUIState.UIWidth / ((70 / 1.5f) * xResolutionScale)), 0f);
            }
            if (isUIOpen || SelenicTabletUI.CurrentState == SelenicTabletUIState)
            {
                SelenicTabletUIState.SelenicUIPanel.Left.Set((Main.screenWidth / 4) - (SelenicTabletUIState.SelenicUIPanel.Width.Pixels / 2), 0f);

                SelenicTabletUIState.nextButton.Left.Set(SelenicTabletUIState.SelenicUIPanel.Width.Pixels - (SelenicTabletUIState.nextButton.Width.Pixels + 10f), 0f);
                SelenicTabletUIState.nextButton.Top.Set(SelenicTabletUIState.SelenicUIPanel.Height.Pixels - 35f, 0f);

                SelenicTabletUIState.SelenicContents.Width.Set((SelenicTabletUIState.SelenicUIPanel.Width.Pixels / SelenicTabletUIState.UIWidth) * 100, 0f);
                SelenicTabletUIState.SelenicContents.Top.Set(-10, 0f);
            }
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "Windfall: Displays the Selenic Tablet's Text",
                    delegate
                    {
                        SelenicTabletUI.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
}
