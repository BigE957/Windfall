using CalamityMod.Items;
using Windfall.Content.Tiles.TileEntities;
using static Windfall.Common.Graphics.Verlet.VerletIntegration;

namespace Windfall.Content.Items.Tools;
public class CordageMeter : ModItem
{
    public override string Texture => "Terraria/Images/Item_3095";

    public override void SetDefaults()
    {
        Item.damage = 0;
        Item.useAnimation = Item.useTime = 20;

        Item.height = Item.width = 32;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
        Item.rare = ItemRarityID.Lime;
        Item.UseSound = SoundID.Thunder;
        Item.autoReuse = false;
        Item.useTurn = true;
    }

    public override bool AltFunctionUse(Player player) => true;

    public HangerEntity HE = null;
    public int HangIndex = -1;

    public override bool? UseItem(Player player)
    {
        int sizeChange = (player.altFunctionUse == 2 ? -1 : 1);
        if (HangIndex == -2)
        {
            if ((Main.MouseWorld - HE.Position.ToWorldCoordinates()).LengthSquared() > 625)
            {
                HE = null;
                HangIndex = -1;
                return true;
            }
            if (!HE.DecorationVerlets.Remove(-1))
            {
                switch (HE.State)
                {
                    case 1:
                        if (HE.SegmentCount == 2 && sizeChange == -1)
                            return true;
                        HE.SegmentCount += sizeChange;
                        break;
                    case 2:
                        HangerEntity mainEntity = FindTileEntity<HangerEntity>(HE.PartnerLocation.Value.X, HE.PartnerLocation.Value.Y, 1, 1);
                        if (mainEntity.SegmentCount == 2 && sizeChange == -1)
                            return true;
                        mainEntity.SegmentCount += sizeChange;
                        break;
                    default:
                        return true;
                }
            }
        }
        else if (HangIndex != -1)
        {
            if ((Main.MouseWorld - HE.MainVerlet[HangIndex].Position).LengthSquared() > 625)
            {
                HE = null;
                HangIndex = -1;
                return true;
            }
            if (HE.DecorationVerlets[HangIndex].segmentCount == 2 && sizeChange == -1)
                return true;
            (List<VerletPoint>, int, int) newDV = new(HE.DecorationVerlets[HangIndex].chain, HE.DecorationVerlets[HangIndex].decorationID, HE.DecorationVerlets[HangIndex].segmentCount + sizeChange);
            HE.DecorationVerlets.Remove(HangIndex);
            HE.DecorationVerlets[HangIndex] = newDV;
            HE.SendSyncPacket();
        }
        return true;
    }
}