using CalamityMod.NPCs.TownNPCs;
using System.IO;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace Windfall.Content.NPCs.Critters;

public class Fingerling : ModNPC
{
    public override string Texture => "Windfall/Assets/NPCs/Critters/Fingerling";
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 4;

        NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new()
        {
            Velocity = 1f,
            Direction = 1
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
    }
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.AddRange([
        new FlavorTextBestiaryInfoElement(GetWindfallTextValue($"Bestiary.{nameof(Fingerling)}")),
    ]);
    }
    public override void SetDefaults()
    {
        NPC.friendly = true; // NPC Will not attack player
        NPC.width = 14;
        NPC.height = 14;
        NPC.aiStyle = -1;
        NPC.damage = 0;
        NPC.defense = 0;
        NPC.lifeMax = 40;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0.5f;  
    }
    public override void OnSpawn(IEntitySource source)
    {
        NPC.ai[3] = Main.rand.Next(6);
        NPC.spriteDirection = NPC.direction = Main.rand.NextBool() ? 1 : -1;
        NPC.directionY = 0;
        NPC.rotation = NPC.spriteDirection == -1 ? Pi : 0;
        NPC.velocity.Y = -0.01f;
    }
    private SlopeType SlopeOn = SlopeType.Solid;
    bool falling = true;
    int rotateCooldown = 0;
    bool onHalfSlab = false;
    public override void AI()
    {
        if (rotateCooldown > 0)
            rotateCooldown--;
        if(NPC.collideY)
            if (TileID.Sets.Platforms[Main.tile[NPC.Center.ToTileCoordinates()].TileType])
                NPC.collideY = false;
        float speed = 0.5f;
        if (falling)
        {
            if (NPC.velocity.Y == 0)
                NPC.rotation = NPC.rotation.ToRotationVector2().RotateTowards(new Vector2(NPC.direction, NPC.directionY).ToRotation(), speed / 10f).ToRotation();
            else
                NPC.rotation += 0.02f * Math.Abs(NPC.velocity.Y) * NPC.spriteDirection;
            if (NPC.collideY || NPC.velocity == Vector2.Zero)
            {
                falling = false;
                NPC.rotation = NPC.spriteDirection == -1 ? Pi : 0;
                NPC.direction = NPC.spriteDirection;
                NPC.directionY = 0;
            }
            return;
        }
        #region Block Ahead Checks
        Vector2 heading = new(NPC.direction, NPC.directionY);
        Point tileCheck = (NPC.Center + (heading.RotatedBy(PiOver2 * NPC.spriteDirection) * 5f) + new Vector2(NPC.width / 1.66f * NPC.direction, NPC.width / 1.66f * NPC.directionY)).ToSafeTileCoordinates();
        //Dust d = Dust.NewDustPerfect(tileCheck.ToWorldCoordinates(), DustID.AncientLight, Vector2.Zero, newColor: Color.Red);
        //d.noGravity = true;
        if (Main.tile[tileCheck] != null && Main.tile[tileCheck].HasTile && Main.tile[tileCheck].IsTileSolid() && ((!Main.tile[tileCheck].IsHalfBlock && onHalfSlab) || !onHalfSlab))
        {
            Tile aheadTile = Main.tile[tileCheck];
            //Main.NewText($"On: {SlopeOn}, Ahead: {aheadTile.Slope}, Heading: {heading}");
            if (heading.X == 1)
            {
                if (heading.Y == 1)
                {
                    if (aheadTile.IsHalfBlock || aheadTile.Slope == SlopeType.Solid)
                    {
                        if (NPC.noGravity)
                            NPC.direction = 0;
                        else
                            NPC.directionY = 0;
                    }
                    else if (NPC.noGravity && aheadTile.Slope == SlopeType.SlopeDownRight)
                    {
                        NPC.direction = -1;
                        NPC.noGravity = false;
                    }
                    else if (aheadTile.Slope == SlopeType.SlopeDownRight)
                        NPC.directionY = -1;
                    else if (SlopeOn != aheadTile.Slope)
                    {
                        NPC.direction *= -1;
                        NPC.directionY *= -1;
                        NPC.spriteDirection *= -1;
                        NPC.rotation += Pi;
                    }
                    SlopeOn = aheadTile.Slope;
                }
                else if (heading.Y == 0)
                {
                    if (NPC.noGravity)
                    {
                        if (aheadTile.Slope == SlopeType.SlopeUpRight)
                            NPC.directionY = 1;
                        else if (aheadTile.RightSlope || aheadTile.IsHalfBlock || aheadTile.Slope == SlopeType.Solid)
                        {
                            NPC.directionY = 1;
                            NPC.direction = 0;
                        }
                        else if (SlopeOn == SlopeType.SlopeDownRight)
                        {
                            NPC.direction *= -1;
                            NPC.directionY *= -1;
                            NPC.spriteDirection *= -1;
                            NPC.rotation += Pi;
                        }
                    }
                    else
                    {
                        if (aheadTile.Slope == SlopeType.SlopeDownRight)
                            NPC.directionY = -1;
                        else if (aheadTile.RightSlope || aheadTile.IsHalfBlock || aheadTile.Slope == SlopeType.Solid)
                        {
                            NPC.directionY = -1;
                            NPC.direction = 0;
                            NPC.noGravity = true;
                        }
                        else if (SlopeOn != aheadTile.Slope)
                        {
                            NPC.direction *= -1;
                            NPC.directionY *= -1;
                            NPC.spriteDirection *= -1;
                            NPC.rotation += Pi;
                        }
                    }
                    SlopeOn = aheadTile.Slope;
                }
                else //heading.Y == -1
                {
                    if (NPC.noGravity)
                    {
                        if (aheadTile.Slope == SlopeType.SlopeUpRight)
                            NPC.directionY = 1;
                        else if (aheadTile.IsHalfBlock || aheadTile.Slope == SlopeType.Solid)
                            NPC.directionY = 0;
                        else if (SlopeOn != aheadTile.Slope)
                        {
                            NPC.direction *= -1;
                            NPC.directionY *= -1;
                        }
                    }
                    else if (aheadTile.Slope == SlopeType.SlopeUpRight)
                    {
                        NPC.direction = -1;
                        NPC.noGravity = true;
                    }
                    else if (aheadTile.IsHalfBlock || aheadTile.Slope == SlopeType.Solid)
                    {
                        NPC.direction = 0;
                        NPC.noGravity = true;
                    }
                    SlopeOn = aheadTile.Slope;
                }
            }
            else if (heading.X == 0)
            {
                if (heading.Y == 1)
                {
                    if (aheadTile.TopSlope)
                    {
                        if ((NPC.spriteDirection == 1 && aheadTile.Slope == SlopeType.SlopeDownRight) || (NPC.spriteDirection == -1 && aheadTile.Slope == SlopeType.SlopeDownLeft))
                        {
                            NPC.direction *= -1;
                            NPC.directionY *= -1;
                            NPC.spriteDirection *= -1;
                            NPC.rotation += Pi;
                            NPC.noGravity = true;
                        }
                        else
                        {
                            NPC.direction = aheadTile.Slope == SlopeType.SlopeDownRight ? -1 : 1;
                            NPC.noGravity = false;
                        }
                    }
                    else if (aheadTile.TopSlope || aheadTile.IsHalfBlock || aheadTile.Slope == SlopeType.Solid)
                    {
                        NPC.directionY = 0;
                        NPC.direction = 1 * NPC.spriteDirection;
                        NPC.noGravity = false;
                    }
                    SlopeOn = aheadTile.Slope;
                }
                else if (heading.Y == -1)
                {
                    if (aheadTile.BottomSlope)
                    {
                        if ((NPC.spriteDirection == -1 && aheadTile.Slope == SlopeType.SlopeUpRight) || (NPC.spriteDirection == 1 && aheadTile.Slope == SlopeType.SlopeUpLeft))
                        {
                            NPC.direction *= -1;
                            NPC.directionY *= -1;
                            NPC.spriteDirection *= -1;
                            NPC.rotation += Pi;
                        }
                        else
                        {
                            NPC.direction = aheadTile.Slope == SlopeType.SlopeUpLeft ? 1 : -1;
                            NPC.directionY = -1;
                        }
                    }
                    else if (aheadTile.BottomSlope || aheadTile.IsHalfBlock || aheadTile.Slope == SlopeType.Solid)
                    {
                        NPC.directionY = 0;
                        NPC.direction = -1 * NPC.spriteDirection;
                    }
                    else if (SlopeOn != aheadTile.Slope)
                    {
                        NPC.direction *= -1;
                        NPC.directionY *= -1;
                        NPC.spriteDirection *= -1;
                        NPC.rotation += Pi;
                    }
                    SlopeOn = aheadTile.Slope;
                }
            }
            else //Heading.X == -1
            {
                if (heading.Y == 1)
                {
                    if (aheadTile.IsHalfBlock || aheadTile.Slope == SlopeType.Solid)
                    {
                        if (NPC.noGravity)
                            NPC.direction = 0;
                        else
                            NPC.directionY = 0;
                    }
                    else if (aheadTile.Slope == SlopeType.SlopeDownLeft)
                    {
                        if (NPC.noGravity)
                        {
                            NPC.direction = 1;
                            NPC.noGravity = false;
                        }
                        else
                            NPC.directionY = -1;
                    }
                    else if (SlopeOn != aheadTile.Slope)
                    {
                        NPC.direction *= -1;
                        NPC.directionY *= -1;
                        NPC.spriteDirection *= -1;
                        NPC.rotation += Pi;
                    }
                    SlopeOn = aheadTile.Slope;
                }
                else if (heading.Y == 0)
                {
                    if (NPC.noGravity)
                    {
                        if (aheadTile.Slope == SlopeType.SlopeUpLeft)
                            NPC.directionY = 1;
                        else if (aheadTile.LeftSlope || aheadTile.IsHalfBlock || aheadTile.Slope == SlopeType.Solid)
                        {
                            NPC.directionY = 1;
                            NPC.direction = 0;
                        }
                        else if (SlopeOn != aheadTile.Slope)
                        {
                            NPC.direction *= -1;
                            NPC.directionY *= -1;
                            NPC.spriteDirection *= -1;
                            NPC.rotation += Pi;
                        }
                    }
                    else
                    {
                        if (aheadTile.Slope == SlopeType.SlopeDownLeft)
                            NPC.directionY = -1;
                        else if (aheadTile.LeftSlope || aheadTile.IsHalfBlock || aheadTile.Slope == SlopeType.Solid)
                        {
                            NPC.directionY = -1;
                            NPC.direction = 0;
                            NPC.noGravity = true;
                        }
                        else if (SlopeOn != aheadTile.Slope)
                        {
                            NPC.direction *= -1;
                            NPC.directionY *= -1;
                            NPC.spriteDirection *= -1;
                            NPC.rotation += Pi;
                        }
                    }
                    SlopeOn = aheadTile.Slope;
                }
                else //heading.Y == -1
                {
                    if (NPC.noGravity)
                    {
                        if (aheadTile.IsHalfBlock || aheadTile.Slope == SlopeType.Solid)
                            NPC.directionY = 0;
                    }
                    else
                    {
                        if (aheadTile.IsHalfBlock || aheadTile.Slope == SlopeType.Solid)
                        {
                            NPC.direction = 0;
                            NPC.noGravity = true;
                        }
                        else if (aheadTile.Slope == SlopeType.SlopeUpLeft)
                        {
                            NPC.direction = 1;
                            NPC.noGravity = true;
                        }
                    }
                    SlopeOn = aheadTile.Slope;
                }
            }
            if (aheadTile.IsHalfBlock)
                onHalfSlab = true;
            else
                onHalfSlab = false;
            //Main.NewText($"Heading: {NPC.direction}, {NPC.directionY}, Gravity: {!NPC.noGravity}, OnTile: {SlopeOn}");
        }
        #endregion

        #region Block Below Checks
        tileCheck = (NPC.Center + (new Vector2(NPC.direction, NPC.directionY) * NPC.width / 1.66f) + new Vector2(NPC.width / 1f * NPC.direction, NPC.width / 1f * NPC.directionY).RotatedBy(PiOver2 * NPC.spriteDirection)).ToTileCoordinates();
        Tile belowTile = Main.tile[tileCheck];
        //d = Dust.NewDustPerfect(tileCheck.ToWorldCoordinates(), DustID.AncientLight, Vector2.Zero, newColor: Color.Green);
        //d.noGravity = true;
        if (rotateCooldown == 0 && Main.tile[tileCheck] != null && (!belowTile.HasTile || !belowTile.IsTileSolid()))
        {
            tileCheck = (NPC.Center - (new Vector2(NPC.direction, NPC.directionY) * NPC.width / 2f) + new Vector2(NPC.width / 1f * NPC.direction, NPC.width / 1f * NPC.directionY).RotatedBy(PiOver2 * NPC.spriteDirection)).ToTileCoordinates();
            belowTile = Main.tile[tileCheck];

            //d = Dust.NewDustPerfect(tileCheck.ToWorldCoordinates(), DustID.AncientLight, Vector2.Zero, newColor: Color.Yellow);
            //d.noGravity = true;
                if (Main.tile[tileCheck] != null && !belowTile.IsHalfBlock && (!belowTile.HasTile || !belowTile.IsTileSolid()))
            {
                if (SlopeOn != SlopeType.Solid)
                {
                    if (NPC.directionY != 0)
                    {
                        heading = new(NPC.direction, NPC.directionY);
                        heading = FixedRotation(heading, NPC.spriteDirection);
                        NPC.direction = (int)heading.X;
                        NPC.directionY = (int)heading.Y;
                    }
                    falling = true;
                }
                else
                {                           
                    rotateCooldown = 15;
                    //Main.NewText("Rotate 90");
                    heading = heading.RotatedBy(PiOver2 * NPC.spriteDirection);
                    if (NPC.direction == 0 && NPC.directionY == -1)
                        NPC.position.Y -= 1;

                    NPC.direction = (int)heading.X;
                    NPC.directionY = (int)heading.Y;

                    if (NPC.noGravity == true)
                    {
                        if (NPC.spriteDirection == NPC.direction)
                            NPC.noGravity = false;
                        //NPC.Center += heading;
                    }
                    else if (NPC.directionY != 0)
                        NPC.noGravity = true;
                }
                //Main.NewText($"Heading: {NPC.direction}, {NPC.directionY}, Gravity: {!NPC.noGravity}");

            }
            else if (belowTile.HasTile && belowTile.Slope != SlopeOn)
            {
                //Main.NewText(belowTile.Slope);
                //Main.NewText(NPC.directionY == 1 && NPC.noGravity && belowTile.TopSlope);
                if (
                    (NPC.directionY == 1 && NPC.noGravity && belowTile.TopSlope) ||
                    (NPC.directionY == 1 && !NPC.noGravity && belowTile.BottomSlope) || 
                    (((NPC.directionY == 0 && NPC.noGravity) || (NPC.directionY != 0 && !NPC.noGravity)) && NPC.spriteDirection == 1 && belowTile.RightSlope) ||
                    (((NPC.directionY == 0 && NPC.noGravity) || (NPC.directionY != 0 && !NPC.noGravity)) && NPC.spriteDirection == -1 && belowTile.LeftSlope)
                )
                {
                    falling = true;
                    NPC.noGravity = false;
                }
                /*
                else if (NPC.direction == 0 && (
                    (NPC.spriteDirection == 1 && belowTile.RightSlope) || 
                    (NPC.spriteDirection == -1 && belowTile.LeftSlope)
                    )
                )
                {
                    if (NPC.directionY == -1)
                    {
                        NPC.direction *= -1;
                        NPC.directionY *= -1;
                        NPC.spriteDirection *= -1;
                        NPC.rotation += Pi;
                    }
                    else if(NPC.directionY == 1)
                    {
                        falling = true;
                        NPC.noGravity = false;
                    }
                }
                */
                else
                {
                    rotateCooldown = 30;
                    //Main.NewText("Rotate 45");
                    heading = new(NPC.direction, NPC.directionY);
                    heading = FixedRotation(heading, NPC.spriteDirection);

                    NPC.direction = (int)heading.X;
                    NPC.directionY = (int)heading.Y;

                    if (NPC.noGravity == true)
                    {
                        if (NPC.spriteDirection == NPC.direction)
                        {
                            NPC.noGravity = false;
                            NPC.position.Y -= 1;
                        }
                    }
                    else if ((NPC.directionY == 1 && NPC.direction != NPC.spriteDirection) || NPC.directionY == -1)
                        NPC.noGravity = true;
                    SlopeOn = belowTile.Slope;
                    if (belowTile.IsHalfBlock)
                        onHalfSlab = true;
                    else
                        onHalfSlab = false;
                }
                //Main.NewText($"Heading: {NPC.direction}, {NPC.directionY}, Gravity: {!NPC.noGravity}");

            }
        }
        else if (NPC.noGravity && (tileCheck.ToWorldCoordinates() - NPC.Center).Length() > 24f)
            NPC.Center += (heading.RotatedBy(PiOver2 * NPC.spriteDirection)) / 15f;
        #endregion

        NPC.velocity.X = speed * NPC.direction;
        NPC.velocity.Y = speed * NPC.directionY;

        if (NPC.direction != 0 && NPC.directionY != 0)
            NPC.velocity /= 2f;
        NPC.rotation = NPC.rotation.ToRotationVector2().RotateTowards(new Vector2(NPC.direction, NPC.directionY).ToRotation(), speed / 10f).ToRotation();
    }
    private static Vector2 FixedRotation(Vector2 vec, int direction = 1)
    {
        if(direction == 1)
            return (vec.X, vec.Y) switch
            {
                (1, 0) => new Vector2(1, 1),
                (1, 1) => new Vector2(0, 1),
                (0, 1) => new Vector2(-1, 1),
                (-1, 1) => new Vector2(-1, 0),
                (-1, 0) => new Vector2(-1, -1),
                (-1, -1) => new Vector2(0, -1),
                (0, -1) => new Vector2(1, -1),
                (1, -1) => new Vector2(1, 0),
                _ => vec,
            };
        else
            return (vec.X, vec.Y) switch
            {
                (1, 1) => new Vector2(1, 0),
                (0, 1) => new Vector2(1, 1),
                (-1, 1) => new Vector2(0, 1),
                (-1, 0) => new Vector2(-1, 1),
                (-1, -1) => new Vector2(-1, 0),
                (0, -1) => new Vector2(-1, -1),
                (1, -1) => new Vector2(0, -1),
                (1, 0) => new Vector2(1, -1),
                _ => vec,
            };
    }
    public override bool? CanFallThroughPlatforms() => true;
    public override void FindFrame(int frameHeight)
    {
        NPC.frame.X = NPC.frame.Width * (int)NPC.ai[3];
        NPC.frame.Width = ModContent.Request<Texture2D>(Texture).Width() / 6;
        NPC.frameCounter++;
        if (NPC.frameCounter >= 8)
        {
            NPC.frameCounter = 0;
            NPC.frame.Y += frameHeight;
            if (NPC.frame.Y >= frameHeight * (Main.npcFrameCount[Type] - 1))
                NPC.frame.Y = 0;
        }
    }
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D texture = TextureAssets.Npc[NPC.type].Value;

        Vector2 drawPosition = NPC.Center - screenPos + (Vector2.UnitY * NPC.gfxOffY);

        SpriteEffects spriteEffects = SpriteEffects.None;
        if (NPC.spriteDirection == -1)
            spriteEffects = SpriteEffects.FlipVertically;

        spriteBatch.Draw(texture, drawPosition, NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, spriteEffects, 0);
        return false;
    }
}
