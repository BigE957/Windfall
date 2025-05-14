namespace Windfall.Content.Items.Vanity.DevVanities;
public abstract class DevVanity : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Accessories.Vanity";

    public virtual string DevName => "";

    public virtual bool HasBack => false;    

    public override void Load()
    {
        if (Main.netMode != NetmodeID.Server)
        {
            EquipLoader.AddEquipTexture(Mod, $"Windfall/Assets/Items/Vanity/DevVanities/{DevName}/Head", EquipType.Head, this);
            EquipLoader.AddEquipTexture(Mod, $"Windfall/Assets/Items/Vanity/DevVanities/{DevName}/Body", EquipType.Body, this);
            EquipLoader.AddEquipTexture(Mod, $"Windfall/Assets/Items/Vanity/DevVanities/{DevName}/Legs", EquipType.Legs, this);
            if(HasBack)
                EquipLoader.AddEquipTexture(Mod, $"Windfall/Assets/Items/Vanity/DevVanities/{DevName}/Back", EquipType.Back, this);
        }
    }

    public override void SetStaticDefaults()
    {
        if (Main.netMode == NetmodeID.Server)
            return;

        int equipSlotHead = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
        ArmorIDs.Head.Sets.DrawHead[equipSlotHead] = false;

        int equipSlotBody = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);
        ArmorIDs.Body.Sets.HidesTopSkin[equipSlotBody] = true;
        ArmorIDs.Body.Sets.HidesArms[equipSlotBody] = true;

        int equipSlotLegs = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);
        ArmorIDs.Legs.Sets.HidesBottomSkin[equipSlotLegs] = true;
    }
}
