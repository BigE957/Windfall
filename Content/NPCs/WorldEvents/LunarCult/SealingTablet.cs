namespace Windfall.Content.NPCs.WorldEvents.LunarCult
{
    public class SealingTablet : ModNPC
    {
        private enum DialogueState
        {
            Initial,
            Guardian,
            Issues,
            End
        }
        public override string Texture => "Windfall/Assets/NPCs/WorldEvents/SealingTablet";
        internal static SoundStyle SpawnSound => new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");
        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            Main.npcFrameCount[Type] = 5;
        }
        public override void SetDefaults()
        {
            NPC.lifeMax = 400;
            NPC.defense = 0;
            NPC.damage = 0;
            NPC.width = 54;
            NPC.height = 54;
            NPC.aiStyle = -1;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.value = 0f;
            NPC.npcSlots = 0f;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.dontTakeDamage = true;
            NPC.netAlways = true;
            NPC.chaseable = false;
        }
        public override bool CheckActive() => false;
        public override void FindFrame(int frameHeight)
        {
            if (NPC.ai[0] == 1)
            {
                NPC.frameCounter += 0.2f;
                NPC.frame.Y = frameHeight * (((int)NPC.frameCounter % 4) + 1);
                Lighting.AddLight(NPC.Center, new Vector3(1f, 0.84f, 0f));
            }
            else
                NPC.frame.Y = 0;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[NPC.type].Value;
            Vector2 halfSizeTexture = new(TextureAssets.Npc[NPC.type].Value.Width / 2, TextureAssets.Npc[NPC.type].Value.Height / Main.npcFrameCount[NPC.type] / 2);
            Vector2 drawPosition = new Vector2(NPC.Center.X, NPC.Center.Y) - screenPos + Vector2.UnitY * NPC.gfxOffY;
            spriteBatch.Draw(texture, drawPosition, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, halfSizeTexture, NPC.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
