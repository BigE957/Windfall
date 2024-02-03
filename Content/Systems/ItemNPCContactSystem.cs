using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria.ID;
using Terraria.Audio;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CalamityMod.Items.Accessories;
using Windfall.Content.Items.Weapons.Misc;
using Windfall.Content.NPCs.Enemies;

namespace Windfall.Content.Systems
{
    public class ItemNPCContactSystem : ModSystem
    {
        int targetNPC = -1;
        int targetItem = -1;
        public override void PostUpdateNPCs()
        {
            if (IsNPCTouchingItem(ModContent.NPCType<WFCnidrion>(), ModContent.ItemType<Cnidrisnack>()))
            {
                if (Main.npc[targetNPC].life <= Main.npc[targetNPC].lifeMax / 4)
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
                            QuestSystem.IncrementQuestProgress(index, 0);
                            Main.NewText("Quest Progress: " + QuestSystem.QuestLog[index].ObjectiveProgress[0], Color.Yellow);
                            if (QuestSystem.QuestLog[index].Completed)
                            {
                                Main.NewText($"Quest Complete!", Color.Yellow);
                            }
                        }
                    }
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
    }
}
