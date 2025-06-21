using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.NPCs.WorldEvents.LunarCult;

namespace Windfall.Content.UI.Activities;
internal class TableOrderUI(Dictionary<int, int> order, int tableID) : UIState
{
    UIPanel OrderPanel;
    UIPanel Hitbox;
    internal Dictionary<int, int> Order = order;
    internal int[] TypeCounts = new int[3];
    private readonly List<(UIItem Icon, UIText Amt)> OrderDisplay = [];
    private readonly int TableID = tableID;
    float Opacity = 0f;

    public override void OnInitialize()
    {
        Hitbox = new UIPanel();
        Hitbox.SetRectangle(0, 0, 120, 160);
        Hitbox.BackgroundColor = Color.Transparent;
        Hitbox.BorderColor = Color.Transparent;

        OrderPanel = new UIPanel
        {
            HAlign = 0.5f,
            VAlign = 0f
        };
        OrderPanel.SetRectangle(0, 0, 90, 64);
        OrderPanel.BackgroundColor = Color.Transparent;
        OrderPanel.BorderColor = Color.Transparent;

        Hitbox.Append(OrderPanel);
        Append(Hitbox);
    }

    public override void OnActivate()
    {
        OrderPanel.BackgroundColor = Color.Transparent;
        OrderPanel.BorderColor = Color.Transparent;
        OrderPanel.RemoveAllChildren();
        OrderDisplay.Clear();

        if (Order != null)
        {
            OrderPanel.Height.Pixels = 8 + (32 * TypeCounts.Where(i => i > 0).Count());
            OrderPanel.Top.Pixels = -OrderPanel.Height.Pixels / 2f;
            OrderPanel.Top.Pixels += 32;

            int typeIndex = 0;
            int count = 0;
            float accumulatedWidth = 0f;
            float[] typeWidths = new float[3];

            foreach (var (FoodID, Count) in Order)
            {
                UIItem icon = new(FoodID);
                float offset = 10 - (icon.Width.Pixels / 2f);
                icon.SetRectangle(offset + accumulatedWidth, 4 + (32 * typeIndex) - (icon.Height.Pixels / 2f), icon.Width.Pixels, icon.Height.Pixels);
                icon.shouldDisplay = () => Opacity >= 0.75f;
                icon.Color = Color.Transparent;
                accumulatedWidth += offset + icon.Width.Pixels;

                UIText amt = null;
                if (Count > 1)
                {
                    amt = new("x" + Count);
                    amt.SetRectangle(6 + accumulatedWidth, 4 + (32 * typeIndex) - (icon.Height.Pixels / 3f), amt.Width.Pixels, amt.Height.Pixels);
                    amt.ShadowColor = Color.Transparent;
                    amt.TextColor = Color.Transparent;
                    accumulatedWidth += 6 + FontAssets.MouseText.Value.MeasureString(amt.Text).X;
                }

                accumulatedWidth += 12;

                OrderDisplay.Add((icon, amt));
                count++;
                if(count >= TypeCounts[typeIndex])
                {
                    count = 0;
                    typeWidths[typeIndex] = accumulatedWidth;
                    accumulatedWidth = 0;
                    typeIndex++;
                }
            }

            foreach (var (Icon, Amt) in OrderDisplay)
            {
                OrderPanel.Append(Icon);
                if (Amt != null)
                    OrderPanel.Append(Amt);
            }

            OrderPanel.Width.Pixels = typeWidths.Max() + 8;
        }

        Vector2 tableScreenPos = LunarCultBaseSystem.CafeteriaTables[TableID].ToWorldCoordinates().ToScreenPosition(); 

        Hitbox.Left.Pixels = tableScreenPos.X - Hitbox.Width.Pixels / 2f;
        Hitbox.Top.Pixels = tableScreenPos.Y - Hitbox.Height.Pixels / 2f;
    }

    public override void Update(GameTime gameTime)
    {
        Vector2 tableWorldCoords = LunarCultBaseSystem.CafeteriaTables[TableID].ToWorldCoordinates();
        Vector2 tableScreenPos = (tableWorldCoords - Main.LocalPlayer.velocity).ToScreenPosition();
        Hitbox.Left.Pixels = tableScreenPos.X - Hitbox.Width.Pixels / 2f;
        Hitbox.Top.Pixels = tableScreenPos.Y - Hitbox.Height.Pixels / 2f;

        float dist = (Math.Abs(tableWorldCoords.X - Main.LocalPlayer.Center.X) - 32) / 40f;
        Opacity = 1 - Clamp(dist, 0f, 1f);

        OrderPanel.BorderColor = Color.Lerp(Color.Transparent, Color.Black, Opacity);
        OrderPanel.BackgroundColor = Color.Lerp(Color.Transparent, (new Color(63, 82, 151) * 0.7f), Opacity);

        foreach(var (Icon, Amt) in OrderDisplay)
        {
            Icon.Color = Color.Lerp(Color.Transparent, Color.White, Opacity);
            if (Amt != null)
            {
                Amt.ShadowColor = Color.Lerp(Color.Transparent, Color.Black, Opacity);
                Amt.TextColor = Color.Lerp(Color.Transparent, Color.White, Opacity);
            }
        }

        if(Opacity >= 0.75f && Hitbox.IsMouseHovering)
            OrderPanel.BorderColor = Color.Lerp(Color.Transparent, Color.Yellow, Opacity);
    }

    public override void RightClick(UIMouseEvent evt)
    {
        if (Opacity >= 0.75f && Hitbox.IsMouseHovering)
        {
            //Phase 1: Check if the player has all of the required Items in the correct amounts
            int[] itemCounts = new int[Order.Count];
            var orderArray = Order.ToArray();

            for(int i = 0; i < Main.LocalPlayer.inventory.Length; i++)
            {
                Item item = Main.LocalPlayer.inventory[i];
                for (int j = 0; j < Order.Count; j++)
                {
                    if (item.type == orderArray[j].Key)
                        itemCounts[j] += item.stack;
                }
            }
            bool canAfford = true;
            for (int i = 0; i < itemCounts.Length; i++)
            {
                if(itemCounts[i] < orderArray[i].Value)
                {
                    canAfford = false;
                    break;
                }
                itemCounts[i] = orderArray[i].Value;
            }

            //Phase 2: If the player can afford the cost, remove the correct amount of items to match the amounts of the order
            if (canAfford)
            {
                for (int i = 0; i < Main.LocalPlayer.inventory.Length; i++)
                {
                    Item item = Main.LocalPlayer.inventory[i];
                    int zeroes = 0;
                    int j = 0;
                    foreach(var pair in Order)
                    {
                        if (item.type == pair.Key && itemCounts[j] > 0)
                        {
                            while (itemCounts[j] > 0 && item.stack > 0)
                            {
                                item.stack--;
                                itemCounts[j]--;
                            }
                        }
                        else if (itemCounts[j] == 0)
                            zeroes++;
                        j++;
                    }
                    if (zeroes == Main.LocalPlayer.inventory.Length)
                        break;
                }

                ModContent.GetInstance<TableOrderUISystem>().DeactivateTableOrderUI(TableID);

                foreach (NPC npc in Main.npc.Where(n => n.active && n.IsSelenicCultist() && n.ai[2] == 2 && ((int)n.ai[3]) == LunarCultBaseSystem.SeatedTables[TableID].Value.PartyID))
                {
                    CombatText.NewText(npc.Hitbox, Color.White, GetWindfallTextValue("Dialogue.LunarCult.LunarBishop.Cafeteria.Thanks." + Main.rand.Next(3)));
                    npc.ai[3] = -1;
                    ++LunarCultBaseSystem.SatisfiedCustomers;
                }

                LunarCultBaseSystem.SeatedTables[TableID] = null;
            }
            else
            {
                NPC npc = Main.npc.Where(n => n.active && n.IsSelenicCultist() && n.ai[2] == 2 && ((int)n.ai[3]) == LunarCultBaseSystem.SeatedTables[TableID].Value.PartyID).ToArray()[0];
                CombatText.NewText(npc.Hitbox, Color.White, GetWindfallTextValue("Dialogue.LunarCult.LunarBishop.Cafeteria.Where." + Main.rand.Next(3)));
            }
        }
    }
}

public class TableOrderUISystem : ModSystem
{
    internal TableOrderUI[] TableOrderUIs = new TableOrderUI[3];
    private UserInterface[] tableUIs = new UserInterface[3];
    public bool uiOpen = false;

    public void ActivateTableOrderUI(int tableID, Dictionary<int, int> order, int[] typeCounts)
    {
        uiOpen = true;
        TableOrderUIs[tableID].Order = order;
        TableOrderUIs[tableID].TypeCounts[0] = typeCounts[0];
        TableOrderUIs[tableID].TypeCounts[1] = typeCounts[1];
        TableOrderUIs[tableID].TypeCounts[2] = typeCounts[2];
        //TableOrderUIs[tableID].OnInitialize();
        TableOrderUIs[tableID].Activate();
        tableUIs[tableID]?.SetState(TableOrderUIs[tableID]);
    }

    public void DeactivateTableOrderUI(int tableID)
    {
        uiOpen = false;
        for(int i = 0; i < 3; i++)
            TableOrderUIs[tableID].TypeCounts[0] = 0;
        TableOrderUIs[tableID].Order = null;
        tableUIs[tableID]?.SetState(null);
    }
    public override void OnModLoad()
    {
        // Create custom interface which can swap between different UIStates
        if (!Main.dedServ)
        {
            tableUIs = [new UserInterface(), new UserInterface(), new UserInterface()];
            // Creating custom UIState

            TableOrderUIs = [new TableOrderUI(null, 0), new TableOrderUI(null, 1), new TableOrderUI(null, 2)];

            // Activate calls Initialize() on the UIState if not initialized, then calls OnActivate and then calls Activate on every child element
            foreach(var TableOrderUI in TableOrderUIs)
                TableOrderUI.Activate();
        }
    }
    private GameTime _lastUpdateUiGameTime;

    public override void UpdateUI(GameTime gameTime)
    {
        _lastUpdateUiGameTime = gameTime;
        foreach(var tableUI in  tableUIs)
            if (tableUI?.CurrentState != null)
                tableUI?.Update(gameTime);
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
                    foreach (var tableUI in tableUIs)
                        tableUI.Draw(Main.spriteBatch, new GameTime());
                    return true;
                },
                InterfaceScaleType.UI)
            );
        }
    }
}
