using Terraria.Graphics.Effects;
using Windfall.Content.NPCs.Bosses.TheOrator;

namespace Windfall.Content.Skies
{
    public class OratorSky : CustomSky
    {
        private bool skyActive;
        private float opacity;

        public override void Deactivate(params object[] args)
        {
            skyActive = (Main.npc.Any(n => n.active && n.type == ModContent.NPCType<TheOrator>()));
        }

        public override void Reset()
        {
            skyActive = false;
        }

        public override bool IsActive()
        {
            return skyActive || opacity > 0f;
        }

        public override void Activate(Vector2 position, params object[] args)
        {
            skyActive = true;
        }
        public override Color OnTileColor(Color inColor)
        {
            return Color.Lerp(inColor, new(117, 255, 159, inColor.A), opacity);
        }
        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            Color oratorGreen = new(117, 255, 159);           
            Texture2D moon = TextureAssets.Moon[Main.moonType].Value;
            spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(0, 0, Main.screenWidth * 2, Main.screenHeight * 2), Color.Black * opacity);
            
            if (Main.netMode != NetmodeID.Server)
            {
                int bgTop = (int)((-Main.screenPosition.Y) / (Main.worldSurface * 16.0 - 600.0) * 200.0);
                float colorMult = 0.952f * opacity;
                Color teal = Color.Teal;
                Color green = Color.Green;
                float width1 = Main.screenWidth / 500f;
                float height1 = Main.screenHeight / 600f;
                float width2 = Main.screenWidth / 600f;
                float height2 = Main.screenHeight / 800f;
                float width3 = Main.screenWidth / 200f;
                float height3 = Main.screenHeight / 900f;
                float width4 = Main.screenWidth / 1000f;
                float height4 = Main.screenHeight / 200f;
                for (int i = 0; i < Main.star.Length; i++)
                {
                    Star star = Main.star[i];
                    if (star == null)
                        continue;

                    Texture2D t2D = TextureAssets.Star[star.type].Value;
                    Vector2 origin = new Vector2(t2D.Width * 0.5f, t2D.Height * 0.5f);
                    float posX = star.position.X * width1;
                    float posY = star.position.Y * height1;
                    Vector2 position = new Vector2(posX + origin.X, posY + origin.Y + bgTop);
                    spriteBatch.Draw(t2D, position, new Rectangle(0, 0, t2D.Width, t2D.Height), oratorGreen * star.twinkle * colorMult, star.rotation, origin, (star.scale * star.twinkle) - 0.2f, SpriteEffects.None, 0f);

                    origin = new Vector2(t2D.Width * 0.2f, t2D.Height * 0.2f);
                    posX = star.position.X * width2;
                    posY = star.position.Y * height2;
                    position = new Vector2(posX + origin.X, posY + origin.Y + bgTop);
                    spriteBatch.Draw(t2D, position, new Rectangle(0, 0, t2D.Width, t2D.Height), teal * star.twinkle * colorMult, star.rotation, origin, (star.scale * star.twinkle) + 0.2f, SpriteEffects.None, 0f);

                    origin = new Vector2(t2D.Width * 0.8f, t2D.Height * 0.8f);
                    posX = star.position.X * width3;
                    posY = star.position.Y * height3;
                    position = new Vector2(posX + origin.X, posY + origin.Y + bgTop);
                    spriteBatch.Draw(t2D, position, new Rectangle(0, 0, t2D.Width, t2D.Height), green * star.twinkle * colorMult, star.rotation, origin, star.scale * star.twinkle, SpriteEffects.None, 0f);

                    origin = new Vector2(t2D.Width * 0.5f, t2D.Height * 0.5f);
                    posX = star.position.X * width4;
                    posY = star.position.Y * height4;
                    position = new Vector2(posX + origin.X, posY + origin.Y + bgTop);
                    spriteBatch.Draw(t2D, position, new Rectangle(0, 0, t2D.Width, t2D.Height), Color.White * star.twinkle * colorMult, star.rotation, origin, star.scale * star.twinkle, SpriteEffects.None, 0f);
                }
                spriteBatch.Draw(moon, new Vector2(Main.screenWidth / 2 - (moon.Width * 0.5f), 200 + bgTop), moon.Frame(1, 8), oratorGreen * opacity);
            }
        }
        public override void Update(GameTime gameTime)
        {
            if ((!Main.npc.Any(n => n.active && n.type == ModContent.NPCType<TheOrator>()) || Main.gameMenu))
                skyActive = false;

            if (skyActive && opacity < 1f)
                opacity += 0.02f;
            else if (!skyActive && opacity > 0f)
                opacity -= 0.02f;

            Opacity = opacity;
        }
    }
}
