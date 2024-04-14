namespace Windfall.Content.NPCs.WorldEvents.LunarCult
{
    public class LunarBishop : ModNPC
    {
        private enum DialogueState
        {
            Initial,
            Knowledge,
            Balance,
            WhyMe,
            Goal,
            Tablet,
            End,
            Despawn,
        }
        private DialogueState CurrentDialogue = 0;
        public override string Texture => "Windfall/Assets/NPCs/WorldEvents/LunarBishop";
        internal static SoundStyle SpawnSound => new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");
        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            //NPCID.Sets.ActsLikeTownNPC[Type] = true;
            Main.npcFrameCount[Type] = 1;
            NPCID.Sets.NoTownNPCHappiness[Type] = true;
        }
        public override void SetDefaults()
        {
            NPC.friendly = true; // NPC Will not attack player
            NPC.width = 36;
            NPC.height = 58;
            NPC.aiStyle = 0;
            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 500;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 1f;
            NPC.immortal = true;
        }
        public override bool CanChat()
        {
            if (NPC.ai[2] == 0)
                return false;
            return true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            if (NPC.ai[2] == 0)
            {
                NPC.alpha = 255;
                Vector2 oldPos = NPC.position;
                NPC.position.Y = Utilities.FindGroundVertical(new Point((int)NPC.position.X, (int)NPC.position.Y)).Y - NPC.height - 8;
                float altY = 0;
                for (int i = 0; i < 16; i++)
                {
                    altY = Utilities.FindGroundVertical(new Point((int)(oldPos.X + i), (int)(oldPos.Y - 64))).Y - NPC.height - 8;
                    if (altY < NPC.position.Y)
                        NPC.position.Y = altY;
                }
                NPC.alpha = 0;
                for (int i = 0; i <= 50; i++)
                {
                    int dustStyle = Main.rand.NextBool() ? 66 : 263;
                    Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                    Dust dust = Dust.NewDustPerfect(NPC.Center, Main.rand.NextBool(3) ? 191 : dustStyle, speed * 3, Scale: Main.rand.NextFloat(1.5f, 2.3f));
                    dust.noGravity = true;
                    dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                }
                SoundEngine.PlaySound(SpawnSound, NPC.Center);
            }
        }
        public override string GetChat()
        {
            return GetWindfallTextValue($"Dialogue.LunarCult.LunarBishop.Conversation.{CurrentDialogue}");
        }
        private readonly List<dialogueDirections> MyDialogue = new()
        {
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Initial,
                Button1 = new(){name = "Knowledge?", heading = (int)DialogueState.Knowledge},
                Button2 = new(){name = "Balance?", heading = (int)DialogueState.Balance},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Knowledge,
                Button1 = new(){name = "Why me?", heading = (int)DialogueState.WhyMe},
                Button2 = new(){name = "Your goal?", heading = (int)DialogueState.Goal},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Balance,
                Button1 = new(){name = "Why me?", heading = (int)DialogueState.WhyMe},
                Button2 = new(){name = "Your goal?", heading = (int)DialogueState.Goal},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.WhyMe,
                Button1 = new(){name = "What's this tablet?", heading = (int)DialogueState.Tablet},
                Button2 = null,
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Goal,
                Button1 = new(){name = "What's this tablet?", heading = (int)DialogueState.Tablet},
                Button2 = null,
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Tablet,
                Button1 = new(){name = "I see...", heading = (int)DialogueState.End},
                Button2 = new(){name = "Cool!", heading = (int)DialogueState.End},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.End,
                Button1 = new(){name = "Goodbye!", heading = (int)DialogueState.Despawn, end = true},
                Button2 = new(){name = "Finally...", heading = (int)DialogueState.Despawn, end = true},
            },
        };
        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            CurrentDialogue = (DialogueState)GetNPCConversation(MyDialogue, (int)CurrentDialogue, firstButton);
            if(CurrentDialogue == DialogueState.Despawn)
            {
                for (int i = 0; i <= 50; i++)
                {
                    int dustStyle = Main.rand.NextBool() ? 66 : 263;
                    Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                    Dust dust = Dust.NewDustPerfect(NPC.Center, Main.rand.NextBool(3) ? 191 : dustStyle, speed * 3, Scale: Main.rand.NextFloat(1.5f, 2.3f));
                    dust.noGravity = true;
                    dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                }
                SoundEngine.PlaySound(SpawnSound, NPC.Center);
                NPC.active = false;
                return;
            }
            Main.npcChatText = GetWindfallTextValue($"Dialogue.LunarCult.LunarBishop.Conversation.{CurrentDialogue}");
        }
        public override void SetChatButtons(ref string button, ref string button2)
        {
            SetConversationButtons(MyDialogue, (int)CurrentDialogue, ref button, ref button2);
        }
        public override bool CheckActive()
        {
            if (NPC.ai[2] == 0)
                return false;
            return true;
        }
    }
}
