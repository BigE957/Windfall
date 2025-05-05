using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.Items.Utility;
using Windfall.Content.Items.Weapons.Misc;
using DialogueHelper.UI.Dialogue;
using Windfall.Content.Items.Quests.Casters;
using Windfall.Content.Items.Tools;
using Windfall.Content.Items.Placeables.Furnature.Plaques;
using Windfall.Content.Buffs.DoT;
using Windfall.Common.Systems;

namespace Windfall.Content.NPCs.WorldEvents.LunarCult;

public class OratorNPC : ModNPC
{
    public enum States
    {
        Idle,
        TutorialChat,
        RitualEvent,
        BetrayalChat,
        DraconicBoneSequence,
        Cutscene
    }

    private States AIState
    {
        get => (States)NPC.ai[0];
        set => NPC.ai[0] = (float)value;
    }

    private int Time
    {
        get => (int)NPC.ai[1];
        set => NPC.ai[1] = value;
    }

    public override string Texture => "Windfall/Assets/NPCs/WorldEvents/TheOrator_NPC";
    public override void SetStaticDefaults()
    {
        this.HideBestiaryEntry();
        NPCID.Sets.ActsLikeTownNPC[Type] = true;
        Main.npcFrameCount[Type] = 1;
        NPCID.Sets.NoTownNPCHappiness[Type] = true;
        ModContent.GetInstance<DialogueUISystem>().TreeInitialize += ModifyTree;
        ModContent.GetInstance<DialogueUISystem>().ButtonClick += ClickEffect;
        ModContent.GetInstance<DialogueUISystem>().TreeClose += CloseEffect;
    }
    public override void SetDefaults()
    {
        NPC.friendly = true; // NPC Will not attack player
        NPC.width = 58;
        NPC.height = 70;
        NPC.aiStyle = -1;
        NPC.damage = 0;
        NPC.defense = 0;
        NPC.lifeMax = 1000;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0f;
        NPC.immortal = true;
    }

    public override void OnSpawn(IEntitySource source)
    {
        if (AIState == States.Cutscene)
        {
            Vector2 oldPos = NPC.position;
            NPC.position.Y = FindSurfaceBelow(new Point((int)NPC.position.X / 16, (int)NPC.position.Y / 16)).Y * 16 - NPC.height;

            for (int i = 0; i < 2; i++)
            {
                float altY = (FindSurfaceBelow(new Point((int)(oldPos.X / 16 + i), (int)(oldPos.Y / 16 - 2))).Y - 1) * 16 - NPC.height + 16;
                if (altY < NPC.position.Y)
                    NPC.position.Y = altY;
            }
        }
    }
    public override void AI()
    {
        if (AIState == States.DraconicBoneSequence)
        {
            if (ModContent.GetInstance<DialogueUISystem>().isDialogueOpen)
                return;

            if (Time == 60)
                ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "Cutscenes/OratorDraconicBone", new(Name, [NPC.whoAmI]));

            if (Time > 60)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient && Time <= 420 && Time % 30 == 0)
                {
                    Vector2 spawnLocation = ((Time / 30) - 2) switch
                    {
                        1 => LunarCultBaseSystem.CultBaseWorldArea.Center() + new Vector2(408, 699),
                        2 => LunarCultBaseSystem.CultBaseWorldArea.Center() + new Vector2(-440, 699),
                        3 => LunarCultBaseSystem.CultBaseWorldArea.Center() + new Vector2(-870, 427),
                        4 => LunarCultBaseSystem.CultBaseWorldArea.Center() + new Vector2(584, 387),
                        5 => LunarCultBaseSystem.CultBaseWorldArea.Center() + new Vector2(391, 427),
                        6 => LunarCultBaseSystem.CultBaseWorldArea.Center() + new Vector2(378, 427),
                        7 => LunarCultBaseSystem.CultBaseWorldArea.Center() + new Vector2(-722, -117),
                        8 => LunarCultBaseSystem.CultBaseWorldArea.Center() + new Vector2(-281, -117),
                        9 => LunarCultBaseSystem.CultBaseWorldArea.Center() + new Vector2(-199, -117),
                        10 => LunarCultBaseSystem.CultBaseWorldArea.Center() + new Vector2(311, -117),
                        11 => LunarCultBaseSystem.CultBaseWorldArea.Center() + new Vector2(538, -117),
                        12 => LunarCultBaseSystem.CultBaseWorldArea.Center() + new Vector2(-300, -389),
                        _ => LunarCultBaseSystem.CultBaseWorldArea.Center(),
                    };
                    NPC.NewNPC(NPC.GetSource_FromThis(), (int)spawnLocation.X, (int)spawnLocation.Y, ModContent.NPCType<LunarCultistDevotee>(), ai2: 8, ai3: 1);
                }

                foreach (Player player in Main.ActivePlayers)
                {
                    Rectangle inflatedArea = new(LunarCultBaseSystem.CultBaseWorldArea.X - 512, LunarCultBaseSystem.CultBaseWorldArea.Y + 512, LunarCultBaseSystem.CultBaseWorldArea.Width + 1024, LunarCultBaseSystem.CultBaseWorldArea.Height + 1024);
                    Rectangle inflatedBridge = new(LunarCultBaseSystem.CultBaseBridgeArea.X * 16 - 512, LunarCultBaseSystem.CultBaseBridgeArea.Y * 16 + 512, LunarCultBaseSystem.CultBaseBridgeArea.Width * 16 + 1024, LunarCultBaseSystem.CultBaseBridgeArea.Height * 16 + 1024);
                    if (inflatedArea.Contains((int)player.Center.X, (int)player.Center.Y) || inflatedBridge.Contains((int)player.Center.X, (int)player.Center.Y))
                        player.AddBuff(ModContent.BuffType<Entropy>(), 2);
                }

                Time++;
            }

            int index = Player.FindClosest(NPC.position, NPC.width, NPC.height);
            if (index != -1)
            {
                Player nearest = Main.player[index];
                NPC.direction = nearest.Center.X > NPC.Center.X ? 1 : -1;
            }
        }
    }

    public override bool CanChat() => !QuestSystem.Quests["DraconicBone"].Complete && !ModContent.GetInstance<DialogueUISystem>().isDialogueOpen && !LunarCultBaseSystem.IsRitualActivityActive() && AIState != States.DraconicBoneSequence && AIState != States.Cutscene;
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
            case 11:
                ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "TheOrator/ApostleQuest3", new(Name, [NPC.whoAmI]), 6);
                return "";
        }

        if (NPC.ai[0] == 0)
            ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "TheOrator/Default", new(Name, [NPC.whoAmI]));
        else
            ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "TheOrator/" + AIState.ToString(), new(Name, [NPC.whoAmI]));           

        return "";
    }
    private static void ModifyTree(string treeKey, int dialogueID, int buttonID, bool swapped)
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
            ItemStack flame = new(new Tuple<int, int>(ModContent.ItemType<DiabolicalInsense>(), 1));
            ItemStack death = new(new Tuple<int, int>(ModContent.ItemType<NecromaticRing>(), 1));
            ItemStack light = new(new Tuple<int, int>(ModContent.ItemType<RaggedSack>(), 1));
            uiSystem.CurrentTree.Dialogues[6].Responses[0].Requirement = CanAffordCost(Main.LocalPlayer, flame) && CanAffordCost(Main.LocalPlayer, death) && CanAffordCost(Main.LocalPlayer, light);
        }
    }

    private static void ClickEffect(string treeKey, int dialogueID, int buttonID, bool swapped)
    {
        if (treeKey == "TheOrator/ApostleQuest1" && dialogueID == 6 && buttonID == 0)
        {
            ItemStack flame = new(new Tuple<int, int>(ModContent.ItemType<DiabolicalInsense>(), 1));
            ItemStack death = new(new Tuple<int, int>(ModContent.ItemType<NecromaticRing>(), 1));
            ItemStack light = new(new Tuple<int, int>(ModContent.ItemType<RaggedSack>(), 1));
            PayAffordCost(Main.LocalPlayer, flame);
            PayAffordCost(Main.LocalPlayer, death);
            PayAffordCost(Main.LocalPlayer, light);
        }
    }

    private static void CloseEffect(string treeKey, int dialogueID, int buttonID, bool swapped)
    {
        if(treeKey.Contains("TheOrator/"))
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
                        switch (buttonID)
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
                case "TheOrator/ApostleQuest1":
                    if (dialogueID == 1)
                        Main.LocalPlayer.LunarCult().apostleQuestTracker = -1;
                    else if (dialogueID == 5 || dialogueID == 12)
                        Main.LocalPlayer.LunarCult().apostleQuestTracker++; //5:5 12:6
                    break;
                case "TheOrator/ApostleQuest2":
                    if (dialogueID == 5 || dialogueID == 8)
                        Main.LocalPlayer.LunarCult().apostleQuestTracker++; //5:8 //8:10
                    break;
                case "TheOrator/ApostleQuest3":
                    if (dialogueID == 6 || dialogueID == 9)
                        Main.LocalPlayer.LunarCult().apostleQuestTracker++; //6:12 //9:??
                    break;
            }
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
