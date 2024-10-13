using Luminance.Core.Graphics;

namespace Windfall.Common.Systems.WorldEvents
{
    public class DesertScourgeRumbleSystem : ModSystem
    {
        public static readonly SoundStyle Roar = new("Calamitymod/Sounds/Custom/DesertScourgeRoar");

        int trueTimer = 0;
        static int shakeCounter = 0;
        int cooldown = 0;
        public enum SystemState
        {
            CheckReqs,
            CheckChance,
            Spawn,
        }
        public SystemState State = SystemState.CheckReqs;

        public override void PreUpdateWorld()
        {
            Player mainPlayer = Main.player[0];

            switch (State)
            {
                case SystemState.CheckReqs:
                    if (mainPlayer.ZoneDesert && !DownedBossSystem.downedDesertScourge && !AnyBossNPCS() && cooldown == 0)
                    {
                        State = SystemState.CheckChance;
                    }
                    else
                        if (!mainPlayer.ZoneDesert)
                        cooldown = 0;
                    break;
                case SystemState.CheckChance:
                    if (Main.rand.NextBool(3) && cooldown == 0)
                        cooldown = Main.rand.Next(-400, -200);
                    else if (cooldown < 0)
                    {
                        cooldown++;
                        if (cooldown == 0)
                        {
                            shakeCounter = 0;
                            trueTimer = 0;
                            cooldown = 1;
                            State = SystemState.Spawn;
                        }
                    }
                    else
                    {
                        cooldown = 1;
                        State = SystemState.CheckReqs;
                    }
                    break;
                case SystemState.Spawn:
                    ScoogShake(mainPlayer, trueTimer++, 135, Main.rand.NextBool(), 0.25f);
                    if (trueTimer >= 270 || !mainPlayer.ZoneDesert)
                        State = SystemState.CheckReqs;
                    break;
            }
        }

        public static void ResetShakeCounter()
        {
            shakeCounter = 0;
        }

        public static void ScoogShake(Player target, int scoogTimer, int midpoint, bool leftSide, float volume)
        {
            float groundShakeTime = 270f;

            // Make the ground shake and the ground create rising sand particles on the ground at first.
            float groundShakeInterpolant = (float)shakeCounter / groundShakeTime;

            for (int i = 0; i < 3; i++)
            {
                if (Main.rand.NextFloat() >= groundShakeInterpolant + 0.2f)
                    continue;
                Point point = Utilities.FindGroundVertical((target.Center + new Vector2(Main.rand.NextFloatDirection() * 1200f, -560f)).ToTileCoordinates());
                bool sandBelow = ParanoidTileRetrieval(point.X, point.Y).TileType == TileID.Sand;
                if (sandBelow)
                    Dust.NewDustPerfect(new Vector2(point.ToWorldCoordinates().X, point.ToWorldCoordinates().Y) + new Vector2(Main.rand.NextFloatDirection() * 8f, -8f), DustID.Sand, Main.rand.NextVector2Circular(1.5f, 1.5f) - Vector2.UnitY * 1.5f);
            }
            // Create screen shake effects.
            ScreenShakeSystem.StartShake((float)(MathF.Pow(groundShakeInterpolant, 1.81f) * 10f));
            if (scoogTimer == midpoint)
                if (leftSide)
                    SoundEngine.PlaySound(Roar with { Volume = volume }, target.Center + new Vector2(Main.rand.Next(-300, -200), 150));
                else
                    SoundEngine.PlaySound(Roar with { Volume = volume }, target.Center + new Vector2(Main.rand.Next(200, 300), 150));
            else if (scoogTimer > midpoint)
                shakeCounter--;
            else
                shakeCounter++;

        }
    }
}
