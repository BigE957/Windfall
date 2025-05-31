using Windfall.Content.Items.Weapons.Summon;
using Windfall.Content.NPCs.Bosses.Orator;

namespace Windfall.Common.Graphics.Metaballs;
public class PostMetaballDrawing : ModSystem
{
    public override void OnModLoad()
    {
        // Prepare event subscribers.
        On_Main.DrawProjectiles += AfterDrawMetaballsAfterProjectiles;
        On_Main.DrawNPCs += AfterDrawMetaballsAfterProjectiles;
        On_Main.DrawPlayers_AfterProjectiles += AfterDrawMetaballsAfterProjectiles;
    }

    private void AfterDrawMetaballsAfterProjectiles(On_Main.orig_DrawPlayers_AfterProjectiles orig, Main self)
    {
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer, null, Matrix.Identity);

        foreach (NPC npc in Main.ActiveNPCs)
            if (npc.ModNPC != null && npc.ModNPC is ShadowHand hand)
                hand.PostDraw(Main.spriteBatch, Main.screenPosition, EmpyreanMetaball.BorderColor);

        Main.spriteBatch.End();

        orig(self);
    }

    private void AfterDrawMetaballsAfterProjectiles(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
    {
        orig(self, behindTiles);
    }

    private void AfterDrawMetaballsAfterProjectiles(On_Main.orig_DrawProjectiles orig, Main self)
    {
        orig(self);
    }
}
