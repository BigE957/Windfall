using CalamityMod;
using CalamityMod.Items.Placeables.Furniture;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Windfall.Content.Items.Placeables.Furnature;
using Windfall.Content.Items.Utility;

namespace Windfall.Content.Tiles.Furnature;
public class SelenicChestTile : ModTile
{
    public override void SetStaticDefaults()
    {
        RegisterItemDrop(ModContent.ItemType<SelenicChest>());

        Main.tileSpelunker[Type] = true;
        Main.tileContainer[Type] = true;
        Main.tileShine2[Type] = true;
        Main.tileShine[Type] = 1200;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileOreFinderPriority[Type] = 500;
        TileID.Sets.BasicChest[Type] = true;
        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;
        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.Origin = new Point16(0, 1);
        TileObjectData.newTile.CoordinateHeights = [16, 18];
        TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(new Func<int, int, int, int, int, int, int>(Chest.FindEmptyChest), -1, 0, true);
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(new Func<int, int, int, int, int, int, int>(Chest.AfterPlacement_Hook), -1, 0, false);
        TileObjectData.newTile.AnchorInvalidTiles = [TileID.MagicalIceBlock];
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.LavaPlacement = LiquidPlacement.Allowed;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
        TileObjectData.addTile(Type);

        AdjTiles = [TileID.Containers];

        AddMapEntry(new Color(174, 129, 92), this.GetLocalization("Unlocked"), MapChestName);
        AddMapEntry(new Color(174, 129, 92), this.GetLocalization("Locked"), MapChestName);
        DustType = DustID.Gold;
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;
    public override void NumDust(int i, int j, bool fail, ref int num) => num = 1;

    public string MapChestName(string name, int i, int j)
    {
        // Bounds check.
        if (!WorldGen.InWorld(i, j, 2))
            return name;

        Tile tile = Main.tile[i, j];
        int left = i;
        int top = j;
        if (tile.TileFrameX % 36 != 0)
            left--;
        if (tile.TileFrameY != 0)
            top--;

        int chest = Chest.FindChest(left, top);

        // Valid chest index check.
        if (chest < 0)
            return name;

        // Concatenate the chest's custom name if it has one.
        if (!string.IsNullOrEmpty(Main.chest[chest].name))
            name += $": {Main.chest[chest].name}";

        return name;
    }

    public override ushort GetMapOption(int i, int j) => (ushort)(Main.tile[i, j].TileFrameX / 36);
    public override LocalizedText DefaultContainerName(int frameX, int frameY)
    {
        int option = frameX / 36;
        return this.GetLocalization("MapEntry" + option);
    }
    public override void MouseOver(int i, int j)
    {
        Player player = Main.LocalPlayer;
        Tile tile = Main.tile[i, j];
        string chestName = TileLoader.DefaultContainerName(tile.TileType, tile.TileFrameX, tile.TileFrameY);
        int left = i;
        int top = j;
        if (tile.TileFrameX % 36 != 0)
        {
            left--;
        }
        if (tile.TileFrameY != 0)
        {
            top--;
        }
        int chest = Chest.FindChest(left, top);
        player.cursorItemIconID = -1;
        if (chest < 0)
        {
            player.cursorItemIconText = Language.GetTextValue("LegacyChestType.0");
        }
        else
        {
            player.cursorItemIconText = Main.chest[chest].name.Length > 0 ? Main.chest[chest].name : chestName;
            if (player.cursorItemIconText == chestName)
            {
                player.cursorItemIconID = ModContent.ItemType<SelenicChest>();
                player.cursorItemIconText = "";
            }
        }
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
    }
    public override void MouseOverFar(int i, int j)
    {
        MouseOver(i, j);
        Player player = Main.LocalPlayer;
        if (player.cursorItemIconText == "")
        {
            player.cursorItemIconEnabled = false;
            player.cursorItemIconID = ItemID.None;
        }
    }
    public override void KillMultiTile(int i, int j, int frameX, int frameY) => Chest.DestroyChest(i, j);

    // Locked Chest stuff
    public override bool IsLockedChest(int i, int j) => Main.tile[i, j].TileFrameX / 36 == 1;
    public override bool UnlockChest(int i, int j, ref short frameXAdjustment, ref int dustType, ref bool manual)
    {
        if (Main.LocalPlayer.HeldItem.type != ModContent.ItemType<CrescentKey>())
            return false;

        Main.LocalPlayer.HeldItem.stack--; // Uses the key

        dustType = DustType;
        return true;
    }
    public override bool RightClick(int i, int j)
    {
        Tile tile = Main.tile[i, j];

        int left = i;
        int top = j;

        if (tile.TileFrameX % 36 != 0)
        {
            left--;
        }
        if (tile.TileFrameY != 0)
        {
            top--;
        }
        return LockedChestRightClick(IsLockedChest(left, top), left, top, i, j);
    }

    public static bool LockedChestRightClick(bool isLocked, int left, int top, int i, int j)
    {
        Player player = Main.LocalPlayer;

        // If the player right clicked the chest while editing a sign, finish that up
        if (player.sign >= 0)
        {
            SoundEngine.PlaySound(SoundID.MenuClose);
            player.sign = -1;
            Main.editSign = false;
            Main.npcChatText = "";
        }

        // If the player right clicked the chest while editing a chest, finish that up
        if (Main.editChest)
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
            Main.editChest = false;
            Main.npcChatText = "";
        }

        // If the player right clicked the chest after changing another chest's name, finish that up
        if (player.editedChestName)
        {
            NetMessage.SendData(MessageID.SyncPlayerChest, -1, -1, NetworkText.FromLiteral(Main.chest[player.chest].name), player.chest, 1f, 0f, 0f, 0, 0, 0);
            player.editedChestName = false;
        }
        if (Main.netMode == NetmodeID.MultiplayerClient && !isLocked)
        {
            // Right clicking the chest you currently have open closes it. This counts as interaction.
            if (left == player.chestX && top == player.chestY && player.chest >= 0)
            {
                player.chest = -1;
                Recipe.FindRecipes();
                SoundEngine.PlaySound(SoundID.MenuClose);
            }

            // Right clicking this chest opens it if it's not already open. This counts as interaction.
            else
            {
                NetMessage.SendData(MessageID.RequestChestOpen, -1, -1, null, left, (float)top, 0f, 0f, 0, 0, 0);
                Main.stackSplit = 600;
            }
            return true;
        }

        else
        {
            if (isLocked)
            {
                // If you right click the locked chest and you can unlock it, it unlocks itself but does not open. This counts as interaction.
                if (Chest.Unlock(left, top))
                {
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        NetMessage.SendData(MessageID.LockAndUnlock, -1, -1, null, player.whoAmI, 1f, (float)left, (float)top);
                    }
                    return true;
                }
            }
            else
            {
                int chest = Chest.FindChest(left, top);
                if (chest >= 0)
                {
                    Main.stackSplit = 600;

                    // If you right click the same chest you already have open, it closes. This counts as interaction.
                    if (chest == player.chest)
                    {
                        player.chest = -1;
                        SoundEngine.PlaySound(SoundID.MenuClose);
                    }

                    // If you right click this chest when you have a different chest selected, that one closes and this one opens. This counts as interaction.
                    else
                    {
                        player.chest = chest;
                        Main.playerInventory = true;
                        Main.recBigList = false;
                        player.chestX = left;
                        player.chestY = top;
                        SoundEngine.PlaySound(player.chest < 0 ? SoundID.MenuOpen : SoundID.MenuTick);
                    }

                    Recipe.FindRecipes();
                    return true;
                }
            }
        }

        // This only occurs when the chest is locked and cannot be unlocked. You did not interact with the chest.
        return false;
    }
}
