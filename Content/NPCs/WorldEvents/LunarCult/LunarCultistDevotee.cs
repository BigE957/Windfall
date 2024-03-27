namespace Windfall.Content.NPCs.WanderingNPCs
{
    public class LunarCultistDevotee : ModNPC
    {
        private enum DialogueState
        {
            Initial,
            SomethingAmazing,
            TheBishop,
            End
        }
        private DialogueState CurrentDialogue
        {
            get => (DialogueState)NPC.ai[1];
            set => NPC.ai[1] = (int)value;
        }
        public override string Texture => "Windfall/Assets/NPCs/WorldEvents/LunarCultistDevotee";
        internal static SoundStyle SpawnSound => new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");
        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            NPCID.Sets.ActsLikeTownNPC[Type] = true;
            Main.npcFrameCount[Type] = 6;
            NPCID.Sets.NoTownNPCHappiness[Type] = true;
        }
        public override void SetDefaults()
        {
            NPC.friendly = true; // NPC Will not attack player
            NPC.width = 34;
            NPC.height = 52;
            NPC.aiStyle = 0;
            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 400;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 1f;
            NPC.immortal = true;

            AnimationType = NPCID.CultistDevote;
        }
        public override void OnSpawn(IEntitySource source)
        {
            if (NPC.ai[0] == 0)
            {
                AnimationType = NPCID.BartenderUnconscious;
                NPC.frame.Y = (NPC.height + 2) * 3;
                NPC.alpha = 255;
                Vector2 oldPos = NPC.position;
                NPC.position.Y = GetSurfacePositionFrom(NPC.position).Y - NPC.height - 8;
                float altY = 0;
                for (int i = 0; i < 16; i++)
                {
                    altY = GetSurfacePositionFrom(new Vector2(oldPos.X + i, oldPos.Y - 64)).Y - NPC.height - 8;
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
        }
        public override bool CanChat()
        {
            if (NPC.ai[0] == 0)
                return false;
            else
                return true;
        }
        public override string GetChat()
        {
            return GetWindfallTextValue($"Dialogue.LunarCult.OcularDevotee.{CurrentDialogue}");
        }
        private readonly List<dialogueDirections> MyDialogue = new()
        {
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Initial,
                Button1 = new(){name = "What are you doing?", heading = (int)DialogueState.SomethingAmazing},
                Button2 = new(){name = "The Bishop?", heading = (int)DialogueState.TheBishop},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.SomethingAmazing,
                Button1 = new(){name = "Interesting.", heading = (int)DialogueState.End},
                Button2 = new(){name = "That sounds...", heading = (int)DialogueState.End},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.TheBishop,
                Button1 = new(){name = "I'll keep that in mind!", heading = (int)DialogueState.End},
                Button2 = new(){name = "No thanks...", heading = (int)DialogueState.End},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.End,
                Button1 = new(){name = "Alright.", heading = -1, end = true},
                Button2 = new(){name = "If you say so...", heading = -1, end = true},
            },
        };
        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            CurrentDialogue = (DialogueState)GetNPCConversation(MyDialogue, (int)CurrentDialogue, firstButton);
            Main.npcChatText = GetWindfallTextValue($"Dialogue.LunarCult.OcularDevotee.{CurrentDialogue}");
        }
        public override void SetChatButtons(ref string button, ref string button2)
        {
            SetConversationButtons(MyDialogue, (int)CurrentDialogue, ref button, ref button2);
        }
        public override bool CheckActive()
        {
            return false;
        }
    }
}
