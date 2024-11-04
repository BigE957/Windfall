using CalamityMod.Projectiles.Ranged;
using CalamityMod.Projectiles.Rogue;
using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.NPCs.Bosses.Orator;
using Windfall.SubModules.DialogueHelper.UI.Dialogue;

namespace Windfall.Content.NPCs.WorldEvents.LunarCult
{
    public class RecruitableLunarCultist : ModNPC
    {
        public override string Texture => "Windfall/Assets/NPCs/WorldEvents/Recruits/Recruits_Cultist";
        internal static SoundStyle SpawnSound => new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");
        private static int TalkingTo = -1;
        public enum RecruitNames
        {
            Tirith,
            Vivian,
            Tania,
            Doro,
            Skylar,
            Jamie,
        }
        public RecruitNames MyName;
        private enum DialogueState
        {
            #region General Use
            Talkable,
            Recruited,
            Recruitable,
            Unrecruited,
            #endregion
        }
        private DialogueState State;
        public bool Chattable = false;
        public bool canRecruit = false;
        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            NPCID.Sets.ActsLikeTownNPC[Type] = true;
            NPCID.Sets.AllowDoorInteraction[Type] = true;
            Main.npcFrameCount[Type] = 25;
            NPCID.Sets.ExtraFramesCount[Type] = 9;
            NPCID.Sets.AttackFrameCount[Type] = 4;
            NPCID.Sets.DangerDetectRange[Type] = 60;
            NPCID.Sets.AttackType[Type] = 2;
            NPCID.Sets.AttackTime[Type] = 12;
            NPCID.Sets.AttackAverageChance[Type] = 1;
            NPCID.Sets.HatOffsetY[Type] = 4;
            NPCID.Sets.ShimmerTownTransform[Type] = false;
            NPCID.Sets.NoTownNPCHappiness[Type] = true;
            //NPCID.Sets.FaceEmote[Type] = ModContent.EmoteBubbleType<RecruitableLunarCultist>();

            // Influences how the NPC looks in the Bestiary
            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new()
            {
                Velocity = 1f,
                Direction = 1
            };

            ModContent.GetInstance<DialogueUISystem>().DialogueClose += CloseEffect;
        }
        public override void SetDefaults()
        {
            NPC.friendly = true; // NPC Will not attack player
            NPC.width = 18;
            NPC.height = 46;
            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 400;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.immortal = true;

            TownNPCStayingHomeless = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.GivenName = MyName.ToString();
            if (Chattable == false)
            {
                AnimationType = NPCID.BartenderUnconscious;
                NPC.frame.Y = 0;

                if (NPC.FindFirstNPC(ModContent.NPCType<LunarBishop>()) != -1)
                {
                    NPC Bishop = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<LunarBishop>())];
                    if (Bishop != null)
                        if (NPC.position.X < Bishop.position.X)
                            NPC.direction = 1;
                        else
                            NPC.direction = -1;
                }
            }
            else
                State = DialogueState.Talkable;
        }
        public override bool PreAI()
        {
            if (Chattable)
            {
                AnimationType = NPCID.Stylist;
                NPC.aiStyle = NPCAIStyleID.Passive;
                AIType = NPCID.SkeletonMerchant;
            }
            else
            {
                if (NPC.FindFirstNPC(ModContent.NPCType<LunarBishop>()) != -1)
                {
                    NPC Bishop = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<LunarBishop>())];
                    if (Bishop != null)
                        if (NPC.position.X < Bishop.position.X)
                            NPC.direction = 1;
                        else
                            NPC.direction = -1;
                }
            }
            return true;
        }
        public override bool CanChat() => Chattable && NPC.FindFirstNPC(ModContent.NPCType<TheOrator>()) == -1;
        public override string GetChat()
        {
            Main.CloseNPCChatOrSign();
            TalkingTo = (int)MyName;
            if (State == DialogueState.Talkable)
            {
                if(LunarCultBaseSystem.CurrentMeetingTopic == 0)
                    ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "Recruits/" + MyName + "/Default");
                else
                    ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "Recruits/" + MyName + "/" + LunarCultBaseSystem.CurrentMeetingTopic);
            }
            else
                ModContent.GetInstance<DialogueUISystem>().DisplayDialogueTree(Windfall.Instance, "Recruits/" + MyName + "/" + State);
            return base.GetChat();
        }
        private void CloseEffect(string treeKey, int dialogueID, int buttonID)
        {
            if (!treeKey.Contains("Recruits") || !treeKey.Contains(((RecruitNames)TalkingTo).ToString()))
                return;
            if (treeKey.Contains("Recruited") || treeKey.Contains("Unrecruited"))
                return;
            NPC me = Main.npc.First(n => n.active && n.type == ModContent.NPCType<RecruitableLunarCultist>() && (int)n.As<RecruitableLunarCultist>().MyName == TalkingTo);
            if (treeKey.Contains("Recruitable") && dialogueID == 1)
            {
                if (!LunarCultBaseSystem.Recruits.Contains(TalkingTo))
                    LunarCultBaseSystem.Recruits.Add(TalkingTo);
                LunarCultBaseSystem.RecruitmentsSkipped = 0;
                foreach (NPC npc in Main.npc.Where(n => n.active && n.type == ModContent.NPCType<RecruitableLunarCultist>()))
                {
                    if (npc.ModNPC is RecruitableLunarCultist recruit)
                    {
                        recruit.canRecruit = false;
                        recruit.State = DialogueState.Unrecruited;
                    }
                }
                me.As<RecruitableLunarCultist>().State = DialogueState.Recruited;
            }
            else
            {
                if(ModContent.GetInstance<DialogueUISystem>().CurrentTree.Dialogues.Length == dialogueID)
                    me.As<RecruitableLunarCultist>().State = DialogueState.Unrecruited;
                else
                    me.As<RecruitableLunarCultist>().State = DialogueState.Recruitable;
            }
            TalkingTo = -1;
        }
        public override bool CheckActive() => Chattable;
        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = 20;
            knockback = 4f;
        }
        public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
        {
            cooldown = 15;
            randExtraCooldown = 8;
        }
        public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
        {
            switch (MyName)
            {
                case (RecruitNames.Tirith):
                    projType = ProjectileID.LostSoulFriendly;
                    break;
                case (RecruitNames.Vivian):
                    projType = ModContent.ProjectileType<BrimstoneBolt>();
                    break;
                case (RecruitNames.Tania):
                    projType = ProjectileID.Typhoon;
                    break;
                case (RecruitNames.Doro):
                    projType = ModContent.ProjectileType<Brick>();
                    break;
                case (RecruitNames.Skylar):
                    projType = ProjectileID.FrostBoltSword;
                    break;
                case (RecruitNames.Jamie):
                    projType = ProjectileID.ThrowingKnife;
                    break;
            }
            attackDelay = 1;
        }
        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Width = 37;
            NPC.frame.Height = frameHeight;
            NPC.frame.X = NPC.frame.Width * (int)MyName;
            if (!Chattable)
                NPC.spriteDirection = NPC.direction;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SpriteEffects direction = SpriteEffects.None;
            if (NPC.spriteDirection == -1)
                direction = SpriteEffects.FlipHorizontally;
            Texture2D texture = ModContent.Request<Texture2D>("Windfall/Assets/NPCs/WorldEvents/Recruits/Recruits_Cultist").Value;
            Vector2 drawPosition = NPC.Center - Main.screenPosition + Vector2.UnitY * NPC.gfxOffY;
            Vector2 origin = NPC.frame.Size() * 0.5f;

            Main.spriteBatch.Draw(texture, drawPosition, NPC.frame, drawColor * NPC.Opacity, NPC.rotation, origin, NPC.scale, direction, 0f);
            return false;
        }
    }
}
