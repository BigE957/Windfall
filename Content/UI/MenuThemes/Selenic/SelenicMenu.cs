﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.MainMenu;
using Windfall.Content.NPCs.Bosses.Orator;
using Windfall.Content.UI.BossBars;

namespace Windfall.Content.UI.MenuThemes.Selenic
{
    public class SelenicMenu : ModMenu
    {
        private int counter = 0;

        public override string DisplayName => "Selenic";

        public override Asset<Texture2D> Logo => ModContent.Request<Texture2D>("Windfall/Content/UI/MenuThemes/Selenic/WindfallLogoWhite");
        public override Asset<Texture2D> SunTexture => ModContent.Request<Texture2D>("CalamityMod/Backgrounds/BlankPixel");
        public override Asset<Texture2D> MoonTexture => ModContent.Request<Texture2D>("CalamityMod/Backgrounds/BlankPixel");

        public override int Music => MusicLoader.GetMusicSlot(Windfall.Instance, "Assets/Music/ItsRainingSomewhereElse");

        public override ModSurfaceBackgroundStyle MenuBackgroundStyle => ModContent.GetInstance<NullSurfaceBackground>();

        public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
        {
            #region Orator Background Stuffs
            Color oratorGreen = new(117, 255, 159);
            spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(0, 0, Main.screenWidth * 2, Main.screenHeight * 2), Color.Black);

            float bgTop = Main.screenHeight / -6f;

            #region Stars
            for (int i = 0; i < Main.star.Length; i++)
            {
                Star star = Main.star[i];
                if (star == null)
                    continue;

                Texture2D t2D = TextureAssets.Star[star.type].Value;
                Vector2 origin = new(t2D.Width * 0.5f, t2D.Height * 0.5f);
                Vector2 pos = new(star.position.X + origin.X, star.position.Y + origin.Y + bgTop);
                spriteBatch.Draw(t2D, pos, new Rectangle(0, 0, t2D.Width, t2D.Height), Color.Yellow * star.twinkle, star.rotation, origin, star.scale * star.twinkle, SpriteEffects.None, 0f);
            }
            #endregion

            //spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(0, 0, Main.screenWidth * 2, Main.screenHeight * 2), Color.Black * (opacity / 2f));

            Vector2 position = new(Main.screenWidth / 2, bgTop + Main.screenHeight / 1.5f);
            Texture2D bloomCircle = ModContent.Request<Texture2D>("CalamityMod/Particles/LargeBloom", AssetRequestMode.ImmediateLoad).Value;

            spriteBatch.UseBlendState(BlendState.Additive);
            Vector2 halfSizeTexture = new(bloomCircle.Width / 2, bloomCircle.Height / 2);
            spriteBatch.Draw(bloomCircle, position, null, oratorGreen * ((float)(Math.Sin(counter / 20D) / 4f + 0.75f)), 0f, bloomCircle.Size() / 2f, 2.5f + 0.3f, SpriteEffects.None, 0f);

            Texture2D bloomRing = ModContent.Request<Texture2D>("Windfall/Assets/Skies/OratorMoonBloom2", AssetRequestMode.ImmediateLoad).Value;
            halfSizeTexture = new(bloomRing.Width / 2, bloomRing.Height / 2);
            spriteBatch.Draw(bloomRing, position, null, Color.LightCyan, counter / 100f, halfSizeTexture, 1.5f + 0.1375f, SpriteEffects.None, 0f);

            spriteBatch.UseBlendState(BlendState.NonPremultiplied);
            Texture2D moon = ModContent.Request<Texture2D>("Windfall/Assets/Skies/OratorMoon", AssetRequestMode.ImmediateLoad).Value;
            halfSizeTexture = new(moon.Width / 2, moon.Height / 2);
            spriteBatch.Draw(moon, position, null, Color.White, 0f, halfSizeTexture, 1.5f + 0.125f, SpriteEffects.None, 0f);

            #region Clouds              
            float cloudHeight = bgTop * 2 + (Main.screenHeight * 1.25f);
            float cloudScale = 0.45f;

            Texture2D CloudBack = (Texture2D)ModContent.Request<Texture2D>("Windfall/Assets/Skies/OratorCloudsBack", AssetRequestMode.ImmediateLoad);
            float backgroundOffset = GetBackgroundOffset(0.1f, CloudBack);
            spriteBatch.Draw(CloudBack, new Vector2(GetBoundedX(backgroundOffset, cloudScale), cloudHeight), CloudBack.Frame(), Color.Gray, 0f, new Vector2(CloudBack.Width / 2, CloudBack.Height / 2), cloudScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(CloudBack, new Vector2(GetBoundedX((backgroundOffset - (CloudBack.Width * (cloudScale))), cloudScale), cloudHeight), CloudBack.Frame(), Color.Gray, 0f, new Vector2(CloudBack.Width / 2, CloudBack.Height / 2), cloudScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(CloudBack, new Vector2(GetBoundedX((backgroundOffset + (CloudBack.Width * (cloudScale))), cloudScale), cloudHeight), CloudBack.Frame(), Color.Gray, 0f, new Vector2(CloudBack.Width / 2, CloudBack.Height / 2), cloudScale, SpriteEffects.None, 0f);

            Texture2D CloudMiddle = (Texture2D)ModContent.Request<Texture2D>("Windfall/Assets/Skies/OratorCloudsMiddle", AssetRequestMode.ImmediateLoad);
            backgroundOffset = GetBackgroundOffset(0.2f, CloudMiddle);
            spriteBatch.Draw(CloudMiddle, new Vector2(GetBoundedX(backgroundOffset, cloudScale), cloudHeight), CloudMiddle.Frame(), Color.Gray, 0f, new Vector2(CloudMiddle.Width / 2, CloudMiddle.Height / 2), cloudScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(CloudMiddle, new Vector2(GetBoundedX((backgroundOffset - (CloudMiddle.Width * (cloudScale))), cloudScale), cloudHeight), CloudMiddle.Frame(), Color.Gray, 0f, new Vector2(CloudMiddle.Width / 2, CloudMiddle.Height / 2), cloudScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(CloudMiddle, new Vector2(GetBoundedX((backgroundOffset + (CloudMiddle.Width * (cloudScale))), cloudScale), cloudHeight), CloudMiddle.Frame(), Color.Gray, 0f, new Vector2(CloudMiddle.Width / 2, CloudMiddle.Height / 2), cloudScale, SpriteEffects.None, 0f);

            Texture2D CloudFront = (Texture2D)ModContent.Request<Texture2D>("Windfall/Assets/Skies/OratorCloudsFront", AssetRequestMode.ImmediateLoad);
            backgroundOffset = GetBackgroundOffset(0.3f, CloudFront);
            spriteBatch.Draw(CloudFront, new Vector2(GetBoundedX(backgroundOffset, cloudScale), cloudHeight), CloudFront.Frame(), Color.Gray, 0f, new Vector2(CloudFront.Width / 2, CloudFront.Height / 2), cloudScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(CloudFront, new Vector2(GetBoundedX((backgroundOffset - (CloudFront.Width * (cloudScale))), cloudScale), cloudHeight), CloudFront.Frame(), Color.Gray, 0f, new Vector2(CloudFront.Width / 2, CloudFront.Height / 2), cloudScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(CloudFront, new Vector2(GetBoundedX((backgroundOffset + (CloudFront.Width * (cloudScale))), cloudScale), cloudHeight), CloudFront.Frame(), Color.Gray, 0f, new Vector2(CloudFront.Width / 2, CloudFront.Height / 2), cloudScale, SpriteEffects.None, 0f);
            #endregion

            counter++;
            #endregion

            // Set the logo draw color to be white and the time to be noon
            // This is because there is not a day/night cycle in this menu, and changing colors would look bad
            drawColor = Color.White;
            Main.time = 27000;
            Main.dayTime = true;

            // Draw the logo using a different spritebatch blending setting so it doesn't have a horrible yellow glow
            Vector2 drawPos = new Vector2(Main.screenWidth / 2f, 100f);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
            spriteBatch.Draw(Logo.Value, drawPos, null, drawColor, logoRotation, Logo.Value.Size() * 0.5f, logoScale, SpriteEffects.None, 0f);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
            return false;
        }
        private float GetBackgroundOffset(float parallaxMultiplier, Texture2D tex)
        {
            return -100000 - (counter * parallaxMultiplier);
        }
        private float GetBoundedX(float initialX, float scale) => (initialX % (Main.screenWidth * (3.4f * scale)) + (Main.screenWidth * (2.8f * scale)));
    }
}