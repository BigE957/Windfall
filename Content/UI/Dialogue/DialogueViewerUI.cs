using DialogueHelper.UI.Dialogue;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Windfall.Content.UI.Dialogue;
public class DialogueSelectionUI : UIState
{
    internal UIPanel background;

    internal UIButton<string> closeButton;

    internal readonly List<(UIButton<string> button, bool isTree)> Choices = [];

    internal UIScrollbar scrollBar;

    internal UIList options;

    internal string currentPath = "";

    public override void OnInitialize()
    {
        background = new UIPanel();
        background.Width.Set(1000, 0);
        background.Height.Set(600, 0);
        background.BackgroundColor = Color.Blue;
        background.HAlign = 0.5f;
        background.VAlign = 0.5f;

        closeButton = new("X")
        {
            HAlign = 1f,
            BackgroundColor = Color.DarkRed,
            HoverPanelColor = Color.Red
        };
        closeButton.OnLeftClick += CloseUI;
        closeButton.Width.Pixels = 32;
        closeButton.Height.Pixels = 32;

        background.Append(closeButton);

        Append(background);

        scrollBar = new UIScrollbar
        {
            HAlign = 1f,
            VAlign = 0.5f
        };
        scrollBar.Height.Pixels = 400;

        options = [];
        options.SetScrollbar(scrollBar);
        options.HAlign = 0.5f;
        options.VAlign = 1f;
        options.Width.Pixels = 500;
        options.Height.Pixels = 600;

        background.Append(options);
        background.Append(scrollBar);
    }

    public override void OnActivate()
    {
        Choices.Clear();
        options.Clear();

        string[] files = [.. Windfall.Instance.GetFileNames().Where(f => f.Contains("Localization/DialogueTrees/en-US/"))];
        for (int i = 0; i < files.Length; i++)
            files[i] = files[i].Replace("Localization/DialogueTrees/en-US/", "").Replace(".json", "");

        foreach (string file in files)
        {
            string name = "";

            string check = file;

            if (!check.Contains(currentPath))
                continue;

            if (currentPath != "" && check[..currentPath.Length] != currentPath)
                continue;

            check = check[currentPath.Length..];

            bool slashHit = false;
            for (int i = 0; i < check.Length; i++)
            {
                if (check[i] == '/')
                {
                    slashHit = true;
                    break;
                }
                name += check[i];
            }

            if (Choices.Any(b => b.button.Text == name))
                continue;

            UIButton<string> button = new(name);
            button.Width.Pixels = 200;
            button.Height.Pixels = 100;
            button.OnLeftClick += ButtonPress;
            Choices.Add(new(button, !slashHit));
        }

        foreach (var v in Choices)
            options.Add(v.button);
    }

    private void ButtonPress(UIMouseEvent evt, UIElement listeningElement)
    {
        var (button, isTree) = Choices.Find(e => e.button == (UIButton<string>)listeningElement);
        if (isTree)
        {
            string key = currentPath + button.Text;

            DialogueViewerUISystem system = ModContent.GetInstance<DialogueViewerUISystem>();
            system.CloseUI();

            ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, key, new("DialogueViewer", [-1]));
        }
        else
        {
            currentPath += button.Text + '/';
            OnActivate();
        }
    }

    private void CloseUI(UIMouseEvent evt, UIElement listeningElement)
    {
        DialogueViewerUISystem system = ModContent.GetInstance<DialogueViewerUISystem>();
        system.CloseUI();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (background.ContainsPoint(Main.MouseScreen))
            Main.LocalPlayer.mouseInterface = true;
    }
}

public class DialogueViewerUISystem : ModSystem
{
    internal DialogueSelectionUI selectionUI;
    internal UserInterface userInterface;
    public bool uiOpen = false;

    public override void OnModLoad()
    {
        if (!Main.dedServ)
        {
            userInterface = new UserInterface();
            selectionUI = new DialogueSelectionUI();
            selectionUI.Activate();
        }
    }

    public void OpenUI()
    {
        uiOpen = true;
        userInterface?.SetState(selectionUI);
    }

    public void CloseUI()
    {
        selectionUI.currentPath = "";
        uiOpen = false;
        userInterface?.SetState(null);
    }

    public override void UpdateUI(GameTime gameTime)
    {
        if (userInterface?.CurrentState != null)
            userInterface?.Update(gameTime);
    }
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        if (mouseTextIndex != -1)
        {
            layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                "Windfall: Displays the Dialogue Viewer UI",
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