using DialogueHelper.UI.Dialogue;
using Luminance.Core.Graphics;
using System.Diagnostics.Metrics;
using Windfall.Content.Buffs.Inhibitors;
using Windfall.Content.Items.Quests;
using Windfall.Content.NPCs.WorldEvents.LunarCult;

namespace Windfall.Content.Projectiles.Other;

public class OratorEntourageSpawner : ModProjectile
{
    public override string Texture => "Windfall/Assets/NPCs/Bosses/TheOrator_Boss_Head";
    public new static string LocalizationCategory => "Projectiles.Other";
    internal SoundStyle SpawnSound => new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");

    public override void SetDefaults()
    {
        Projectile.width = 10;
        Projectile.height = 10;
        Projectile.penetrate = -1;
        Projectile.alpha = 255;
    }

    int Time = 0;
    int OratorIndex = -1;
    float zoom = 0;

    public override void AI()
    {
        Player player = Main.player[0];
        switch (Time)
        {
            case 60 * 2:
                NPC.NewNPCDirect(Entity.GetSource_FromAI(), player.Center + new Vector2(150, 0), ModContent.NPCType<LunarCultistArcher>(), 0, 0);
                NPC.NewNPCDirect(Entity.GetSource_FromAI(), player.Center + new Vector2(-150, 0), ModContent.NPCType<LunarCultistArcher>(), 0, 0);
                break;

            case 60 * 3:
                NPC.NewNPCDirect(Entity.GetSource_FromAI(), player.Center + new Vector2(110, 0), ModContent.NPCType<LunarCultistDevotee>(), 0, 0);
                NPC.NewNPCDirect(Entity.GetSource_FromAI(), player.Center + new Vector2(-110, 0), ModContent.NPCType<LunarCultistDevotee>(), 0, 0);
                break;

            case 60 * 4:
                NPC.NewNPCDirect(Entity.GetSource_FromAI(), player.Center + new Vector2(-60, 0), ModContent.NPCType<LunarBishop>(), 0, 0);
                break;

            case 60 * 8:
                Vector2 spawnPos = player.Center + new Vector2(60, 0);
                for (int i = 0; i <= 50; i++)
                {
                    int dustStyle = Main.rand.NextBool() ? 66 : 263;
                    Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                    Dust dust = Dust.NewDustPerfect(spawnPos, Main.rand.NextBool(3) ? 191 : dustStyle, speed * 3, Scale: Main.rand.NextFloat(1.5f, 2.3f));
                    dust.noGravity = true;
                    dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                }
                OratorIndex = NPC.NewNPC(Entity.GetSource_FromAI(), (int)spawnPos.X, (int)spawnPos.Y, ModContent.NPCType<OratorNPC>(), (int)OratorNPC.States.Cutscene);
                SoundEngine.PlaySound(SpawnSound, spawnPos);
                break;
        }

        DialogueUISystem uiSystem = ModContent.GetInstance<DialogueUISystem>();
        if (Time == 60 * 9)
            uiSystem.DisplayDialogueTree(Windfall.Instance, "Cutscenes/SelenicEncounter", new(Name, [Projectile.whoAmI]));
        else if (!uiSystem.isDialogueOpen)
        {
            if (Time == 60 * 10)
            {
                Item i = Main.item[Item.NewItem(Entity.GetSource_Loot(), Main.npc[OratorIndex].Center, Vector2.Zero, ModContent.ItemType<SelenicTablet>())];
                i.velocity = new Vector2(4, 0) * Main.npc[OratorIndex].spriteDirection;
            }
            if (Time > 60 * 11)
            {
                foreach (NPC npc in Main.npc.Where(n => (n.type == ModContent.NPCType<LunarCultistArcher>() || n.type == ModContent.NPCType<LunarCultistDevotee>() || n.type == ModContent.NPCType<OratorNPC>()) && n.active))
                {
                    for (int i = 0; i < 50; i++)
                    {
                        int dustStyle = Main.rand.NextBool() ? 66 : 263;
                        Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                        Dust dust = Dust.NewDustPerfect(npc.Center, Main.rand.NextBool(3) ? 191 : dustStyle, speed * 3, Scale: Main.rand.NextFloat(1.5f, 2.3f));
                        dust.noGravity = true;
                        dust.color = dust.type == dustStyle ? Color.LightGreen : default;
                    }
                    SoundEngine.PlaySound(SpawnSound, npc.Center);
                    npc.active = false;
                }

                Main.npc[NPC.FindFirstNPC(ModContent.NPCType<LunarBishop>())].ai[2] = 1;
                Projectile.active = false;
            }
        }

        if(Time > 60 * 8)
        {
            if (Time < (60 * 8) + 100)
                zoom = Lerp(zoom, 0.4f, 0.075f);
            else
                zoom = 0.4f;
            CameraPanSystem.Zoom = zoom;
            CameraPanSystem.PanTowards(Main.npc[OratorIndex].Center + Vector2.UnitY * 64, zoom);
        }

        if (Time <= 60 * 9 || !uiSystem.isDialogueOpen)
            Time++;

        foreach(Player p in Main.ActivePlayers)
            player.AddBuff(ModContent.BuffType<SpacialLock>(), 2);
    }
}
