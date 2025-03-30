using CalamityMod;
using CalamityMod.NPCs.AquaticScourge;
using CalamityMod.NPCs.AstrumDeus;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using CalamityMod.NPCs.Perforator;
using CalamityMod.NPCs.StormWeaver;
using static Terraria.ModLoader.ModContent;

namespace Windfall.Common.Systems;
public class WindfallLists : ModSystem
{
    public static List<int> WormIDs = [];

    public static List<int> WormHeadIDs = [];

    public override void OnModLoad()
    {
        WormIDs = [
            //Vanilla Worms
            NPCID.BloodEelHead,
            NPCID.BloodEelBody, 
            NPCID.BloodEelTail, 
            NPCID.BoneSerpentHead, 
            NPCID.BoneSerpentBody, 
            NPCID.BoneSerpentTail, 
            NPCID.CultistDragonHead,
            NPCID.CultistDragonBody1, 
            NPCID.CultistDragonBody2, 
            NPCID.CultistDragonBody3, 
            NPCID.CultistDragonBody4, 
            NPCID.CultistDragonTail, 
            NPCID.DevourerHead, 
            NPCID.DevourerBody, 
            NPCID.DevourerTail, 
            NPCID.DiggerHead, 
            NPCID.DiggerBody, 
            NPCID.DiggerTail, 
            NPCID.DuneSplicerHead,
            NPCID.DuneSplicerBody, 
            NPCID.DuneSplicerTail, 
            NPCID.EaterofWorldsHead,
            NPCID.EaterofWorldsBody, 
            NPCID.EaterofWorldsTail, 
            NPCID.GiantWormHead, 
            NPCID.GiantWormBody, 
            NPCID.GiantWormTail, 
            NPCID.LeechHead,
            NPCID.LeechBody, 
            NPCID.LeechTail, 
            NPCID.SeekerHead,
            NPCID.SeekerBody, 
            NPCID.SeekerTail, 
            NPCID.SolarCrawltipedeHead,
            NPCID.SolarCrawltipedeBody, //Immune
            NPCID.SolarCrawltipedeTail, 
            NPCID.StardustWormHead,
            NPCID.StardustWormBody, //immune
            NPCID.StardustWormTail, //immune
            NPCID.TheDestroyer,
            NPCID.TheDestroyerBody, 
            NPCID.TheDestroyerTail, 
            NPCID.TombCrawlerHead, 
            NPCID.TombCrawlerBody, 
            NPCID.TombCrawlerTail, 
            NPCID.WyvernHead, 
            NPCID.WyvernBody, 
            NPCID.WyvernBody2, 
            NPCID.WyvernBody3, 
            NPCID.WyvernLegs,
            NPCID.WyvernTail
        ];

        //Calamity Worms
        WormIDs.AddRange(CalamityLists.AstrumDeusIDs);
        WormIDs.AddRange(CalamityLists.AquaticScourgeIDs);
        WormIDs.AddRange(CalamityLists.DevourerOfGodsIDs);
        WormIDs.AddRange(CalamityLists.DesertScourgeIDs);
        WormIDs.AddRange(CalamityLists.PerforatorIDs);
        WormIDs.AddRange(CalamityLists.StormWeaverIDs);
        WormIDs.AddRange(CalamityLists.ThanatosIDs);

        WormHeadIDs = [
            //Vanilla Worms
            NPCID.BloodEelHead,
            NPCID.BoneSerpentHead,
            NPCID.CultistDragonHead,
            NPCID.DevourerHead,
            NPCID.DiggerHead,
            NPCID.DuneSplicerHead,
            NPCID.EaterofWorldsHead,
            NPCID.GiantWormHead,
            NPCID.LeechHead,
            NPCID.SeekerHead,
            NPCID.SolarCrawltipedeHead,
            NPCID.StardustWormHead,
            NPCID.TheDestroyer,
            NPCID.TombCrawlerHead,
            NPCID.WyvernHead,
            //Calamity Worms
            NPCType<AstrumDeusHead>(),
            NPCType<AquaticScourgeHead>(),
            NPCType<DevourerofGodsHead>(),
            NPCType<DesertScourgeHead>(),
            NPCType<DesertNuisanceHead>(),
            NPCType<DesertNuisanceHeadYoung>(),
            NPCType<PerforatorHeadSmall>(),
            NPCType<PerforatorHeadMedium>(),
            NPCType<PerforatorHeadLarge>(),
            NPCType<StormWeaverHead>(),
            NPCType<ThanatosHead>(),            
        ];
    }

    public override void Unload()
    {
        WormIDs.Clear();
        WormIDs = null;

        WormHeadIDs.Clear();
        WormHeadIDs = null;
    }
}
