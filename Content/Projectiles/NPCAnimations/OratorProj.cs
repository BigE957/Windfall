using CalamityMod.Dusts;
using DialogueHelper.UI.Dialogue;
using Windfall.Content.Items.Lore;
using Windfall.Content.Items.Quests;
using Windfall.Content.NPCs.WorldEvents.LunarCult;

namespace Windfall.Content.Projectiles.NPCAnimations;

public class OratorProj : ModProjectile, ILocalizedModType
{
    public override string Texture => "Windfall/Assets/NPCs/WorldEvents/TheOrator_NPC";
    internal SoundStyle SpawnSound => new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");
    public override void OnSpawn(IEntitySource source)
    {
        for (int i = 0; i <= 50; i++)
        {
            int dustStyle = Main.rand.NextBool() ? 66 : 263;
            Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(3) ? 191 : dustStyle, speed * 3, Scale: Main.rand.NextFloat(1.5f, 2.3f));
            dust.noGravity = true;
            dust.color = dust.type == dustStyle ? Color.LightGreen : default;
        }
        SoundEngine.PlaySound(SpawnSound, Projectile.Center);
    }
    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 1;
    }
    public override void SetDefaults()
    {
        Projectile.width = 58;
        Projectile.height = 70;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.netImportant = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = int.MaxValue;
        Projectile.alpha = 255;
        Projectile.ai[0] = 0;
    }

    private int Time = 0;

    public override void AI()
    {
        DialogueUISystem uiSystem = ModContent.GetInstance<DialogueUISystem>();
        if (Time == 60)
            uiSystem.DisplayDialogueTree(Windfall.Instance, "Cutscenes/SelenicEncounter", new(Name, [Projectile.whoAmI]), 5);
        else if (!uiSystem.isDialogueOpen)
        {
            if (Time == 120)
            {
                Item i = Main.item[Item.NewItem(Entity.GetSource_Loot(), Projectile.Center, new Vector2(8, 4), ModContent.ItemType<SelenicTablet>())];
                i.velocity = new Vector2(Projectile.direction, 0) * -4;
            }
            if (Time > 240)
            {
                foreach (NPC npc in Main.npc.Where(n => (n.type == ModContent.NPCType<LunarCultistArcher>() || n.type == ModContent.NPCType<LunarCultistDevotee>()) && n.active))
                {
                    for (int i = 0; i < 50; i++)
                    {
                        Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                        Dust d = Dust.NewDustPerfect(npc.Center, DustID.GoldFlame, speed * 3, Scale: 1.5f);
                        d.noGravity = true;
                    }
                    SoundEngine.PlaySound(SpawnSound, npc.Center);
                    npc.active = false;
                }
                SoundEngine.PlaySound(SpawnSound, Projectile.Center);
                for (int i = 0; i < 75; i++)
                {
                    Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, (int)CalamityDusts.Ectoplasm, speed * 2, Scale: 1.5f);
                    d.noGravity = true;
                }
                Main.npc[NPC.FindFirstNPC(ModContent.NPCType<LunarBishop>())].ai[2] = 1;
                Projectile.active = false;
            }
        }
        if(Time < 60 || !uiSystem.isDialogueOpen)
            Time++;
    }
}
