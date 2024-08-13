namespace Windfall.Content.NPCs.WanderingNPCs
{
    public class LunarCultistArcher : ModNPC
    {
        /*
        private enum DialogueState
        {
            Initial,
            WhoAreWe,
            WannaJoin,
            NuhUh,
            ExploringHuh,
            FairEnough,
            SomethingBad,
            End
        }
        private DialogueState CurrentDialogue
        {
            get => (DialogueState)NPC.ai[1];
            set => NPC.ai[1] = (int)value;
        }
        */
        private enum States
        {
            Idle,
            Chatting,
            CafeteriaEvent,
        }
        private States AIState
        {
            get => (States)NPC.ai[2];
            set => NPC.ai[2] = (float)value;
        }
        public override string Texture => "Windfall/Assets/NPCs/WorldEvents/LunarCultistArcher";
        private static SoundStyle SpawnSound => new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");
        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            NPCID.Sets.ActsLikeTownNPC[Type] = true;
            Main.npcFrameCount[Type] = 5;
            NPCID.Sets.NoTownNPCHappiness[Type] = true;
        }
        public override void SetDefaults()
        {
            NPC.friendly = true; // NPC Will not attack player
            NPC.width = 38;
            NPC.height = 52;
            NPC.aiStyle = 0;
            NPC.damage = 45;
            NPC.defense = 14;
            NPC.lifeMax = 210;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 1f;
            NPC.immortal = true;

            AnimationType = NPCID.BartenderUnconscious;
        }    
        public override void OnSpawn(IEntitySource source)
        {
            if (AIState == States.Idle)
            {
                NPC.GivenName = "Lunar Cultist Archer";
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
        }
        public override bool CheckActive() => false;

        /*
        public override bool CanChat()
        {
            if (NPC.ai[0] == 0)
                return false;
            else
                return true;
        }
        public override string GetChat() => GetWindfallTextValue($"Dialogue.LunarCult.MechanicShed.{CurrentDialogue}");
        
        private readonly List<dialogueDirections> MyDialogue = new()
        {
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Initial,
                Button1 = new(){name = "Your guardian?", heading = (int)DialogueState.ExploringHuh},
                Button2 = new(){name = "What issues?", heading = (int)DialogueState.WhoAreWe},
            },
        };
        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            CurrentDialogue = (DialogueState)GetNPCConversation(MyDialogue, (int)CurrentDialogue, firstButton);
            if (CurrentDialogue == DialogueState.End)
            {
                foreach (NPC npc in Main.npc.Where(n => n.type == NPC.type && n.active))
                {
                    for (int i = 0; i < 50; i++)
                    {
                        Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                        Dust d = Dust.NewDustPerfect(npc.Center, DustID.GoldFlame, speed * 3, Scale: 1.5f);
                        d.noGravity = true;
                    }
                    npc.active = false;
                }
            }
            else
                Main.npcChatText = GetWindfallTextValue($"Dialogue.LunarCult.MechanicShed.{CurrentDialogue}");
        }
        public override void SetChatButtons(ref string button, ref string button2)
        {
            SetConversationButtons(MyDialogue, (int)CurrentDialogue, ref button, ref button2);
        }
        */
    }
}
