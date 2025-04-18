using Terraria.ModLoader.IO;
using Windfall.Content.Tiles.Furnature.VerletHangers.Hangers;
using static Windfall.Common.Netcode.WindfallNetcode;
using static Windfall.Common.Graphics.Verlet.VerletIntegration;
using System;

namespace Windfall.Content.Tiles.TileEntities;
public class HangerEntity : ModTileEntity
{
    private enum PairedState
    {
        Unpaired,
        Start,
        End
    }
    private PairedState state = PairedState.Unpaired;
    public byte State
    {
        get => (byte)state;
        set
        {
            if (value >= 0 && value < 3)
            {
                state = (PairedState)value;
                SendSyncPacket();
            }
        }
    }

    private Point16 partnerLocation = new(-1, -1);
    public Point16? PartnerLocation
    {
        get => partnerLocation == new Point16(-1, -1) ? null : partnerLocation;
        set
        {
            partnerLocation = value ?? new(-1, -1);
            SendSyncPacket();
        }
    }

    private byte cordID = 0;
    public byte? CordID
    {
        get => cordID == 0 ? null : cordID;
        set
        {
            cordID = value.HasValue && value.Value != 0 ? value.Value : byte.MinValue;
            SendSyncPacket();
        }
    }

    private int segmentCount = 0;
    public int SegmentCount
    {
        get => segmentCount;
        set
        {
            segmentCount = value;
            SendSyncPacket();
        }
    }

    public List<VerletPoint> MainVerlet = [];

    public Dictionary<int, (List<VerletPoint> chain, int decorationID, int segmentCount)> DecorationVerlets = [];

    public override bool IsTileValidForEntity(int x, int y)
    {
        Tile tile = Main.tile[x, y];
        if(!tile.HasTile || tile.TileType != ModContent.TileType<HangerTile>())
            return false;

        return true;
    }
    public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            NetMessage.SendTileSquare(Main.myPlayer, i, j, 1);
            NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type, 0f, 0, 0, 0);
            return -1;
        }
        return Place(i, j);
    }

    public override void OnNetPlace()
    {
        if (Main.netMode == NetmodeID.Server)
        {
            NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
        }
    }

    public override void Update()
    {
        if (partnerLocation != Point16.NegativeOne)
        {
            if (FindTileEntity<HangerEntity>(partnerLocation.X, partnerLocation.Y, 1, 1) == null)
            {
                PartnerLocation = null;
                State = 0;
            }
        }
    }

    public override void OnKill()
    {
        if (partnerLocation != Point16.NegativeOne)
        {
            HangerEntity partner = FindTileEntity<HangerEntity>(partnerLocation.X, partnerLocation.Y, 1, 1);
            if (partner != null)
            {
                partner.state = 0;
                partner.PartnerLocation = null;
            }
        }
    }

    public override void SaveData(TagCompound tag)
    {
        if(state != PairedState.Unpaired)
            tag["State"] = State;
        if (partnerLocation != Point16.NegativeOne)
        {
            tag["PartnerLocationX"] = partnerLocation.X;
            tag["PartnerLocationY"] = partnerLocation.Y;
        }
        if (segmentCount != 0)
            tag["Distance"] = segmentCount;
        if (cordID != 0)
            tag["RopeID"] = cordID;
        
        if(DecorationVerlets.Count > 0)
        {
            tag["DecorationCount"] = DecorationVerlets.Count;
            for (int i = 0; i < DecorationVerlets.Count; i++)
            {
                tag[$"DecorationIndex{i}"] = DecorationVerlets.Keys.ToArray()[i];
                tag[$"DecorationType{i}"] = DecorationVerlets.Values.ToArray()[i].decorationID;
                tag[$"DecorationLength{i}"] = DecorationVerlets.Values.ToArray()[i].segmentCount;
            }
        }
    }

    public override void LoadData(TagCompound tag)
    {
        state = (PairedState)tag.GetByte("State");
        int x = tag.GetShort("PartnerLocationX");
        int y = tag.GetShort("PartnerLocationY");
        partnerLocation = new(x, y);
        segmentCount = tag.GetInt("Distance");
        cordID = tag.GetByte("RopeID");
        
        int decorationCount = tag.GetInt("DecorationCount");
        for (int i = 0; i < decorationCount; i++)
            DecorationVerlets.Add(tag.GetInt($"DecorationIndex{i}"), new([], tag.GetInt($"DecorationType{i}"), tag.GetInt($"DecorationLength{i}")));
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(State);
        writer.Write(partnerLocation.X);
        writer.Write(partnerLocation.Y);
        writer.Write(segmentCount);
        writer.Write(cordID);

        writer.Write(DecorationVerlets.Count);
        for (int i = 0; i < DecorationVerlets.Count; i++)
        {
            writer.Write(DecorationVerlets.Keys.ToArray()[i]);
            writer.Write(DecorationVerlets.Values.ToArray()[i].decorationID);
            writer.Write(DecorationVerlets.Values.ToArray()[i].segmentCount);
        }
    }

    public override void NetReceive(BinaryReader reader)
    {
        state = (PairedState)reader.ReadByte();
        int x = reader.ReadInt16();
        int y = reader.ReadInt16();
        partnerLocation = new(x, y);
        segmentCount = reader.ReadInt32();
        cordID = reader.ReadByte();

        DecorationVerlets.Clear();
        int decorationCount = reader.ReadInt32();
        for (int i = 0; i < decorationCount; i++)
            DecorationVerlets.Add(reader.ReadInt32(), new([], reader.ReadInt32(), reader.ReadInt32()));
    }

    public void SendSyncPacket()
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            return;
        ModPacket packet = Mod.GetPacket();
        packet.Write((byte)WFNetcodeMessages.HangerSync);
        packet.Write(ID);

        packet.Write(State);
        packet.Write(partnerLocation.X);
        packet.Write(partnerLocation.Y);
        packet.Write(segmentCount);
        packet.Write(cordID);

        packet.Write(DecorationVerlets.Count);
        for (int i = 0; i < DecorationVerlets.Count; i++)
        {
            packet.Write(DecorationVerlets.Keys.ToArray()[i]);
            packet.Write(DecorationVerlets.Values.ToArray()[i].decorationID);
            packet.Write(DecorationVerlets.Values.ToArray()[i].segmentCount);
        }
        packet.Send(-1, -1);
    }

    internal static bool ReadSyncPacket(Mod mod, BinaryReader reader)
    {
        int teID = reader.ReadInt32();
        bool exists = ByID.TryGetValue(teID, out TileEntity te);

        PairedState state = (PairedState)reader.ReadByte();
        int x = reader.ReadInt16();
        int y = reader.ReadInt16();
        Point16 partnerLocation = new(x, y);
        int segmentCount = reader.ReadInt32();
        byte cordID = reader.ReadByte();

        Dictionary<int, (List<VerletPoint>, int, int)> DecorationVerlets = [];
        int decorationCount = reader.ReadInt32();
        for (int i = 0; i < decorationCount; i++)
            DecorationVerlets.Add(reader.ReadInt32(), new([], reader.ReadInt32(), reader.ReadInt32()));

        if (exists && te is HangerEntity hanger)
        {
            hanger.state = state;
            hanger.partnerLocation = partnerLocation;
            hanger.segmentCount = segmentCount;
            hanger.cordID = cordID;
            hanger.DecorationVerlets = DecorationVerlets;

            return true;
        }

        return false;
    }
}
