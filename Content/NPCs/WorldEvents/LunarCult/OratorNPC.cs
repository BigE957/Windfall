using Windfall.Common.Systems.WorldEvents;
using DialogueHelper.Content.UI.Dialogue;
using Windfall.Content.Items.Utility;
using Windfall.Content.Items.Weapons.Misc;

namespace Windfall.Content.NPCs.WorldEvents.LunarCult
{
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
        public override bool CanChat() => !ModContent.GetInstance<DialogueUISystem>().isDialogueOpen;
        public override string GetChat()
        {
            Main.CloseNPCChatOrSign();

            if (NPC.ai[0] == 0)
                ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "TheOrator/Default");
            else
                ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "TheOrator/" + AIState.ToString());           

            return "In the Cult Base, straight Orating it. And by it i mean, lets just say, my Tablet";
        }
        private void OpenEffect(string treeKey, int dialogueID, int buttonID)
        {
            if(treeKey == "TheOrator/Default" && !Main.LocalPlayer.LunarCult().awareOfLunarCoins)
            {
                DialogueUISystem uiSystem = ModContent.GetInstance<DialogueUISystem>();
                uiSystem.CurrentTree.Dialogues[0].Responses[0].SwapToTreeKey = "TheOrator/LunarCoins";
            }
        }

        private void CloseEffect(string treeKey, int dialogueID, int buttonID)
        {
            switch(treeKey)
            {
                case "TheOrator/TutorialChat":
                    NPC orator = Main.npc.First(n => n.active && n.type == ModContent.NPCType<OratorNPC>() && n.ai[0] == (int)States.TutorialChat);
                    LunarCultBaseSystem.TutorialComplete = true;
                    orator.ai[0] = 0;
                    break;
                case "TheOrator/RitualEvent":
                    if (dialogueID == 1)
                    {
                        orator = Main.npc.First(n => n.active && n.type == ModContent.NPCType<OratorNPC>() && n.ai[0] == (int)States.RitualEvent);
                        LunarCultBaseSystem.State = LunarCultBaseSystem.SystemStates.Ritual;
                        LunarCultBaseSystem.Active = true;
                        orator.ai[0] = 0;
                        int itemID = -1;
                        switch(buttonID)
                        {
                            case 0: itemID = ModContent.ItemType<RiftWeaver>(); break;
                        }
                        Item i = Main.item[Item.NewItem(Entity.GetSource_Loot(), NPC.Center, new Vector2(8, 4), itemID)];
                        i.velocity = new Vector2(NPC.direction, 0) * -4;
                    }
                    break;
                case "TheOrator/BetrayalChat":
                    orator = Main.npc.First(n => n.active && n.type == ModContent.NPCType<OratorNPC>() && n.ai[0] == (int)States.TutorialChat);
                    LunarCultBaseSystem.BetrayalActive = true;
                    orator.ai[0] = 0;
                    break;
                case "TheOrator/LunarCoins":
                    Main.LocalPlayer.LunarCult().awareOfLunarCoins = true;
                    if (buttonID == 0)
                    {
                        orator = Main.npc.First(n => n.active && n.type == ModContent.NPCType<OratorNPC>() && n.ai[0] == 0);
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
                    if(buttonID == 0 && Main.LocalPlayer.LunarCult().awareOfLunarCoins)
                    {
                        orator = Main.npc.First(n => n.active && n.type == ModContent.NPCType<OratorNPC>() && n.ai[0] == 0);
                        Main.playerInventory = true;
                        Main.stackSplit = 9999;
                        Main.npcChatText = "";
                        Main.LocalPlayer.SetTalkNPC(orator.whoAmI);
                        Main.SetNPCShopIndex(1);
                        Main.instance.shop[Main.npcShop].SetupShop(NPCShopDatabase.GetShopName(orator.type, "Shop"), orator);
                        SoundEngine.PlaySound(SoundID.MenuTick);
                    }
                    break;
            }
        }
        public override void OnChatButtonClicked(bool firstButton, ref string shopName)
        {
            shopName = "Shop";
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
            //Vanity
            shop.Add(new Item(ItemID.MoonMask)
            {
                shopCustomPrice = 2,
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
        }
    }
}
