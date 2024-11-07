using Humanizer;
using Terraria.Enums;
using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.Items.Quest;
using Windfall.Content.Items.Quest.Tailor;
using Windfall.Content.UI;
using DialogueHelper.UI.Dialogue;
using static Windfall.Common.Systems.WorldEvents.LunarCultBaseSystem;

namespace Windfall.Content.NPCs.WorldEvents.LunarCult
{
    public class Seamstress : ModNPC
    {
        public override string Texture => "Windfall/Assets/NPCs/WorldEvents/LunarBishop";
        private static SoundStyle SpawnSound => new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");
        private float TalkDelay
        {
            get => NPC.ai[1];
            set => NPC.ai[1] = value;
        }
        private bool BeginActivity = false;
        public bool EndActivity = false;

        private int yapCounter
        {
            get => (int)NPC.ai[0];
            set => NPC.ai[0] = value;
        }
        private List<int> ClothingIDs = [];
        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            NPCID.Sets.ActsLikeTownNPC[Type] = true;
            Main.npcFrameCount[Type] = 1;
            NPCID.Sets.NoTownNPCHappiness[Type] = true;
            ModContent.GetInstance<DialogueUISystem>().DialogueOpen += ModifyTree;
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

            ClothingIDs =
            [
                ModContent.ItemType<LunarDevoteeMask>(),
                ModContent.ItemType<LunarArcherMask>(),
                ModContent.ItemType<LunarBishopMask>(),
                ModContent.ItemType<LunarCultistHood>(),
                ModContent.ItemType<LunarBishopHood>(),
                ModContent.ItemType<LunarCultistRobes>(),
                ModContent.ItemType<LunarBishopRobes>(),
            ];

            AnimationType = NPCID.BartenderUnconscious;
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.GivenName = "The Seamstress";

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
            SoundEngine.PlaySound(SpawnSound, NPC.Center);
        }
        public override void AI()
        {
            if (TalkDelay > 0)
                TalkDelay--;
            if (BeginActivity)
            {
                #region Start Activity Dialogue
                switch (yapCounter)
                {
                    case 60:
                        Rectangle location = new((int)NPC.Center.X, (int)NPC.Center.Y, NPC.width, NPC.width);
                        CombatText text = Main.combatText[CombatText.NewText(location, Color.LimeGreen, "3!", true)];
                        text.lifeTime /= 2;
                        break;
                    case 120:
                        location = new((int)NPC.Center.X, (int)NPC.Center.Y, NPC.width, NPC.width);
                        text = Main.combatText[CombatText.NewText(location, Color.LimeGreen, "2!", true)];
                        text.lifeTime /= 2;
                        break;
                    case 180:
                        location = new((int)NPC.Center.X, (int)NPC.Center.Y, NPC.width, NPC.width);
                        text = Main.combatText[CombatText.NewText(location, Color.LimeGreen, "1!", true)];
                        text.lifeTime /= 2;
                        break;
                    case 240:
                        location = new((int)NPC.Center.X, (int)NPC.Center.Y, NPC.width, NPC.width);
                        CombatText.NewText(location, Color.LimeGreen, "Go!", true);

                        BeginActivity = false;
                        break;
                }
                #endregion
                yapCounter++;
            }
            else if (EndActivity)
            {
                #region End Activity Dialogue
                Rectangle location = new((int)NPC.Center.X, (int)NPC.Center.Y, NPC.width, NPC.width);
                ModContent.GetInstance<TimerUISystem>().TimerEnd();
                if (CompletedClothesCount >= ClothesGoal)
                {
                    switch (yapCounter)
                    {
                        case 60:
                            CombatText.NewText(location, Color.LimeGreen, "Wait...", true);
                            break;
                        case 180:
                            CombatText.NewText(location, Color.LimeGreen, "I can't believe it...", true);
                            break;
                        case 300:
                            CombatText text = Main.combatText[CombatText.NewText(location, Color.LimeGreen, "We met our quota!!", true)];
                            text.lifeTime /= 2;
                            break;
                        case 360:
                            CombatText.NewText(location, Color.LimeGreen, "Great job!", true);
                            break;
                        case 480:
                            CombatText.NewText(location, Color.LimeGreen, "I suppose you've earned this...", true);
                            break;
                        case 540:
                            Item i = Main.item[Item.NewItem(NPC.GetSource_Loot(), NPC.Center, new Vector2(8, 4), ModContent.ItemType<LunarCoin>())];
                            i.velocity = (Main.player[Main.myPlayer].Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
                            break;
                        case 600:
                            CombatText.NewText(location, Color.LimeGreen, "I'll look forward to when next we work together!", true);
                            break;
                        case 720:
                            Active = false;
                            EndActivity = false;
                            break;
                    }
                }
                else
                {
                    switch (yapCounter)
                    {
                        case 60:
                            CombatText.NewText(location, Color.LimeGreen, "Alright, y'know what.", true);
                            break;
                        case 180:
                            CombatText.NewText(location, Color.LimeGreen, "I've had it!", true);
                            break;
                        case 300:
                            CombatText text = Main.combatText[CombatText.NewText(location, Color.LimeGreen, "GET OUT!!", true)];
                            text.lifeTime /= 2;
                            break;
                        case 360:
                            CombatText.NewText(location, Color.LimeGreen, "I'll do this myself.", true);
                            break;
                        case 480:
                            CombatText.NewText(location, Color.LimeGreen, "Can't trust anyone for help these days...", true);
                            break;
                        case 600:
                            Active = false;
                            EndActivity = false;
                            break;
                    }
                }
                #endregion
                yapCounter++;
            }
            else
                yapCounter = 0;
        }
        public override bool CanChat() => TalkDelay <= 0 && yapCounter == 0;
        public bool DeclinedStartActivity = false;
        public override string GetChat()
        {
            Main.CloseNPCChatOrSign();
            Player MyPlayer = Main.player[Main.myPlayer];

            if (IsTailorActivityActive())
            {
                #region Active Activity
                string activityPath = "Dialogue.LunarCult.Seamstress.Activity.";
                if (CompletedClothesCount >= ClothesGoal)
                {
                    EndActivity = true;
                    foreach (Player player in Main.player.Where(p => p.active && CalamityUtils.ManhattanDistance(NPC.Center, p.Center) < 600f))
                        player.LunarCult().SeamstressTalked = true;
                    return "Done!";
                }
                else
                {
                    if (AssignedClothing[Main.myPlayer] == 0)
                    {
                        AssignedClothing[Main.myPlayer] = ClothingIDs[Main.rand.Next(ClothingIDs.Count)];

                        Rectangle location = new((int)NPC.Center.X, (int)NPC.Center.Y, NPC.width, NPC.width);

                        Item item = new();
                        item.SetDefaults(AssignedClothing[Main.myPlayer]);
                        string name = item.Name;

                        CombatText.NewText(location, Color.LimeGreen, GetWindfallTextValue(activityPath + "Request." + Main.rand.Next(3)).FormatWith(name), true);

                        #region Item Dispensing
                        switch (item.ModItem)
                        {
                            case LunarDevoteeMask:
                                Item i = Main.item[Item.NewItem(Entity.GetSource_Loot(), NPC.Center, new Vector2(8, 4), ItemID.Bone, 8)];
                                i.velocity = (MyPlayer.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
                                i = Main.item[Item.NewItem(Entity.GetSource_Loot(), NPC.Center, new Vector2(8, 4), ItemID.SilverDye)];
                                i.velocity = (MyPlayer.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
                                i = Main.item[Item.NewItem(Entity.GetSource_Loot(), NPC.Center, new Vector2(8, 4), ItemID.BrownDye)];
                                i.velocity = (MyPlayer.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
                                break;
                            case LunarArcherMask:
                                i = Main.item[Item.NewItem(Entity.GetSource_Loot(), NPC.Center, new Vector2(8, 4), ItemID.Bone, 8)];
                                i.velocity = (MyPlayer.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
                                i = Main.item[Item.NewItem(Entity.GetSource_Loot(), NPC.Center, new Vector2(8, 4), ItemID.SilverDye)];
                                i.velocity = (MyPlayer.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
                                i = Main.item[Item.NewItem(Entity.GetSource_Loot(), NPC.Center, new Vector2(8, 4), ItemID.BrownDye)];
                                i.velocity = (MyPlayer.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
                                break;
                            case LunarBishopMask:
                                i = Main.item[Item.NewItem(Entity.GetSource_Loot(), NPC.Center, new Vector2(8, 4), ItemID.Bone, 8)];
                                i.velocity = (MyPlayer.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
                                i = Main.item[Item.NewItem(Entity.GetSource_Loot(), NPC.Center, new Vector2(8, 4), ItemID.SilverDye)];
                                i.velocity = (MyPlayer.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
                                i = Main.item[Item.NewItem(Entity.GetSource_Loot(), NPC.Center, new Vector2(8, 4), ItemID.BrownDye)];
                                i.velocity = (MyPlayer.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
                                break;
                            case LunarCultistHood:
                                i = Main.item[Item.NewItem(Entity.GetSource_Loot(), NPC.Center, new Vector2(8, 4), ItemID.Cobweb, 84)];
                                i.velocity = (MyPlayer.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
                                i = Main.item[Item.NewItem(Entity.GetSource_Loot(), NPC.Center, new Vector2(8, 4), ItemID.GreenMushroom)];
                                i.velocity = (MyPlayer.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
                                i = Main.item[Item.NewItem(Entity.GetSource_Loot(), NPC.Center, new Vector2(8, 4), ItemID.BlackInk)];
                                i.velocity = (MyPlayer.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
                                break;
                            case LunarBishopHood:
                                i = Main.item[Item.NewItem(Entity.GetSource_Loot(), NPC.Center, new Vector2(8, 4), ItemID.Cobweb, 70)];
                                i.velocity = (MyPlayer.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
                                i = Main.item[Item.NewItem(Entity.GetSource_Loot(), NPC.Center, new Vector2(8, 4), ItemID.GreenMushroom)];
                                i.velocity = (MyPlayer.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
                                i = Main.item[Item.NewItem(Entity.GetSource_Loot(), NPC.Center, new Vector2(8, 4), ItemID.BlackInk)];
                                i.velocity = (MyPlayer.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
                                i = Main.item[Item.NewItem(Entity.GetSource_Loot(), NPC.Center, new Vector2(8, 4), ItemID.Cobweb, 70)];
                                i.velocity = (MyPlayer.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
                                i = Main.item[Item.NewItem(Entity.GetSource_Loot(), NPC.Center, new Vector2(8, 4), ItemID.FlinxFur, 2)];
                                i.velocity = (MyPlayer.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
                                i = Main.item[Item.NewItem(Entity.GetSource_Loot(), NPC.Center, new Vector2(8, 4), ItemID.CopperBar, 6)];
                                i.velocity = (MyPlayer.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
                                break;
                            case LunarCultistRobes:
                                i = Main.item[Item.NewItem(Entity.GetSource_Loot(), NPC.Center, new Vector2(8, 4), ItemID.Cobweb, 168)];
                                i.velocity = (MyPlayer.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
                                i = Main.item[Item.NewItem(Entity.GetSource_Loot(), NPC.Center, new Vector2(8, 4), ItemID.GreenMushroom)];
                                i.velocity = (MyPlayer.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
                                i = Main.item[Item.NewItem(Entity.GetSource_Loot(), NPC.Center, new Vector2(8, 4), ItemID.BlackInk)];
                                i.velocity = (MyPlayer.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
                                break;
                            case LunarBishopRobes:
                                i = Main.item[Item.NewItem(Entity.GetSource_Loot(), NPC.Center, new Vector2(8, 4), ItemID.Cobweb, 140)];
                                i.velocity = (MyPlayer.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
                                i = Main.item[Item.NewItem(Entity.GetSource_Loot(), NPC.Center, new Vector2(8, 4), ItemID.GreenMushroom)];
                                i.velocity = (MyPlayer.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
                                i = Main.item[Item.NewItem(Entity.GetSource_Loot(), NPC.Center, new Vector2(8, 4), ItemID.BlackInk)];
                                i.velocity = (MyPlayer.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
                                i = Main.item[Item.NewItem(Entity.GetSource_Loot(), NPC.Center, new Vector2(8, 4), ItemID.FlinxFur, 4)];
                                i.velocity = (MyPlayer.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
                                i = Main.item[Item.NewItem(Entity.GetSource_Loot(), NPC.Center, new Vector2(8, 4), ItemID.CopperBar, 4)];
                                i.velocity = (MyPlayer.Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
                                break;
                        }
                        #endregion
                    }
                    else
                    {
                        if (HasItem(MyPlayer, AssignedClothing[Main.myPlayer]))
                        {
                            Rectangle location = new((int)NPC.Center.X, (int)NPC.Center.Y, NPC.width, NPC.width);
                            CombatText.NewText(location, Color.LimeGreen, GetWindfallTextValue(activityPath + "Completed." + Main.rand.Next(3)), true);
                            AssignedClothing[Main.myPlayer] = 0;
                            ModContent.GetInstance<TimerUISystem>().EventTimer.timer += 30 * 60;
                            CompletedClothesCount++;
                        }
                        else
                        {
                            Rectangle location = new((int)NPC.Center.X, (int)NPC.Center.Y, NPC.width, NPC.width);

                            Item item = new();
                            item.SetDefaults(AssignedClothing[Main.myPlayer]);
                            string name = item.Name;

                            if (MyPlayer.inventory.Where(i => i.type == AssignedClothing[Main.myPlayer]).Any())
                                CombatText.NewText(location, Color.LimeGreen, GetWindfallTextValue(activityPath + "Garbage." + Main.rand.Next(3)).FormatWith(name), true);
                            else
                                CombatText.NewText(location, Color.LimeGreen, GetWindfallTextValue(activityPath + "Repeat." + Main.rand.Next(3)).FormatWith(name), true);
                        }
                    }
                }
                TalkDelay = 120;
                #endregion
            }
            else if (Main.moonPhase == (int)MoonPhase.HalfAtLeft || Main.moonPhase == (int)MoonPhase.HalfAtRight && State == SystemStates.Ready)
            {
                if (DeclinedStartActivity)
                    ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "TheSeamstress/ReActivityStart");
                else
                {
                    if (MyPlayer.LunarCult().SeamstressTalked)
                        ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "TheSeamstress/ActivityStart");
                    else
                        ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "TheSeamstress/TailorTutorial");
                }
            }
            else if (SealingRitualSystem.RitualSequenceSeen)
                ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "TheSeamstress/Abandoned");
            else
                ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "TheSeamstress/Default", MyPlayer.LunarCult().SeamstressTalked ? 1 : 0);
                
            return "";
        }
        public override bool CheckActive() => !NPC.downedAncientCultist;
        private void ModifyTree(string treeKey, int dialogueID, int buttonID)
        {
            DialogueUISystem uiSystem = ModContent.GetInstance<DialogueUISystem>();
            switch (treeKey)
            {
                case "TheSeamstress/Default":
                    int questTracker = Main.LocalPlayer.LunarCult().apostleQuestTracker;
                    List<Response> response = new(uiSystem.CurrentTree.Dialogues[2].Responses);
                    if (questTracker != 1)
                        response[2] = null;
                    if (questTracker == 3)
                        response[2] = response[3];
                    response[3] = null;
                    uiSystem.CurrentTree.Dialogues[2].Responses = response.Where(r => r != null).ToArray();

                    if (questTracker < 4)
                    {
                        response = new(uiSystem.CurrentTree.Dialogues[13].Responses)
                        {
                            [3] = null
                        };
                        uiSystem.CurrentTree.Dialogues[13].Responses = response.Where(r => r != null).ToArray();

                        response = new(uiSystem.CurrentTree.Dialogues[14].Responses)
                        {
                            [4] = null
                        };
                        uiSystem.CurrentTree.Dialogues[14].Responses = response.Where(r => r != null).ToArray();

                    }
                    break;
                case "TheSeamstress/Abandoned":
                    if(!Main.LocalPlayer.LunarCult().spokeToAbandonedChef)
                    {
                        response = new(uiSystem.CurrentTree.Dialogues[0].Responses)
                        {
                            [2] = null
                        };
                        uiSystem.CurrentTree.Dialogues[0].Responses = response.Where(r => r != null).ToArray();
                    }
                    break;
            }
        }
        private void CloseEffect(string treeKey, int dialogueID, int buttonID)
        {
            if (buttonID == 1)
            {
                if ((treeKey == "TheSeamstress/TailorTutorial" && dialogueID == 2) || treeKey == "TheSeamstress/ActivityStart" || treeKey == "TheSeamstress/ReActivityStart")
                {
                    CompletedClothesCount = 0;
                    State = SystemStates.Tailor;
                    Active = true;
                    BeginActivity = true;
                }
            }
            if (treeKey == "TheSeamstress/Default" && dialogueID == 6 && Main.LocalPlayer.LunarCult().apostleQuestTracker == 1)
                Main.LocalPlayer.LunarCult().apostleQuestTracker++;
            if (treeKey == "TheSeamstress/Default" && dialogueID == 12 && Main.LocalPlayer.LunarCult().apostleQuestTracker == 3)
                Main.LocalPlayer.LunarCult().apostleQuestTracker++;
        }

        private static bool HasItem(Player player, int id)
        {
            if (player.inventory.Where(i => i.type == id && i.LunarCult().madeDuringTailorActivity).Any())
            {
                player.inventory.Where(i => i.type == id && i.LunarCult().madeDuringTailorActivity).First().stack--;
                return true;
            }
            return false;
        }
    }
}
