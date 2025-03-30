using DialogueHelper.UI.Dialogue;
using Terraria.Enums;
using Windfall.Content.Items.Placeables.Furnature.Plaques;
using Windfall.Content.Items.Tools;
using Windfall.Content.Tiles.TileEntities;
using Windfall.Content.UI.StonePlaque;

namespace Windfall.Content.Tiles.Furnature.Plaques;
public class DarkStonePlaqueTile : ModTile
{
    public const int Width = 5;
    public const int Height = 2;
    public const int SheetSquare = 16;

    private static StonePlaqueEntity DialogueTileEntity = null;

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = false;
        TileID.Sets.DisableSmartCursor[Type] = true;
        TileID.Sets.AvoidedByNPCs[Type] = true;
        TileID.Sets.TileInteractRead[Type] = true;
        TileID.Sets.InteractibleByNPCs[Type] = true;

        RegisterItemDrop(ModContent.ItemType<DarkStonePlaque>());

        TileObjectData.newTile.CopyFrom(TileObjectData.Style5x4); // Uses 5x4 style, but reduces height to 2.
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.Height = 2;
        TileObjectData.newTile.CoordinateHeights = [16, 16];
        TileObjectData.newTile.Origin = new Point16(2, 1);
        TileObjectData.newTile.CoordinatePadding = 0;

        ModTileEntity te = ModContent.GetInstance<StonePlaqueEntity>();
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(te.Hook_AfterPlacement, -1, 0, true);

        TileObjectData.newTile.StyleWrapLimit = 2;
        TileObjectData.newTile.StyleMultiplier = 2;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;

        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;

        TileObjectData.addAlternate(1);

        TileObjectData.addTile(Type);

        AddMapEntry(Color.DarkGray);

        DustType = DustID.t_Granite;

        ModContent.GetInstance<DialogueUISystem>().TreeInitialize += ModifyTree;
    }

    public override void MouseOver(int i, int j)
    {
        Player player = Main.LocalPlayer;
        StonePlaqueEntity te = FindTileEntity<StonePlaqueEntity>(i, j, Width, Height, SheetSquare);
        if ((te == null || string.IsNullOrEmpty(te.PlaqueText)) && player.HeldItem.type != ModContent.ItemType<HammerChisel>())
            return;

        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        if (player.HeldItem.type == ModContent.ItemType<HammerChisel>())
            player.cursorItemIconID = ModContent.ItemType<HammerChisel>();
        else
            player.cursorItemIconID = ModContent.ItemType<DarkStonePlaque>();

    }

    public override bool RightClick(int i, int j)
    {
        if (string.IsNullOrEmpty(FindTileEntity<StonePlaqueEntity>(i, j, Width, Height, SheetSquare).PlaqueText) && Main.LocalPlayer.HeldItem.type != ModContent.ItemType<HammerChisel>())
            return false;

        DialogueTileEntity = FindTileEntity<StonePlaqueEntity>(i, j, Width, Height, SheetSquare);

        if (Main.LocalPlayer.HeldItem.type == ModContent.ItemType<HammerChisel>())
            ModContent.GetInstance<StonePlaqueUISystem>().OpenDarkStonePlaqueUI(DialogueTileEntity);
        else
        {
            DialogueUISystem uiSystem = ModContent.GetInstance<DialogueUISystem>();
            if (uiSystem.isDialogueOpen)
                uiSystem.isDialogueOpen = false;
            else
                uiSystem.DisplayDialogueTree(WindfallMod.Instance, "DarkStonePlaqueText", new(Name, []));
        }
        return true;
    }

    private static void ModifyTree(string treeKey, int dialogueID, int buttonID, bool swapped)
    {
        if (treeKey == "DarkStonePlaqueText")
            ModContent.GetInstance<DialogueUISystem>().CurrentTree.Dialogues[0].DialogueText[0].Text = DialogueTileEntity.PlaqueText;
    }
}
