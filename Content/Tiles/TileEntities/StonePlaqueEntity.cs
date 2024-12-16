using System.Reflection;
using Terraria.ModLoader.IO;
using Windfall.Content.Tiles.Furnature;
using static Windfall.Common.Netcode.WindfallNetcodeMessages;

namespace Windfall.Content.Tiles.TileEntities;
public class StonePlaqueEntity : ModTileEntity
{
    public Vector2 Center => Position.ToWorldCoordinates(8f * DarkStonePlaqueTile.Width, 8f * DarkStonePlaqueTile.Height);
    private string myText = "";

    public string PlaqueText
    {
        get => myText;
        set
        {
            myText = value;
            SendSyncPacket();
        }
    }

    public override bool IsTileValidForEntity(int x, int y)
    {
        Tile tile = Main.tile[x, y];

        int style = 0, alt = 0;
        TileObjectData.GetTileInfo(tile, ref style, ref alt);
        TileObjectData data = TileObjectData.GetTileData(tile.TileType, style, alt);

        int sheetSquare = 16 + data.CoordinatePadding;
        int FrameX = tile.TileFrameX / sheetSquare % data.Width;
        int FrameY = tile.TileFrameY / sheetSquare % data.Height;

        return tile.HasTile && (tile.TileType == ModContent.TileType<DarkStonePlaqueTile>() || tile.TileType == ModContent.TileType<WhiteStonePlaqueTile>()) && FrameX == 0 && FrameY == 0;
    }

    public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            NetMessage.SendTileSquare(Main.myPlayer, i, j, DarkStonePlaqueTile.Width, DarkStonePlaqueTile.Height);
            NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);
            return -1;
        }

        int id = Place(i, j);
        return id;
    }

    public override void OnNetPlace() => NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);

    /*
    public override void OnKill()
    {
        foreach (Player p in Main.ActivePlayers)
        {
            // Use reflection to stop TML from spitting an error here.
            // Try-catching will not stop this error, TML will print it to console anyway. The error is harmless.
            ModPlayer[] mpStorageArray = (ModPlayer[])typeof(Player).GetField("modPlayers", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(p);
            if (mpStorageArray.Length == 0)
                continue;

            CalamityPlayer mp = p.Calamity();
            if (mp.CurrentlyViewedFactoryID == ID)
                mp.CurrentlyViewedFactoryID = -1;
        }
    }
    */
    public override void SaveData(TagCompound tag)
    {
        tag["text"] = myText;
    }

    public override void LoadData(TagCompound tag)
    {
        myText = tag.GetString("text");
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(myText);
    }

    public override void NetReceive(BinaryReader reader)
    {
        myText = reader.ReadString();
    }

    private void SendSyncPacket()
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            return;
        ModPacket packet = Mod.GetPacket();
        packet.Write((byte)WFNetcodeMessages.StonePlaqueSync);
        packet.Write(ID);
        packet.Write(myText);
        packet.Send(-1, -1);
    }

    internal static bool ReadSyncPacket(Mod mod, BinaryReader reader)
    {
        int teID = reader.ReadInt32();
        bool exists = ByID.TryGetValue(teID, out TileEntity te);

        // The rest of the packet must be read even if it turns out the factory doesn't exist for whatever reason.
        string text = reader.ReadString();

        // When a server gets this packet, it immediately sends an equivalent packet to all clients.
        if (Main.netMode == NetmodeID.Server)
        {
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)WFNetcodeMessages.StonePlaqueSync);
            packet.Write(teID);
            packet.Write(text);
            packet.Send(-1, -1);
        }

        if (exists && te is StonePlaqueEntity stonePlaque)
        {
            stonePlaque.myText = text;
            return true;
        }
        return false;
    }
}
