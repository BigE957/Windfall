using CalamityMod.NPCs.TownNPCs;
using CalamityMod;
using System;
using Terraria.GameContent.Events;

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
            Main.npcFrameCount[Type] = 4;
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
        public override void OnSpawn(IEntitySource source)
        {
            
        }
        public override bool CheckActive() => false;
        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 0.2f;
            NPC.frame.Y = frameHeight * ((int)NPC.frameCounter % Main.npcFrameCount[Type]);
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
