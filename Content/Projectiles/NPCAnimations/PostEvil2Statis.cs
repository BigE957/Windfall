using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Windfall.Common.Systems;
using Windfall.Common.Utilities;
using Windfall.Content.NPCs.WanderingNPCs;

namespace Windfall.Content.Projectiles.NPCAnimations
{
    public class PostEvil2Statis : ModProjectile, ILocalizedModType
    {
        public override string Texture => "Windfall/Assets/Projectiles/NPCAnimations/IlmeranPaladinDig";
        private bool InsideTiles = false;
        private static readonly SoundStyle Teleport = new("CalamityMod/Sounds/Custom/SCalSounds/BrimstoneHellblastSound");

        private enum AIState
        {
            Chilling,
            Yapping,
            TurnToNPC,
        }
        private AIState CurrentAI
        {
            get => (AIState)Projectile.ai[0];
            set => Projectile.ai[0] = (int)value;
        }
        private int counter
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        internal struct dialogue
        {
            internal string text;
            internal int delay;
        }
        internal static List<dialogue> StatisDialogue = new()
        { 
            new dialogue {text = "Howdy!" , delay = 4},
            new dialogue {text = "I'm Statis!" , delay = 4},
            new dialogue {text = "Statis the Lone Ronin!" , delay = 4},

        };
        int dialogueCounter = 0;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 1;
        }
        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 40;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = int.MaxValue;
            Projectile.alpha = 255;
            Projectile.ai[0] = 0;

        }
        public override void OnSpawn(IEntitySource source)
        {
            int i;
            Player closestPlayer = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
            
            if (Utilities.AlignProjectileWithGround(Main.projectile[Projectile.whoAmI]))
                Projectile.position.Y = closestPlayer.position.Y;

            for (i = 0; i < 50; i++)
            {
                Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Blood, speed * 5, Scale: 1.5f);
                d.noGravity = true;
            }
            SoundEngine.PlaySound(Teleport, Projectile.Center);
            Projectile.alpha = 0;
        }
        public override void AI()
        {
            switch (CurrentAI)
            {
                case AIState.Chilling:
                    if(counter == 60)
                    {
                        dialogueCounter = 0;
                        CurrentAI = AIState.Yapping;
                    }
                    break;
                case AIState.Yapping:
                    if (StatisDialogue.Count == dialogueCounter)
                    {
                        CurrentAI = AIState.TurnToNPC;
                        break;
                    }
                    int Delay = 0;
                    for (int i = dialogueCounter; i >= 0; i--)
                    {
                        Delay += StatisDialogue[i].delay;
                    }
                    if (counter == (60 * (Delay)))
                    {                       
                        StatisMessage(StatisDialogue[dialogueCounter].text, Main.projectile[Projectile.whoAmI]);
                        dialogueCounter++;
                    }
                    break;
                case AIState.TurnToNPC:
                    NPC.NewNPC(NPC.GetSource_NaturalSpawn(), (int)Projectile.Center.X, (int)Projectile.Center.Y, ModContent.NPCType<IlmeranPaladin>(), 0, Projectile.velocity.Y, Projectile.direction);
                    Projectile.active = false;
                    break;

            }
            counter++;
        }
        internal static void StatisMessage(string text, Projectile Statis)
        {
            Rectangle location = new((int)Statis.Center.X, (int)Statis.Center.Y, Statis.width, Statis.width);
            CombatText MyDialogue = Main.combatText[CombatText.NewText(location, Color.Purple, text, true)];
            if (MyDialogue.text.Length > 50)
            {
                MyDialogue.lifeTime += (5 * (MyDialogue.text.Length - 50));
            }
        }
    }
}
