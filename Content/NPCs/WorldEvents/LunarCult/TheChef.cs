using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.UI.Dialogue;

namespace Windfall.Content.NPCs.WorldEvents.LunarCult
{
    public class TheChef : ModNPC
    {
        public override string Texture => "Windfall/Assets/NPCs/WorldEvents/LunarBishop";
        private static SoundStyle SpawnSound => new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");
        private float TimeCooking
        {
            get => NPC.ai[2];
            set => NPC.ai[2] = value;
        }
        private const int CookTIme = 120;
        private float ItemCooking
        {
            get => NPC.ai[3];
            set => NPC.ai[3] = value;
        }
        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            NPCID.Sets.ActsLikeTownNPC[Type] = true;
            Main.npcFrameCount[Type] = 1;
            NPCID.Sets.NoTownNPCHappiness[Type] = true;
            ModContent.GetInstance<DialogueUISystem>().ButtonClick += ClickEffect;
        }
        public override void SetDefaults()
        {
            NPC.friendly = true;
            NPC.width = 36;
            NPC.height = 58;
            NPC.damage = 45;
            NPC.defense = 14;
            NPC.lifeMax = 210;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.immortal = true;           

            AnimationType = NPCID.BartenderUnconscious;
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.GivenName = "The Chef";

            NPC.alpha = 255;
            Vector2 oldPos = NPC.position;
            NPC.position.Y = FindSurfaceBelow(new Point((int)NPC.position.X / 16, (int)NPC.position.Y / 16)).Y * 16 - NPC.height;

            float altY = 0;
            for (int i = 0; i < 2; i++)
            {
                altY = (FindSurfaceBelow(new Point((int)(oldPos.X / 16 + i), (int)(oldPos.Y / 16 - 2))).Y - 1) * 16 - NPC.height + 16;
                if (altY < NPC.position.Y)
                    NPC.position.Y = altY;
            }
            NPC.alpha = 0;
            for (int i = 0; i < 50; i++)
            {
                Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                Dust d = Dust.NewDustPerfect(NPC.Center, DustID.GoldFlame, speed * 3, Scale: 1.5f);
                d.noGravity = true;
            }
            ItemCooking = -1;
            NPC.spriteDirection = 1;
            SoundEngine.PlaySound(SpawnSound, NPC.Center);
        }
        public override void AI()
        {
            string chefPath = "Dialogue.LunarCult.TheChef.";

            if (ItemCooking != -1)
            {
                TimeCooking++;
                if (TimeCooking >= CookTIme)
                {
                    Rectangle location = new((int)NPC.Center.X, (int)NPC.Center.Y, NPC.width, NPC.width);
                    CombatText.NewText(location, Color.LimeGreen, GetWindfallTextValue(chefPath + "Activity.Completed." + Main.rand.Next(3)), true);

                    Item item = Main.item[Item.NewItem(Item.GetSource_NaturalSpawn(), NPC.Center, Vector2.Zero, (int)ItemCooking)];
                    item.maxStack = 1;
                    item.velocity = new Vector2(1.75f, Main.rand.NextFloat(-3, 0));
                    item.LunarCult().madeDuringCafeteriaActivity = true;
                    ItemCooking = -1;
                    TimeCooking = 0;
                }
                NPC.direction = -1;
            }
            else
                NPC.direction = 1;
            NPC.spriteDirection = NPC.direction;
        }
        public override bool CanChat() => ItemCooking == -1;
        public override string GetChat()
        {
            Main.CloseNPCChatOrSign();

            if (LunarCultActivitySystem.IsCafeteriaActivityActive())
            {
                DialogueHolder.DialogueTrees["FoodSelection"].Dialogues[0].Responses = LunarCultActivitySystem.GetMenuResponses();
                ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree("FoodSelection");
            }
            else
                ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree("CafeteriaActivityStart");

            return "Hey chat!";
        }
        public override void SetChatButtons(ref string button, ref string button2)
        {
            base.SetChatButtons(ref button, ref button2);
        }
        public override bool CheckActive() => false;
        private void ClickEffect(string treeKey, int dialogueID, int buttonID)
        {
            if(treeKey == "FoodSelection" && dialogueID == 0)
                Main.npc[NPC.FindFirstNPC(NPC.type)].ai[3] = LunarCultActivitySystem.MenuFoodIDs[buttonID];
            else if (treeKey == "CafeteriaActivityStart" && buttonID == 1)
            {
                LunarCultActivitySystem.SatisfiedCustomers = 0;
                LunarCultActivitySystem.State = LunarCultActivitySystem.SystemState.Cafeteria;
                LunarCultActivitySystem.Active = true;
            }
        }
    }
}
