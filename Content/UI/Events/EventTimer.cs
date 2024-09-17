using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Windfall.Common.Players;

namespace Windfall.Content.UI
{
    internal class EventTimer : UIState
    {
        private UIElement area;
        private UIText timerText;
        public int timer;
        public int timerStart;

        public override void OnInitialize()
        {
            area = new UIElement();
            area.Left.Set(Main.screenWidth / 2 - area.Width.Pixels / 2, 0f);
            area.Top.Set(Main.screenHeight / 10f, 0f);
            area.Width.Set(180, 0f);
            area.Height.Set(60, 0f);

            timerText = new UIText("", large: true);
            timerText.Left.Set(22, 0f);
            timerText.Top.Set(0, 0f);
            timerText.Width.Set(138, 0f);
            timerText.Height.Set(34, 0f);

            area.Append(timerText);
            Append(area);
        }

        public override void Update(GameTime gameTime)
        {
            if (!GodlyPlayer.AnyGodlyEssence(Main.LocalPlayer))
                return;

            if (timerText.ContainsPoint(Main.MouseScreen))
            {
                var modPlayer = Main.LocalPlayer.Godly();
                string category = "UI";
                Main.LocalPlayer.mouseInterface = true;
                Main.instance.MouseText(GetWindfallLocalText($"{category}.Ambrosia").Format(modPlayer.Ambrosia, 100));
            }
            if(timer < 0)
            {
                ModContent.GetInstance<TimerUISystem>().TimerEnd();
            }
            int timerInSeconds = timer / 60;
            int timerMinutes = timerInSeconds / 60;
            int timerSeconds = timerInSeconds % 60;
            timerText.SetText($"{timerMinutes}:{"" + (timerSeconds < 10 ? "0" : "")}{timerSeconds}");
            float timerRatio = (float)timer / timerStart;
            if(timerRatio < 0.5f)
                timerText.TextColor = Color.Lerp(Color.Red, Color.Yellow, timerRatio / 0.5f);
            else
                timerText.TextColor = Color.Lerp(Color.Yellow, Color.White, (timerRatio - 0.5f) / 0.5f);
            timer--;
            base.Update(gameTime);
        }
    }

    [Autoload(Side = ModSide.Client)]
    internal class TimerUISystem : ModSystem
    {
        private UserInterface TimerUI;

        internal EventTimer EventTimer;

        public override void Load()
        {
            EventTimer = new()
            {
                timer = -1,
                timerStart = -1
            };
            TimerUI = new();
            TimerUI.SetState(EventTimer);
        }

        public void TimerStart(int time)
        {
            EventTimer = new()
            {
                timer = time,
                timerStart = time
            };
            TimerUI = new();
            TimerUI.SetState(EventTimer);           
        }

        public void TimerEnd()
        {
            EventTimer = null;
            TimerUI.SetState(null);
        }

        public override void UpdateUI(GameTime gameTime)
        {
            TimerUI?.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
            if (resourceBarIndex != -1)
            {
                layers.Insert(resourceBarIndex, new LegacyGameInterfaceLayer(
                    "Windfall: Event Timer",
                    delegate {
                        TimerUI.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
}