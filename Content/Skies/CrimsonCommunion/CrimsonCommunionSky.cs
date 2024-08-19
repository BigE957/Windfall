using CalamityMod.Events;
using Terraria.Graphics.Effects;

namespace Windfall.Content.Skies.ScreenShaders
{
    public class CrimsonCommunionSky : CustomSky
    {
        private bool isActive = false;
        private float intensity = 0f;

        public override void Update(GameTime gameTime)
        {
            if (!Main.LocalPlayer.Godly().CrimsonCommunion || BossRushEvent.BossRushActive)
            {
                isActive = false;
            }

            if (isActive && intensity < 1f)
            {
                intensity += 0.01f;
            }
            else if (!isActive && intensity > 0f)
            {
                intensity -= 0.01f;
            }
        }

        public override Color OnTileColor(Color inColor) => Color.Lerp(inColor, new(160, 0, 0, inColor.A), intensity * 0.66f);

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0 && minDepth < 0)
            {
                spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(0, 0, Main.screenWidth * 2, Main.screenHeight * 2), new Color(160, 0, 0) * (intensity * 0.4f));
            }
        }

        public override float GetCloudAlpha() => 0f;

        public override void Activate(Vector2 position, params object[] args)
        {
            isActive = true;
        }

        public override void Deactivate(params object[] args)
        {
            isActive = false;
        }

        public override void Reset()
        {
            isActive = false;
        }

        public override bool IsActive() => isActive || intensity > 0f;
    }
}
