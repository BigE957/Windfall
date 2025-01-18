using CalamityMod.Items;
using System.Collections.Generic;
using Windfall.Content.Items.Placeables.Furnature.VerletHangers.Decorations;
using Windfall.Content.Tiles.Furnature.VerletHangers.Hangers;
using Windfall.Content.Tiles.TileEntities;

namespace Windfall.Content.Items.Tools;
public class CordShears : ModItem
{
    public override string Texture => "Terraria/Images/Item_3352";
    private static SoundStyle useSound = new ("CalamityMod/Sounds/Custom/ScissorGuillotineSnap");

    public override void SetDefaults()
    {
        Item.damage = 0;
        Item.useAnimation = Item.useTime = 15;

        Item.height = Item.width = 32;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
        Item.rare = ItemRarityID.Lime;
        Item.UseSound = useSound;
        Item.autoReuse = true;
        Item.useTurn = true;
    }

    public HangerEntity HE = null;
    public int HangIndex = -1;

    public override bool? UseItem(Player player)
    {
        if (HangIndex == -2)
        {
            if ((Main.MouseWorld - HE.Position.ToWorldCoordinates()).LengthSquared() > 625)
            {
                HE = null;
                HangIndex = -1;
                return true;
            }

            if(HE.DecorationVerlets.TryGetValue(-1, out Tuple<List<Luminance.Common.VerletIntergration.VerletSegment>, int, int> value))
                Item.NewItem(Item.GetSource_DropAsItem(), HE.Position.ToWorldCoordinates(), DecorationID.DecorationTypes[value.Item2]);
            bool removalFail = !HE.DecorationVerlets.Remove(-1);

            if (removalFail)
            {
                #region State Dependent Clearing
                switch (HE.State)
                {
                    case 1:
                        HE.MainVerlet.Clear();
                        HE.DecorationVerlets.Clear();
                        break;
                    case 2:
                        HangerEntity mainEntity = FindTileEntity<HangerEntity>(HE.PartnerLocation.Value.X, HE.PartnerLocation.Value.Y, 1, 1);
                        mainEntity.MainVerlet.Clear();
                        if (mainEntity.DecorationVerlets.ContainsKey(-1))
                        {
                            var hangerDecor = mainEntity.DecorationVerlets[-1];
                            mainEntity.DecorationVerlets.Clear();
                            mainEntity.DecorationVerlets.Add(-1, hangerDecor);
                        }
                        else
                            mainEntity.DecorationVerlets.Clear();
                        break;
                    default:
                        return true;
                }
                #endregion

                HE.State = 0;
                HE.SegmentCount = 0;
                if (HE.PartnerLocation.HasValue)
                {
                    HangerEntity partnerEntity = FindTileEntity<HangerEntity>(HE.PartnerLocation.Value.X, HE.PartnerLocation.Value.Y, 1, 1);
                    partnerEntity.State = 0;
                    partnerEntity.PartnerLocation = null;
                    partnerEntity.SegmentCount = 0;
                }
                HE.PartnerLocation = null;
            }
            HE.SendSyncPacket();
        }
        else if(HangIndex != -1)
        {
            if ((Main.MouseWorld - HE.MainVerlet[HangIndex].Position).LengthSquared() > 625)
            {
                HE = null;
                HangIndex = -1;
                return true;
            }
            Item.NewItem(Item.GetSource_DropAsItem(), HE.MainVerlet[HangIndex].Position, DecorationID.DecorationTypes[HE.DecorationVerlets[HangIndex].Item2]);
            HE.DecorationVerlets.Remove(HangIndex);
            HE.SendSyncPacket();
        }
        return true;
    }
}
