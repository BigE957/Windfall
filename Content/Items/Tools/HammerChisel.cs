using CalamityMod.Items;

namespace Windfall.Content.Items.Tools;
public class HammerChisel : ModItem
{
    public override string Texture => "Windfall/Assets/Items/Tools/HammerAndChiselSmall";
    public override void SetDefaults()
    {
        Item.damage = 0;
        Item.useAnimation = Item.useTime = 15;

        Item.width = 30;
        Item.height = 40;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
        Item.rare = ItemRarityID.Lime;
        Item.UseSound = SoundID.Item1;
        Item.autoReuse = true;
        Item.useTurn = true;
        Item.noUseGraphic = true;
        Item.shoot = ModContent.ProjectileType<HammerChiselProjectile>();
    }
}

public class HammerChiselProjectile : ModProjectile
{
    public override string Texture => "Windfall/Assets/Items/Tools/HammerAndChiselProj";
    public override void SetDefaults()
    {
        Projectile.width = 28;
        Projectile.height = 17;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 28;
    }

    private int aiCounter
    {
        get => (int)Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    Vector2 spawnPos = Vector2.Zero;
    Vector2 goalPos = Vector2.Zero;
    Vector2 rotationVector = Vector2.Zero;
    float DistanceToTile = 0f;

    public override void OnSpawn(IEntitySource source)
    {
        spawnPos = Projectile.Center;
        Projectile.rotation = PiOver2;

        if (Main.player[Projectile.owner] == Main.LocalPlayer)
        {
            if (Main.SmartCursorIsUsed && Main.SmartCursorShowing)
            {
                goalPos = new(Main.SmartCursorX * 16f, Main.SmartCursorY * 16f);
                goalPos += Vector2.UnitX * 8f;
                DistanceToTile = (spawnPos - goalPos).Length();
                rotationVector = (spawnPos - goalPos).SafeNormalize(Vector2.Zero);
            }
            else
            {
                goalPos = Main.MouseWorld.ToTileCoordinates().ToWorldCoordinates();
                DistanceToTile = (spawnPos - goalPos).Length();
                rotationVector = (spawnPos - goalPos).SafeNormalize(Vector2.Zero);
            }
        }
        Tile approachingTile = Main.tile[Main.MouseWorld.ToTileCoordinates()];
        switch (approachingTile.Slope)
        {
            case SlopeType.SlopeDownLeft:
                Projectile.rotation = Pi - PiOver4;
                break;
            case SlopeType.SlopeDownRight:
                Projectile.rotation = PiOver4;
                break;
            case SlopeType.SlopeUpLeft:
                Projectile.rotation = Pi + PiOver4;
                break;
            case SlopeType.SlopeUpRight:
                Projectile.rotation = TwoPi - PiOver4;
                break;
            default:
                Projectile.rotation = PiOver2;
                if (!approachingTile.IsHalfBlock)
                    goalPos.Y -= 8;
                break;
        }
    }

    public override void AI()
    {
        if (aiCounter < 5)
            Projectile.Center = Vector2.Lerp(spawnPos, goalPos, CalamityMod.CalamityUtils.SineOutEasing(aiCounter / 5f, 1));
        else if (aiCounter >= 24)
            Projectile.Center = Vector2.Lerp(goalPos, spawnPos, SineOutEasing((aiCounter - 20) / 4f));
        else
            Projectile.Center = goalPos;

        if (aiCounter == 18)
        {
            Point tileLocation = goalPos.ToTileCoordinates();
            VanillaHammerSlopingLogic(tileLocation);
            Tile tile = Main.tile[tileLocation];

            if (!tile.HasTile)
            {
                aiCounter++;
                return;
            }

            Vector2 tileCenter = goalPos + Vector2.UnitY * 8f;
            for (int i = 0; i < 10; i++)
            {
                int id = i - 5;
                switch (tile.Slope)
                {
                    case SlopeType.SlopeDownLeft:
                        Dust.NewDustPerfect(tileCenter + (Vector2.One * id), DustID.GemTopaz, Vector2.One * id / 2f).noGravity = true;
                        break;
                    case SlopeType.SlopeUpRight:
                        Dust.NewDustPerfect(tileCenter + (Vector2.One * id), DustID.GemTopaz, Vector2.One * id / 2f).noGravity = true;
                        break;
                    case SlopeType.SlopeUpLeft:
                        Dust.NewDustPerfect(tileCenter + (new Vector2(1, -1) * id), DustID.GemTopaz, new Vector2(1, -1) * id / 2f).noGravity = true;
                        break;
                    case SlopeType.SlopeDownRight:
                        Dust.NewDustPerfect(tileCenter + (new Vector2(1, -1) * id), DustID.GemTopaz, new Vector2(1, -1) * id / 2f).noGravity = true;
                        break;
                    default:
                        if (tile.IsHalfBlock)
                            Dust.NewDustPerfect(tileCenter + (Vector2.UnitX * id), DustID.GemTopaz, Vector2.UnitX * id / 2f).noGravity = true;
                        else
                            Dust.NewDustPerfect(goalPos + (Vector2.UnitX * id), DustID.GemTopaz, Vector2.UnitX * id / 2f).noGravity = true;
                        break;
                }
            }
        }

        aiCounter++;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
        Rectangle chiselFrame = tex.Frame(2, 1, 0, 0);
        float rotationOffset = Clamp(1 - aiCounter / 5f, 0, 1) * TwoPi;
        if (aiCounter >= 24)
            rotationOffset = Clamp(1 - (aiCounter - 24) / 5f, 0, 1) * -TwoPi;
        rotationOffset *= rotationVector.X > 0 ? 1 : -1;
        Vector2 chiselPosition = Projectile.Center - Main.screenPosition + (aiCounter > 17 ? Vector2.UnitY * -Projectile.height / 3f : Vector2.UnitY * -Projectile.height).RotatedBy(Projectile.rotation - PiOver2);

        Main.EntitySpriteDraw(tex, chiselPosition, chiselFrame, Color.White * Projectile.Opacity, Projectile.rotation + PiOver2 + rotationOffset, chiselFrame.Size() * 0.5f, Projectile.scale, SpriteEffects.None);

        int angleNegative = rotationVector.X > 0 ? -1 : 1;

        Rectangle hammerFrame = tex.Frame(2, 1, 1, 0);
        Vector2 offset = new(24 * (rotationVector.X > 0 ? 1 : -1), Projectile.height * -2);


        if (aiCounter > 5)
        {

            if (aiCounter <= 12)
            {
                float lerp = Clamp((aiCounter - 5) / 7f, 0, 1);
                offset += Vector2.Lerp(Vector2.Zero, new Vector2(rotationVector.X > 0 ? 24 : -24, -16), lerp);
                rotationOffset += Lerp(0f, -Pi, lerp) * angleNegative;
            }
            else
            {
                float lerp = Clamp((aiCounter - 12) / 3f, 0, 1);
                offset += Vector2.Lerp(new Vector2(rotationVector.X > 0 ? 24 : -24, -8), Vector2.UnitX * (rotationVector.X > 0 ? -16 : 16), lerp);
                rotationOffset += Lerp(-Pi, PiOver2, lerp) * angleNegative;

            }
        }

        Vector2 hammerPosition = Projectile.Center - Main.screenPosition + (offset * Clamp(aiCounter / 5f, 0, 1)).RotatedBy(Projectile.rotation - PiOver2) + rotationVector.RotatedBy(Projectile.rotation + PiOver2 * (rotationVector.X > 0 ? -1 : 1)) * (aiCounter <= 5 ? (1f - ((float)Math.Abs(aiCounter / 5f - 0.5f) * 2)) * 72 : 0);

        Main.EntitySpriteDraw(tex, hammerPosition, hammerFrame, Color.White * Projectile.Opacity, Projectile.rotation + rotationOffset + (-PiOver2 * angleNegative), chiselFrame.Size() * 0.5f, Projectile.scale, rotationVector.X > 0 ? SpriteEffects.FlipVertically : SpriteEffects.None);

        return false;
    }
}

