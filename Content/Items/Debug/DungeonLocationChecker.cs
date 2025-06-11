using Luminance;
using Luminance.Assets;
using Luminance.Core.Graphics;
using Windfall.Content.NPCs.Bosses.Orator;
using Windfall.Content.Projectiles.Boss.Orator;
using Windfall.Content.Projectiles.NPCAnimations;

namespace Windfall.Content.Items.Debug;

public class DungeonLocationChecker : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Debug";
    public override string Texture => "CalamityMod/Items/Weapons/Summon/StaffOfNecrosteocytes";
    public override void SetDefaults()
    {
        Item.width = 25;
        Item.height = 29;
        Item.rare = ItemRarityID.Red;
        Item.useAnimation = Item.useTime = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
    }

    public override void HoldItem(Player player)
    {
        //Shader Playground
        //ManagedScreenFilter testFilter = ShaderManager.GetFilter("Windfall.TestFilter");

        Vector3[] colors =
        [
            #region Soft Sunset
            /*
            FromHexToVec3("#020105"),
            FromHexToVec3("#260434"),
            FromHexToVec3("#66055C"),
            FromHexToVec3("#9A0452"),
            FromHexToVec3("#D1001C"),
            FromHexToVec3("#FF3F05"),
            FromHexToVec3("#FFB338"),
            FromHexToVec3("#FFFF6B"),
            FromHexToVec3("#DAFF9E"),
            FromHexToVec3("#DCFFD1")
            */
            #endregion

            #region Soft Sunset
            /*
            FromHexToVec3("#280F2A"),
            FromHexToVec3("#4F1743"),
            FromHexToVec3("#781C4B"),
            FromHexToVec3("#A41E3F"),
            FromHexToVec3("#D31D1D"),
            FromHexToVec3("#EC6332"),
            FromHexToVec3("#F6AB55"),
            FromHexToVec3("#FDE37C"),
            FromHexToVec3("#F9FFA8"),
            FromHexToVec3("#F1FFD6")
            */
            #endregion

            #region Sephia Tone (ish? idk)
            /*
            FromHexToVec3("#16090C"),
            FromHexToVec3("#3A171D"),
            FromHexToVec3("#5E2629"),
            FromHexToVec3("#813936"),
            FromHexToVec3("#A45346"),
            FromHexToVec3("#BB7763"),
            FromHexToVec3("#C99D87"),
            FromHexToVec3("#D8BFAB"),
            FromHexToVec3("#E8DCCE"),
            FromHexToVec3("#F8F6F1")
            */
            #endregion
        ];

        //colors = [.. colors.Reverse()];
        //float strength = (float)Math.Sin(Main.GlobalTimeWrappedHourly) / 2f + 0.5f;
        /*
        testFilter.SetTexture(ModContent.Request<Texture2D>("Windfall/Assets/Graphics/Extra/BlueNoise"), 1, SamplerState.LinearWrap);
        testFilter.TrySetParameter("colors", colors);
        testFilter.TrySetParameter("strength", 1);
        testFilter.TrySetParameter("time", Main.GlobalTimeWrappedHourly * 2);
        //Main.NewText();

        testFilter.Activate();
        testFilter.Apply();
        */
    }

    public override bool? UseItem(Player player)
    {
        //NPC.NewNPCDirect(Entity.GetSource_FromAI(), new Vector2(Main.dungeonX, Main.dungeonY).ToWorldCoordinates(), NPCID.Zombie);
        
        return true;
    }
}
