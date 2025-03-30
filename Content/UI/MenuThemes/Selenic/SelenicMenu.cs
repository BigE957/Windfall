namespace Windfall.Content.UI.MenuThemes.Selenic;
    public class SelenicMenu : ModMenu
    {
        private int counter = 0;

        public override string DisplayName => "Selenic";

        public override Asset<Texture2D> Logo => ModContent.Request<Texture2D>("Windfall/Content/UI/MenuThemes/Selenic/WindfallLogoWhite");
        public override Asset<Texture2D> SunTexture => ModContent.Request<Texture2D>("CalamityMod/Backgrounds/BlankPixel");
        public override Asset<Texture2D> MoonTexture => ModContent.Request<Texture2D>("CalamityMod/Backgrounds/BlankPixel");

        public override int Music => MusicLoader.GetMusicSlot(WindfallMod.Instance, "Assets/Music/ItsRainingSomewhereElse");

        public override ModSurfaceBackgroundStyle MenuBackgroundStyle => ModContent.GetInstance<NullSurfaceBackground>();

        public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
        {
            Vector2 resolutionScale = new(Main.screenWidth / 1366f, Main.screenHeight / 768);

            #region Orator Background Stuffs
            Color oratorGreen = new(117, 255, 159);
            spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(0, 0, Main.screenWidth * 2, Main.screenHeight * 2), Color.Black);

            #region Stars
            for (int i = 0; i < Main.star.Length; i++)
            {
                Star star = Main.star[i];
                if (star == null)
                    continue;

                Texture2D t2D = TextureAssets.Star[star.type].Value;
                Vector2 origin = new(t2D.Width * 0.5f, t2D.Height * 0.5f);
                Vector2 pos = new(star.position.X + origin.X, star.position.Y + origin.Y);
                spriteBatch.Draw(t2D, pos, new Rectangle(0, 0, t2D.Width, t2D.Height), Color.Yellow * star.twinkle, star.rotation, origin, star.scale * star.twinkle, SpriteEffects.None, 0f);
            }
            #endregion

            #region Back Clouds

            float cloudHeight = Main.screenHeight / -3.5f + (Main.screenHeight * 1.25f);
            float cloudScale = 0.5f * resolutionScale.X;

            Texture2D CloudBack = (Texture2D)ModContent.Request<Texture2D>("Windfall/Assets/Skies/OratorCloudsBack", AssetRequestMode.ImmediateLoad);
            float backgroundOffset = GetBackgroundOffset(0.1f);
            spriteBatch.Draw(CloudBack, new Vector2(GetBoundedX(backgroundOffset,                                  CloudBack.Width * cloudScale), cloudHeight - (450 * cloudScale)), CloudBack.Frame(), Color.Gray, 0f, new Vector2(CloudBack.Width / 2, CloudBack.Height / 2), cloudScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(CloudBack, new Vector2(GetBoundedX(backgroundOffset - (CloudBack.Width * cloudScale), CloudBack.Width * cloudScale), cloudHeight - (450 * cloudScale)), CloudBack.Frame(), Color.Gray, 0f, new Vector2(CloudBack.Width / 2, CloudBack.Height / 2), cloudScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(CloudBack, new Vector2(GetBoundedX(backgroundOffset + (CloudBack.Width * cloudScale), CloudBack.Width * cloudScale), cloudHeight - (450 * cloudScale)), CloudBack.Frame(), Color.Gray, 0f, new Vector2(CloudBack.Width / 2, CloudBack.Height / 2), cloudScale, SpriteEffects.None, 0f);
            #endregion

            #region Moon
            Vector2 position = new(Main.screenWidth / 2f, Main.screenHeight / 1.8f);
            Texture2D bloomCircle = ModContent.Request<Texture2D>("CalamityMod/Particles/LargeBloom", AssetRequestMode.ImmediateLoad).Value;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
            spriteBatch.Draw(bloomCircle, position, null, Color.AliceBlue * ((float)(Math.Sin(counter / 20D) / 4f + 0.75f)), 0f, bloomCircle.Size() / 2f, 2.4f * resolutionScale.X, SpriteEffects.None, 0f);

            Texture2D bloomRing = ModContent.Request<Texture2D>("Windfall/Assets/Skies/OratorMoonBloom2", AssetRequestMode.ImmediateLoad).Value;
            Vector2 halfSizeTexture = new(bloomRing.Width / 2, bloomRing.Height / 2);
            spriteBatch.Draw(bloomRing, position, null, Color.DarkOliveGreen, counter / 100f, halfSizeTexture, 1.4375f * resolutionScale.X, SpriteEffects.None, 0f);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

            Texture2D moon = ModContent.Request<Texture2D>("Windfall/Assets/Skies/OratorMoon", AssetRequestMode.ImmediateLoad).Value;
            halfSizeTexture = new(moon.Width / 2, moon.Height / 2);
            spriteBatch.Draw(moon, position, null, Color.White, 0f, halfSizeTexture, 1.4f * resolutionScale.X, SpriteEffects.None, 0f);
            #endregion

            #region Front Clouds       
            /*
            Texture2D CloudMiddle = (Texture2D)ModContent.Request<Texture2D>("Windfall/Assets/Skies/OratorCloudsMiddle", AssetRequestMode.ImmediateLoad);
            backgroundOffset = GetBackgroundOffset(0.2f, CloudMiddle);
            spriteBatch.Draw(CloudMiddle, new Vector2(GetBoundedX(backgroundOffset, cloudScale), cloudHeight), CloudMiddle.Frame(), Color.Gray, 0f, new Vector2(CloudMiddle.Width / 2, CloudMiddle.Height / 2), cloudScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(CloudMiddle, new Vector2(GetBoundedX((backgroundOffset - (CloudMiddle.Width * (cloudScale))), cloudScale), cloudHeight), CloudMiddle.Frame(), Color.Gray, 0f, new Vector2(CloudMiddle.Width / 2, CloudMiddle.Height / 2), cloudScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(CloudMiddle, new Vector2(GetBoundedX((backgroundOffset + (CloudMiddle.Width * (cloudScale))), cloudScale), cloudHeight), CloudMiddle.Frame(), Color.Gray, 0f, new Vector2(CloudMiddle.Width / 2, CloudMiddle.Height / 2), cloudScale, SpriteEffects.None, 0f);
            */
            Texture2D CloudFront = (Texture2D)ModContent.Request<Texture2D>("Windfall/Assets/Skies/OratorCloudsFront", AssetRequestMode.ImmediateLoad);
            backgroundOffset = GetBackgroundOffset(0.3f);
            cloudHeight = Main.screenHeight / -3.5f + (Main.screenHeight * 1.25f);
            spriteBatch.Draw(CloudFront, new Vector2(GetBoundedX(backgroundOffset,                                   CloudFront.Width * cloudScale), cloudHeight - (250 * cloudScale)), CloudFront.Frame(), Color.Gray, 0f, new Vector2(CloudFront.Width / 2, CloudFront.Height / 2), cloudScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(CloudFront, new Vector2(GetBoundedX(backgroundOffset - (CloudFront.Width * cloudScale), CloudFront.Width * cloudScale), cloudHeight - (250 * cloudScale)), CloudFront.Frame(), Color.Gray, 0f, new Vector2(CloudFront.Width / 2, CloudFront.Height / 2), cloudScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(CloudFront, new Vector2(GetBoundedX(backgroundOffset + (CloudFront.Width * cloudScale), CloudFront.Width * cloudScale), cloudHeight - (250 * cloudScale)), CloudFront.Frame(), Color.Gray, 0f, new Vector2(CloudFront.Width / 2, CloudFront.Height / 2), cloudScale, SpriteEffects.None, 0f);
            #endregion

            counter++;
            #endregion

            drawColor = Color.White;
            Main.time = 27000;
            Main.dayTime = true;

            Vector2 drawPos = new (Main.screenWidth / 2f, 100f);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
            spriteBatch.Draw(Logo.Value, drawPos, null, Color.White, logoRotation, Logo.Value.Size() * 0.5f, logoScale, SpriteEffects.None, 0f);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
            return false;
        }
        private float GetBackgroundOffset(float parallaxMultiplier)
        {
            return -100000 - (counter * parallaxMultiplier);
        }
        private static float GetBoundedX(float initialX, float scaledWidth) => initialX % (scaledWidth * 3) + Main.screenWidth + (scaledWidth / 2f);

    }
