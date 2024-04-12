using Windfall.Content.Items.Debug;
using Windfall.Content.Items.Weapons.Misc;
using Windfall.Content.NPCs.Enemies;
using Windfall.Content.Projectiles.Misc;

namespace Windfall.Common.Systems
{
    public class ContactSystem : ModSystem
    {
        int targetNPC = -1;
        int targetItem = -1;
        public override void PostUpdateNPCs()
        {
            if (IsNPCTouchingItem(ModContent.NPCType<WFCnidrion>(), ModContent.ItemType<Cnidrisnack>()) || IsNPCTouchingItem(ModContent.NPCType<WFCnidrion>(), ModContent.ItemType<SuperCnidrisnack>()))
            {
                if ((Main.npc[targetNPC].life <= Main.npc[targetNPC].lifeMax / 4 && IsNPCTouchingItem(ModContent.NPCType<WFCnidrion>(), ModContent.ItemType<Cnidrisnack>())) || IsNPCTouchingItem(ModContent.NPCType<WFCnidrion>(), ModContent.ItemType<SuperCnidrisnack>()))
                {
                    Main.npc[targetNPC].ai[0] = 5f;
                    Main.npc[targetNPC].ai[1] = 0f;
                    Main.npc[targetNPC].life = Main.npc[targetNPC].lifeMax;
                    if (Main.item[targetItem].stack > 1)
                        Main.item[targetItem].stack--;
                    else
                        Main.item[targetItem].active = false;
                    SoundEngine.PlaySound(SoundID.Item2, Main.npc[targetNPC].Center);
                    int index = QuestSystem.QuestLog.FindIndex(quest => quest.Name == "CnidrionHunt");
                    if (index != -1)
                    {
                        if (QuestSystem.QuestLog[index].Active)
                        {
                            if (QuestSystem.QuestLog[index].ObjectiveProgress[0] == 4)
                            {
                                ModLoader.TryGetMod("CalValEX", out Mod CalVal);
                                if (CalVal != null)
                                {
                                    Item.NewItem(null, Main.npc[targetNPC].Center, 8, 4, CalVal.Find<ModItem>("SunDriedShrimp").Type);
                                }
                            }
                            QuestSystem.IncrementQuestProgress(index);
                        }
                    }
                }
            }
        }
        int myProjectile = -1;
        int targetProjectile = -1;
        Player owner = null;
        public static readonly SoundStyle Parry = new("Windfall/Assets/Sounds/Items/ParrySound");

        public override void PostUpdateProjectiles()
        {
            if (IsAnyProjectileTouchingProjectile(ModContent.ProjectileType<ParryBladeProj>()))
            {
                Projectile target = Main.projectile[targetProjectile];
                if (owner.immune == false && target.velocity != Vector2.Zero && target.damage > 0)
                {
                    Vector2 vectorFromPlayerToMouse = Main.MouseWorld - owner.Center;
                    SoundEngine.PlaySound(Parry, owner.Center);
                    Projectile.NewProjectile(Entity.GetSource_NaturalSpawn(), target.Center, vectorFromPlayerToMouse.SafeNormalize(Vector2.UnitX) * 30, ModContent.ProjectileType<ParryProj>(), target.damage * 10, 0.5f);
                    target.active = false;
                    Main.projectile[myProjectile].active = false;
                }
            }
        }
        internal bool IsNPCTouchingItem(int npcType, int itemType)
        {
            targetNPC = targetItem = -1;
            foreach (NPC npc in Main.npc.Where(n => n.type == npcType && n.active))
            {
                targetNPC = npc.whoAmI;
                foreach (Item item in Main.item.Where(n => n.active && n.type == itemType))
                {
                    if (Main.npc[targetNPC].Hitbox.Intersects(item.Hitbox))
                    {
                        targetItem = item.whoAmI;
                        return true;
                    }
                }
            }
            return false;
        }
        internal bool IsAnyProjectileTouchingProjectile(int projType)
        {
            targetProjectile = -1;
            owner = null;
            myProjectile = -1;
            foreach (Projectile projectile in Main.projectile.Where(n => n.type == projType && n.active))
            {
                myProjectile = projectile.whoAmI;
                owner = Main.player[projectile.owner];
                foreach (Projectile projectile2 in Main.projectile.Where(n => n.friendly == false && n.active))
                {
                    if (Main.projectile[myProjectile].Hitbox.Intersects(projectile2.Hitbox))
                    {
                        targetProjectile = projectile2.whoAmI;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
