using Windfall.Content.Projectiles.NPCAnimations;

namespace Windfall.Content.Items.Debug
{
    public class DungeonLocationChecker : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Debug";
        public override string Texture => "CalamityMod/Items/Weapons/Summon/StaffOfNecrosteocytes";
        public override void SetDefaults()
        {
            Item.width = 25;
            Item.height = 29;
            Item.rare = ItemRarityID.Red;
            Item.useAnimation = Item.useTime = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
        }
        public override bool? UseItem(Player player)
        {
            //NPC.NewNPCDirect(Entity.GetSource_FromAI(), new Vector2(Main.dungeonX, Main.dungeonY).ToWorldCoordinates(), NPCID.Zombie);
            Projectile.NewProjectile(Entity.GetSource_FromAI(), new Vector2(Main.dungeonX, Main.dungeonY).ToWorldCoordinates(), Vector2.Zero, ModContent.ProjectileType<LunarBishopProj>(), 0, 0);
            Main.NewText($"Your dungeon is located at X: {Main.dungeonX * 16}, Y: {Main.dungeonY * 16}", Color.Yellow);
            Main.NewText($"Your position is at X: {player.position.X}, Y: {player.position.Y}", Color.Yellow);
            return true;
        }
    }
}
