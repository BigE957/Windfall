using static Windfall.Common.Netcode.WindfallNetcode;
using Terraria.ModLoader.IO;
using Luminance.Common.VerletIntergration;
using Windfall.Common.Graphics.Verlet;
using Windfall.Content.Tiles.Furnature.VerletHangers.Hangers;
using Windfall.Content.Items.Placeables.Furnature.VerletHangers.Decorations;

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

    private byte ropeID = 0;
    public byte? RopeID
    {
        get => ropeID == 0 ? null : ropeID;
        set
        {
            ropeID = value.HasValue && value.Value != 0 ? value.Value : byte.MinValue;
            SendSyncPacket();
        }
    }

    private float distance = 0;
    public float Distance
    {
        get => distance;
        set
        {
            distance = value;
            SendSyncPacket();
        }
    }

    public List<VerletSegment> MainVerlet = [];
    
    public Dictionary<int, Tuple<List<VerletSegment>, int, int>> DecorationVerlets = [];

    public override bool IsTileValidForEntity(int x, int y)
    {
        Tile tile = Main.tile[x, y];
        if(!tile.HasTile || tile.TileType != ModContent.TileType<HangerTile>())
            return false;

        return true;
    }

    public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
    {
        Main.NewText("Placement Hook");
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            NetMessage.SendTileSquare(Main.myPlayer, i, j, 1, 1);
            NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);
            return -1;
        }

        int id = Place(i, j);
        return id;
    }

    public override void OnNetPlace() => NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);


    public override void Update()
    {
        if (!Main.tile[Position].HasTile || Main.tile[Position].TileType != ModContent.TileType<HangerTile>())
            Kill(Position.X, Position.Y);
        if (partnerLocation != Point16.NegativeOne)
        {
            if (!Main.tile[partnerLocation].HasTile || Main.tile[partnerLocation].TileType != ModContent.TileType<HangerTile>())
                Kill(partnerLocation.X, partnerLocation.Y);
            if (FindTileEntity<HangerEntity>(partnerLocation.X, partnerLocation.Y, 1, 1) == null)
            {
                PartnerLocation = null;
                State = 0;
            }
        }
        if (DecorationID.DecorationIDs.Contains(Main.LocalPlayer.HeldItem.type))
        {
            Decoration decor = (Decoration)Main.LocalPlayer.HeldItem.ModItem;
            Vector2 worldPos = Position.ToWorldCoordinates();

            Color color = Color.White;
            if (DecorationVerlets.ContainsKey(-1))
                color = Color.Red;
            else if ((worldPos - Main.MouseWorld).LengthSquared() < 25)
            {
                decor.HangIndex = -2;
                decor.StartingEntity = this;
                color = Color.Green;
            }

            Particle particle = new GlowOrbParticle(worldPos, Vector2.Zero, false, 4, 0.5f, color, needed: true);
            GeneralParticleHandler.SpawnParticle(particle);
        }
    }

    public override void OnKill()
    {
        VerletHangerDrawing.hangers.Remove(Position);
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
        if (distance != 0)
            tag["Distance"] = distance;
        if (ropeID != 0)
            tag["RopeID"] = ropeID;
        if(DecorationVerlets.Count > 0)
        {
            tag["DecorationCount"] = DecorationVerlets.Count;
            for (int i = 0; i < DecorationVerlets.Count; i++)
            {
                tag[$"DecorationIndex{i}"] = DecorationVerlets.Keys.ToArray()[i];
                tag[$"DecorationType{i}"] = DecorationVerlets.Values.ToArray()[i].Item2;
                tag[$"DecorationLength{i}"] = DecorationVerlets.Values.ToArray()[i].Item3;
            }
        }
    }

    public override void LoadData(TagCompound tag)
    {
        state = (PairedState)tag.GetByte("State");
        int x = tag.GetShort("PartnerLocationX");
        int y = tag.GetShort("PartnerLocationY");
        partnerLocation = new(x, y);
        distance = tag.GetFloat("Distance");
        ropeID = tag.GetByte("RopeID");
        int decorationCount = tag.GetInt("DecorationCount");
        for (int i = 0; i < decorationCount; i++)
        {
            DecorationVerlets.Add(tag.GetInt($"DecorationIndex{i}"), new([], tag.GetInt($"DecorationType{i}"), tag.GetInt($"DecorationLength{i}")));
        }

        VerletHangerDrawing.hangers.Add(Position);
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(State);
        writer.Write(partnerLocation.X);
        writer.Write(partnerLocation.Y);
        writer.Write(distance);
        writer.Write(ropeID);
    }

    public override void NetReceive(BinaryReader reader)
    {
        state = (PairedState)reader.ReadByte();
        int x = reader.ReadInt32();
        int y = reader.ReadInt32();
        partnerLocation = new(x, y);
        distance = reader.ReadSingle();
        ropeID = reader.ReadByte();
    }

    private void SendSyncPacket()
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            return;
        ModPacket packet = Mod.GetPacket();
        packet.Write((byte)WFNetcodeMessages.StonePlaqueSync);
        packet.Write(ID);
        packet.Write(State);
        packet.Write(partnerLocation.X);
        packet.Write(partnerLocation.Y);
        packet.Write(distance);
        packet.Write(ropeID);

        packet.Send();
    }

    internal static bool ReadSyncPacket(Mod mod, BinaryReader reader)
    {
        int teID = reader.ReadInt32();
        bool exists = ByID.TryGetValue(teID, out TileEntity te);

        // The rest of the packet must be read even if it turns out the factory doesn't exist for whatever reason.
        PairedState paired = (PairedState)reader.ReadByte();
        int x = reader.ReadInt32();
        int y = reader.ReadInt32();
        Point16 partner = new(x, y);
        float dist = reader.ReadSingle();
        byte id = reader.ReadByte();


        // When a server gets this packet, it immediately sends an equivalent packet to all clients.
        if (Main.netMode == NetmodeID.Server)
        {
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)WFNetcodeMessages.HangerSync);
            packet.Write(teID);
            packet.Write(partner.X);
            packet.Write(partner.Y);
            packet.Write(dist);
            packet.Write(id);

            packet.Send();
        }

        if (exists && te is HangerEntity HE)
        {
            HE.state = paired;
            HE.partnerLocation = partner;
            HE.distance = dist;
            HE.ropeID = id;
            return true;
        }
        return false;
    }
}
