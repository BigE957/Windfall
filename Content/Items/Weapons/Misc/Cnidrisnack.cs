using CalamityMod.Items;

namespace Windfall.Content.Items.Weapons.Misc;

public class Cnidrisnack : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Weapons.Misc";
    public override string Texture => "Windfall/Assets/Items/Weapons/Misc/Cnidrisnack";

    public override void SetDefaults()
    {
        Item.damage = 1;
        Item.knockBack = 0f;
        Item.useAnimation = Item.useTime = 10;
        Item.DamageType = DamageClass.Default;
        Item.noMelee = true;
        Item.autoReuse = false;
        Item.consumable = true;
        Item.shootSpeed = 14f;
        Item.shoot = ModContent.ProjectileType<CnidrisnackThrow>();
        Item.maxStack = 99;
        Item.bait = 25;

        Item.width = 32;
        Item.height = 34;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.UseSound = SoundID.Item7;
        Item.value = CalamityGlobalItem.RarityBlueBuyPrice;
        Item.rare = ItemRarityID.Blue;
    }
    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
    {
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.FishingBait;
    }
    public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
    {
        Texture2D texture = ModContent.Request<Texture2D>("Windfall/Assets/Items/Weapons/Misc/Cnidrisnack_Glow", AssetRequestMode.ImmediateLoad).Value;
        spriteBatch.Draw
        (
            texture,
            new Vector2(Item.Center.X - Main.screenPosition.X, Item.Center.Y - Main.screenPosition.Y),
            new Rectangle(0, 0, texture.Width, texture.Height),
            Color.White,
            rotation,
            texture.Size() * 0.5f,
            scale,
            SpriteEffects.None,
            0f
        );
    }
}

public class CnidrisnackThrow : ModProjectile
{
    public override string Texture => "Windfall/Assets/Items/Weapons/Misc/Cnidrisnack";
    public override void SetDefaults()
    {
        Projectile.width = 38;
        Projectile.height = 40;
        Projectile.DamageType = DamageClass.Default;
        Projectile.friendly = true;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 90000;
        Projectile.tileCollide = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 15;
        Projectile.aiStyle = 2;
    }
    public override void OnKill(int timeLeft)
    {
        Item.NewItem(Projectile.GetSource_DropAsItem(), (int)Projectile.position.X, (int)Projectile.position.Y, Projectile.width, Projectile.height, ModContent.ItemType<Cnidrisnack>());
    }
    public override void PostDraw(Color lightColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        int height = texture.Height;
        int drawStart = height * Projectile.frame;
        Vector2 origin = Projectile.Center;
        Main.EntitySpriteDraw(ModContent.Request<Texture2D>("Windfall/Assets/Items/Weapons/Misc/Cnidrisnack_Glow").Value, new Vector2(Projectile.Center.X - Main.screenPosition.X, Projectile.Center.Y - Main.screenPosition.Y), new Microsoft.Xna.Framework.Rectangle?(new Rectangle(0, drawStart, texture.Width, height)), Color.White * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
    }
}
