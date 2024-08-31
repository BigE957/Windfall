using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Windfall.Common.Players;
using Windfall.Content.NPCs.WorldEvents.LunarCult;

namespace Windfall.Content.UI.Events
{
    // This custom UI will show whenever the player is holding the ExampleCustomResourceWeapon item and will display the player's custom resource amounts that are tracked in ExampleResourcePlayer
    internal class ChefProgressBar : UIState
    {
        // For this bar we'll be using a frame texture and then a gradient inside bar, as it's one of the more simpler approaches while still looking decent.
        // Once this is all set up make sure to go and do the required stuff for most UI's in the ModSystem class.
        private UIElement area;
        private UIPanel barBackground;
        private Color gradientA;
        private Color gradientB;

        public override void OnInitialize()
        {
            // Create a UIElement for all the elements to sit on top of, this simplifies the numbers as nested elements can be positioned relative to the top left corner of this element. 
            // UIElement is invisible and has no padding.
            area = new UIElement();
            area.Left.Set(0, 0f); // Place the resource bar to the left of the hearts.
            area.Top.Set(0, 0f); // Placing it just a bit below the top of the screen.
            area.Width.Set(182, 0f); // We will be placing the following 2 UIElements within this 182x60 area.
            area.Height.Set(60, 0f);

            barBackground = new(); // Frame of our resource bar
            barBackground.BackgroundColor = Color.Transparent;
            barBackground.BorderColor = Color.Silver;
            barBackground.Left.Set(0, 0f);
            barBackground.Top.Set(0, 0f);
            barBackground.Width.Set(58, 0f);
            barBackground.Height.Set(18, 0f);

            gradientA = Color.DarkGreen;
            gradientB = Color.LimeGreen;

            area.Append(barBackground);
            Append(area);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!NPC.AnyNPCs(ModContent.NPCType<TheChef>()))
                return;

            NPC chef = Main.npc.First(n => n.active && n.type == ModContent.NPCType<TheChef>());

            if (chef.ai[3] == -1)
                return;

            base.Draw(spriteBatch);
        }

        // Here we draw our UI
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            NPC chef = Main.npc.First(n => n.active && n.type == ModContent.NPCType<TheChef>());
            // Calculate quotient
            float quotient = chef.ai[2] / 120; // Creating a quotient that represents the difference of your currentResource vs your maximumResource, resulting in a float of 0-1f.
            quotient = Utils.Clamp(quotient, 0f, 1f); // Clamping it to 0-1f so it doesn't go over that.

            // Here we get the screen dimensions of the barFrame element, then tweak the resulting rectangle to arrive at a rectangle within the barFrame texture that we will draw the gradient. These values were measured in a drawing program.
            Rectangle hitbox = barBackground.GetInnerDimensions().ToRectangle();
            hitbox.X -= 8;
            hitbox.Width += 17;
            hitbox.Y -= 10;
            hitbox.Height += 20;

            // Now, using this hitbox, we draw a gradient by drawing vertical lines while slowly interpolating between the 2 colors.
            int left = hitbox.Left;
            int right = hitbox.Right;
            int steps = (int)((right - left) * quotient);
            for (int i = 0; i < steps; i++)
            {
                float percent = (float)i / steps; // Alternate Gradient Approach
                //float percent = (float)i / (right - left);
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(left + i, hitbox.Y, 1, hitbox.Height), Color.Lerp(gradientA, gradientB, percent));
            }
            if (quotient != 1f)
            {
                for (int i = steps; i < right - left; i++)
                {
                    spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(left + i, hitbox.Y, 1, hitbox.Height), Color.Black);
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (!NPC.AnyNPCs(ModContent.NPCType<TheChef>()))
                return;

            NPC chef = Main.npc.First(n => n.active && n.type == ModContent.NPCType<TheChef>());

            if (chef.ai[3] == -1)
                return;

            area.Left.Pixels = chef.Center.ToScreenPosition().X - 30;
            area.Top.Pixels = chef.Center.ToScreenPosition().Y + 40;

            base.Update(gameTime);
        }
    }
    internal class ChefProgressUISystem : ModSystem
    {
        private UserInterface ChefUI;
        internal ChefProgressBar ChefProgressBar;


        public override void Load()
        {
            ChefProgressBar = new();
            ChefUI = new();
            ChefUI.SetState(ChefProgressBar);
        }

        public override void UpdateUI(GameTime gameTime)
        {
            ChefUI?.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "Windfall: Chef Progress Bar",
                    delegate
                    {
                        ChefUI.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
}