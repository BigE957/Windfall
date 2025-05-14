
namespace Windfall.Content.Items.Vanity.DevVanities;
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
            if(devVanity.HasBack)
                self.back = EquipLoader.GetEquipSlot(Mod, item.ModItem.Name, EquipType.Back);
            self.legs = EquipLoader.GetEquipSlot(Mod, item.ModItem.Name, EquipType.Legs);
            self.body = EquipLoader.GetEquipSlot(Mod, item.ModItem.Name, EquipType.Body);
            self.head = EquipLoader.GetEquipSlot(Mod, item.ModItem.Name, EquipType.Head);
        }
    }
}
