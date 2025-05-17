using CalamityMod;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using CalamityMod.Projectiles.Summon;
using Windfall.Content.UI.DraedonsDatabase;

namespace Windfall.Content.Projectiles.Other;
/*
public class DraeDashDropPod : ModProjectile, ILocalizedModType
{
    public override string Texture => "CalamityMod/Projectiles/Summon/AtlasMunitionsDropPod";
    public new string LocalizationCategory => "Projectiles.Other";
    public Player Owner => Main.player[Projectile.owner];

    public float TileCollisionYThreshold => Projectile.ai[0];

    public bool HasCollidedWithGround
    {
        get => Projectile.ai[1] == 1f;
        set => Projectile.ai[1] = value.ToInt();
    }

    public ref float SquishFactor => ref Projectile.localAI[0];

    public const float Gravity = 1.1f;

    public const float MaxFallSpeed = 24f;

    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 14;
    }

    public override void SetDefaults()
    {
        Projectile.width = 86;
        Projectile.height = 130;
        Projectile.netImportant = true;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 90000;
        Projectile.penetrate = -1;
        Projectile.tileCollide = true;
        Projectile.hide = true;
    }

    public override void AI()
    {
        if (SquishFactor <= 0f)
            SquishFactor = 1f;

        if (Projectile.velocity.Y == 0f && !HasCollidedWithGround)
        {
            PerformGroundCollisionEffects();
            HasCollidedWithGround = true;
            Projectile.netUpdate = true;
            Projectile.timeLeft = 60 * 5;
        }

        // fade away once near despawn
        if (HasCollidedWithGround && Projectile.timeLeft < 100)
            if (Projectile.alpha + 3 >= 255)
                Projectile.active = false;
            else
                Projectile.alpha += 3;

        // Undo squish effects.
        SquishFactor = Lerp(SquishFactor, 1f, 0.08f);

        // Determine whether to collide with tiles.
        Projectile.tileCollide = Projectile.Bottom.Y >= TileCollisionYThreshold;

        // Calculate frames.
        Projectile.frameCounter++;
        if (!HasCollidedWithGround)
            Projectile.frame = Projectile.frameCounter / 6 % 5;
        else
        {
            Projectile.velocity.X = 0f;
            if (Projectile.frame < 5)
                Projectile.frame = 5;
            if (Projectile.frameCounter % 8 == 7)
            {
                Projectile.frame++;

                // Release the upper part of the pod into the air and spawn the cannon.
                if (Projectile.frame == 8)
                {
                    SoundEngine.PlaySound(ThanatosHead.VentSound, Projectile.Top);
                    if (Main.myPlayer == Projectile.owner)
                    {
                        //handles getting the DraeDash order items
                        foreach (KeyValuePair<string, int> itemSet in DraedonUISystem.DraeDashOrder)
                        {
                            int itemType = ItemID.Search.GetId(itemSet.Key);
                            if (ModContent.GetModItem(itemType).CanStack(null))
                                Item.NewItem(null, Projectile.Center, 8, 4, itemType, itemSet.Value);
                            else
                            {
                                foreach (int index in Enumerable.Range(1, itemSet.Value))
                                {
                                    Item.NewItem(null, Projectile.Center, 8, 4, itemType);
                                }
                            }
                        }
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Top + Vector2.UnitY * 72f, Vector2.Zero, ModContent.ProjectileType<AtlasMunitionsDropPodUpper>(), 0, 0f, Projectile.owner);
                        DraedonUISystem.DraeDashOrder = [];
                        DraeDash.orderEnRoute = false;
                    }
                }
            }
            if (Projectile.frame >= Main.projFrames[Projectile.type])
                Projectile.frame = Main.projFrames[Projectile.type] - 1;
        }

        // Fall downward.
        Projectile.velocity.Y += Gravity;
        if (Projectile.velocity.Y > MaxFallSpeed)
            Projectile.velocity.Y = MaxFallSpeed;
    }

    public void PerformGroundCollisionEffects()
    {
        // Become squished.
        SquishFactor = 1.4f;

        // Mechanical Cart laser dust. Looks epic.
        int dustID = 182;
        int dustCount = 54;
        for (int i = 0; i < dustCount; i += 2)
        {
            float pairSpeed = Main.rand.NextFloat(0.5f, 16f);
            Dust d = Dust.NewDustDirect(Projectile.Bottom, 0, 0, dustID);
            d.velocity = Vector2.UnitX * pairSpeed;
            d.scale = 2.7f;
            d.noGravity = true;

            d = Dust.NewDustDirect(Projectile.BottomRight, 0, 0, dustID);
            d.velocity = Vector2.UnitX * -pairSpeed;
            d.scale = 2.7f;
            d.noGravity = true;
        }

        SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.Center);
        Owner.Calamity().GeneralScreenShakePower = Utils.Remap(Owner.Distance(Projectile.Center), 1800f, 1000f, 0f, 4.5f);
    }

    // As a means of obscuring contents when they spawn (such as ensuring that the minigun doesn't seem to pop into existence), this projectile draws above most other projectiles.
    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        overPlayers.Add(index);
    }

    public override bool? CanDamage() => false;

    public override bool OnTileCollide(Vector2 oldVelocity) => false;

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
    {
        fallThrough = false;
        return true;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        Texture2D glowmask = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Summon/AtlasMunitionsDropPodGlow").Value;
        Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Vector2 scale = Projectile.scale * new Vector2(SquishFactor, 1f / SquishFactor);
        Vector2 origin = frame.Size() * new Vector2(0.5f, 0.5f / SquishFactor);
        Main.EntitySpriteDraw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, scale, 0, 0);
        Main.EntitySpriteDraw(glowmask, drawPosition, frame, Color.White, Projectile.rotation, origin, scale, 0, 0);
        return false;
    }
}
*/