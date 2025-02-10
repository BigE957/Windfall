using CalamityMod.Items;

namespace Windfall.Content.Items.Debug;

public class SuperCnidrisnack : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Debug";
    public override string Texture => "Windfall/Assets/Items/Debug/SuperCnidrisnack";

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
        Item.shoot = ModContent.ProjectileType<SuperCnidrisnackThrow>();
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
}

public class SuperCnidrisnackThrow : ModProjectile
{
    public override string Texture => "Windfall/Assets/Items/Debug/SuperCnidrisnack";
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 16;
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
        Item.NewItem(Projectile.GetSource_DropAsItem(), (int)Projectile.position.X, (int)Projectile.position.Y, Projectile.width, Projectile.height, ModContent.ItemType<SuperCnidrisnack>());
    }
    public override void PostDraw(Color lightColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
        int height = texture.Height;
        int drawStart = height * Projectile.frame;
        Vector2 origin = Projectile.Center;
        Main.EntitySpriteDraw(ModContent.Request<Texture2D>("Windfall/Assets/Items/Debug/SuperCnidrisnack_Glow").Value, new Vector2(Projectile.Center.X - Main.screenPosition.X, Projectile.Center.Y - Main.screenPosition.Y), new Microsoft.Xna.Framework.Rectangle?(new Rectangle(0, drawStart, texture.Width, height)), Color.White * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

    }
}

