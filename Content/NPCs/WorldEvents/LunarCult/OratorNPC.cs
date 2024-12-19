using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.Items.Utility;
using Windfall.Content.Items.Weapons.Misc;
using DialogueHelper.UI.Dialogue;
using Windfall.Content.Items.Quest.Casters;
using Terraria.ID;
using Windfall.Content.Items.Tools;
using Windfall.Content.Items.Placeables.Furnature;

namespace Windfall.Content.NPCs.WorldEvents.LunarCult;

public class OratorNPC : ModNPC
{
    private enum States
    {
        Idle,
        TutorialChat,
        RitualEvent,
        BetrayalChat,
    }
    private States AIState
    {
        get => (States)NPC.ai[0];
        set => NPC.ai[0] = (float)value;
    }
    public override string Texture => "Windfall/Assets/NPCs/WorldEvents/TheOrator_NPC";
    public override void SetStaticDefaults()
    {
        this.HideFromBestiary();
        NPCID.Sets.ActsLikeTownNPC[Type] = true;
        Main.npcFrameCount[Type] = 1;
        NPCID.Sets.NoTownNPCHappiness[Type] = true;
        ModContent.GetInstance<DialogueUISystem>().DialogueOpen += OpenEffect;
        ModContent.GetInstance<DialogueUISystem>().ButtonClick += ClickEffect;
        ModContent.GetInstance<DialogueUISystem>().DialogueClose += CloseEffect;
    }
    public override void SetDefaults()
    {
        NPC.friendly = true; // NPC Will not attack player
        NPC.width = 58;
        NPC.height = 70;
        NPC.aiStyle = 0;
        NPC.damage = 0;
        NPC.defense = 0;
        NPC.lifeMax = 1000;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0f;
        NPC.immortal = true;
    }
    public override bool CanChat() => !ModContent.GetInstance<DialogueUISystem>().isDialogueOpen && !LunarCultBaseSystem.IsRitualActivityActive();
    public override string GetChat()
    {
        Main.CloseNPCChatOrSign();

        switch(Main.LocalPlayer.LunarCult().apostleQuestTracker)
        {
            case 4:
                ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "TheOrator/ApostleQuest1", new(Name, [NPC.whoAmI]));
                return "";
            case 7:
                ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "TheOrator/ApostleQuest2", new(Name, [NPC.whoAmI]));
                return "";
            case 9:
                ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "TheOrator/ApostleQuest2", new(Name, [NPC.whoAmI]), 6);
                return "";
        }

        if (NPC.ai[0] == 0)
            ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "TheOrator/Default", new(Name, [NPC.whoAmI]));
        else
            ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "TheOrator/" + AIState.ToString(), new(Name, [NPC.whoAmI]));           

        return "";
    }
    private static void OpenEffect(string treeKey, int dialogueID, int buttonID)
    {
        DialogueUISystem uiSystem = ModContent.GetInstance<DialogueUISystem>();

        if (treeKey == "TheOrator/Default")
        {
            if (!Main.LocalPlayer.LunarCult().awareOfLunarCoins)
            {
                uiSystem.CurrentTree.Dialogues[0].Responses[0].SwapToTreeKey = "TheOrator/LunarCoins";
                uiSystem.CurrentTree.Dialogues[0].Responses[0].Heading = 0;
            }
            switch(Main.LocalPlayer.LunarCult().apostleQuestTracker)
            {
                case 5:
                    uiSystem.CurrentTree.Dialogues[0].Responses[1].Requirement = true;
                    uiSystem.CurrentTree.Dialogues[0].Responses[1].SwapToTreeKey = "TheOrator/ApostleQuest1";
                    uiSystem.CurrentTree.Dialogues[0].Responses[1].Heading = 6;
                    break;
            }
        }
        if(treeKey == "TheOrator/ApostleQuest1")
        {
            ItemStack flame = new()
            {
                ItemID = ModContent.ItemType<FlameIcon>(),
                SourceMod = "Windfall",
                ItemName = "FlameIcon",
                Stack = 1
            };
            ItemStack death = new()
            {
                ItemID = ModContent.ItemType<DeathIcon>(),
                SourceMod = "Windfall",
                ItemName = "FlameIcon",
                Stack = 1
            };
            ItemStack light = new()
            {
                ItemID = ModContent.ItemType<LightIcon>(),
                SourceMod = "Windfall",
                ItemName = "FlameIcon",
                Stack = 1
            };
            uiSystem.CurrentTree.Dialogues[6].Responses[0].Requirement = CanAffordCost(Main.LocalPlayer, flame) && CanAffordCost(Main.LocalPlayer, death) && CanAffordCost(Main.LocalPlayer, light);
        }
    }

    private static void ClickEffect(string treeKey, int dialogueID, int buttonID)
    {
        if (treeKey == "TheOrator/ApostleQuest1" && dialogueID == 6 && buttonID == 0)
        {
            ItemStack flame = new()
            {
                ItemID = ModContent.ItemType<FlameIcon>(),
                SourceMod = "Windfall",
                ItemName = "FlameIcon",
                Stack = 1
            };
            ItemStack death = new()
            {
                ItemID = ModContent.ItemType<DeathIcon>(),
                SourceMod = "Windfall",
                ItemName = "FlameIcon",
                Stack = 1
            };
            ItemStack light = new()
            {
                ItemID = ModContent.ItemType<LightIcon>(),
                SourceMod = "Windfall",
                ItemName = "FlameIcon",
                Stack = 1
            };
            PayAffordCost(Main.LocalPlayer, flame);
            PayAffordCost(Main.LocalPlayer, death);
            PayAffordCost(Main.LocalPlayer, light);
        }
    }

    private static void CloseEffect(string treeKey, int dialogueID, int buttonID)
    {
        NPC orator = Main.npc[(int)ModContent.GetInstance<DialogueUISystem>().CurrentDialogueContext.Arguments[0]];

        switch (treeKey)
        {
            case "TheOrator/TutorialChat":
                LunarCultBaseSystem.TutorialComplete = true;
                orator.ai[0] = 0;
                break;
            case "TheOrator/RitualEvent":
                if (dialogueID == 1)
                {
                    LunarCultBaseSystem.State = LunarCultBaseSystem.SystemStates.Ritual;
                    LunarCultBaseSystem.Active = true;
                    orator.ai[0] = 0;
                    int itemID = -1;
                    switch(buttonID)
                    {
                        case 0: itemID = ModContent.ItemType<RiftWeaver>(); break;
                    }
                    Item i = Main.item[Item.NewItem(orator.GetSource_Loot(), orator.Center, new Vector2(8, 4), itemID)];
                    i.velocity = new Vector2(orator.direction, 0) * -4;
                }
                break;
            case "TheOrator/BetrayalChat":
                LunarCultBaseSystem.BetrayalActive = true;
                orator.ai[0] = 0;
                break;
            case "TheOrator/LunarCoins":
                Main.LocalPlayer.LunarCult().awareOfLunarCoins = true;
                if (buttonID == 0)
                {
                    Main.playerInventory = true;
                    Main.stackSplit = 9999;
                    Main.npcChatText = "";
                    Main.LocalPlayer.SetTalkNPC(orator.whoAmI);
                    Main.SetNPCShopIndex(1);
                    Main.instance.shop[Main.npcShop].SetupShop(NPCShopDatabase.GetShopName(orator.type, "Shop"), orator);
                    SoundEngine.PlaySound(SoundID.MenuTick);
                }
                break;
            case "TheOrator/Default":
                if (buttonID == 0 && Main.LocalPlayer.LunarCult().awareOfLunarCoins)
                {
                    Main.playerInventory = true;
                    Main.stackSplit = 9999;
                    Main.npcChatText = "";
                    Main.LocalPlayer.SetTalkNPC(orator.whoAmI);
                    Main.SetNPCShopIndex(1);
                    Main.instance.shop[Main.npcShop].SetupShop(NPCShopDatabase.GetShopName(orator.type, "Shop"), orator);
                    SoundEngine.PlaySound(SoundID.MenuTick);
                }
                break;
            case "THeOrator/ApostleQuest1":
                if (dialogueID == 1)
                    Main.LocalPlayer.LunarCult().apostleQuestTracker = -1;
                else if(dialogueID == 5 || dialogueID == 12)
                    Main.LocalPlayer.LunarCult().apostleQuestTracker++; //5:5 12:6
                break;
            case "THeOrator/ApostleQuest2":
                if (dialogueID == 5 || dialogueID == 8)
                    Main.LocalPlayer.LunarCult().apostleQuestTracker++; //5:8 //8:10
                break;
        }
    }
    public override void AddShops()
    {
        var shop = new NPCShop(Type);
        //Ritual Weapons
        shop.Add(new Item(ModContent.ItemType<RiftWeaver>())
        {
            DamageType = DamageClass.Melee,
            shopCustomPrice = 5,
            shopSpecialCurrency = Windfall.LunarCoinCurrencyID
        });                      
        //Accessories
        shop.Add(new Item(ItemID.MoonCharm)
        {
            shopCustomPrice = 3,
            shopSpecialCurrency = Windfall.LunarCoinCurrencyID
        });
        shop.Add(new Item(ItemID.MoonStone)
        {
            shopCustomPrice = 3,
            shopSpecialCurrency = Windfall.LunarCoinCurrencyID
        });
        //Misc Utility Items
        shop.Add(new Item(ModContent.ItemType<Moonlight>())
        {
            shopCustomPrice = 3,
            shopSpecialCurrency = Windfall.LunarCoinCurrencyID
        });
        shop.Add(new Item(ItemID.Moondial)
        {
            shopCustomPrice = 3,
            shopSpecialCurrency = Windfall.LunarCoinCurrencyID
        });
        shop.Add(new Item(ItemID.MoonGlobe)
        {
            shopCustomPrice = 2,
            shopSpecialCurrency = Windfall.LunarCoinCurrencyID
        });
        //Furnature/Decor
        shop.Add(new Item(ModContent.ItemType<HammerChisel>())
        {
            shopCustomPrice = 2,
            shopSpecialCurrency = Windfall.LunarCoinCurrencyID
        });
        shop.Add(new Item(ModContent.ItemType<DarkStonePlaque>())
        {
            shopCustomPrice = 3,
            shopSpecialCurrency = Windfall.LunarCoinCurrencyID
        });
        shop.Add(new Item(ModContent.ItemType<WhiteStonePlaque>())
        {
            shopCustomPrice = 3,
            shopSpecialCurrency = Windfall.LunarCoinCurrencyID
        });
        //Paintings
        shop.Add(new Item(ItemID.MoonmanandCompany)
        {
            shopCustomPrice = 2,
            shopSpecialCurrency = Windfall.LunarCoinCurrencyID
        });
        shop.Add(new Item(ItemID.ShiningMoon)
        {
            shopCustomPrice = 2,
            shopSpecialCurrency = Windfall.LunarCoinCurrencyID
        });
        shop.Register();

        //Vanity
        shop.Add(new Item(ItemID.MoonMask)
        {
            shopCustomPrice = 2,
            shopSpecialCurrency = Windfall.LunarCoinCurrencyID
        });
    }

    private static bool CanAffordCost(Player player, ItemStack price)
    {
        int amount = price.Stack;
        int itemID = price.ItemID;
        if (itemID == -1)
            itemID = price.FetchItemID();
        if (itemID == -1) //If the ItemType is unable to be found, then returns false; as the player can't pay with something that doesn't exist :P
            return false;
        foreach (Item item in player.inventory.Where(i => i.type == itemID))
        {
            if (item.stack >= amount)
            {
                amount = 0;
                break;
            }
            else
                amount -= item.stack;
        }
        return amount == 0;
    }
    private static void PayAffordCost(Player player, ItemStack price)
    {
        foreach (Item item in player.inventory.Where(i => i.type == price.ItemID))
        {
            int amount = price.Stack;
            if (item.stack >= amount)
            {
                item.stack -= amount;
                amount = 0;
                break;
            }
            else
            {
                amount -= item.stack;
                item.stack = 0;
            }
        }
    }
}
