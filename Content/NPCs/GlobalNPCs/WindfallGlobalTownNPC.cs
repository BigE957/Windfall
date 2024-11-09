using CalamityMod.NPCs.TownNPCs;
using Windfall.Content.NPCs.TravellingNPCs;
using Windfall.Content.NPCs.WorldEvents.CalClone;
using static Terraria.ModLoader.ModContent;

namespace Windfall.Content.NPCs.GlobalNPCs;

public class WindfallGlobalTownNPC : GlobalNPC
{
    #region NPC Chat
    private static readonly string Path = "Dialogue.Others";
    public override void GetChat(NPC npc, ref string chat)
    {
        bool DragonDeez = NPC.FindFirstNPC(NPCType<TravellingCultist>()) != -1;
        bool DrugDealer = NPC.FindFirstNPC(NPCType<WanderingPotionSeller>()) != -1;
        bool Calculus = NPC.FindFirstNPC(NPCType<WanderingCalClone>()) != -1;

        bool fapsol = NPC.FindFirstNPC(NPCType<FAP>()) != -1;
        bool permadong = NPC.FindFirstNPC(NPCType<DILF>()) != -1;
        bool seahorse = NPC.FindFirstNPC(NPCType<SEAHOE>()) != -1;
        bool thief = NPC.FindFirstNPC(NPCType<THIEF>()) != -1;

        bool angelstatue = NPC.FindFirstNPC(NPCID.Merchant) != -1;

        if(npc.type == NPCType<FAP>())
        {

        }
        if (npc.type == NPCType<DILF>())
        {
            if (Main.rand.NextBool(2) && Calculus)
                chat = GetWindfallTextValue($"{Path}.Archmage.CalClone");
        }
        if (npc.type == NPCType<SEAHOE>())
        {

        }
        if (npc.type == NPCType<THIEF>())
        {

        }
        switch (npc.type)
        {
            case NPCID.Angler:
                break;
            case NPCID.ArmsDealer:
                if (Main.rand.NextBool(2) && Calculus)
                    chat = GetWindfallTextValue($"{Path}.ArmsDealer.CalClone");
                break;
            case NPCID.Clothier:
                break;
            case NPCID.Cyborg:
                if (Main.rand.NextBool(2) && Calculus)
                    chat = GetWindfallTextValue($"{Path}.Cyborg.CalClone");
                break;
            case NPCID.Demolitionist:
                break;
            case NPCID.Dryad:
                if (Main.rand.NextBool(2) && DrugDealer)
                    chat = GetWindfallTextValue($"{Path}.Dryad.Potionseller");
                break;
            case NPCID.DyeTrader:
                break;
            case NPCID.GoblinTinkerer:
                break;
            case NPCID.Golfer:
                if (Main.rand.NextBool(2) && Calculus)
                    chat = GetWindfallTextValue($"{Path}.Golfer.CalClone");
                break;
            case NPCID.Guide:
                break;
            case NPCID.Mechanic:
                if (DragonDeez && Main.rand.NextBool(2))
                    chat = GetWindfallTextValue($"{Path}.Mechanic.TravellingCultist");
                else if (Main.rand.NextBool(10))
                    chat = GetWindfallTextValue($"{Path}.Mechanic.DragonCult");
                break;
            case NPCID.Merchant:
                if (Main.rand.NextBool(2) && DrugDealer)
                    chat = GetWindfallTextValue($"{Path}.Merchant.Potionseller");
                break;
            case NPCID.Nurse:
                if (Main.rand.NextBool(2) && Calculus)
                    chat = GetWindfallTextValue($"{Path}.Nurse.CalClone");
                break;
            case NPCID.Painter:
                if (Main.rand.NextBool(2) && Calculus)
                    chat = GetWindfallTextValue($"{Path}.Painter.CalClone");
                break;
            case NPCID.PartyGirl:
                if (Main.rand.NextBool(2) && DrugDealer)
                    chat = GetWindfallTextValue($"{Path}.PartyGirl.Potionseller");
                break;
            case NPCID.Pirate:
                break;
            case NPCID.Princess:
                break;
            case NPCID.SantaClaus:
                break;
            case NPCID.SkeletonMerchant:
                break;
            case NPCID.Steampunker:
                break;
            case NPCID.Stylist:
                break;
            case NPCID.DD2Bartender:
                break;
            case NPCID.TaxCollector:
                if (Main.rand.NextBool(2) && DrugDealer)
                    chat = GetWindfallTextValue($"{Path}.TaxCollector.Potionseller");
                break;
            case NPCID.TravellingMerchant:
                break;
            case NPCID.Truffle:
                if (Main.rand.NextBool(2) && DrugDealer)
                    chat = GetWindfallTextValue($"{Path}.Truffle.Potionseller");
                break;
            case NPCID.WitchDoctor:
                if (Main.rand.NextBool(2) && DrugDealer)
                    chat = GetWindfallTextValue($"{Path}.WitchDoctor.Potionseller");
                break;
            case NPCID.Wizard:
                break;
            case NPCID.BestiaryGirl: // Zoologist
                break;
        }
    }
    public override void ModifyActiveShop(NPC npc, string shopName, Item[] items)
    {
        bool Calculus = NPC.FindFirstNPC(NPCType<WanderingCalClone>()) != -1;
        if(npc.type == NPCID.Painter && Calculus)
            foreach(Item item in items)
                if(item != null && item.placeStyle != 0)
                {
                    item.type = ItemID.AshBlock;
                    item.shopCustomPrice = 1;
                }
    }
    #endregion
}