namespace Windfall.Common.Systems.WorldEvents;

public class EyeCultistsSpawnSystem : ModSystem
{
    /*
    internal enum SystemState
    {
        CheckReqs,
        Spawn,
    }
    internal SystemState State = SystemState.CheckReqs;
    int cooldown = 0;
    public static Point GroundsLocation = new(-1, -1);
    public override void ClearWorld()
    {
        GroundsLocation = new(-1, -1);
    }
    public override void LoadWorldData(TagCompound tag)
    {
        GroundsLocation = tag.Get<Point>("GroundsLocation");
    }
    public override void SaveWorldData(TagCompound tag)
    {
        tag["GroundsLocation"] = GroundsLocation;
    }
    public override void PreUpdateWorld()
    {
        Player mainPlayer = Main.player[0];
        //Requirements for Calamitas to spawn.
        switch (State)
        {
            case SystemState.CheckReqs:
                if (!NPC.downedBoss1 && !Main.dayTime && cooldown == 0)
                {
                    State = SystemState.Spawn;
                }
                else if(Main.dayTime)
                {
                    cooldown = 0;
                }
                break;
            case SystemState.Spawn:
                if(GroundsLocation.X != -1)
                {
                    State = SystemState.CheckReqs;
                    cooldown = 1;

                    Vector2 Cultist1Coords = new Vector2(GroundsLocation.X - 12, GroundsLocation.Y - 17).ToWorldCoordinates();
                    Cultist1Coords.X += 8;
                    for (int i = 0; i < 50; i++)
                    {
                        Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                        Dust d = Dust.NewDustPerfect(Cultist1Coords, DustID.GoldFlame, speed * 3, Scale: 1.5f);
                        d.noGravity = true;
                    }
                    NPC.NewNPCDirect(Entity.GetSource_NaturalSpawn(), Cultist1Coords, ModContent.NPCType<LunarCultistDevotee>(), 0, 1);

                    Vector2 Cultist2Coords = new Vector2(GroundsLocation.X + 10, GroundsLocation.Y - 17).ToWorldCoordinates();
                    Cultist2Coords.X -= 8;
                    for (int i = 0; i < 50; i++)
                    {
                        Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                        Dust d = Dust.NewDustPerfect(Cultist2Coords, DustID.GoldFlame, speed * 3, Scale: 1.5f);
                        d.noGravity = true;
                    }
                    NPC.NewNPCDirect(Entity.GetSource_NaturalSpawn(), Cultist2Coords, ModContent.NPCType<LunarCultistDevotee>(), 0, 1);

                    Vector2 BishopCoords;
                    if(GroundsLocation.X > Main.maxTilesX / 2)
                    {
                        BishopCoords = new Vector2(GroundsLocation.X + 4, GroundsLocation.Y - 18).ToWorldCoordinates();
                    }
                    else
                    {
                        BishopCoords = new Vector2(GroundsLocation.X - 4, GroundsLocation.Y - 18).ToWorldCoordinates();
                    }
                    for (int i = 0; i < 50; i++)
                    {
                        Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                        Dust d = Dust.NewDustPerfect(Cultist2Coords, DustID.GoldFlame, speed * 3, Scale: 1.5f);
                        d.noGravity = true;
                    }
                    NPC.NewNPCDirect(Entity.GetSource_NaturalSpawn(), BishopCoords, ModContent.NPCType<LunarBishop>(), 0, 1);
                }
                break;
        }
    }
    */
}
