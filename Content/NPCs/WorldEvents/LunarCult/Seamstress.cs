﻿using CalamityMod.UI;
using Humanizer;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Enums;
using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.Items.Quest;
using Windfall.Content.Items.Quest.Seamstress;
using Windfall.Content.Items.Summoning;
using Windfall.Content.UI.Events;
using static Windfall.Common.Systems.WorldEvents.LunarCultActivitySystem;

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
        private bool EndActivity = false;

        private bool Test = false;

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
            NPC.knockBackResist = 1f;
            NPC.immortal = true;

            ClothingIDs = new List<int>()
            {
                ModContent.ItemType<LunarDevoteeMask>(),
                ModContent.ItemType<LunarArcherMask>(),
                ModContent.ItemType<LunarBishopMask>(),
                ModContent.ItemType<LunarCultistHood>(),
                ModContent.ItemType<LunarBishopHood>(),
                ModContent.ItemType<LunarCultistRobes>(),
                ModContent.ItemType<LunarBishopRobes>(),
            };

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
            if(TalkDelay > 0)
                TalkDelay--;
            if (BeginActivity)
            {
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
                yapCounter++;
            }
            else if (EndActivity)
            {
                Rectangle location = new((int)NPC.Center.X, (int)NPC.Center.Y, NPC.width, NPC.width);

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
                        Item i = Main.item[Item.NewItem(Entity.GetSource_Loot(), NPC.Center, new Vector2(8, 4), ModContent.ItemType<RuneOfGnosi>())];
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
                yapCounter++;
            }
            else
                yapCounter = 0;
        }
        public override bool CanChat() => TalkDelay <= 0 && yapCounter == 0;
        public override string GetChat()
        {
            if (LunarCultActivitySystem.IsTailorActivityActive())
            {
                string activityPath = "Dialogue.LunarCult.Seamstress.Activity.";
                Main.CloseNPCChatOrSign();
                Player MyPlayer = Main.player[Main.myPlayer];
                if (CompletedClothesCount >= ClothesGoal)
                {
                    EndActivity = true;
                    return "Done!";
                }
                else
                {
                    if (AssignedClothing[Main.myPlayer] == 0)
                    {
                        AssignedClothing[Main.myPlayer] = ClothingIDs[Main.rand.Next(ClothingIDs.Count)];

                        Rectangle location = new((int)NPC.Center.X, (int)NPC.Center.Y, NPC.width, NPC.width);

                        Item item = new Item();
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
                                i = Main.item[Item.NewItem(Entity.GetSource_Loot(), NPC.Center, new Vector2(8, 4), ItemID.Cobweb, 164)];
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
                            CompletedClothesCount++;
                        }
                        else
                        {
                            Rectangle location = new((int)NPC.Center.X, (int)NPC.Center.Y, NPC.width, NPC.width);

                            Item item = new Item();
                            item.SetDefaults(AssignedClothing[Main.myPlayer]);
                            string name = item.Name;

                            CombatText.NewText(location, Color.LimeGreen, GetWindfallTextValue(activityPath + "Repeat." + Main.rand.Next(3)).FormatWith(name), true);
                        }
                    }
                }
                TalkDelay = 120;
                return "Erm, what the sigma?";
            }
            else if (Test)//(Main.moonPhase == (int)MoonPhase.HalfAtLeft || Main.moonPhase == (int)MoonPhase.HalfAtRight)
                return "So you're the runt who's gonna be helping me tonight? Great... Just lemme know when you're ready to get started.";
            else
                return "Why are you talking to me.";
        }
        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            Main.CloseNPCChatOrSign();
            if (Test == false)
                Test = true;
            if (!firstButton)
            {
                CompletedClothesCount = 0;
                State = SystemState.Tailor;
                Active = true;
                BeginActivity = true;
            }
        }
        public override void SetChatButtons(ref string button, ref string button2)
        {
            if (!LunarCultActivitySystem.IsTailorActivityActive())
            {
                if (Test)//(Main.moonPhase == (int)MoonPhase.HalfAtLeft || Main.moonPhase == (int)MoonPhase.HalfAtRight)
                {
                    button = "Maybe later.";
                    button2 = "I'm ready!";
                }
                else
                    button = "Oh, okay...";
            }
        }
        public override bool CheckActive() => false;

        private static bool HasItem(Player player, int id)
        {
            if (player.inventory.Where(i => i.type == id).Any())
            {
                player.inventory.Where(i => i.type == id).First().stack--;
                return true;
            }
            return false;
        }
    }
}