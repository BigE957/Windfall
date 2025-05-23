using Humanizer;
using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.Items.Quests;
using Windfall.Content.Items.Quests.Tailor;
using Windfall.Content.UI;
using DialogueHelper.UI.Dialogue;
using static Windfall.Common.Systems.WorldEvents.LunarCultBaseSystem;
using Windfall.Content.Items.Quests.Cafeteria;
using CalamityMod;
using Windfall.Common.Systems;

namespace Windfall.Content.NPCs.WorldEvents.LunarCult;

public class Seamstress : ModNPC
{
    public override string Texture => "Windfall/Assets/NPCs/WorldEvents/Seamstress";
    private static SoundStyle SpawnSound => new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");
    private static readonly string seamstressPath = "Dialogue.LunarCult.Seamstress.";
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

    private enum Animation
    {
        Idle,
        Shock,
        Clap
    }
    private Animation CurrentAnimation
    {
        get => (Animation)NPC.ai[2];
        set => NPC.ai[2] = (float)value;
    }

    private List<int> ClothingIDs = [];
    public override void SetStaticDefaults()
    {
        this.HideFromBestiary();
        NPCID.Sets.ActsLikeTownNPC[Type] = true;
        Main.npcFrameCount[Type] = 10;
        NPCID.Sets.NoTownNPCHappiness[Type] = true;
        ModContent.GetInstance<DialogueUISystem>().TreeInitialize += ModifyTree;
        ModContent.GetInstance<DialogueUISystem>().TreeClose += CloseEffect;
    }
    public override void SetDefaults()
    {
        NPC.friendly = true;
        NPC.width = 56;
        NPC.height = 64;
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
        CurrentAnimation = Animation.Idle;
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

    int Time = 0;
    public override void AI()
    {
        if (TalkDelay > 0)
            TalkDelay--;
        if (BeginActivity)
        {
            #region Start Activity Dialogue
            switch (yapCounter)
            {
                case 0:
                    Item i = Main.item[Item.NewItem(Entity.GetSource_Loot(), NPC.Center, new Vector2(8, 4), ModContent.ItemType<TailorInstructions>())];
                    i.velocity = Vector2.UnitX * -4;
                    break;
                case 60:
                    Rectangle location = new((int)NPC.Center.X, (int)NPC.Center.Y, NPC.width, NPC.width);
                    CombatText text = Main.combatText[CombatText.NewText(location, Color.LimeGreen, GetWindfallTextValue(seamstressPath + "Activity.Start.0"), true)];
                    text.lifeTime /= 2;
                    break;
                case 120:
                    location = new((int)NPC.Center.X, (int)NPC.Center.Y, NPC.width, NPC.width);
                    text = Main.combatText[CombatText.NewText(location, Color.LimeGreen, GetWindfallTextValue(seamstressPath + "Activity.Start.1"), true)];
                    text.lifeTime /= 2;
                    break;
                case 180:
                    location = new((int)NPC.Center.X, (int)NPC.Center.Y, NPC.width, NPC.width);
                    text = Main.combatText[CombatText.NewText(location, Color.LimeGreen, GetWindfallTextValue(seamstressPath + "Activity.Start.2"), true)];
                    text.lifeTime /= 2;
                    break;
                case 240:
                    location = new((int)NPC.Center.X, (int)NPC.Center.Y, NPC.width, NPC.width);
                    CombatText.NewText(location, Color.LimeGreen, GetWindfallTextValue(seamstressPath + "Activity.Start.3"), true);
                    ModContent.GetInstance<TimerUISystem>().TimerStart(2 * 60 * 60);
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
                        CombatText.NewText(location, Color.LimeGreen, GetWindfallTextValue(seamstressPath + "Activity.GoodEnd.0"), true);
                        break;
                    case 180:
                        CombatText.NewText(location, Color.LimeGreen, GetWindfallTextValue(seamstressPath + "Activity.GoodEnd.1"), true);
                        break;
                    case 300:
                        CombatText text = Main.combatText[CombatText.NewText(location, Color.LimeGreen, GetWindfallTextValue(seamstressPath + "Activity.GoodEnd.2"), true)];
                        text.lifeTime /= 2;
                        break;
                    case 360:
                        CombatText.NewText(location, Color.LimeGreen, GetWindfallTextValue(seamstressPath + "Activity.GoodEnd.3"), true);
                        break;
                    case 480:
                        CombatText.NewText(location, Color.LimeGreen, GetWindfallTextValue(seamstressPath + "Activity.GoodEnd.4"), true);
                        break;
                    case 540:
                        Item i = Main.item[Item.NewItem(NPC.GetSource_Loot(), NPC.Center, new Vector2(8, 4), ModContent.ItemType<LunarCoin>())];
                        i.velocity = (Main.player[Main.myPlayer].Center - NPC.Center).SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
                        CurrentAnimation = Animation.Clap;
                        break;
                    case 600:
                        CombatText.NewText(location, Color.LimeGreen, GetWindfallTextValue(seamstressPath + "Activity.GoodEnd.5"), true);
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
                        CombatText.NewText(location, Color.LimeGreen, GetWindfallTextValue(seamstressPath + "Activity.BadEnd.0"), true);
                        break;
                    case 180:
                        CombatText.NewText(location, Color.LimeGreen, GetWindfallTextValue(seamstressPath + "Activity.BadEnd.1"), true);
                        break;
                    case 300:
                        CombatText text = Main.combatText[CombatText.NewText(location, Color.LimeGreen, GetWindfallTextValue(seamstressPath + "Activity.BadEnd.2"), true)];
                        text.lifeTime /= 2;
                        break;
                    case 360:
                        CombatText.NewText(location, Color.LimeGreen, GetWindfallTextValue(seamstressPath + "Activity.BadEnd.3"), true);
                        break;
                    case 480:
                        CombatText.NewText(location, Color.LimeGreen, GetWindfallTextValue(seamstressPath + "Activity.BadEnd.4"), true);
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

    public override bool CanChat() => !QuestSystem.Quests["DraconicBone"].Complete && !ModContent.GetInstance<DialogueUISystem>().isDialogueOpen && TalkDelay <= 0 && yapCounter == 0;
    public bool DeclinedStartActivity = false;
    public override string GetChat()
    {
        Main.CloseNPCChatOrSign();
        Player MyPlayer = Main.player[Main.myPlayer];

        if (IsTailorActivityActive())
        {
            #region Active Activity
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

                    CombatText.NewText(location, Color.LimeGreen, GetWindfallTextValue(seamstressPath + "Activity.Request." + Main.rand.Next(3)).FormatWith(name), true);
                    CurrentAnimation = Animation.Shock;

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
                        CombatText.NewText(location, Color.LimeGreen, GetWindfallTextValue(seamstressPath + "Activity.Completed." + Main.rand.Next(3)), true);
                        AssignedClothing[Main.myPlayer] = 0;
                        ModContent.GetInstance<TimerUISystem>().EventTimer.timer += 30 * 60;
                        CompletedClothesCount++;

                        CurrentAnimation = Animation.Clap;
                    }
                    else
                    {
                        Rectangle location = new((int)NPC.Center.X, (int)NPC.Center.Y, NPC.width, NPC.width);

                        Item item = new();
                        item.SetDefaults(AssignedClothing[Main.myPlayer]);
                        string name = item.Name;

                        if (MyPlayer.inventory.Where(i => i.type == AssignedClothing[Main.myPlayer]).Any())
                            CombatText.NewText(location, Color.LimeGreen, GetWindfallTextValue(seamstressPath + "Activity.Garbage." + Main.rand.Next(3)).FormatWith(name), true);
                        else
                            CombatText.NewText(location, Color.LimeGreen, GetWindfallTextValue(seamstressPath + "Activity.Repeat." + Main.rand.Next(3)).FormatWith(name), true);

                        CurrentAnimation = Animation.Shock;
                    }
                }
            }
            TalkDelay = 120;
            #endregion
        }
        else if (PlannedActivity == SystemStates.Tailor && State == SystemStates.Ready)
        {
            if (DeclinedStartActivity)
                ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "TheSeamstress/ReActivityStart", new(Name, [NPC.whoAmI]));
            else
            {
                if (MyPlayer.LunarCult().SeamstressTalked)
                    ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "TheSeamstress/ActivityStart", new(Name, [NPC.whoAmI]));
                else
                    ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "TheSeamstress/TailorTutorial", new(Name, [NPC.whoAmI]));
            }
        }
        else if (SealingRitualSystem.RitualSequenceSeen)
            ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "TheSeamstress/Abandoned", new(Name, [NPC.whoAmI]));
        else
            ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "TheSeamstress/Default", new(Name, [NPC.whoAmI]), MyPlayer.LunarCult().SeamstressTalked ? 1 : 0);
            
        return "";
    }
    public override bool CheckActive() => !NPC.downedAncientCultist;


    public override void FindFrame(int frameHeight)
    {
        int frameWidth = TextureAssets.Npc[Type].Width() / 3;

        switch(CurrentAnimation)
        {
            case Animation.Idle:
                NPC.frame.X = 0; 

                NPC.frame.Y = 0;

                NPC.frameCounter = 0;
                break;
            case Animation.Shock:
                NPC.frame.X = frameWidth;

                if (NPC.frameCounter + 0.2f >= Main.npcFrameCount[NPC.type])
                {
                    CurrentAnimation = Animation.Idle;
                    break;
                }
                NPC.frame.Y = frameHeight * ((int)NPC.frameCounter % Main.npcFrameCount[NPC.type]);

                NPC.frameCounter += 0.2f;
                break;
            case Animation.Clap:
                NPC.frame.X = frameWidth * 2;

                if(NPC.frameCounter + 0.2f >= 13f)
                {
                    CurrentAnimation = Animation.Idle;
                    break;
                }
                NPC.frame.Y = frameHeight * ((int)NPC.frameCounter % 6);

                NPC.frameCounter += 0.2f;
                break;
        }

        NPC.frame.Width = frameWidth;
        NPC.frame.Height = frameHeight;
    }
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        SpriteEffects direction = SpriteEffects.None;
        if (NPC.spriteDirection == -1)
            direction = SpriteEffects.FlipHorizontally;
        Texture2D texture = TextureAssets.Npc[Type].Value;
        Vector2 drawPosition = NPC.Center - Main.screenPosition - Vector2.UnitY * 8;
        Vector2 origin = NPC.frame.Size() * 0.5f;

        Main.spriteBatch.Draw(texture, drawPosition, NPC.frame, drawColor * NPC.Opacity, NPC.rotation, origin, NPC.scale, direction, 0f);
        return false;
    }

    private static void ModifyTree(string treeKey, int dialogueID, int buttonID, bool swapped)
    {
        DialogueUISystem uiSystem = ModContent.GetInstance<DialogueUISystem>();
        switch (treeKey)
        {
            case "TheSeamstress/Default":
                int questTracker = 0;// Main.LocalPlayer.LunarCult().apostleQuestTracker;
                List<Response> response = new(uiSystem.CurrentTree.Dialogues[2].Responses);
                if (questTracker != 1)
                    response[2].Requirement = false;
                if (questTracker != 3)
                    response[3].Requirement = false;

                if (questTracker < 4)
                {
                    uiSystem.CurrentTree.Dialogues[13].Responses[3].Requirement = false;
                    uiSystem.CurrentTree.Dialogues[14].Responses[4].Requirement = false;
                }
                break;
            case "TheSeamstress/Abandoned":
                if(!Main.LocalPlayer.LunarCult().spokeToAbandonedChef)
                    uiSystem.CurrentTree.Dialogues[0].Responses[2].Requirement = false;
                break;
        }
    }
    private static void CloseEffect(string treeKey, int dialogueID, int buttonID, bool swapped)
    {
        DialogueUISystem uiSystem = ModContent.GetInstance<DialogueUISystem>();
        if (uiSystem.CurrentDialogueContext.Catagory != nameof(Seamstress))
            return;

        if (treeKey.Contains("TheSeamstress/"))
        {
            NPC me = Main.npc[(int)ModContent.GetInstance<DialogueUISystem>().CurrentDialogueContext.Arguments[0]];

            if (buttonID == 1)
            {
                if ((treeKey == "TheSeamstress/TailorTutorial" && dialogueID == 2) || treeKey == "TheSeamstress/ActivityStart" || treeKey == "TheSeamstress/ReActivityStart")
                {
                    CompletedClothesCount = 0;
                    State = SystemStates.Tailor;
                    Active = true;
                    me.As<Seamstress>().BeginActivity = true;
                }
            }
            if (treeKey == "TheSeamstress/Default" && dialogueID == 6 && Main.LocalPlayer.LunarCult().apostleQuestTracker == 1)
                Main.LocalPlayer.LunarCult().apostleQuestTracker++;
            if (treeKey == "TheSeamstress/Default" && dialogueID == 12 && Main.LocalPlayer.LunarCult().apostleQuestTracker == 3)
                Main.LocalPlayer.LunarCult().apostleQuestTracker++;
        }
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
