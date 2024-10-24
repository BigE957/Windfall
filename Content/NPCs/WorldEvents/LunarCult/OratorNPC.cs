using Windfall.Common.Systems.WorldEvents;
using DialogueHelper.Content.UI.Dialogue;
using CalamityMod.Items.Accessories;
using Windfall.Common.Utils;
using Windfall.Content.Items.Fishing;
using Windfall.Content.Items.Utility;
using Windfall.Content.Items.Weapons.Misc;
using Windfall.Content.Items.Quest;
using Terraria;
using Terraria.ModLoader;
using Windfall.Content.Items.Lore;
using Terraria.Map;

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
                    orator = Main.npc.First(n => n.active && n.type == ModContent.NPCType<OratorNPC>() && n.ai[0] == (int)States.RitualEvent);
                    LunarCultBaseSystem.State = LunarCultBaseSystem.SystemStates.Ritual;
                    LunarCultBaseSystem.Active = true;
                    orator.ai[0] = 0;
                    break;
                case "TheOrator/BetrayalChat":
                    orator = Main.npc.First(n => n.active && n.type == ModContent.NPCType<OratorNPC>() && n.ai[0] == (int)States.TutorialChat);
                    LunarCultBaseSystem.BetrayalActive = true;
                    orator.ai[0] = 0;
                    break;
                case "TheOrator/Default":
                    if(buttonID == 0)
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
            shop.Add(new Item(ModContent.ItemType<RiftWeaver>())
            {
                shopCustomPrice = 3,
                shopSpecialCurrency = Windfall.LunarCoinCurrencyID
            });
            shop.Add(new Item(ModContent.ItemType<Moonlight>())
            {
                shopCustomPrice = 2,
                shopSpecialCurrency = Windfall.LunarCoinCurrencyID
            });
            shop.Register();
        }
    }
}
