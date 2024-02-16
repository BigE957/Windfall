using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Windfall.Common.Utilities;

namespace Windfall.Content.Projectiles.NPCAnimations
{
    public abstract class ProjectileNPC : ModProjectile
    {
        private enum AIState
        {
            SpawnDelay,
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
        int dialogueCounter = 0;
        internal struct dialogue
        {
            internal string text;
            internal int delay;
        }
        internal virtual List<dialogue> MyDialogue => new();
        internal virtual SoundStyle SpawnSound => new();
        internal virtual int NPCType => 0;
        internal virtual Color TextColor => new(); 
        public override void OnSpawn(IEntitySource source)
        {
            Player closestPlayer = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];

            if (!Utilities.AlignProjectileWithGround(Main.projectile[Projectile.whoAmI]))
                Projectile.position.Y = closestPlayer.position.Y;
        }
        public override void AI()
        {
            Player closestPlayer = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
            Projectile.spriteDirection = Projectile.direction = (closestPlayer.Center.X > Projectile.Center.X).ToDirectionInt() * -1;
            switch (CurrentAI)
            {
                case AIState.SpawnDelay:
                    Vector2 vectorFromNpcToPlayer = closestPlayer.Center - Projectile.Center;
                    float distance = vectorFromNpcToPlayer.Length();
                    if (counter >= 60 * 4 && distance < 250f)
                    {
                        dialogueCounter = 0;
                        for (int i = 0; i < 75; i++)
                        {
                            Vector2 speed = Main.rand.NextVector2Circular(1.5f, 2f);
                            Dust d = Dust.NewDustPerfect(Projectile.Center, (int)CalamityDusts.PurpleCosmilite, speed * 4, Scale: 1.5f);
                            d.noGravity = true;
                        }
                        SoundEngine.PlaySound(SpawnSound, Projectile.Center);
                        Projectile.alpha = 0;
                        counter = 60 * 4;
                        CurrentAI = AIState.Yapping;
                    }
                    break;
                case AIState.Yapping:
                    if (MyDialogue.Count == dialogueCounter)
                    {
                        CurrentAI = AIState.TurnToNPC;
                        break;
                    }
                    int Delay = 0;
                    for (int i = dialogueCounter; i >= 0; i--)
                    {
                        Delay += MyDialogue[i].delay;
                    }
                    if (counter == (60 * 4) + (60 * Delay))
                    {
                        DisplayMessage(MyDialogue[dialogueCounter].text, Main.projectile[Projectile.whoAmI], TextColor);
                        dialogueCounter++;
                    }
                    break;
                case AIState.TurnToNPC:
                    NPC.NewNPC(NPC.GetSource_NaturalSpawn(), (int)Projectile.Center.X, (int)Projectile.Bottom.Y-1, NPCType, 0, Projectile.velocity.Y, Projectile.direction);
                    Projectile.active = false;
                    break;

            }
            counter++;
        }

        internal static void DisplayMessage(string text, Projectile projectile, Color color)
        {
            Rectangle location = new((int)projectile.Center.X, (int)projectile.Center.Y, projectile.width, projectile.width);
            CombatText MyDialogue = Main.combatText[CombatText.NewText(location, color, text, true)];
            if (MyDialogue.text.Length > 50)
            {
                MyDialogue.lifeTime = 60 + MyDialogue.text.Length;
            }
        }
    }
}
