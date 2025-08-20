using CalamityMod.Items;
using Windfall.Content.Items.Placeables.Furnature.VerletHangers.Decorations;
using Windfall.Content.Tiles.TileEntities;
using static Windfall.Common.Graphics.Verlet.VerletIntegration;

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
            Main.NewText("NEGATIVE 2");

            if ((Main.MouseWorld - HE.Position.ToWorldCoordinates()).LengthSquared() > 625)
            {
                HE = null;
                HangIndex = -1;
                return true;
            }

            if(HE.DecorationVerlets.TryGetValue(-1, out (VerletObject chain, int id, int count) value))
                Item.NewItem(Item.GetSource_DropAsItem(), HE.Position.ToWorldCoordinates(), DecorationID.DecorationTypes[value.id]);
            bool removalFail = !HE.DecorationVerlets.Remove(-1);

            if (removalFail)
            {
                #region State Dependent Clearing
                switch (HE.State)
                {
                    case 1:
                        Item.NewItem(Item.GetSource_NaturalSpawn(), HE.Position.ToWorldCoordinates(), Vector2.Zero, Items.Placeables.Furnature.VerletHangers.Cords.CordID.CordTypes[(int)HE.CordID]);
                        foreach (var v in HE.DecorationVerlets.Values)
                        {
                            if (v.decorationID != 0)
                            {
                                Vector2 position = v.chain.Positions[0];
                                Item.NewItem(Item.GetSource_NaturalSpawn(), position, Vector2.Zero, DecorationID.DecorationTypes[v.decorationID]);
                            }
                        }

                        HE.MainVerlet = null;
                        HE.DecorationVerlets.Clear();
                        break;
                    case 2:
                        HangerEntity mainEntity = FindTileEntity<HangerEntity>(HE.PartnerLocation.Value.X, HE.PartnerLocation.Value.Y, 1, 1);

                        Item.NewItem(Item.GetSource_NaturalSpawn(), HE.Position.ToWorldCoordinates(), Vector2.Zero, Items.Placeables.Furnature.VerletHangers.Cords.CordID.CordTypes[(int)mainEntity.CordID]);
                        foreach (var v in mainEntity.DecorationVerlets.Values)
                        {
                            if (v.decorationID != 0)
                            {
                                Vector2 position = v.chain.Positions[0];
                                Item.NewItem(Item.GetSource_NaturalSpawn(), position, Vector2.Zero, DecorationID.DecorationTypes[v.decorationID]);
                            }
                        }

                        mainEntity.MainVerlet = null;
                        if (mainEntity.DecorationVerlets.TryGetValue(-1, out (VerletObject chain, int decorationID, int segmentCount) hangerDecor))
                        {
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
            Main.NewText("NOT NEGATIVE 1");

            if ((Main.MouseWorld - HE.MainVerlet[HangIndex].Position).LengthSquared() > 625)
            {
                HE = null;
                HangIndex = -1;
                return true;
            }
            Item.NewItem(Item.GetSource_DropAsItem(), HE.MainVerlet[HangIndex].Position, DecorationID.DecorationTypes[HE.DecorationVerlets[HangIndex].decorationID]);
            HE.DecorationVerlets.Remove(HangIndex);
            HE.SendSyncPacket();
        }
        return true;
    }
}
