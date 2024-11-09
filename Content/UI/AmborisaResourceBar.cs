using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Windfall.Common.Players;

namespace Windfall.Content.UI;

internal class AmbrosiaResourceBar : UIState
{
    private UIElement area;
    private UIImage barFrame;
    private Color gradientA;
    private Color gradientB;

    public override void OnInitialize()
    {
        area = new UIElement();
        area.Left.Set(-area.Width.Pixels - 600, 1f);
        area.Top.Set(30, 0f);
        area.Width.Set(182, 0f);
        area.Height.Set(60, 0f);

        barFrame = new UIImage(ModContent.Request<Texture2D>("Windfall/Assets/UI/ProgressFrame"));
        barFrame.Left.Set(22, 0f);
        barFrame.Top.Set(0, 0f);
        barFrame.Width.Set(138, 0f);
        barFrame.Height.Set(34, 0f);

        gradientA = Color.DarkRed;
        gradientB = Color.Gold;

        area.Append(barFrame);
        Append(area);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!GodlyPlayer.AnyGodlyEssence(Main.LocalPlayer))
            return;

        base.Draw(spriteBatch);
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        var modPlayer = Main.LocalPlayer.Godly();
        float quotient = (float)modPlayer.Ambrosia / 100;
        quotient = Utils.Clamp(quotient, 0f, 1f);

        Rectangle hitbox = barFrame.GetInnerDimensions().ToRectangle();
        hitbox.X += 8;
        hitbox.Width -= 16;
        hitbox.Y += 4;
        hitbox.Height -= 8;

        int left = hitbox.Left;
        int right = hitbox.Right;
        int steps = (int)((right - left) * quotient);
        for (int i = 0; i < steps; i++)
        {
            float percent = (float)i / (right - left);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(left + i, hitbox.Y, 1, hitbox.Height), Color.Lerp(gradientA, gradientB, percent));
        }
        if (quotient != 1f)
        {
            for (int i = steps; i < (right - left); i++)
            {
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(left + i, hitbox.Y, 1, hitbox.Height), Color.Black);
            }
        }
    }

    public override void Update(GameTime gameTime)
    {
        if (!GodlyPlayer.AnyGodlyEssence(Main.LocalPlayer))
            return;

        if (barFrame.ContainsPoint(Main.MouseScreen))
        {
            var modPlayer = Main.LocalPlayer.Godly();
            string category = "UI";
            Main.LocalPlayer.mouseInterface = true;
            Main.instance.MouseText(GetWindfallLocalText($"{category}.Ambrosia").Format(modPlayer.Ambrosia, 100));
        }
        
        base.Update(gameTime);
    }
}

[Autoload(Side = ModSide.Client)]
internal class AmbrosiaResourceUISystem : ModSystem
{
    private UserInterface AmbrosiaUI;

    internal AmbrosiaResourceBar AmbrosiaResourceBar;

    public override void Load()
    {
        AmbrosiaResourceBar = new();
        AmbrosiaUI = new();
        AmbrosiaUI.SetState(AmbrosiaResourceBar);
    }

    public override void UpdateUI(GameTime gameTime)
    {
        AmbrosiaUI?.Update(gameTime);
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
        if (resourceBarIndex != -1)
        {
            layers.Insert(resourceBarIndex, new LegacyGameInterfaceLayer(
                "Windfall: Ambrosia Resource Bar",
                delegate {
                    AmbrosiaUI.Draw(Main.spriteBatch, new GameTime());
                    return true;
                },
                InterfaceScaleType.UI)
            );
        }
    }
}