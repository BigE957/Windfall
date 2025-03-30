using CalamityMod;

namespace Windfall.Common.Systems.WorldEvents;

public class PolterSoundSystem : ModSystem
{
    public enum SystemState
    {
        CheckReqs,
        CheckChance,
        Play,
    }
    public SystemState State = SystemState.CheckReqs;

    SoundStyle PolterAmbiance = new("Windfall/Assets/Sounds/Ambiance/Polterghast/PolterAmbiance_", 3);
    private bool OnCooldown = false;
    private int Counter = 1200 * 60;
    public override void PreUpdateWorld()
    {
        Player mainPlayer = Main.player[0];
        //if(Counter != 0)
            //DisplayLocalizedText($"{State}, {Counter}");
        switch (State)
        {
            case SystemState.CheckReqs:
                if (mainPlayer.ZoneDungeon && !DownedBossSystem.downedPolterghast && !AnyBossNPCS() && !OnCooldown && Counter == 0)
                    State = SystemState.CheckChance;
                else
                {
                    if(!mainPlayer.ZoneDungeon || DownedBossSystem.downedPolterghast || AnyBossNPCS() || OnCooldown)
                        Counter = 30 * 60;
                    if (!mainPlayer.ZoneDungeon)
                        OnCooldown = false;
                }
                break;
            case SystemState.CheckChance:
                if (Main.rand.NextBool(1200))
                    State = SystemState.Play;
                else
                    State = SystemState.CheckReqs;
                break;
            case SystemState.Play:
                if (NPC.downedMoonlord)
                    SoundEngine.PlaySound(PolterAmbiance with { Volume = 1f }, mainPlayer.Center);
                else if (NPC.downedPlantBoss)
                    SoundEngine.PlaySound(PolterAmbiance with { Volume = 1f }, mainPlayer.Center + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f)).SafeNormalize(Vector2.UnitX) * (Main.rand.Next(800, 1000)));
                else if (NPC.downedBoss3)
                    SoundEngine.PlaySound(PolterAmbiance with { Volume = 1f }, mainPlayer.Center + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f)).SafeNormalize(Vector2.UnitX) * (Main.rand.Next(1200, 1500)));
                else
                    SoundEngine.PlaySound(PolterAmbiance with { Volume = 0.5f }, mainPlayer.Center + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f)).SafeNormalize(Vector2.UnitX) * (Main.rand.Next(1200, 1500)));

                State = SystemState.CheckReqs;
                Counter = 30 * 60;
                break;
        }

        if(Counter > 0)
            Counter--;
    }
}
