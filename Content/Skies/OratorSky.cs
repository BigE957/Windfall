using CalamityMod.Events;
using Terraria.Graphics.Effects;
using Windfall.Content.NPCs.Bosses.Orator;
using Windfall.Content.UI.BossBars;

namespace Windfall.Content.Skies
{
    public class OratorSky : CustomSky
    {
        private bool skyActive;
        private float opacity;
        private int counter = 0;
        private float windOffset = 0;
        //float moveRatio = 0f;
        float crackRatio = 0f;

        public override void Deactivate(params object[] args)
        {
            skyActive = (Main.npc.Any(n => n.active && n.type == ModContent.NPCType<TheOrator>()) || Main.LocalPlayer.Monolith().OratorMonolith > 0);
        }

        public override void Reset()
        {
            Main.LocalPlayer.Monolith().OratorMonolith = 0;
            skyActive = false;
        }

        public override bool IsActive() => skyActive || opacity > 0f;

        public override void Activate(Vector2 position, params object[] args)
        {
            skyActive = true;
            counter = 0;
            windOffset = 0;
        }
        public override Color OnTileColor(Color inColor) => Color.Lerp(inColor, Color.AliceBlue * 0.5f, opacity);
        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            Color oratorGreen = new(117, 255, 159);            
            spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(0, 0, Main.screenWidth * 2, Main.screenHeight * 2), Color.Black * opacity);

            if (Main.netMode != NetmodeID.Server)
            {
                float bgTop = (float)((-Main.screenPosition.Y) / (Main.worldSurface * 16.0 - 600.0) * 200.0);

                #region Stars
                float colorMult = 1f * opacity;
                for (int i = 0; i < Main.star.Length; i++)
                {
                    Star star = Main.star[i];
                    if (star == null)
                        continue;

                    Texture2D t2D = TextureAssets.Star[star.type].Value;
                    Vector2 origin = new(t2D.Width * 0.5f, t2D.Height * 0.5f);
                    Vector2 pos = new(star.position.X + origin.X, star.position.Y + origin.Y + bgTop);
                    spriteBatch.Draw(t2D, pos, new Rectangle(0, 0, t2D.Width, t2D.Height), Color.Yellow * star.twinkle * colorMult, star.rotation, origin, star.scale * star.twinkle, SpriteEffects.None, 0f);
                }
                #endregion

                #region Back Clouds
                float cloudHeight = bgTop + (Main.screenHeight * 1.15f);
                float cloudScale = 0.45f;

                Texture2D CloudBack = (Texture2D)ModContent.Request<Texture2D>("Windfall/Assets/Skies/OratorCloudsBack", AssetRequestMode.ImmediateLoad);
                float backgroundOffset = GetBackgroundOffset(0.1f, CloudBack);
                spriteBatch.Draw(CloudBack, new Vector2(GetBoundedX(backgroundOffset, cloudScale), cloudHeight - (450 * cloudScale)), CloudBack.Frame(), Color.Gray * opacity, 0f, new Vector2(CloudBack.Width / 2, CloudBack.Height / 2), cloudScale, SpriteEffects.None, 0f);
                spriteBatch.Draw(CloudBack, new Vector2(GetBoundedX((backgroundOffset - (CloudBack.Width * (cloudScale))), cloudScale), cloudHeight - (450 * cloudScale)), CloudBack.Frame(), Color.Gray * opacity, 0f, new Vector2(CloudBack.Width / 2, CloudBack.Height / 2), cloudScale, SpriteEffects.None, 0f);
                spriteBatch.Draw(CloudBack, new Vector2(GetBoundedX((backgroundOffset + (CloudBack.Width * (cloudScale))), cloudScale), cloudHeight - (450 * cloudScale)), CloudBack.Frame(), Color.Gray * opacity, 0f, new Vector2(CloudBack.Width / 2, CloudBack.Height / 2), cloudScale, SpriteEffects.None, 0f);
                #endregion

                #region Moon
                Vector2 position = new(Main.screenWidth / 2, bgTop + Main.screenHeight / 1.25f);
                Texture2D bloomCircle = ModContent.Request<Texture2D>("CalamityMod/Particles/LargeBloom", AssetRequestMode.ImmediateLoad).Value;

                spriteBatch.UseBlendState(BlendState.Additive);
                Vector2 halfSizeTexture = new(bloomCircle.Width / 2, bloomCircle.Height / 2);
                spriteBatch.Draw(bloomCircle, position, null, oratorGreen * opacity * ((float)(Math.Sin(counter/ 20D) / 4f + 0.75f)), 0f, bloomCircle.Size() / 2f, 2.8f, SpriteEffects.None, 0f);

                Texture2D bloomRing = ModContent.Request<Texture2D>("Windfall/Assets/Skies/OratorMoonBloom2", AssetRequestMode.ImmediateLoad).Value;
                halfSizeTexture = new(bloomRing.Width / 2, bloomRing.Height / 2);
                spriteBatch.Draw(bloomRing, position, null, Color.LightCyan * opacity, counter / 100f, halfSizeTexture, 1.6375f, SpriteEffects.None, 0f);

                spriteBatch.UseBlendState(BlendState.NonPremultiplied);
                Texture2D moon = ModContent.Request<Texture2D>("Windfall/Assets/Skies/OratorMoon", AssetRequestMode.ImmediateLoad).Value;
                halfSizeTexture = new(moon.Width / 2, moon.Height / 2);
                spriteBatch.Draw(moon, position, null, Color.White * opacity, 0f, halfSizeTexture, 1.625f, SpriteEffects.None, 0f);
                #endregion

                #region Front Clouds       
                /*
                Texture2D CloudMiddle = (Texture2D)ModContent.Request<Texture2D>("Windfall/Assets/Skies/OratorCloudsMiddle", AssetRequestMode.ImmediateLoad);
                backgroundOffset = GetBackgroundOffset(0.2f, CloudMiddle);
                spriteBatch.Draw(CloudMiddle, new Vector2(GetBoundedX(backgroundOffset, cloudScale), cloudHeight), CloudMiddle.Frame(), Color.Gray * opacity, 0f, new Vector2(CloudMiddle.Width / 2, CloudMiddle.Height / 2), cloudScale, SpriteEffects.None, 0f);
                spriteBatch.Draw(CloudMiddle, new Vector2(GetBoundedX((backgroundOffset - (CloudMiddle.Width * (cloudScale))), cloudScale), cloudHeight), CloudMiddle.Frame(), Color.Gray * opacity, 0f, new Vector2(CloudMiddle.Width / 2, CloudMiddle.Height / 2), cloudScale, SpriteEffects.None, 0f);
                spriteBatch.Draw(CloudMiddle, new Vector2(GetBoundedX((backgroundOffset + (CloudMiddle.Width * (cloudScale))), cloudScale), cloudHeight), CloudMiddle.Frame(), Color.Gray * opacity, 0f, new Vector2(CloudMiddle.Width / 2, CloudMiddle.Height / 2), cloudScale, SpriteEffects.None, 0f);
                */
                Texture2D CloudFront = (Texture2D)ModContent.Request<Texture2D>("Windfall/Assets/Skies/OratorCloudsFront", AssetRequestMode.ImmediateLoad);
                backgroundOffset = GetBackgroundOffset(0.3f, CloudFront);
                spriteBatch.Draw(CloudFront, new Vector2(GetBoundedX(backgroundOffset, cloudScale), cloudHeight - (190 * cloudScale)), CloudFront.Frame(), Color.Gray * opacity, 0f, new Vector2(CloudFront.Width / 2, CloudFront.Height / 2), cloudScale, SpriteEffects.None, 0f);
                spriteBatch.Draw(CloudFront, new Vector2(GetBoundedX((backgroundOffset - (CloudFront.Width * (cloudScale))), cloudScale), cloudHeight - (190 * cloudScale)), CloudFront.Frame(), Color.Gray * opacity, 0f, new Vector2(CloudFront.Width / 2, CloudFront.Height / 2), cloudScale, SpriteEffects.None, 0f);
                spriteBatch.Draw(CloudFront, new Vector2(GetBoundedX((backgroundOffset + (CloudFront.Width * (cloudScale))), cloudScale), cloudHeight - (190 * cloudScale)), CloudFront.Frame(), Color.Gray * opacity, 0f, new Vector2(CloudFront.Width / 2, CloudFront.Height / 2), cloudScale, SpriteEffects.None, 0f);
                #endregion
            }
        }
        public override void Update(GameTime gameTime)
        {
            if ((!NPC.AnyNPCs(ModContent.NPCType<TheOrator>()) && Main.LocalPlayer.Monolith().OratorMonolith <= 0) || BossRushEvent.BossRushActive || Main.gameMenu)
                skyActive = false;

            if (skyActive)
            {
                if (opacity < 1f)
                    opacity += 0.02f;
                else if(NPC.AnyNPCs(ModContent.NPCType<TheOrator>()))
                {
                    Main.dayTime = false;
                    Main.time = 16200;
                }
            }
            else if (!skyActive && opacity > 0f)
                opacity -= 0.01f;

            Opacity = opacity;

            counter++;
        }
        public override float GetCloudAlpha() => 1f;
        private float GetBackgroundOffset(float parallaxMultiplier, Texture2D tex)
        {
            windOffset += (Main.windSpeedCurrent);
            return -100000 - (((Main.LocalPlayer.Center.X / 10) - windOffset) * parallaxMultiplier);
        }
        private static float GetBoundedX(float initialX, float scale) => (initialX % (Main.screenWidth * (3.425f * scale)) + (Main.screenWidth * (2.8f * scale)));
    }
}
