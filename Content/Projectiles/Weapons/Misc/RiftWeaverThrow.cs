using Luminance.Common.VerletIntergration;
using Luminance.Core.Graphics;
using Microsoft.Build.Tasks;
using static Windfall.Content.NPCs.Bosses.Orator.TheOrator;

namespace Windfall.Content.Projectiles.Weapons.Misc
{
    public class RiftWeaverThrow : ModProjectile
    {
        public override string Texture => "Windfall/Assets/Items/Weapons/Misc/RiftWeaverProj";
        private List<VerletSegment> NeedleString = [];
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.DamageType = DamageClass.Default;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = int.MaxValue;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }
        public override void OnSpawn(IEntitySource source)
        {
            damage = Projectile.damage;
            Projectile.damage = 0;
            if (NeedleString.Count == 0)
            {
                for (int i = 0; i < 16; i++)
                    NeedleString.Add(new((Projectile.Center - new Vector2(24, -32)) - (Vector2.UnitY * (-12 * i)), Vector2.Zero, i == 0));
            }
        }
        private int damage = 0;
        private int aiCounter
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = (float)value;
        }
        private int impaleCounter
        {
            get => (int)Projectile.ai[2];
            set => Projectile.ai[2] = (float)value;
        }
        private bool impaled = false;
        private NPC Target = null;
        private Vector2 positionDiff = Vector2.Zero;

        public enum AIState
        {
            Aiming,
            Throwing,
        }
        AIState state = AIState.Aiming;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            //Main.NewText(impaleCounter);
            switch (state)
            {
                case AIState.Aiming:
                    Projectile.Center = owner.Center - (Vector2.UnitY * owner.height / 2.5f) - (Vector2.UnitX * owner.width / 2f * owner.direction);
                    Projectile.rotation = (owner.Calamity().mouseWorld - Projectile.Center).ToRotation();
                    owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Quarter, (Pi - (Pi/3f)) * owner.direction);
                    owner.heldProj = Projectile.whoAmI;

                    if (!owner.channel)
                    {
                        SoundEngine.PlaySound(SoundID.DD2_DarkMageAttack, owner.Center);
                        Projectile.damage = damage;
                        Projectile.velocity = Projectile.rotation.ToRotationVector2() * 80f;
                        state = AIState.Throwing;
                    }
                    break;

                case AIState.Throwing:
                    if (impaled)
                    {
                        if (impaleCounter < 180)
                        {
                            if (Target != null && Target.active)
                            {
                                Projectile.Center = Target.Center + positionDiff;
                                Projectile.velocity = Target.velocity;
                            }
                            else if(Target != null)
                            {
                                impaled = false;
                                Target = null;
                                aiCounter = 45;
                                Projectile.velocity = (owner.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 8f;
                            }
                        }
                        else
                        {
                            if (impaleCounter != 180 && (Target == null || Target.active))
                            {
                                if (Target != null)
                                {
                                    owner.velocity = ((Projectile.Center - owner.Center) * 0.055f).ClampLength(16f, 24f);
                                    owner.velocity.X *= 0.8f;
                                    owner.velocity.Y *= 1.2f;
                                }
                                else
                                    owner.velocity = ((Projectile.Center - owner.Center) * 0.05f).ClampLength(20f, 20f);
                            }
                            impaled = false;
                            Target = null;
                            aiCounter = 45;
                            Projectile.velocity = (owner.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 8f;
                        }
                        impaleCounter++;
                    }
                    else
                    {
                        if (aiCounter < 45 && (owner.Center - Projectile.Center).Length() < 600f)
                        {
                            Projectile.velocity *= 0.85f;
                        }
                        else
                        {
                            Projectile.tileCollide = false;
                            if (Projectile.damage > 0)
                                Projectile.damage = 0;
                            if (aiCounter < 45)
                                aiCounter = 45;
                            if (Projectile.velocity.Length() < 64f)
                            {
                                Projectile.velocity = (owner.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * Projectile.velocity.Length();
                                Projectile.velocity *= 1.1f;
                            }
                            else
                                Projectile.velocity = (owner.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 64f;
                            if (owner.Hitbox.Intersects(Projectile.Hitbox))
                                Projectile.active = false;
                        }
                        Projectile.rotation = (owner.Center - Projectile.Center).ToRotation() + Pi;
                        aiCounter++;
                    }
                    break;
            }
            VerletSimulations.RopeVerletSimulation(NeedleString, Projectile.Center - (Vector2.UnitX * 32f).RotatedBy(Projectile.rotation), 200f, new(), owner.Center);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!impaled)
            {
                Projectile.damage = 0;
                Projectile.tileCollide = false;
                if (Target == null)
                {
                    impaled = true;
                    Target = target;
                    positionDiff = Projectile.Center - Target.Center;
                    Projectile.velocity = Vector2.Zero;
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            impaled = true;
            Projectile.velocity = Vector2.Zero;
            Projectile.tileCollide = false;
            Projectile.Center += Projectile.rotation.ToRotationVector2() * 16f;
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2[] segmentPositions = NeedleString.Select(x => x.Position).ToArray();

            PrimitiveRenderer.RenderTrail(segmentPositions, new(widthFunction, colorFunction));

            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            Main.EntitySpriteDraw(tex, drawPosition, null, Color.White * Projectile.Opacity, Projectile.rotation + PiOver4, tex.Frame().Size() * 0.5f, Projectile.scale, SpriteEffects.None);

            return false;
        }

        private readonly PrimitiveSettings.VertexWidthFunction widthFunction = WidthFunction;
        private static float WidthFunction(float interpolent) => 1.6f;

        private readonly PrimitiveSettings.VertexColorFunction colorFunction = ColorFunction;
        private static Color ColorFunction(float interpolent)
        {
            return Color.Lerp(Color.Cyan, Color.LimeGreen, interpolent * 2f) * (0.9f - interpolent);
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }
    }
}
