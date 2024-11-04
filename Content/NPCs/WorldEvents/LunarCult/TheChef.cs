using Terraria.Enums;
using Windfall.Common.Systems.WorldEvents;
using Windfall.SubModules.DialogueHelper.UI.Dialogue;
using static Windfall.Common.Systems.WorldEvents.LunarCultBaseSystem;

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
            ModContent.GetInstance<DialogueUISystem>().DialogueOpen += ModifyTree;
            ModContent.GetInstance<DialogueUISystem>().ButtonClick += ClickEffect;
            ModContent.GetInstance<DialogueUISystem>().DialogueClose += CloseEffect;

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
            if (State == SystemStates.Cafeteria)
            {
                if (AtMaxTimer >= 10 * 60)
                {
                    ItemCooking = -1;
                    int shutdownTimer = AtMaxTimer - (10 * 60);
                    switch (shutdownTimer)
                    {
                        case 0:
                            CombatText.NewText(NPC.Hitbox, Color.LimeGreen, GetWindfallTextValue(chefPath + "Activity.Failed.0"), true);
                            break;
                        case 120:
                            CombatText.NewText(NPC.Hitbox, Color.LimeGreen, GetWindfallTextValue(chefPath + "Activity.Failed.1"), true);
                            break;
                        case 240:
                            CombatText.NewText(NPC.Hitbox, Color.LimeGreen, GetWindfallTextValue(chefPath + "Activity.Failed.2"), true);
                            break;
                        case 360:
                            CombatText.NewText(NPC.Hitbox, Color.LimeGreen, GetWindfallTextValue(chefPath + "Activity.Failed.3"), true);
                            break;
                        case 480:
                            CombatText.NewText(NPC.Hitbox, Color.LimeGreen, GetWindfallTextValue(chefPath + "Activity.Failed.4"), true);
                            break;
                        case 600:
                            Active = false;
                            break;
                    }

                }
                if (ItemCooking != -1)
                {
                    TimeCooking++;
                    if (TimeCooking >= CookTIme)
                    {
                        CombatText.NewText(NPC.Hitbox, Color.LimeGreen, GetWindfallTextValue(chefPath + "Activity.Completed." + Main.rand.Next(3)), true);

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
            }
            NPC.spriteDirection = NPC.direction;
        }
        public override bool CanChat() => ItemCooking == -1;
        public override string GetChat()
        {
            Main.CloseNPCChatOrSign();

            if (IsCafeteriaActivityActive())
                ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "TheChef/FoodSelection");               
            else if ((Main.moonPhase == (int)MoonPhase.ThreeQuartersAtLeft || Main.moonPhase == (int)MoonPhase.ThreeQuartersAtRight) && State == SystemStates.Ready)
                ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "TheChef/CafeteriaActivityStart");
            else if(SealingRitualSystem.RitualSequenceSeen)
                ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "TheChef/Abandoned");
            else
                ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "TheChef/Default");                
            return "Hey chat!";
        }
        public override bool CheckActive() => !NPC.downedAncientCultist;
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (ItemCooking == -1)
                return;
            float barScale = 1.34f;

            var barBG = ModContent.Request<Texture2D>("CalamityMod/UI/MiscTextures/GenericBarBack").Value;
            var barFG = ModContent.Request<Texture2D>("CalamityMod/UI/MiscTextures/GenericBarFront").Value;

            Vector2 barOrigin = barBG.Size() * 0.5f;
            float yOffset = 23f;
            Vector2 drawPos = (NPC.Center - screenPos) + Vector2.UnitY * barScale * (NPC.frame.Height - yOffset);
            Rectangle frameCrop = new(0, 0, (int)(TimeCooking / CookTIme * barFG.Width), barFG.Height);

            Color bgColor = Color.DarkGray * 0.5f;
            bgColor.A = 255;

            spriteBatch.Draw(barBG, drawPos, null, bgColor, 0f, barOrigin, barScale, 0f, 0f);
            spriteBatch.Draw(barFG, drawPos, frameCrop, Color.Lerp(Color.Yellow, Color.LimeGreen, TimeCooking / CookTIme), 0f, barOrigin, barScale, 0f, 0f);
        }
        private void ModifyTree(string treeKey, int dialogueID, int buttonID)
        {
            DialogueUISystem uiSystem = ModContent.GetInstance<DialogueUISystem>();
            switch (treeKey)
            {
                case "TheChef/FoodSelection":
                    uiSystem.CurrentTree.Dialogues[0].Responses = GetMenuResponses();
                    break;
                case "TheChef/Default":
                    List<Response> response = new(uiSystem.CurrentTree.Dialogues[0].Responses);
                    if (Main.LocalPlayer.LunarCult().hasRecievedChefMeal)
                    {
                        response[1] = null;
                        uiSystem.CurrentTree.Dialogues[0].Responses = response.Where(r => r != null).ToArray();
                        return;
                    }
                    uiSystem.CurrentTree.Dialogues[1].Responses = GetMenuResponses();
                    for (int i = 0; i < uiSystem.CurrentTree.Dialogues[1].Responses.Length; i++)
                    {
                        uiSystem.CurrentTree.Dialogues[1].Responses[i].Heading = 2;
                    }
                    break;
            }
        }
        private void ClickEffect(string treeKey, int dialogueID, int buttonID)
        {
            if(treeKey == "TheChef/FoodSelection" && dialogueID == 0)
                Main.npc[NPC.FindFirstNPC(NPC.type)].ai[3] = MenuFoodIDs[buttonID];
            else if (treeKey == "TheChef/CafeteriaActivityStart" && buttonID == 1)
            {
                SatisfiedCustomers = 0;
                CustomerQueue = [];

                State = SystemStates.Cafeteria;
                Active = true;
            }
            else if(treeKey == "TheChef/Default")
            {
                if (dialogueID == 1)
                {
                    Main.LocalPlayer.LunarCult().hasRecievedChefMeal = true;
                    Item item = Main.item[Item.NewItem(Item.GetSource_NaturalSpawn(), NPC.Center, Vector2.Zero, MenuFoodIDs[buttonID])];
                    item.velocity = new Vector2(1.75f, Main.rand.NextFloat(-3, 0));
                }
                else if (Main.LocalPlayer.LunarCult().apostleQuestTracker == 0 && dialogueID == 7)
                    Main.LocalPlayer.LunarCult().apostleQuestTracker++;
            }
        }

        private void CloseEffect(string treeKey, int dialogueID, int buttonID)
        {
            if (treeKey == "TheChef/Abandoned")
                Main.LocalPlayer.LunarCult().spokeToAbandonedChef = true;
        }
    }
}
