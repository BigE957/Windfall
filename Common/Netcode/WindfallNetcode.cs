using Terraria.Chat;
using Windfall.Content.Tiles.TileEntities;

namespace Windfall.Common.Netcode;
public class WindfallNetcode
{
    public enum WFNetcodeMessages : byte
    {
        StonePlaqueSync,
        HangerSync,
    }
    public static void HandlePacket(Mod mod, BinaryReader reader, int whoAmI)
    {
        try
        {
            WFNetcodeMessages msgType = (WFNetcodeMessages)reader.ReadByte();

            switch (msgType)
            {
                #region Tile Entities
                case WFNetcodeMessages.StonePlaqueSync:
                    StonePlaqueEntity.ReadSyncPacket(mod, reader);
                    break;
                case WFNetcodeMessages.HangerSync:
                    HangerEntity.ReadSyncPacket(mod, reader);
                    break;
                #endregion
                default:
                    WindfallMod.Instance.Logger.Error($"Failed to parse Windfall packet: No Calamity packet exists with ID {msgType}.");
                    throw new Exception("Failed to parse Windfall packet: Invalid Windfall packet ID.");
            }
        }
        catch (Exception e)
        {
            if (e is EndOfStreamException eose)
                WindfallMod.Instance.Logger.Error("Failed to parse Windfall packet: Packet was too short, missing data, or otherwise corrupt.", eose);
            else if (e is ObjectDisposedException ode)
                WindfallMod.Instance.Logger.Error("Failed to parse Windfall packet: Packet reader disposed or destroyed.", ode);
            else if (e is IOException ioe)
                WindfallMod.Instance.Logger.Error("Failed to parse Windfall packet: An unknown I/O error occurred.", ioe);
            else
                throw; // this either will crash the game or be caught by TML's packet policing
        }
    }
}
