using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.UI;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent.UI.Elements;
using WindfallAttempt1.UI.DraedonsDatabase;
using WindfallAttempt1.Items.Utility;
using Microsoft.Xna.Framework;
using System;
using Terraria.GameContent;
using Terraria.ID;
using CalamityMod.Items.Placeables.DraedonStructures;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WindfallAttempt1.Projectiles.Other;

namespace WindfallAttempt1.UI.DraedonsDatabase
{
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

            Asset<Texture2D> BackgroundTexture = ModContent.Request<Texture2D>("WindfallAttempt1/UI/DraedonsDatabase/DraedonUIBG");
            UIImage Background = new(BackgroundTexture);
            SetRectangle(Background, left: -125f, top: -125f, width: 400f, height: 518f);
            panel.Append(Background);

            Asset<Texture2D> CloseButtonTexture = ModContent.Request<Texture2D>("WindfallAttempt1/UI/DraedonsDatabase/DraedonUIClose");
            UIButton CloseButton = new(CloseButtonTexture, "Close Database");
            SetRectangle(CloseButton, left: 1270f, top: 115f, width: 26f, height: 26f);
            CloseButton.OnLeftClick += new MouseEvent(CloseUI);
            panel.Append(CloseButton);

            Asset<Texture2D> DraeDashIconTexture = ModContent.Request<Texture2D>("WindfallAttempt1/UI/DraedonsDatabase/DraedonUIDraeDashLogo");
            UIButton DraeDashIcon = new(DraeDashIconTexture, "Open Drae-Dash");
            SetRectangle(DraeDashIcon, left: 270f, top: 115f, width: 52f, height: 52f);
            DraeDashIcon.OnLeftClick += new MouseEvent(OpenDraeDash);
            panel.Append(DraeDashIcon);

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
        private void CloseUI(UIMouseEvent evt, UIElement listeningElement)
        {
            ModContent.GetInstance<DraedonUISystem>().CloseDraedonDatabase();
        }

        private void OpenDraeDash(UIMouseEvent evt, UIElement listeningElement)
        {
            ModContent.GetInstance<DraedonUISystem>().OpenDraedonApp("DraeDash");
        }
    }
    internal class DraeDash : UIState
    {
        public MouseBlockingUIPanel panel;
        internal int[,] DraeDashItems =
            {
                { ModContent.ItemType<DraedonCharger>(), 1 },

                { ModContent.ItemType<LaboratoryPlating>(), 100 },

                { ModContent.ItemType<LaboratoryPanels>(), 100 },

                { ModContent.ItemType<HazardChevronPanels>(), 100 },

                { ModContent.ItemType<LaboratoryPipePlating>(), 100 },

                { ModContent.ItemType<LaboratoryConsoleItem>(), 10 },

                { ModContent.ItemType<LaboratoryContainmentBoxItem>(), 10 },

                { ModContent.ItemType<LaboratoryDisplayItem>(), 10 },

                { ModContent.ItemType<LaboratoryElectricPanelItem>(), 10 },

                { ModContent.ItemType<LaboratoryScreenItem>(), 10 },

                { ModContent.ItemType<LaboratoryServerItem>(), 10 },

                { ModContent.ItemType<LaboratoryTerminalItem>(), 10 },

                { ModContent.ItemType<ReinforcedCrateItem>(), 10 },

                { ModContent.ItemType<LabHologramProjectorItem>(), 1 },

                { ModContent.ItemType<LaboratoryDoorItem>(), 10 },

                { ModContent.ItemType<SecurityChest>(), 1 },

                { ModContent.ItemType<PowerCellFactoryItem>(), 1 },

                { ModContent.ItemType<ChargingStationItem>(), 1 },
            };
        public override void OnInitialize()
        {
            panel = new();
            panel.Width.Set(Main.screenWidth * 1.5f, 0);
            panel.Height.Set(Main.screenHeight * 1.5f, 0);
            panel.HAlign = panel.VAlign = 0.5f;
            Append(panel);

            Asset<Texture2D> BackgroundTexture = ModContent.Request<Texture2D>("WindfallAttempt1/UI/DraedonsDatabase/DraedonUIBG");
            UIImage Background = new(BackgroundTexture);
            SetRectangle(Background, left: -125f, top: -125f, width: 56f, height: 56);
            panel.Append(Background);

            Asset<Texture2D> CloseButtonTexture = ModContent.Request<Texture2D>("WindfallAttempt1/UI/DraedonsDatabase/DraedonUIClose");
            UIButton CloseButton = new(CloseButtonTexture, "Close Drae-Dash");
            SetRectangle(CloseButton, left: 1270f, top: 115f, width: 56f, height: 26f);
            CloseButton.OnLeftClick += new MouseEvent(CloseDraeDash);
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
                CreateDraeDashItemUI(DraeDashItems[i, 0], DraeDashItems[i, 1], g, j);
                g++;
                if(g > 6)
                {
                    g = 0;
                    j++;
                }
            }
            
            Mod calamityMusic = null;
            ModLoader.TryGetMod("CalamityModMusic", out calamityMusic);
            if(calamityMusic != null)
                CreateDraeDashItemUI(calamityMusic.Find<ModItem>("BioLabMusicBox").Type, 1, g, j);

            Asset<Texture2D> BuyButtonTexture = ModContent.Request<Texture2D>("WindfallAttempt1/UI/DraedonsDatabase/DraedonUIPurchase");
            UIButton BuyButton = new(BuyButtonTexture, "Complete Order");
            SetRectangle(BuyButton, left: (Main.screenWidth / 4) * 3, top: (Main.screenHeight / 4) * 3, width: 56f, height: 26f);
            BuyButton.OnLeftClick += (evt, listeningElement) =>
            {
                Player player = Main.LocalPlayer;
                Vector2 positionForVelocity;
                if (Main.rand.NextBool(2))
                    positionForVelocity = new Vector2(player.Center.X + 400f, player.Center.Y - 40f);
                else
                    positionForVelocity = new Vector2(player.Center.X - 400f, player.Center.Y - 40f);
                Vector2 position = new Vector2(positionForVelocity.X, positionForVelocity.Y - 4080);
                Vector2 velocity = (positionForVelocity - position).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(9f, 10f);
                Projectile.NewProjectile(null, position, velocity, ModContent.ProjectileType<DraeDashDropPod>(), 0, 0, player.whoAmI, player.Center.Y - 40f);

                ModContent.GetInstance<DraedonUISystem>().OpenDraedonDatabase();
            };
            panel.Append(BuyButton);

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
        private void CloseDraeDash(UIMouseEvent evt, UIElement listeningElement)
        {
            ModContent.GetInstance<DraedonUISystem>().OpenDraedonDatabase();
        }
        private void CreateDraeDashItemUI(int itemType, int quantityPerOrder, float x, float y)
        {
            x = ((Main.screenWidth / 2) - 400) + (150 * x);
            y = ((Main.screenHeight / 2) - 200) + (100 * y);

            Asset<Texture2D> IconBackgroundTexture = ModContent.Request<Texture2D>("WindfallAttempt1/UI/DraedonsDatabase/DraedonUIIconBG");
            UIImage IconBackground = new(IconBackgroundTexture);
            SetRectangle(IconBackground, left: x - 52 / 2, top: y - 52 / 2 - 15f, width: 52, height: 52);
            panel.Append(IconBackground);

            ModItem item = ModContent.GetModItem(itemType);
            string itemName = ItemID.Search.GetName(itemType);
            Asset<Texture2D> ItemTexture = ModContent.Request<Texture2D>(item.Texture);
            UIImage ItemIcon = new(ItemTexture);
            SetRectangle(ItemIcon, left: x - ItemTexture.Width()/2, top: (y - ItemTexture.Height()/2) - 15f, width: ItemTexture.Width(), height: ItemTexture.Height());
            panel.Append(ItemIcon);

            ItemOrderCount itemOrderCount = new()
            {
                itemToCount = itemName,
                drawX = x + 2f,
                drawY = y + 20f,
            };
            panel.Append(itemOrderCount);

            Asset<Texture2D> AddButtonTexture = ModContent.Request<Texture2D>("WindfallAttempt1/UI/DraedonsDatabase/DraedonUIArrowRight");
            UIButton AddButton = new(AddButtonTexture, "+1");
            SetRectangle(AddButton, left: (x - (56 / 2)) + 45f, top: (y - (26 / 2)) + 20f, width: 56f, height: 26f);
            AddButton.OnLeftClick += (evt, listeningElement) =>
            {
                if (DraedonUISystem.DraeDashOrder.ContainsKey(itemName))
                {
                    DraedonUISystem.DraeDashOrder[itemName] += quantityPerOrder;
                }
                else
                {
                    DraedonUISystem.DraeDashOrder.Add(itemName, quantityPerOrder);
                }
            };
            panel.Append(AddButton);

            Asset<Texture2D> SubtractButtonTexture = ModContent.Request<Texture2D>("WindfallAttempt1/UI/DraedonsDatabase/DraedonUIArrowLeft");
            UIButton SubtractButton = new(SubtractButtonTexture, "-1");
            SetRectangle(SubtractButton, left: (x - (56 / 2)) - 45f, top: (y - (26 / 2)) + 20f, width: 56f, height: 26f);
            SubtractButton.OnLeftClick += (evt, listeningElement) =>
            {
                if (DraedonUISystem.DraeDashOrder.ContainsKey(itemName))
                {
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
    public class ItemOrderCount : UIElement
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
}
