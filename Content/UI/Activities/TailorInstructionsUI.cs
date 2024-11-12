using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.Items.Quest.Tailor;

namespace Windfall.Content.UI.Activities;
internal class TailorInstructionsUIState : UIState
{
    public DraggableUIPanel UIPanel;
    public enum Pages
    {
        None = -1,
        Components,
        Clothing,
        Masks
    }
    public Pages page = Pages.None;
    public Vector2 start = new(200, 100);
    public override void OnInitialize()
    {
        DraggableUIPanel UIPanel = new();
        UIPanel.SetPadding(0);

        SetRectangle(UIPanel, left: start.X, top: start.Y, width: 400f, height: 518f);
        UIPanel.BackgroundColor = Color.Transparent;
        Append(UIPanel);

        Asset<Texture2D> pageTexture = ModContent.Request<Texture2D>($"{nameof(Windfall)}/Assets/UI/WanderersJournals/JournalPage");
        UIImage pageImage = new(pageTexture);
        SetRectangle(pageImage, left: -55f, top: -25f, width: 400f, height: 518f);
        UIPanel.Append(pageImage);

        Asset<Texture2D> journalArrowRightTexture = ModContent.Request<Texture2D>($"{nameof(Windfall)}/Assets/UI/WanderersJournals/JournalArrowRight");
        UIButton nextPageButton = new(journalArrowRightTexture, "Next Page");
        SetRectangle(nextPageButton, left: 325f, top: 500f, width: 56f, height: 26f);
        nextPageButton.OnLeftClick += new MouseEvent(NextPage);
        UIPanel.Append(nextPageButton);

        Asset<Texture2D> journalArrowLeftTexture = ModContent.Request<Texture2D>($"{nameof(Windfall)}/Assets/UI/WanderersJournals/JournalArrowLeft");
        UIButton previousPageButton = new(journalArrowLeftTexture, "Previous Page");
        SetRectangle(previousPageButton, left: 25f, top: 500f, width: 56f, height: 26f);
        previousPageButton.OnLeftClick += new MouseEvent(PreviousPage);
        UIPanel.Append(previousPageButton);

        float y = 90f;
        const float offset = 55f;

        switch (page)
        {
            case Pages.Components:
                #region Hood
                UIItem silkIcon = new(ItemID.Silk);
                SetRectangle(silkIcon, left: 40f - (silkIcon.Width.Pixels / 2f), top: y, width: silkIcon.Width.Pixels, height: silkIcon.Height.Pixels);
                UIPanel.Append(silkIcon);
                UIText itemCount = new("10x", 0.75f);
                SetRectangle(itemCount, left: 0, top: silkIcon.Height.Pixels, width: itemCount.Width.Pixels, height: itemCount.Height.Pixels);
                itemCount.HAlign = 0.5f;
                silkIcon.Append(itemCount);

                UIText at = new("@", 0.5f, true);
                SetRectangle(at, left: 60f, top: y + 5, width: at.Width.Pixels, height: at.Height.Pixels);
                UIPanel.Append(at);

                UIItem loomIcon = new(ItemID.Loom);
                SetRectangle(loomIcon, left: 100f - (silkIcon.Width.Pixels / 2f), top: y, width: loomIcon.Width.Pixels, height: loomIcon.Height.Pixels);
                UIPanel.Append(loomIcon);

                UIText equals = new("=", 0.5f, true);
                SetRectangle(equals, left: 120f, top: y, width: equals.Width.Pixels, height: equals.Height.Pixels);
                UIPanel.Append(equals);

                UIItem hoodIcon = new(ModContent.ItemType<Hood>());
                SetRectangle(hoodIcon, left: 160f - (hoodIcon.Width.Pixels / 2f), top: y, width: hoodIcon.Width.Pixels, height: hoodIcon.Height.Pixels);
                UIPanel.Append(hoodIcon);
                #endregion
                y += offset;
                #region Robe
                silkIcon = new(ItemID.Silk);
                SetRectangle(silkIcon, left: 40f - (silkIcon.Width.Pixels / 2f), top: y, width: silkIcon.Width.Pixels, height: silkIcon.Height.Pixels);
                UIPanel.Append(silkIcon);
                itemCount = new("20x", 0.75f);
                SetRectangle(itemCount, left: 0, top: silkIcon.Height.Pixels, width: itemCount.Width.Pixels, height: itemCount.Height.Pixels);
                itemCount.HAlign = 0.5f;
                silkIcon.Append(itemCount);

                at = new("@", 0.5f, true);
                SetRectangle(at, left: 60f, top: y + 5, width: at.Width.Pixels, height: at.Height.Pixels);
                UIPanel.Append(at);

                loomIcon = new(ItemID.Loom);
                SetRectangle(loomIcon, left: 100f - (silkIcon.Width.Pixels / 2f), top: y, width: loomIcon.Width.Pixels, height: loomIcon.Height.Pixels);
                UIPanel.Append(loomIcon);

                equals = new("=", 0.5f, true);
                SetRectangle(equals, left: 120f, top: y, width: equals.Width.Pixels, height: equals.Height.Pixels);
                UIPanel.Append(equals);

                UIItem robeIcon = new(ItemID.Robe);
                SetRectangle(robeIcon, left: 160f - (robeIcon.Width.Pixels / 2f), top: y, width: robeIcon.Width.Pixels, height: robeIcon.Height.Pixels);
                UIPanel.Append(robeIcon);
                #endregion
                y += offset;
                #region Dyed hood
                hoodIcon = new(ModContent.ItemType<Hood>());
                SetRectangle(hoodIcon, left: 40f - (hoodIcon.Width.Pixels / 2f), top: y, width: hoodIcon.Width.Pixels, height: hoodIcon.Height.Pixels);
                UIPanel.Append(hoodIcon);

                UIText plus = new("+", 0.5f, true);
                SetRectangle(plus, left: 65f, top: y + 5, width: plus.Width.Pixels, height: plus.Height.Pixels);
                UIPanel.Append(plus);

                UIItem dyeIcon = new(ItemID.GreenandBlackDye);
                SetRectangle(dyeIcon, left: 100f - (dyeIcon.Width.Pixels / 2f), top: y, width: dyeIcon.Width.Pixels, height: dyeIcon.Height.Pixels);
                UIPanel.Append(dyeIcon);

                at = new("@", 0.5f, true);
                SetRectangle(at, left: 120f, top: y + 5, width: at.Width.Pixels, height: at.Height.Pixels);
                UIPanel.Append(at);

                UIItem vatIcon = new(ItemID.DyeVat);
                SetRectangle(vatIcon, left: 160f - (vatIcon.Width.Pixels / 2f), top: y - 5, width: vatIcon.Width.Pixels, height: vatIcon.Height.Pixels);
                vatIcon.ImageScale = 0.75f;
                UIPanel.Append(vatIcon);

                equals = new("=", 0.5f, true);
                SetRectangle(equals, left: 180f, top: y, width: equals.Width.Pixels, height: equals.Height.Pixels);
                UIPanel.Append(equals);

                UIItem dyedHoodIcon = new(ModContent.ItemType<DyedHood>());
                SetRectangle(dyedHoodIcon, left: 220f - (dyedHoodIcon.Width.Pixels / 2f), top: y, width: dyedHoodIcon.Width.Pixels, height: dyedHoodIcon.Height.Pixels);
                UIPanel.Append(dyedHoodIcon);
                #endregion
                y += offset;                
                #region Dyed Robe
                robeIcon = new(ItemID.Robe);
                SetRectangle(robeIcon, left: 40f - (robeIcon.Width.Pixels / 2f), top: y, width: robeIcon.Width.Pixels, height: robeIcon.Height.Pixels);
                UIPanel.Append(robeIcon);

                plus = new("+", 0.5f, true);
                SetRectangle(plus, left: 65f, top: y + 5, width: plus.Width.Pixels, height: plus.Height.Pixels);
                UIPanel.Append(plus);

                dyeIcon = new(ItemID.GreenandBlackDye);
                SetRectangle(dyeIcon, left: 100f - (dyeIcon.Width.Pixels / 2f), top: y, width: dyeIcon.Width.Pixels, height: dyeIcon.Height.Pixels);
                UIPanel.Append(dyeIcon);

                at = new("@", 0.5f, true);
                SetRectangle(at, left: 120f, top: y + 5, width: at.Width.Pixels, height: at.Height.Pixels);
                UIPanel.Append(at);

                vatIcon = new(ItemID.DyeVat);
                SetRectangle(vatIcon, left: 160f - (vatIcon.Width.Pixels / 2f), top: y - 5, width: vatIcon.Width.Pixels, height: vatIcon.Height.Pixels);
                vatIcon.ImageScale = 0.75f;
                UIPanel.Append(vatIcon);

                equals = new("=", 0.5f, true);
                SetRectangle(equals, left: 180f, top: y, width: equals.Width.Pixels, height: equals.Height.Pixels);
                UIPanel.Append(equals);

                UIItem dyedRobeIcon = new(ModContent.ItemType<DyedRobes>());
                SetRectangle(dyedRobeIcon, left: 220f - (dyedRobeIcon.Width.Pixels / 2f), top: y, width: dyedRobeIcon.Width.Pixels, height: dyedRobeIcon.Height.Pixels);
                UIPanel.Append(dyedRobeIcon);
                #endregion
                

                break;
            case Pages.Clothing:
                #region Disciple Hood
                dyedHoodIcon = new(ModContent.ItemType<DyedHood>());
                SetRectangle(dyedHoodIcon, left: 40f - (dyedHoodIcon.Width.Pixels / 2f), top: y, width: dyedHoodIcon.Width.Pixels, height: dyedHoodIcon.Height.Pixels);
                UIPanel.Append(dyedHoodIcon);

                plus = new("+", 0.5f, true);
                SetRectangle(plus, left: 65f, top: y + 5, width: plus.Width.Pixels, height: plus.Height.Pixels);
                UIPanel.Append(plus);

                silkIcon = new(ItemID.Silk);
                SetRectangle(silkIcon, left: 100f - (silkIcon.Width.Pixels / 2f), top: y, width: silkIcon.Width.Pixels, height: silkIcon.Height.Pixels);
                UIPanel.Append(silkIcon);
                itemCount = new("2x", 0.75f);
                SetRectangle(itemCount, left: 0, top: silkIcon.Height.Pixels, width: itemCount.Width.Pixels, height: itemCount.Height.Pixels);
                itemCount.HAlign = 0.5f;
                silkIcon.Append(itemCount);

                at = new("@", 0.5f, true);
                SetRectangle(at, left: 120f, top: y + 5, width: at.Width.Pixels, height: at.Height.Pixels);
                UIPanel.Append(at);

                loomIcon = new(ItemID.Loom);
                SetRectangle(loomIcon, left: 160f - (loomIcon.Width.Pixels / 2f), top: y, width: loomIcon.Width.Pixels, height: loomIcon.Height.Pixels);
                loomIcon.ImageScale = 1f;
                UIPanel.Append(loomIcon);

                equals = new("=", 0.5f, true);
                SetRectangle(equals, left: 180f, top: y, width: equals.Width.Pixels, height: equals.Height.Pixels);
                UIPanel.Append(equals);

                UIItem discipleHoodIcon = new(ModContent.ItemType<LunarCultistHood>());
                SetRectangle(discipleHoodIcon, left: 220f - (discipleHoodIcon.Width.Pixels / 2f), top: y, width: discipleHoodIcon.Width.Pixels, height: discipleHoodIcon.Height.Pixels);
                UIPanel.Append(discipleHoodIcon);
                #endregion
                y += offset;
                #region Disciple Robe
                dyedRobeIcon = new(ModContent.ItemType<DyedRobes>());
                SetRectangle(dyedRobeIcon, left: 40f - (dyedRobeIcon.Width.Pixels / 2f), top: y, width: dyedRobeIcon.Width.Pixels, height: dyedRobeIcon.Height.Pixels);
                UIPanel.Append(dyedRobeIcon);

                plus = new("+", 0.5f, true);
                SetRectangle(plus, left: 65f, top: y + 5, width: plus.Width.Pixels, height: plus.Height.Pixels);
                UIPanel.Append(plus);

                silkIcon = new(ItemID.Silk);
                SetRectangle(silkIcon, left: 100f - (silkIcon.Width.Pixels / 2f), top: y, width: silkIcon.Width.Pixels, height: silkIcon.Height.Pixels);
                UIPanel.Append(silkIcon);
                itemCount = new("4x", 0.75f);
                SetRectangle(itemCount, left: 0, top: silkIcon.Height.Pixels, width: itemCount.Width.Pixels, height: itemCount.Height.Pixels);
                itemCount.HAlign = 0.5f;
                silkIcon.Append(itemCount);                

                at = new("@", 0.5f, true);
                SetRectangle(at, left: 120f, top: y + 5, width: at.Width.Pixels, height: at.Height.Pixels);
                UIPanel.Append(at);

                loomIcon = new(ItemID.Loom);
                SetRectangle(loomIcon, left: 160f - (loomIcon.Width.Pixels / 2f), top: y, width: loomIcon.Width.Pixels, height: loomIcon.Height.Pixels);
                loomIcon.ImageScale = 1f;
                UIPanel.Append(loomIcon);

                equals = new("=", 0.5f, true);
                SetRectangle(equals, left: 180f, top: y, width: equals.Width.Pixels, height: equals.Height.Pixels);
                UIPanel.Append(equals);

                UIItem discipleRobeIcon = new(ModContent.ItemType<LunarCultistRobes>());
                SetRectangle(discipleRobeIcon, left: 220f - (discipleRobeIcon.Width.Pixels / 2f), top: y, width: discipleRobeIcon.Width.Pixels, height: discipleRobeIcon.Height.Pixels);
                UIPanel.Append(discipleRobeIcon);
                #endregion
                y += offset;
                #region Bishop Hood
                dyedHoodIcon = new(ModContent.ItemType<DyedHood>());
                SetRectangle(dyedHoodIcon, left: 40f - (dyedHoodIcon.Width.Pixels / 2f), top: y, width: dyedHoodIcon.Width.Pixels, height: dyedHoodIcon.Height.Pixels);
                UIPanel.Append(dyedHoodIcon);

                plus = new("+", 0.5f, true);
                SetRectangle(plus, left: 65f, top: y + 5, width: plus.Width.Pixels, height: plus.Height.Pixels);
                UIPanel.Append(plus);

                UIItem copperIcon = new(ItemID.CopperBar);
                SetRectangle(copperIcon, left: 100f - (copperIcon.Width.Pixels / 2f), top: y, width: copperIcon.Width.Pixels, height: copperIcon.Height.Pixels);
                UIPanel.Append(copperIcon);
                itemCount = new("6x", 0.75f);
                SetRectangle(itemCount, left: 0, top: copperIcon.Height.Pixels, width: itemCount.Width.Pixels, height: itemCount.Height.Pixels);
                itemCount.HAlign = 0.5f;
                copperIcon.Append(itemCount);

                plus = new("+", 0.5f, true);
                SetRectangle(plus, left: 120f, top: y + 5, width: plus.Width.Pixels, height: plus.Height.Pixels);
                UIPanel.Append(plus);

                silkIcon = new(ItemID.FlinxFur);
                SetRectangle(silkIcon, left: 150f - (silkIcon.Width.Pixels / 2f), top: y, width: silkIcon.Width.Pixels, height: silkIcon.Height.Pixels);
                UIPanel.Append(silkIcon);
                itemCount = new("2x", 0.75f);
                SetRectangle(itemCount, left: 0, top: silkIcon.Height.Pixels, width: itemCount.Width.Pixels, height: itemCount.Height.Pixels);
                itemCount.HAlign = 0.5f;
                silkIcon.Append(itemCount);

                at = new("@", 0.5f, true);
                SetRectangle(at, left: 170f, top: y + 5, width: at.Width.Pixels, height: at.Height.Pixels);
                UIPanel.Append(at);

                UIItem benchIcon = new(ItemID.WorkBench);
                SetRectangle(benchIcon, left: 210f - (benchIcon.Width.Pixels / 2f), top: y + 4, width: benchIcon.Width.Pixels, height: benchIcon.Height.Pixels);
                UIPanel.Append(benchIcon);

                equals = new("=", 0.5f, true);
                SetRectangle(equals, left: 230f, top: y, width: equals.Width.Pixels, height: equals.Height.Pixels);
                UIPanel.Append(equals);

                UIItem bishopHoodIcon = new(ModContent.ItemType<LunarBishopHood>());
                SetRectangle(bishopHoodIcon, left: 270f - (bishopHoodIcon.Width.Pixels / 2f), top: y - 8, width: bishopHoodIcon.Width.Pixels, height: bishopHoodIcon.Height.Pixels);
                UIPanel.Append(bishopHoodIcon);
                #endregion
                y += offset;
                #region Bishop Robe
                dyedRobeIcon = new(ModContent.ItemType<DyedRobes>());
                SetRectangle(dyedRobeIcon, left: 40f - (dyedHoodIcon.Width.Pixels / 2f), top: y, width: dyedRobeIcon.Width.Pixels, height: dyedRobeIcon.Height.Pixels);
                UIPanel.Append(dyedRobeIcon);

                plus = new("+", 0.5f, true);
                SetRectangle(plus, left: 65f, top: y + 5, width: plus.Width.Pixels, height: plus.Height.Pixels);
                UIPanel.Append(plus);

                copperIcon = new(ItemID.CopperBar);
                SetRectangle(copperIcon, left: 100f - (copperIcon.Width.Pixels / 2f), top: y, width: copperIcon.Width.Pixels, height: copperIcon.Height.Pixels);
                UIPanel.Append(copperIcon);
                itemCount = new("4x", 0.75f);
                SetRectangle(itemCount, left: 0, top: copperIcon.Height.Pixels, width: itemCount.Width.Pixels, height: itemCount.Height.Pixels);
                itemCount.HAlign = 0.5f;
                copperIcon.Append(itemCount);

                plus = new("+", 0.5f, true);
                SetRectangle(plus, left: 120f, top: y + 5, width: plus.Width.Pixels, height: plus.Height.Pixels);
                UIPanel.Append(plus);

                UIItem furIcon = new(ItemID.FlinxFur);
                SetRectangle(furIcon, left: 150f - (furIcon.Width.Pixels / 2f), top: y, width: furIcon.Width.Pixels, height: furIcon.Height.Pixels);
                UIPanel.Append(furIcon);
                itemCount = new("4x", 0.75f);
                SetRectangle(itemCount, left: 0, top: furIcon.Height.Pixels, width: itemCount.Width.Pixels, height: itemCount.Height.Pixels);
                itemCount.HAlign = 0.5f;
                furIcon.Append(itemCount);

                at = new("@", 0.5f, true);
                SetRectangle(at, left: 170f, top: y + 5, width: at.Width.Pixels, height: at.Height.Pixels);
                UIPanel.Append(at);

                benchIcon = new(ItemID.WorkBench);
                SetRectangle(benchIcon, left: 210f - (benchIcon.Width.Pixels / 2f), top: y + 4, width: benchIcon.Width.Pixels, height: benchIcon.Height.Pixels);
                UIPanel.Append(benchIcon);

                equals = new("=", 0.5f, true);
                SetRectangle(equals, left: 230f, top: y, width: equals.Width.Pixels, height: equals.Height.Pixels);
                UIPanel.Append(equals);

                UIItem bishopRobesIcon = new(ModContent.ItemType<LunarBishopRobes>());
                SetRectangle(bishopRobesIcon, left: 270f - (bishopRobesIcon.Width.Pixels / 2f), top: y, width: bishopRobesIcon.Width.Pixels, height: bishopRobesIcon.Height.Pixels);
                UIPanel.Append(bishopRobesIcon);
                #endregion
                break;
            case Pages.Masks:
                #region Mask
                UIItem boneIcon = new(ItemID.Bone);
                SetRectangle(boneIcon, left: 40f - (boneIcon.Width.Pixels / 2f), top: y, width: boneIcon.Width.Pixels, height: boneIcon.Height.Pixels);
                UIPanel.Append(boneIcon);
                itemCount = new("8x", 0.75f);
                SetRectangle(itemCount, left: 0, top: boneIcon.Height.Pixels, width: itemCount.Width.Pixels, height: itemCount.Height.Pixels);
                itemCount.HAlign = 0.5f;
                boneIcon.Append(itemCount);

                at = new("@", 0.5f, true);
                SetRectangle(at, left: 60f, top: y + 5, width: at.Width.Pixels, height: at.Height.Pixels);
                UIPanel.Append(at);

                benchIcon = new(ItemID.WorkBench);
                SetRectangle(benchIcon, left: 100f - (benchIcon.Width.Pixels / 2f), top: y + 4, width: benchIcon.Width.Pixels, height: benchIcon.Height.Pixels);
                UIPanel.Append(benchIcon);

                equals = new("=", 0.5f, true);
                SetRectangle(equals, left: 120f, top: y, width: equals.Width.Pixels, height: equals.Height.Pixels);
                UIPanel.Append(equals);

                UIItem maskIcon = new(ModContent.ItemType<Mask>());
                maskIcon.ImageScale = 1.25f;
                SetRectangle(maskIcon, left: 160f - (maskIcon.Width.Pixels / 2f), top: y, width: maskIcon.Width.Pixels, height: maskIcon.Height.Pixels);
                UIPanel.Append(maskIcon);
                #endregion
                y += offset;
                #region Devotee Mask
                maskIcon = new(ModContent.ItemType<Mask>());
                maskIcon.ImageScale = 1.25f;
                SetRectangle(maskIcon, left: 40f - (maskIcon.Width.Pixels / 2f), top: y + 4, width: maskIcon.Width.Pixels, height: maskIcon.Height.Pixels);
                UIPanel.Append(maskIcon);

                plus = new("+", 0.5f, true);
                SetRectangle(plus, left: 65f, top: y + 5, width: plus.Width.Pixels, height: plus.Height.Pixels);
                UIPanel.Append(plus);

                dyeIcon = new(ItemID.SilverDye);
                SetRectangle(dyeIcon, left: 100f - (dyeIcon.Width.Pixels / 2f), top: y, width: dyeIcon.Width.Pixels, height: dyeIcon.Height.Pixels);
                UIPanel.Append(dyeIcon);

                plus = new("+", 0.5f, true);
                SetRectangle(plus, left: 120f, top: y + 5, width: plus.Width.Pixels, height: plus.Height.Pixels);
                UIPanel.Append(plus);

                dyeIcon = new(ItemID.BrownDye);
                SetRectangle(dyeIcon, left: 150f - (dyeIcon.Width.Pixels / 2f), top: y, width: dyeIcon.Width.Pixels, height: dyeIcon.Height.Pixels);
                UIPanel.Append(dyeIcon);

                at = new("@", 0.5f, true);
                SetRectangle(at, left: 170f, top: y + 5, width: at.Width.Pixels, height: at.Height.Pixels);
                UIPanel.Append(at);

                vatIcon = new(ItemID.DyeVat);
                SetRectangle(vatIcon, left: 210f - (vatIcon.Width.Pixels / 2f), top: y - 5, width: vatIcon.Width.Pixels, height: vatIcon.Height.Pixels);
                vatIcon.ImageScale = 0.75f;
                UIPanel.Append(vatIcon);

                equals = new("=", 0.5f, true);
                SetRectangle(equals, left: 230f, top: y, width: equals.Width.Pixels, height: equals.Height.Pixels);
                UIPanel.Append(equals);

                maskIcon = new(ModContent.ItemType<LunarDevoteeMask>());
                maskIcon.ImageScale = 1.25f;
                SetRectangle(maskIcon, left: 270f - (maskIcon.Width.Pixels / 2f), top: y, width: maskIcon.Width.Pixels, height: maskIcon.Height.Pixels);
                UIPanel.Append(maskIcon);
                #endregion
                y += offset;
                #region Archer Mask
                maskIcon = new(ModContent.ItemType<Mask>());
                maskIcon.ImageScale = 1.25f;
                SetRectangle(maskIcon, left: 40f - (maskIcon.Width.Pixels / 2f), top: y + 4, width: maskIcon.Width.Pixels, height: maskIcon.Height.Pixels);
                UIPanel.Append(maskIcon);

                plus = new("+", 0.5f, true);
                SetRectangle(plus, left: 65f, top: y + 5, width: plus.Width.Pixels, height: plus.Height.Pixels);
                UIPanel.Append(plus);

                dyeIcon = new(ItemID.SilverDye);
                SetRectangle(dyeIcon, left: 100f - (dyeIcon.Width.Pixels / 2f), top: y, width: dyeIcon.Width.Pixels, height: dyeIcon.Height.Pixels);
                UIPanel.Append(dyeIcon);

                plus = new("+", 0.5f, true);
                SetRectangle(plus, left: 120f, top: y + 5, width: plus.Width.Pixels, height: plus.Height.Pixels);
                UIPanel.Append(plus);

                dyeIcon = new(ItemID.BrownDye);
                SetRectangle(dyeIcon, left: 150f - (dyeIcon.Width.Pixels / 2f), top: y, width: dyeIcon.Width.Pixels, height: dyeIcon.Height.Pixels);
                UIPanel.Append(dyeIcon);

                at = new("@", 0.5f, true);
                SetRectangle(at, left: 170f, top: y + 5, width: at.Width.Pixels, height: at.Height.Pixels);
                UIPanel.Append(at);

                vatIcon = new(ItemID.DyeVat);
                SetRectangle(vatIcon, left: 210f - (vatIcon.Width.Pixels / 2f), top: y - 5, width: vatIcon.Width.Pixels, height: vatIcon.Height.Pixels);
                vatIcon.ImageScale = 0.75f;
                UIPanel.Append(vatIcon);

                equals = new("=", 0.5f, true);
                SetRectangle(equals, left: 230f, top: y, width: equals.Width.Pixels, height: equals.Height.Pixels);
                UIPanel.Append(equals);

                maskIcon = new(ModContent.ItemType<LunarArcherMask>());
                maskIcon.ImageScale = 1.25f;
                SetRectangle(maskIcon, left: 270f - (maskIcon.Width.Pixels / 2f), top: y, width: maskIcon.Width.Pixels, height: maskIcon.Height.Pixels);
                UIPanel.Append(maskIcon);
                #endregion
                y += offset;
                #region Bishop Mask
                maskIcon = new(ModContent.ItemType<Mask>());
                maskIcon.ImageScale = 1.25f;
                SetRectangle(maskIcon, left: 40f - (maskIcon.Width.Pixels / 2f), top: y + 4, width: maskIcon.Width.Pixels, height: maskIcon.Height.Pixels);
                UIPanel.Append(maskIcon);

                plus = new("+", 0.5f, true);
                SetRectangle(plus, left: 65f, top: y + 5, width: plus.Width.Pixels, height: plus.Height.Pixels);
                UIPanel.Append(plus);

                dyeIcon = new(ItemID.SilverDye);
                SetRectangle(dyeIcon, left: 100f - (dyeIcon.Width.Pixels / 2f), top: y, width: dyeIcon.Width.Pixels, height: dyeIcon.Height.Pixels);
                UIPanel.Append(dyeIcon);

                plus = new("+", 0.5f, true);
                SetRectangle(plus, left: 120f, top: y + 5, width: plus.Width.Pixels, height: plus.Height.Pixels);
                UIPanel.Append(plus);

                dyeIcon = new(ItemID.BrownDye);
                SetRectangle(dyeIcon, left: 150f - (dyeIcon.Width.Pixels / 2f), top: y, width: dyeIcon.Width.Pixels, height: dyeIcon.Height.Pixels);
                UIPanel.Append(dyeIcon);

                at = new("@", 0.5f, true);
                SetRectangle(at, left: 170f, top: y + 5, width: at.Width.Pixels, height: at.Height.Pixels);
                UIPanel.Append(at);

                vatIcon = new(ItemID.DyeVat);
                SetRectangle(vatIcon, left: 210f - (vatIcon.Width.Pixels / 2f), top: y - 5, width: vatIcon.Width.Pixels, height: vatIcon.Height.Pixels);
                vatIcon.ImageScale = 0.75f;
                UIPanel.Append(vatIcon);

                equals = new("=", 0.5f, true);
                SetRectangle(equals, left: 230f, top: y, width: equals.Width.Pixels, height: equals.Height.Pixels);
                UIPanel.Append(equals);

                maskIcon = new(ModContent.ItemType<LunarBishopMask>());
                maskIcon.ImageScale = 1.25f;
                SetRectangle(maskIcon, left: 270f - (maskIcon.Width.Pixels / 2f), top: y, width: maskIcon.Width.Pixels, height: maskIcon.Height.Pixels);
                UIPanel.Append(maskIcon);
                #endregion
                y += offset;
                break;
        }

        UIText PageTitle = new(page.ToString(), large: true);
        SetRectangle(PageTitle, left: 0f, top: 30f, width: PageTitle.Width.Pixels, height: PageTitle.Height.Pixels);
        PageTitle.HAlign = 0.5f;
        UIPanel.Append(PageTitle);

    }
    private static void SetRectangle(UIElement uiElement, float left, float top, float width, float height)
    {
        uiElement.Left.Set(left, 0f);
        uiElement.Top.Set(top, 0f);
        uiElement.Width.Set(width, 0f);
        uiElement.Height.Set(height, 0f);
    }
    private void NextPage(UIMouseEvent evt, UIElement listeningElement)
    {

        if (page == Pages.Masks)
            page = 0;
        else
            page++;
        ModContent.GetInstance<TailorInstructionsUISystem>().ShowTailorInstructionsUI();
    }
    private void PreviousPage(UIMouseEvent evt, UIElement listeningElement)
    {
        if (page == Pages.Components)
            page = Pages.Masks;
        else
            page--;
        ModContent.GetInstance<TailorInstructionsUISystem>().ShowTailorInstructionsUI();
    }
}