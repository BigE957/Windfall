using Terraria.GameContent.Bestiary;

namespace Windfall.Content.NPCs.Critters
{
    public class Fingerling : ModNPC
    {
        public override string Texture => "Windfall/Assets/NPCs/Critters/Fingerling";
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 4;

            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new()
            {
                Velocity = 1f,
                Direction = 1
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange([
            new FlavorTextBestiaryInfoElement(GetWindfallTextValue($"Bestiary.{nameof(Fingerling)}")),
        ]);
        }
        public override void SetDefaults()
        {
            NPC.friendly = true; // NPC Will not attack player
            NPC.width = 24;
            NPC.height = 24;
            NPC.aiStyle = NPCAIStyleID.Snail;
            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 40;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.5f;           
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.ai[3] = Main.rand.Next(6);
        }
        public override void FindFrame(int frameHeight)
        {
            NPC.frame.X = NPC.frame.Width * (int)NPC.ai[3];
            NPC.frame.Width = ModContent.Request<Texture2D>(this.Texture).Width() / 6;
            NPC.frameCounter++;
            if (NPC.frameCounter >= 8)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y >= frameHeight * (Main.npcFrameCount[Type] - 1))
                    NPC.frame.Y = 0;
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[NPC.type].Value;

            Vector2 drawPosition = NPC.Center - screenPos + (Vector2.UnitY * NPC.gfxOffY);

            SpriteEffects spriteEffects = SpriteEffects.None;
            if ((NPC.velocity.X < 0 && (NPC.rotation < 1.57f || NPC.rotation < 6.32f)) || (NPC.velocity.Y < 0 && NPC.rotation < 3.14f) || (NPC.velocity.X > 0 && (NPC.rotation < 4.71f && NPC.rotation > 0f)) || (NPC.velocity.Y > 0 && NPC.rotation < 6.32f))
                spriteEffects = SpriteEffects.FlipHorizontally;

            spriteBatch.Draw(texture, drawPosition, NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, spriteEffects, 0);
            return false;
        }
    }
}
