using Luminance.Core.Graphics;

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
        internal virtual string key => ""; 
        internal virtual List<float> Delays => new();
        internal virtual SoundStyle SpawnSound => new();
        internal virtual int NPCType => 0;
        internal virtual Color TextColor => new();
        internal virtual bool SpawnConditions(Player player)
        {
            return true;
        }
        internal virtual int DespawnDelay => 150;
        public override void OnSpawn(IEntitySource source)
        {
            Vector2 oldPos = Projectile.position;
            Projectile.position.Y = Utilities.FindGroundVertical(new Point((int)Projectile.position.X, (int)Projectile.position.Y)).Y - Projectile.height - 8;
            float altY = 0;
            for (int i = 0; i < 16; i++)
            {
                altY = Utilities.FindGroundVertical(new Point((int)(oldPos.X + i), (int)(oldPos.Y - 64))).Y - Projectile.height - 8;
                if (altY < Projectile.position.Y)
                    Projectile.position.Y = altY;
            }
        }
        int finalDelay = 0;
        float zoom = 0;

        public override void AI()
        {
            Player closestPlayer = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
            Projectile.spriteDirection = Projectile.direction = (closestPlayer.Center.X > Projectile.Center.X).ToDirectionInt() * -1;
            switch (CurrentAI)
            {
                case AIState.SpawnDelay:
                    if (counter >= 60 * 4 && SpawnConditions(closestPlayer))
                    {
                        zoom = 0;
                        dialogueCounter = 0;
                        DoOnSpawn();
                        Projectile.alpha = 0;
                        counter = 60 * 4;
                        CurrentAI = AIState.Yapping;
                    }
                    break;
                case AIState.Yapping:
                    if (counter > 60 * 4)
                    {
                        if (counter < (60 * 4) + 100)
                            zoom = Lerp(zoom, 0.4f, 0.075f);
                        else
                            zoom = 0.4f;
                        CameraPanSystem.Zoom = zoom;
                        CameraPanSystem.PanTowards(Projectile.Center, zoom);
                    }
                    if (Delays.Count == dialogueCounter)
                    {
                        finalDelay = counter + 1;
                        CurrentAI = AIState.TurnToNPC;
                        break;
                    }
                    float Delay = 0;
                    for (int i = dialogueCounter; i >= 0; i--)
                    {
                        Delay += Delays[i];
                    }
                    if (counter == (60 * 4) + (60 * Delay))
                    {
                        DisplayMessage($"{key}.{dialogueCounter}", Main.projectile[Projectile.whoAmI], TextColor);
                        dialogueCounter++;
                    }
                    break;
                case AIState.TurnToNPC:
                    if (counter == finalDelay + 1)
                        DoBeforeDespawn();
                    if (counter >= finalDelay + DespawnDelay)
                    {
                        DoOnDespawn();
                        if (NPCType != -1)
                            NPC.NewNPC(Terraria.Entity.GetSource_NaturalSpawn(), (int)Projectile.Center.X, (int)Projectile.Bottom.Y - 1, NPCType, 0, Projectile.velocity.Y, Projectile.direction, 1);
                        Projectile.active = false;
                    }
                    else
                    {
                        CameraPanSystem.Zoom = 0.4f;
                        CameraPanSystem.PanTowards(Projectile.Center, zoom);
                    }
                    break;

            }
            counter++;
        }
        internal static void DisplayMessage(string key, Projectile projectile, Color color)
        {
            Rectangle location = new((int)projectile.Center.X, (int)projectile.Center.Y, projectile.width, projectile.width);
            CombatText MyDialogue = Main.combatText[CombatText.NewText(location, color, GetWindfallTextValue($"Dialogue.{key}"), true)];
            if (MyDialogue.text.Length > 50)
                MyDialogue.lifeTime = 60 + MyDialogue.text.Length;
        }
        internal virtual void DoOnSpawn() { }
        internal virtual void DoBeforeDespawn() { }
        internal virtual void DoOnDespawn() { }
    }
}
