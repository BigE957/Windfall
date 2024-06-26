﻿namespace Windfall.Content.NPCs.WorldEvents.DragonCult
{
    public class DragonArcher : ModNPC
    {
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
            
        }
        public override bool CanChat()
        {
            return true;
        }
        public override string GetChat()
        {
            return GetWindfallTextValue($"Dialogue.DragonCult.DragonArcher.MechanicShed.{CurrentDialogue}");
        }
        private readonly List<dialogueDirections> MyDialogue = new()
        {
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.Initial,
                Button1 = new(){name = "I'm just exploring.", heading = (int)DialogueState.ExploringHuh},
                Button2 = new(){name = "Who are you?", heading = (int)DialogueState.WhoAreWe},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.ExploringHuh,
                Button1 = new(){name = "Just looking for supplies.", heading = (int)DialogueState.FairEnough},
                Button2 = new(){name = "What's it matter to you?", heading = (int)DialogueState.SomethingBad},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.WhoAreWe,
                Button1 = new(){name = "Sounds interesting.", heading = (int)DialogueState.WannaJoin},
                Button2 = new(){name = "So... a cult?", heading = (int)DialogueState.NuhUh},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.WannaJoin,
                Button1 = new(){name = "I'll keep that in mind!", heading = (int)DialogueState.End},
                Button2 = new(){name = "No thanks...", heading = (int)DialogueState.End},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.NuhUh,
                Button1 = new(){name = "I guess that makes sense.", heading = (int)DialogueState.End},
                Button2 = new(){name = "Surely...", heading = (int)DialogueState.End},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.FairEnough,
                Button1 = new(){name = "Can do.", heading = (int)DialogueState.End},
                Button2 = new(){name = "Fine.", heading = (int)DialogueState.End},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.SomethingBad,
                Button1 = new(){name = "You might be right.", heading = (int)DialogueState.End},
                Button2 = new(){name = "Are you...?", heading = -1, end = true},
            },
            new dialogueDirections()
            {
                MyPos = (int)DialogueState.End,
                Button1 = new(){name = "Thank you.", heading = -1, end = true},
                Button2 = new(){name = "Finally...", heading = -1, end = true},
            },
        };
        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            CurrentDialogue = (DialogueState)GetNPCConversation(MyDialogue, (int)CurrentDialogue, firstButton);
            if ((int)CurrentDialogue == -1)
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
                Main.npcChatText = GetWindfallTextValue($"Dialogue.DragonCult.DragonArcher.MechanicShed.{CurrentDialogue}");
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
