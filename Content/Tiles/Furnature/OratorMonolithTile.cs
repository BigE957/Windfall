using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Windfall.Common.Graphics.Metaballs;
using Windfall.Content.Items.Placeables.Furnature;

namespace Windfall.Content.Tiles.Furnature
{
    public class OratorMonolithTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            RegisterItemDrop(ModContent.ItemType<OratorMonolith>());
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
            TileObjectData.newTile.Width = 3;
            TileObjectData.newTile.Origin = new Point16(2, 3);
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 18 };
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, 3, 0);
            TileObjectData.addTile(Type);

            AddMapEntry(Color.LimeGreen);

            DustType = DustID.Terra;
            AnimationFrameHeight = 70;
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            Player player = Main.LocalPlayer;

            if (player is null)
                return;

            if (Main.tile[i, j].TileFrameY < AnimationFrameHeight)
            {
                if (player.active)
                    Main.LocalPlayer.Monolith().NearActiveMonolith = false;
                return;
            }       

            if (player.active)
            {
                Main.LocalPlayer.Monolith().NearActiveMonolith = true;
                Main.LocalPlayer.Monolith().OratorMonolith = true;
            }

            if (Main.tile[i, j + 1].TileType != Type && Main.tile[i - 1 , j].TileType != Type)
            {
                EmpyreanMetaball.SpawnDefaultParticle(new Vector2(i * 16, j * 16) + new Vector2(24, -32), Vector2.Zero, 40f);
            }
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = ModContent.ItemType<OratorMonolith>();
        }

        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

        public override bool RightClick(int i, int j)
        {
            HitWire(i, j);
            SoundEngine.PlaySound(SoundID.Mech, new Vector2(i * 16, j * 16));
            return true;
        }

        public override void HitWire(int i, int j)
        {
            int x = i - Main.tile[i, j].TileFrameX / 18 % 4;
            int y = j - Main.tile[i, j].TileFrameY / 18 % 4;
            int tileXX18 = AnimationFrameHeight;
            for (int l = x; l < x + 4; l++)
            {
                for (int m = y; m < y + 4; m++)
                {
                    if (Main.tile[l, m].HasTile && Main.tile[l, m].TileType == Type)
                    {
                        if (Main.tile[l, m].TileFrameY < tileXX18)
                            Main.tile[l, m].TileFrameY += (short)(tileXX18);
                        else
                            Main.tile[l, m].TileFrameY -= (short)(tileXX18);
                    }
                }
            }
            if (Wiring.running)
            {
                for (int o = 0; o < 4; o++)
                {
                    for (int p = 0; p < 4; p++)
                    {
                        Wiring.SkipWire(x + 0, x + p);
                    }
                }
            }
            NetMessage.SendTileSquare(-1, x, y + 1, 3);
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            base.KillMultiTile(i, j, frameX, frameY);

            Player player = Main.LocalPlayer;

            if (player is null)
                return;

            player.Monolith().NearActiveMonolith = false;
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            Texture2D texture;
            texture = TextureAssets.Tile[Type].Value;
            Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
            if (Main.drawToScreen)
            {
                zero = Vector2.Zero;
            }
            int height = 18;
            int animate = 0;
            if (tile.TileFrameY >= AnimationFrameHeight)
            {
                animate = Main.tileFrame[Type] * AnimationFrameHeight;
            }
            Main.spriteBatch.Draw(texture, new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero, new Rectangle(tile.TileFrameX, tile.TileFrameY + animate, 16, height), Lighting.GetColor(i, j), 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
            return false;
        }
    }
}

