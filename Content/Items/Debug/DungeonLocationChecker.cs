using Windfall.Content.Projectiles.Boss.Orator;
using Windfall.Content.Projectiles.NPCAnimations;

namespace Windfall.Content.Items.Debug;

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
        if(Main.netMode != NetmodeID.MultiplayerClient)
            Projectile.NewProjectile(null, Main.player[0].Center - Vector2.UnitY * 72, Vector2.UnitX.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * (float)Math.Sin(Main.GlobalTimeWrappedHourly), ModContent.ProjectileType<EmpyreanThorn>(), 0, 0, ai0: 60, ai1: 16, ai2: 1);
        return true;
    }
}
