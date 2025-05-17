using CalamityMod.Items.Placeables.DraedonStructures;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Windfall.Common.Systems;
using Windfall.Content.Items.Utility;
using Windfall.Content.Projectiles.Other;

namespace Windfall.Content.UI.DraedonsDatabase;
/*
internal class Desktop : UIState
{
    public MouseBlockingUIPanel panel;
    public override void OnInitialize()
    {
        panel = new();
        panel.Width.Set(Main.screenWidth * 1.5f, 0);
        panel.Height.Set(Main.screenHeight * 1.5f, 0);
        panel.HAlign = panel.VAlign = 0.5f;
        Append(panel);

        Asset<Texture2D> BackgroundTexture = ModContent.Request<Texture2D>($"{nameof(Windfall)}/Assets/UI/DraedonsDatabase/DraedonUIBG");
        UIImage Background = new(BackgroundTexture);
        SetRectangle(Background, left: -125f, top: -125f, width: 400f, height: 518f);
        panel.Append(Background);

        Asset<Texture2D> CloseButtonTexture = ModContent.Request<Texture2D>($"{nameof(Windfall)}/Assets/UI/DraedonsDatabase/DraedonUIClose");
        UIButton CloseButton = new(CloseButtonTexture, "Close Database");
        SetRectangle(CloseButton, left: 1270f, top: 115f, width: 26f, height: 26f);
        CloseButton.OnLeftClick += (evt, listeningElement) =>
        {
            ModContent.GetInstance<DraedonUISystem>().CloseDraedonDatabase();
        };
        panel.Append(CloseButton);

        Asset<Texture2D> DraeDashIconTexture = ModContent.Request<Texture2D>($"{nameof(Windfall)}/Assets/UI/DraedonsDatabase/DraedonUIDraeDashLogo");
        UIButton DraeDashIcon = new(DraeDashIconTexture, "Open Drae-Dash");
        SetRectangle(DraeDashIcon, left: 210f, top: 115f, width: 52f, height: 52f);
        DraeDashIcon.OnLeftClick += (evt, listeningElement) =>
        {
            ModContent.GetInstance<DraedonUISystem>().OpenDraedonApp("DraeDash");
        };
        panel.Append(DraeDashIcon);

        Asset<Texture2D> PlutusVaultIconTexture = ModContent.Request<Texture2D>($"{nameof(Windfall)}/Assets/UI/DraedonsDatabase/DraedonUIDraeDashLogo");
        UIButton PlutusVault = new(DraeDashIconTexture, "Open Plutus' Vault");
        SetRectangle(PlutusVault, left: 270f, top: 115f, width: 52f, height: 52f);
        PlutusVault.OnLeftClick += (evt, listeningElement) =>
        {
            ModContent.GetInstance<DraedonUISystem>().OpenDraedonApp("PlutusVault");
        };
        panel.Append(PlutusVault);

        UIText header = new("Draedon's Database")
        {
            HAlign = 0.5f  // 1
        };
        header.Top.Set(115, 0); // 2
        panel.Append(header);

        UIText text = new("Draedon Gaming!")
        {
            HAlign = 0.5f, // 1
            VAlign = 0.5f // 1
        };
        panel.Append(text);
    }
    private void SetRectangle(UIElement uiElement, float left, float top, float width, float height)
    {
        uiElement.Left.Set(left, 0f);
        uiElement.Top.Set(top, 0f);
        uiElement.Width.Set(width, 0f);
        uiElement.Height.Set(height, 0f);
    }
}
internal class DraeDash : UIState
{
    public MouseBlockingUIPanel panel;
    public static bool orderEnRoute = false;
    int orderCost = 0;
    internal int[,] DraeDashItems =
        {
            { ModContent.ItemType<DraedonCharger>(), 1, 1000000},

            { ModContent.ItemType<LaboratoryPlating>(), 100 , 1000},

            { ModContent.ItemType<LaboratoryPanels>(), 100, 1000 },

            { ModContent.ItemType<HazardChevronPanels>(), 100, 1000 },

            { ModContent.ItemType<LaboratoryPipePlating>(), 100, 1000 },

            { ModContent.ItemType<LaboratoryConsoleItem>(), 10, 10000 },

            { ModContent.ItemType<LaboratoryContainmentBoxItem>(), 10, 10000 },

            { ModContent.ItemType<LaboratoryDisplayItem>(), 10, 10000 },

            { ModContent.ItemType<LaboratoryElectricPanelItem>(), 10, 10000 },

            { ModContent.ItemType<LaboratoryScreenItem>(), 10, 10000 },

            { ModContent.ItemType<LaboratoryServerItem>(), 10, 10000 },

            { ModContent.ItemType<LaboratoryTerminalItem>(), 10, 10000 },

            { ModContent.ItemType<ReinforcedCrateItem>(), 10, 10000 },

            { ModContent.ItemType<LabHologramProjectorItem>(), 1, 100000 },

            { ModContent.ItemType<LaboratoryDoorItem>(), 10, 10000 },

            { ModContent.ItemType<SecurityChest>(), 1, 10000 },

            { ModContent.ItemType<PowerCellFactoryItem>(), 1, 100000 },

            { ModContent.ItemType<ChargingStationItem>(), 1, 100000 },
        };
    public override void OnInitialize()
    {
        panel = new();
        panel.Width.Set(Main.screenWidth * 1.5f, 0);
        panel.Height.Set(Main.screenHeight * 1.5f, 0);
        panel.HAlign = panel.VAlign = 0.5f;
        Append(panel);

        Asset<Texture2D> BackgroundTexture = ModContent.Request<Texture2D>($"{nameof(Windfall)}/Assets/UI/DraedonsDatabase/DraedonUIBG");
        UIImage Background = new(BackgroundTexture);
        SetRectangle(Background, left: -125f, top: -125f, width: 56f, height: 56);
        panel.Append(Background);

        Asset<Texture2D> CloseButtonTexture = ModContent.Request<Texture2D>($"{nameof(Windfall)}/Assets/UI/DraedonsDatabase/DraedonUIClose");
        UIButton CloseButton = new(CloseButtonTexture, "Close Drae-Dash");
        SetRectangle(CloseButton, left: 1270f, top: 115f, width: 56f, height: 26f);
        CloseButton.OnLeftClick += (evt, listeningElement) =>
        {
            ModContent.GetInstance<DraedonUISystem>().OpenDraedonDatabase();
        };
        panel.Append(CloseButton);

        UIText header = new("Drae-Dash")
        {
            HAlign = 0.5f  // 1
        };
        header.Top.Set(115, 0); // 2
        panel.Append(header);

        int g = 0;
        int j = 0;
        for (int i = 0; i < DraeDashItems.GetLength(0); i++)
        {
            CreateDraeDashItemUI(DraeDashItems[i, 0], DraeDashItems[i, 1], DraeDashItems[i, 2], g, j);
            g++;
            if (g > 6)
            {
                g = 0;
                j++;
            }
        }

        ModLoader.TryGetMod("CalamityModMusic", out Mod calamityMusic);
        if (calamityMusic != null)
            CreateDraeDashItemUI(calamityMusic.Find<ModItem>("BioLabMusicBox").Type, 1, 100000, g, j);

        Asset<Texture2D> BuyButtonTexture = ModContent.Request<Texture2D>($"{nameof(Windfall)}/Assets/UI/DraedonsDatabase/DraedonUIPurchase");
        UIButton BuyButton = new(BuyButtonTexture, "Complete Order");
        SetRectangle(BuyButton, left: Main.screenWidth / 4 * 3, top: Main.screenHeight / 4 * 3, width: 56f, height: 26f);
        BuyButton.OnLeftClick += (evt, listeningElement) =>
        {
            int index = WorldSaveSystem.CreditDataNames.FindIndex(n => n == Main.LocalPlayer.name);
            WorldSaveSystem.CreditDataCredits[index] -= orderCost;
            Player player = Main.LocalPlayer;
            Vector2 positionForVelocity;
            if (Main.rand.NextBool(2))
                positionForVelocity = new Vector2(player.Center.X + 400f, player.Center.Y - 40f);
            else
                positionForVelocity = new Vector2(player.Center.X - 400f, player.Center.Y - 40f);
            Vector2 position = new(positionForVelocity.X, positionForVelocity.Y - 4080);
            Vector2 velocity = (positionForVelocity - position).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(9f, 10f);
            Projectile.NewProjectile(null, position, velocity, ModContent.ProjectileType<DraeDashDropPod>(), 0, 0, player.whoAmI, player.Center.Y - 40f);
            orderEnRoute = true;
            orderCost = 0;

            ModContent.GetInstance<DraedonUISystem>().OpenDraedonDatabase();
        };
        panel.Append(BuyButton);

        CreditCount creditCount = new()
        {
            drawX = 1100f,
            drawY = 130f,
        };
        panel.Append(creditCount);

        UIText text = new("Order all your favorite Draedon Enterprise\nproducts all from the comfort of your home!")
        {
            HAlign = 0.5f, // 1
            VAlign = 0.75f // 1
        };
        panel.Append(text);
    }
    private void SetRectangle(UIElement uiElement, float left, float top, float width, float height)
    {
        uiElement.Left.Set(left, 0f);
        uiElement.Top.Set(top, 0f);
        uiElement.Width.Set(width, 0f);
        uiElement.Height.Set(height, 0f);
    }
    private void CreateDraeDashItemUI(int itemType, int quantityPerOrder, int cost, float x, float y)
    {
        x = Main.screenWidth / 2 - 400 + 150 * x;
        y = Main.screenHeight / 2 - 200 + 100 * y;

        Asset<Texture2D> IconBackgroundTexture = ModContent.Request<Texture2D>($"{nameof(Windfall)}/Assets/UI/DraedonsDatabase/DraedonUIIconBG");
        UIImage IconBackground = new(IconBackgroundTexture);
        SetRectangle(IconBackground, left: x - 52 / 2, top: y - 52 / 2 - 15f, width: 52, height: 52);
        panel.Append(IconBackground);

        ModItem item = ModContent.GetModItem(itemType);
        string itemName = ItemID.Search.GetName(itemType);
        Asset<Texture2D> ItemTexture = ModContent.Request<Texture2D>(item.Texture);
        UIImage ItemIcon = new(ItemTexture);
        SetRectangle(ItemIcon, left: x - ItemTexture.Width() / 2, top: y - ItemTexture.Height() / 2 - 15f, width: ItemTexture.Width(), height: ItemTexture.Height());
        panel.Append(ItemIcon);

        ItemOrderCount itemOrderCount = new()
        {
            itemToCount = itemName,
            drawX = x + 2f,
            drawY = y + 20f,
        };
        panel.Append(itemOrderCount);

        Asset<Texture2D> AddButtonTexture = ModContent.Request<Texture2D>($"{nameof(Windfall)}/Assets/UI/DraedonsDatabase/DraedonUIArrowRight");
        UIButton AddButton = new(AddButtonTexture, "+1");
        SetRectangle(AddButton, left: x - 56 / 2 + 45f, top: y - 26 / 2 + 20f, width: 56f, height: 26f);
        AddButton.OnLeftClick += (evt, listeningElement) =>
        {
            int index = WorldSaveSystem.CreditDataNames.FindIndex(n => n == Main.LocalPlayer.name);
            if (WorldSaveSystem.CreditDataCredits[index] >= orderCost + cost)
            {
                orderCost += cost;
                if (DraedonUISystem.DraeDashOrder.ContainsKey(itemName))
                {
                    DraedonUISystem.DraeDashOrder[itemName] += quantityPerOrder;
                }
                else
                {
                    DraedonUISystem.DraeDashOrder.Add(itemName, quantityPerOrder);
                }
            }
        };
        panel.Append(AddButton);

        Asset<Texture2D> SubtractButtonTexture = ModContent.Request<Texture2D>($"{nameof(Windfall)}/Assets/UI/DraedonsDatabase/DraedonUIArrowLeft");
        UIButton SubtractButton = new(SubtractButtonTexture, "-1");
        SetRectangle(SubtractButton, left: x - 56 / 2 - 45f, top: y - 26 / 2 + 20f, width: 56f, height: 26f);
        SubtractButton.OnLeftClick += (evt, listeningElement) =>
        {
            if (DraedonUISystem.DraeDashOrder.ContainsKey(itemName))
            {
                orderCost -= cost;
                if (DraedonUISystem.DraeDashOrder[itemName] == 1)
                {
                    DraedonUISystem.DraeDashOrder.Remove(itemName);
                }
                else
                {
                    DraedonUISystem.DraeDashOrder[itemName] -= quantityPerOrder;
                }
            }
        };
        panel.Append(SubtractButton);
    }
}
internal class ItemOrderCount : UIElement
{
    internal string itemToCount;
    internal float drawX;
    internal float drawY;
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        CalculatedStyle innerDimensions = GetInnerDimensions();
        // Getting top left position of this UIElement
        string orderNum;
        if (DraedonUISystem.DraeDashOrder.ContainsKey(itemToCount))
            orderNum = Convert.ToString(DraedonUISystem.DraeDashOrder[itemToCount]);
        else
            orderNum = "0";
        Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.ItemStack.Value, orderNum, drawX, drawY, Color.DarkCyan, Color.Black, Vector2.Zero, 1f);
    }
}
internal class PlutusVault : UIState
{
    public MouseBlockingUIPanel panel;
    public static List<int> coins =
    [
        0,
        0,
        0,
        0,
    ];
    public override void OnInitialize()
    {
        panel = new();
        panel.Width.Set(Main.screenWidth * 1.5f, 0);
        panel.Height.Set(Main.screenHeight * 1.5f, 0);
        panel.HAlign = panel.VAlign = 0.5f;
        Append(panel);

        Asset<Texture2D> BackgroundTexture = ModContent.Request<Texture2D>($"{nameof(Windfall)}/Assets/UI/DraedonsDatabase/DraedonUIBG");
        UIImage Background = new(BackgroundTexture);
        SetRectangle(Background, left: -125f, top: -125f, width: 400f, height: 518f);
        panel.Append(Background);

        Asset<Texture2D> CloseButtonTexture = ModContent.Request<Texture2D>($"{nameof(Windfall)}/Assets/UI/DraedonsDatabase/DraedonUIClose");
        UIButton CloseButton = new(CloseButtonTexture, "Close Database");
        SetRectangle(CloseButton, left: 1270f, top: 115f, width: 26f, height: 26f);
        CloseButton.OnLeftClick += (evt, listeningElement) =>
        {
            ModContent.GetInstance<DraedonUISystem>().OpenDraedonDatabase();
        };
        panel.Append(CloseButton);

        for (int i = 0; i < 4; i++)
        {
            CreatePlutusVaultCoinUI(i + 71, i, 0);
        }

        Asset<Texture2D> CheckButtonTexture = ModContent.Request<Texture2D>($"{nameof(Windfall)}/Assets/UI/DraedonsDatabase/DraedonUICheckmark");
        UIButton CheckButton = new(CheckButtonTexture, "Transfer to Credits");
        SetRectangle(CheckButton, left: 800f, top: 250, width: 26f, height: 26f);
        CheckButton.OnLeftClick += (evt, listeningElement) =>
        {
            int index = WorldSaveSystem.CreditDataNames.FindIndex(n => n == Main.LocalPlayer.name);
            int creditsToAdd = 0;
            creditsToAdd += coins[0];
            creditsToAdd += coins[1] * 100;
            creditsToAdd += coins[2] * 100 * 100;
            creditsToAdd += coins[3] * 100 * 100 * 100;
            Main.LocalPlayer.BuyItem(creditsToAdd);
            WorldSaveSystem.CreditDataCredits[index] += creditsToAdd;
            for (int i = 0; i < 4; i++)
                coins[i] = 0;
            creditsToAdd = 0;
        };
        panel.Append(CheckButton);

        CreditCount creditCount = new()
        {
            drawX = 1100f,
            drawY = 130f,
        };
        panel.Append(creditCount);

        UIText header = new("Plutus' Vault")
        {
            HAlign = 0.5f  // 1
        };
        header.Top.Set(115, 0); // 2
        panel.Append(header);

        UIText text = new("In need of some Credits?\nConvert your coins to Credits and safely store them here!")
        {
            HAlign = 0.5f, // 1
            VAlign = 0.5f // 1
        };
        panel.Append(text);
    }
    private void SetRectangle(UIElement uiElement, float left, float top, float width, float height)
    {
        uiElement.Left.Set(left, 0f);
        uiElement.Top.Set(top, 0f);
        uiElement.Width.Set(width, 0f);
        uiElement.Height.Set(height, 0f);
    }
    private void CreatePlutusVaultCoinUI(int itemType, float x, float y)
    {
        x = Main.screenWidth / 2 - 400 + 150 * x;
        y = Main.screenHeight / 2 - 200 + 100 * y;

        Asset<Texture2D> IconBackgroundTexture = ModContent.Request<Texture2D>($"{nameof(Windfall)}/Assets/UI/DraedonsDatabase/DraedonUIIconBG");
        UIImage IconBackground = new(IconBackgroundTexture);
        SetRectangle(IconBackground, left: x - 52 / 2, top: y - 52 / 2 - 15f, width: 52, height: 52);
        panel.Append(IconBackground);

        Main.GetItemDrawFrame(itemType, out Texture2D ItemTexture, out Rectangle itemFrame);
        UIImage ItemIcon = new(ItemTexture);
        SetRectangle(ItemIcon, left: x - ItemTexture.Width / 2, top: y - ItemTexture.Height / 2 - 15f, width: ItemTexture.Width, height: ItemTexture.Height);
        panel.Append(ItemIcon);

        CoinCount coinCount = new()
        {
            itemType = itemType,
            drawX = x - 2f,
            drawY = y + 20f,
        };
        panel.Append(coinCount);

        Asset<Texture2D> AddButtonTexture = ModContent.Request<Texture2D>($"{nameof(Windfall)}/Assets/UI/DraedonsDatabase/DraedonUIArrowRight");
        UIButton AddButton = new(AddButtonTexture, "+1");
        SetRectangle(AddButton, left: x - 56 / 2 + 45f, top: y - 26 / 2 + 20f, width: 56f, height: 26f);
        AddButton.OnLeftClick += (evt, listeningElement) =>
        {
            int creditsToAdd = 0;
            creditsToAdd += coins[0];
            creditsToAdd += coins[1] * 100;
            creditsToAdd += coins[2] * 100 * 100;
            creditsToAdd += coins[3] * 100 * 100 * 100;
            creditsToAdd += (int)Math.Pow(100, itemType - 71);
            if (Main.LocalPlayer.CanAfford(creditsToAdd))
                coins[itemType - 71]++;
        };
        panel.Append(AddButton);

        Asset<Texture2D> SubtractButtonTexture = ModContent.Request<Texture2D>($"{nameof(Windfall)}/Assets/UI/DraedonsDatabase/DraedonUIArrowLeft");
        UIButton SubtractButton = new(SubtractButtonTexture, "-1");
        SetRectangle(SubtractButton, left: x - 56 / 2 - 45f, top: y - 26 / 2 + 20f, width: 56f, height: 26f);
        SubtractButton.OnLeftClick += (evt, listeningElement) =>
        {
            if (coins[itemType - 71] != 0)
            {
                coins[itemType - 71]--;
            }
        };
        panel.Append(SubtractButton);
    }
}
internal class CreditCount : UIElement
{
    internal float drawX;
    internal float drawY;
    string creditNum;
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        int index = WorldSaveSystem.CreditDataNames.FindIndex(n => n == Main.LocalPlayer.name); CalculatedStyle innerDimensions = GetInnerDimensions();
        // Getting top left position of this UIElement
        if (index != -1)
            creditNum = Convert.ToString(WorldSaveSystem.CreditDataCredits[index]);
        else
            creditNum = "Nuh uh!";
        creditNum = "Credits: " + creditNum;
        Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.ItemStack.Value, creditNum, drawX, drawY, Color.DarkCyan, Color.Black, Vector2.Zero, 1f);
    }
}
internal class CoinCount : UIElement
{
    internal int itemType;
    internal float drawX;
    internal float drawY;
    string coinNum;
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        CalculatedStyle innerDimensions = GetInnerDimensions();
        // Getting top left position of this UIElement
        coinNum = Convert.ToString(PlutusVault.coins[itemType - 71]);
        Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.ItemStack.Value, coinNum, drawX, drawY, Color.DarkCyan, Color.Black, Vector2.Zero, 1f);
    }
}
internal class OrderEnRoute : UIState
{
    public MouseBlockingUIPanel panel;
    public override void OnInitialize()
    {
        panel = new();
        panel.Width.Set(Main.screenWidth * 1.5f, 0);
        panel.Height.Set(Main.screenHeight * 1.5f, 0);
        panel.HAlign = panel.VAlign = 0.5f;
        Append(panel);

        Asset<Texture2D> BackgroundTexture = ModContent.Request<Texture2D>($"{nameof(Windfall)}/Assets/UI/DraedonsDatabase/DraedonUIBG");
        UIImage Background = new(BackgroundTexture);
        SetRectangle(Background, left: -125f, top: -125f, width: 56f, height: 56);
        panel.Append(Background);

        Asset<Texture2D> CloseButtonTexture = ModContent.Request<Texture2D>($"{nameof(Windfall)}/Assets/UI/DraedonsDatabase/DraedonUIClose");
        UIButton CloseButton = new(CloseButtonTexture, "Close Drae-Dash");
        SetRectangle(CloseButton, left: 1270f, top: 115f, width: 56f, height: 26f);
        CloseButton.OnLeftClick += (evt, listeningElement) =>
        {
            ModContent.GetInstance<DraedonUISystem>().OpenDraedonDatabase();
        };
        panel.Append(CloseButton);

        UIText header = new("Drae-Dash")
        {
            HAlign = 0.5f  // 1
        };
        header.Top.Set(115, 0); // 2
        panel.Append(header);
        CreditCount creditCount = new()
        {
            drawX = 1100f,
            drawY = 130f,
        };
        panel.Append(creditCount);

        UIText text = new("Your order is on the way!\nPlease be patient!")
        {
            HAlign = 0.5f, // 1
            VAlign = 0.5f // 1
        };
        panel.Append(text);
    }
    private void SetRectangle(UIElement uiElement, float left, float top, float width, float height)
    {
        uiElement.Left.Set(left, 0f);
        uiElement.Top.Set(top, 0f);
        uiElement.Width.Set(width, 0f);
        uiElement.Height.Set(height, 0f);
    }
}
*/