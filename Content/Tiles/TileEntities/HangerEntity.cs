using static Windfall.Common.Netcode.WindfallNetcode;
using Terraria.ModLoader.IO;
using Luminance.Common.VerletIntergration;
using Windfall.Common.Graphics.Verlet;
using Windfall.Content.Tiles.Furnature.VerletHangers.Hangers;
using Windfall.Content.Items.Placeables.Furnature.VerletHangers.Twine;
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
    
    public Dictionary<int, Tuple<List<VerletSegment>, int>> DecorationVerlets = [];

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
        if (state == PairedState.Start && PartnerLocation.HasValue)
        {
            Vector2 StringStart = Position.ToWorldCoordinates();
            Vector2 StringEnd = PartnerLocation.Value.ToWorldCoordinates();

            if (MainVerlet.Count == 0)
            {
                MainVerlet = [];
                for (int k = 0; k < (int)distance; k++)
                    MainVerlet.Add(new VerletSegment(Vector2.Lerp(StringStart, StringEnd, k / distance), Vector2.Zero, false));
                MainVerlet[0].Locked = MainVerlet.Last().Locked = true;
            }

            Vector2[] segmentPositions = MainVerlet.Select(x => x.Position).ToArray();

            for (int k = 0; k < MainVerlet.Count; k++)
            {               
                if (DecorationID.DecorationIDs.Contains(Main.LocalPlayer.HeldItem.type) && (k % 5 == 2))
                {
                    Decoration decor = (Decoration)Main.LocalPlayer.HeldItem.ModItem;

                    if (!decor.PlacingInProgress)
                    {
                        Color color = Color.White;
                        if (DecorationVerlets.ContainsKey(k))
                            color = Color.Red;
                        else if ((segmentPositions[k] - Main.MouseWorld).LengthSquared() < 25)
                        {
                            decor.HangIndex = k;
                            decor.StartingEntity = this;
                            color = Color.Green;
                        }

                        Particle particle = new GlowOrbParticle(segmentPositions[k], Vector2.Zero, false, 4, 0.5f, color, needed: true);
                        GeneralParticleHandler.SpawnParticle(particle);
                    }
                }

                if (!MainVerlet[k].Locked)
                    MainVerlet[k].Position += Vector2.UnitX * (((float)Math.Sin(Main.windCounter / 13f) / 4f + 1) * Main.windSpeedCurrent * 16);
            }

            VerletSimulations.RopeVerletSimulation(MainVerlet, StringStart, 50f, new(Gravity: 0.5f), StringEnd);

            for (int i = 0; i < DecorationVerlets.Count; i++)
            {
                int index = DecorationVerlets.ElementAt(i).Key;
                List<VerletSegment> subVerlet = DecorationVerlets.ElementAt(i).Value.Item1;

                for (int k = 0; k < subVerlet.Count; k++)
                {
                    if (!subVerlet[k].Locked)
                    {
                        if(Math.Abs(Main.windSpeedCurrent) < 0.0425f)
                            subVerlet[k].Velocity.X += (subVerlet[k].Position - subVerlet[k].OldPosition).X * 0.3f;

                        subVerlet[k].Velocity.X *= 0.925f;
                        if (Math.Abs(subVerlet[k].Velocity.X) < 0.1f)
                            subVerlet[k].Velocity.X = 0;                            

                        foreach (Player p in Main.ActivePlayers)
                        {
                            Rectangle hitbox = p.Hitbox;
                            hitbox.Inflate(4, 10);
                            if (hitbox.Contains((int)subVerlet[k].Position.X, (int)subVerlet[k].Position.Y))
                            {
                                subVerlet[k].Velocity.X = Math.Sign(p.velocity.X) * (p.velocity.Length());
                            }
                        }
                        float windSpeed = ((float)Math.Sin((Main.windCounter + index) / 13f) / 4f + 1) * Main.windSpeedCurrent * (2 * k);
                        subVerlet[k].Position.X += windSpeed;
                        //subVerlet[k].OldPosition.X += windSpeed;
                    }
                }

                VerletSimulations.RopeVerletSimulation(subVerlet, MainVerlet[index].Position, 50f, new());
            }
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
    }

    public override void LoadData(TagCompound tag)
    {
        state = (PairedState)tag.GetByte("State");
        int x = tag.GetShort("PartnerLocationX");
        int y = tag.GetShort("PartnerLocationY");
        partnerLocation = new(x, y);
        distance = tag.GetFloat("Distance");

        VerletHangerDrawing.hangers.Add(Position);
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(State);
        writer.Write(partnerLocation.X);
        writer.Write(partnerLocation.Y);
        writer.Write(distance);
    }

    public override void NetReceive(BinaryReader reader)
    {
        state = (PairedState)reader.ReadByte();
        int x = reader.ReadInt32();
        int y = reader.ReadInt32();
        partnerLocation = new(x, y);
        distance = reader.ReadSingle();
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


        // When a server gets this packet, it immediately sends an equivalent packet to all clients.
        if (Main.netMode == NetmodeID.Server)
        {
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)WFNetcodeMessages.StonePlaqueSync);
            packet.Write(teID);
            packet.Write(partner.X);
            packet.Write(partner.Y);
            packet.Write(dist);

            packet.Send();
        }

        if (exists && te is HangerEntity jewelery)
        {
            jewelery.state = paired;
            jewelery.partnerLocation = partner;
            jewelery.distance = dist;
            return true;
        }
        return false;
    }
}
