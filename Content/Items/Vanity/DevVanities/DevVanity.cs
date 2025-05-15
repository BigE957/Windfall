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

public class DevVanityPlayer : ModPlayer
{
    public override void Load()
    {
        On_Player.UpdateVisibleAccessory += SetDevVanity;
    }

    private void SetDevVanity(On_Player.orig_UpdateVisibleAccessory orig, Player self, int itemSlot, Item item, bool modded)
    {
        orig(self, itemSlot, item, modded);

        if (item.ModItem is DevVanity devVanity)
        {
            if (devVanity.HasBack)
                self.back = EquipLoader.GetEquipSlot(Mod, item.ModItem.Name, EquipType.Back);
            self.legs = EquipLoader.GetEquipSlot(Mod, item.ModItem.Name, EquipType.Legs);
            self.body = EquipLoader.GetEquipSlot(Mod, item.ModItem.Name, EquipType.Body);
            self.head = EquipLoader.GetEquipSlot(Mod, item.ModItem.Name, EquipType.Head);
        }
    }
}