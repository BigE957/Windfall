using CalamityMod;
using CalamityMod.Tiles.Ores;

namespace Windfall.Content.Items.Quests.SealingRitual;

public class UnchargedWayfinder : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Quest";
    public override string Texture => "CalamityMod/Items/Accessories/AscendantInsignia";
    public override void SetDefaults()
    {
        Item.width = 22;
        Item.height = 24;
        Item.useAnimation = 2;
        Item.useTime = 2;
        Item.autoReuse = true;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.channel = true;
        Item.rare = ItemRarityID.Quest;
        Item.maxStack = 1;
    }

    private readonly List<int> DustIndexes = [];
    private Point currentPoint = new(-1, -1);
    private int holdTime = 0;
    private int charge = 0;

    public override bool? UseItem(Player player)
    {
        Vector2 toMouse = (player.Calamity().mouseWorld - player.Center);
        float dist = toMouse.Length();
        toMouse = toMouse.SafeNormalize(Vector2.Zero);

        player.ChangeDir(Math.Sign(toMouse.X));

        if (player.Calamity().mouseWorld.ToTileCoordinates() != currentPoint)
        {
            currentPoint = player.Calamity().mouseWorld.ToTileCoordinates();
            holdTime = 0;
        }

        Tile hoverTile = Main.tile[currentPoint];
        if(hoverTile != null && hoverTile.TileType == ModContent.TileType<AerialiteOre>())
        {
            if(holdTime >= 120)
            {
                Main.tile[currentPoint].ResetToType((ushort)ModContent.TileType<AerialiteOreDisenchanted>());
                WorldGen.KillTile(currentPoint.X, currentPoint.Y, fail: true);
                charge++;
                holdTime = 0;
                for (int i = 0; i < 12; i++)
                    Dust.NewDustPerfect(player.Calamity().mouseWorld, DustID.BlueFairy);
                if (charge >= 10)
                    Item.ChangeItemType(ModContent.ItemType<Wayfinder>());
                return true;
            }
            if ((int)(Main.GlobalTimeWrappedHourly * 60) % 2 == 0)
            {
                Dust d = Dust.NewDustPerfect(player.Calamity().mouseWorld, DustID.BlueFairy, toMouse * -4);
                d.scale = Clamp(dist / 150f, 0.5f, 2f);
                DustIndexes.Add(d.dustIndex);
            }
            if (Main.rand.NextBool(4))
            {
                Dust d = Dust.NewDustPerfect(player.Calamity().mouseWorld, DustID.BlueFairy);
                d.velocity /= 2f;
            }
            holdTime++;
        }
        return base.UseItem(player);
    }

    public override void HoldItem(Player player)
    {
        foreach (int i in DustIndexes)
        {
            Vector2 toPlayer = (player.Center - Main.dust[i].position);
            float dist = Vector2.Distance(Main.dust[i].position, player.Center);
            if (dist < 16)
            {
                Main.dust[i].active = false;
                continue;
            }
            toPlayer = toPlayer.SafeNormalize(Vector2.Zero);
            Main.dust[i].velocity = toPlayer * 4;
            Main.dust[i].scale = Clamp(dist / 150f, 0.5f, 2f);
        }

        DustIndexes.RemoveAll(i => !Main.dust[i].active);
    }
}
