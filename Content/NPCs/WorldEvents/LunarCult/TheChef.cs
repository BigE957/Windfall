﻿using Windfall.Common.Systems.WorldEvents;
using DialogueHelper.UI.Dialogue;
using static Windfall.Common.Systems.WorldEvents.LunarCultBaseSystem;
using Windfall.Content.Items.Quests.Cafeteria;

namespace Windfall.Content.NPCs.WorldEvents.LunarCult;

public class TheChef : ModNPC
{
    public override string Texture => "Windfall/Assets/NPCs/WorldEvents/LunarBishop";
    private static SoundStyle SpawnSound => new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");
    private static readonly string chefPath = "Dialogue.LunarCult.TheChef.";

    private float TimeCooking
    {
        get => NPC.ai[2];
        set => NPC.ai[2] = value;
    }
    private const int CookTime = 120;
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
        ModContent.GetInstance<DialogueUISystem>().TreeInitialize += ModifyTree;
        ModContent.GetInstance<DialogueUISystem>().ButtonClick += ClickEffect;
        ModContent.GetInstance<DialogueUISystem>().TreeClose += CloseEffect;

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
    private bool dozedOff = false;
    private int interuptedTimer = 0;
    public override void AI()
    {
        if (State == SystemStates.Cafeteria)
        {
            if (AtMaxTimer >= 10 * 60)
            {
                ItemCooking = -1;
                interuptedTimer = 0;
                dozedOff = false;
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
                if (!dozedOff && interuptedTimer == 0)
                {
                    TimeCooking++;

                    if(TimeCooking == CookTime / 2 && Main.rand.NextBool(3))
                        dozedOff = true;

                    if (TimeCooking >= CookTime)
                    {
                        CombatText.NewText(NPC.Hitbox, Color.LimeGreen, GetWindfallTextValue(chefPath + "Activity.Completed." + Main.rand.Next(3)), true);

                        Item item = Main.item[Item.NewItem(Item.GetSource_NaturalSpawn(), NPC.Center, Vector2.Zero, (int)ItemCooking)];
                        item.maxStack = 1;
                        item.velocity = new Vector2(1.75f, Main.rand.NextFloat(-3, 0));
                        item.LunarCult().madeDuringCafeteriaActivity = true;
                        ItemCooking = -1;
                        TimeCooking = 0;
                    }
                }
                else if (interuptedTimer > 0)
                    interuptedTimer--;
                NPC.direction = interuptedTimer == 0 ? -1 : 1;
            }
            else
                NPC.direction = 1;

            if (dozedOff && (Main.GlobalTimeWrappedHourly - (int)Main.GlobalTimeWrappedHourly) < 0.015)
            {
                CombatText z = Main.combatText[CombatText.NewText(new((int)NPC.Center.X, (int)NPC.Bottom.Y, 1, 1), Color.LimeGreen, "Z", true)];
                z.lifeTime /= 2;
            }
        }
        NPC.spriteDirection = NPC.direction;
    }
    public override bool CanChat() => interuptedTimer == 0;
    public override string GetChat()
    {
        Main.CloseNPCChatOrSign();

        if (IsCafeteriaActivityActive())
        {
            if(dozedOff)
            {
                dozedOff = false;
                interuptedTimer = 30;
                CombatText.NewText(NPC.Hitbox, Color.LimeGreen, GetWindfallTextValue(chefPath + "Activity.Awoken." + Main.rand.Next(3)), true);
                return "";
            }
            else if(ItemCooking != -1)
            {
                interuptedTimer = 60;
                CombatText.NewText(NPC.Hitbox, Color.LimeGreen, GetWindfallTextValue(chefPath + "Activity.Interuptted." + Main.rand.Next(3)), true);
                return "";
            }
            ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "TheChef/FoodSelection", new(Name, [NPC.whoAmI]));
        }
        else if (PlannedActivity == SystemStates.Cafeteria && State == SystemStates.Ready)
            ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "TheChef/CafeteriaActivityStart", new(Name, [NPC.whoAmI]));
        else if (SealingRitualSystem.RitualSequenceSeen)
            ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "TheChef/Abandoned", new(Name, [NPC.whoAmI]));
        else
            ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "TheChef/Default", new(Name, [NPC.whoAmI]));                
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
        Rectangle frameCrop = new(0, 0, (int)(TimeCooking / CookTime * barFG.Width), barFG.Height);

        Color bgColor = Color.DarkGray * 0.5f;
        bgColor.A = 255;

        spriteBatch.Draw(barBG, drawPos, null, bgColor, 0f, barOrigin, barScale, 0f, 0f);
        spriteBatch.Draw(barFG, drawPos, frameCrop, Color.Lerp(Color.Yellow, Color.LimeGreen, TimeCooking / CookTime), 0f, barOrigin, barScale, 0f, 0f);
    }
    private static void ModifyTree(string treeKey, int dialogueID, int buttonID, bool swapped)
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
                    response[1].Requirement = false;
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
    private static void ClickEffect(string treeKey, int dialogueID, int buttonID, bool swapped)
    {
        if (!treeKey.Contains("TheChef"))
            return;
        NPC chef = Main.npc[(int)ModContent.GetInstance<DialogueUISystem>().CurrentDialogueContext.Arguments[0]];
        if (treeKey == "TheChef/FoodSelection" && dialogueID == 0)
            chef.ai[3] = MenuFoodIDs[buttonID];
        else if (treeKey == "TheChef/CafeteriaActivityStart" && dialogueID == 0 && buttonID == 1)
        {
            Item i = Main.item[Item.NewItem(chef.GetSource_Loot(), chef.Center, new Vector2(8, 4), ModContent.ItemType<ChefMenu>())];
            i.velocity = Vector2.UnitX * 4;
        }
        else if(treeKey == "TheChef/Default")
        {
            if (dialogueID == 1)
            {
                Main.LocalPlayer.LunarCult().hasRecievedChefMeal = true;
                Item item = Main.item[Item.NewItem(Item.GetSource_NaturalSpawn(), chef.Center, Vector2.Zero, MenuFoodIDs[buttonID])];
                item.velocity = new Vector2(1.75f, Main.rand.NextFloat(-3, 0));
            }
            else if (Main.LocalPlayer.LunarCult().apostleQuestTracker == 0 && dialogueID == 7)
                Main.LocalPlayer.LunarCult().apostleQuestTracker++;
        }
    }

    private static void CloseEffect(string treeKey, int dialogueID, int buttonID, bool swapped)
    {
        if (treeKey == "TheChef/Abandoned")
            Main.LocalPlayer.LunarCult().spokeToAbandonedChef = true;
        else if (treeKey == "TheChef/CafeteriaActivityStart" && dialogueID == 1)
        {
            SatisfiedCustomers = 0;
            CustomerQueue = [];

            State = SystemStates.Cafeteria;
            Active = true;
        }
    }
}
