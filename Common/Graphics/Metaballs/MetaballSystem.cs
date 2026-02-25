using Daybreak.Common.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.Liquid;
using Terraria.Graphics.Shaders;
using Windfall.Common.Systems;
using static Windfall.Common.Graphics.Metaballs.MetaballSystem;

namespace Windfall.Common.Graphics.Metaballs;

public class MetaballSystem : ModSystem
{
    internal static List<MyMetaball> metaballs = [];

    public enum MetaballDrawLayer
    {
        BeforeAllTiles,
        BeforeSolidTiles,
        BeforeNPCs,
        AfterNPCs,
        BeforeProjectiles,
        AfterProjectiles,
        AfterPlayers,
        AfterDusts,
        AfterEverything,
    }

    public override void Load()
    {
        On_Main.CheckMonoliths += PrepareMetaballs;
        On_Main.DrawBackgroundBlackFill += DrawMetaballs_BeforeAllTiles;
        On_Main.DoDraw_Tiles_Solid += DrawMetaballs_BeforeSolidTiles;
        On_Main.DoDraw_DrawNPCsOverTiles += DrawMetaballs_NPCs;
        On_Main.DrawProjectiles += DrawMetaballs_Projectiles;
        On_Main.DrawPlayers_AfterProjectiles += DrawMetaballs_AfterPlayers;
        On_Main.DrawDust += DrawMetaballs_AfterDusts;
        On_Main.DrawInfernoRings += DrawMetaballs_AfterEverything;
    }

    private void PrepareMetaballs(On_Main.orig_CheckMonoliths orig)
    {
        orig();

        foreach (var metaball in metaballs)
            metaball.PreDrawCallContent();
    }

    private void DrawMetaballs_BeforeAllTiles(On_Main.orig_DrawBackgroundBlackFill orig, Main self)
    {
        Main.spriteBatch.End(out var ss);
        DrawMetaballs(MetaballDrawLayer.BeforeAllTiles);
        Main.spriteBatch.Begin(ss);
        orig(self);
    }

    private void DrawMetaballs_BeforeSolidTiles(On_Main.orig_DoDraw_Tiles_Solid orig, Main self)
    {
        DrawMetaballs(MetaballDrawLayer.BeforeSolidTiles);
        orig(self);
    }

    private void DrawMetaballs_NPCs(On_Main.orig_DoDraw_DrawNPCsOverTiles orig, Main self)
    {
        DrawMetaballs(MetaballDrawLayer.BeforeNPCs);
        orig(self);
        DrawMetaballs(MetaballDrawLayer.AfterNPCs);
    }

    private void DrawMetaballs_Projectiles(On_Main.orig_DrawProjectiles orig, Main self)
    {
        DrawMetaballs(MetaballDrawLayer.BeforeProjectiles);
        orig(self);
        DrawMetaballs(MetaballDrawLayer.AfterProjectiles);
    }

    private void DrawMetaballs_AfterPlayers(On_Main.orig_DrawPlayers_AfterProjectiles orig, Main self)
    {
        orig(self);
        DrawMetaballs(MetaballDrawLayer.AfterPlayers);
    }

    private void DrawMetaballs_AfterDusts(On_Main.orig_DrawDust orig, Main self)
    {
        orig(self);
        DrawMetaballs(MetaballDrawLayer.AfterDusts);
    }

    private void DrawMetaballs_AfterEverything(On_Main.orig_DrawInfernoRings orig, Main self)
    {
        orig(self);
        Main.spriteBatch.End(out var ss);
        DrawMetaballs(MetaballDrawLayer.AfterEverything);
        Main.spriteBatch.Begin(ss);
    }

    public override void PreUpdateEntities()
    {
        // Get a list of all metaballs currently in use.
        var activeMetaballs = metaballs.Where(m => m.ShouldRender);

        // Don't bother wasting resources if metaballs are not in use at the moment.
        if (!activeMetaballs.Any())
            return;

        foreach (var metaball in activeMetaballs)
            metaball.Update();
    }

    public override void OnWorldUnload()
    {
        foreach (MyMetaball metaball in metaballs)
            metaball.ClearInstances();
    }

    private static void DrawMetaballs(MetaballDrawLayer targetLayer)
    {
        if (Main.gameMenu && (!GameShaders.Misc.ContainsKey("Windfall:Masking") || !GameShaders.Misc.ContainsKey("Windfall:MaskEdge")))
            return;

        var activeMetaballs = metaballs.Where(m => m.ShouldRender && m.DrawLayers.Contains(targetLayer));

        if (!activeMetaballs.Any())
            return;

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer, null, Main.Transform);

        foreach (var metaball in metaballs)
        {
            if (!metaball.ShouldRender)
                continue;

            for (int i = 0; i < metaball.DrawLayers.Length; i++)
            {
                if (metaball.DrawLayers[i] != targetLayer) 
                    continue;

                //Construct Mask
                using (metaball.metaballDrawLayerLeases[i].Scope(clearColor: Color.Black))
                {
                    foreach (MetaballDrawCommand command in metaball.FillDrawCalls[i])
                        command.Draw(Main.spriteBatch, Color.White);

                    //Must restart to flush contents to RT
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer, null, Main.Transform);
                }

                //Apply masking shader
                MiscShaderData maskingShader = GameShaders.Misc["Windfall:Masking"];
                maskingShader.Shader.Parameters["screenSize"]?.SetValue(new Vector2(Main.screenWidth, Main.screenHeight));

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.Default, Main.Rasterizer, maskingShader.Shader, Main.Transform);

                Main.instance.GraphicsDevice.Textures[1] = metaball.metaballDrawLayerLeases[i].Target;
                Main.instance.GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;

                //draw texture layers
                int layerNum = 0;
                foreach (var layer in metaball.LayerTextures)
                {
                    Vector2 offset = metaball.CalculateManualOffsetForLayer(layerNum);
                    if(!metaball.FixedToScreen)
                        offset += Main.screenPosition;

                    Main.spriteBatch.Draw(layer, new Rectangle(-2, -2, Main.screenWidth + 2, Main.screenHeight + 2), new Rectangle((int)offset.X % layer.Width, (int)offset.Y % layer.Height, Main.screenWidth, Main.screenHeight), metaball.DrawLayerColor(i));
                    
                    layerNum++;
                }

                //Reset for edge drawing
                MiscShaderData edgeShader = GameShaders.Misc["Windfall:MaskEdge"];
                edgeShader.Shader.Parameters["screenSize"]?.SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
                edgeShader.Shader.Parameters["edgeWidth"]?.SetValue(1);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.Default, Main.Rasterizer, edgeShader.Shader, Matrix.Identity);

                Main.spriteBatch.Draw(metaball.metaballDrawLayerLeases[i].Target, new Rectangle(-1, -1, Main.screenWidth + 2, Main.screenHeight + 2), metaball.EdgeColor(i));

                //reset the sprite batch for the next iteration.
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer, null, Main.Transform);

                foreach (MetaballDrawCommand command in metaball.EdgeDrawCalls[i])
                    command.Draw(Main.spriteBatch, metaball.EdgeColor(i));

                metaball.FillDrawCalls[i].Clear();
                metaball.EdgeDrawCalls[i].Clear();
                metaball.PostDrawCallContent(Main.spriteBatch, i);
            }
        }

        Main.spriteBatch.End();
    }

    public static void AddMetaballFill<T>(MetaballDrawCommand command, byte layer) where T : MyMetaball
    {
        ModContent.GetInstance<T>().AddMetaballFill(command, layer);
    }

    public static void AddMetaballEdge<T>(MetaballDrawCommand command, byte layer) where T : MyMetaball
    {
        ModContent.GetInstance<T>().AddMetaballEdge(command, layer);
    }
}

public struct MetaballDrawCommand
{
    public Texture2D Texture;
    public Vector2 Position;
    public Rectangle? Frame;
    public float Rotation;
    public Vector2 Origin;
    public Vector2 Scale;
    public SpriteEffects Effect;

    public MetaballDrawCommand(Texture2D tex, Vector2 pos, Rectangle? frame, float rot, Vector2 orig, Vector2 scale, SpriteEffects eff)
    {
        Texture = tex;
        Position = pos;
        Frame = frame;
        Rotation = rot;
        Origin = orig;
        Scale = scale;
        Effect = eff;
    }

    public MetaballDrawCommand(Texture2D tex, Vector2 pos, Rectangle? frame, float rot, Vector2 orig, float scale, SpriteEffects eff)
    {
        Texture = tex;
        Position = pos;
        Frame = frame;
        Rotation = rot;
        Origin = orig;
        Scale = scale * Vector2.One;
        Effect = eff;
    }

    public MetaballDrawCommand(Projectile projectile)
    {
        Texture = TextureAssets.Projectile[projectile.type].Value;
        Position = projectile.Center - Main.screenPosition;
        Frame = null;
        Rotation = projectile.rotation;
        Origin = Texture.Size() * 0.5f;
        Scale = projectile.scale * Vector2.One;
        Effect = 0;
    }

    public readonly void Draw(SpriteBatch sb, Color color)
    {
        sb.Draw(Texture, Position, Frame, color, Rotation, Origin, Scale, Effect, 0);
    }
}

public abstract class MyMetaball : ModType
{
    protected override void Register()
    {
        ModTypeLookup<MyMetaball>.Register(this);

        MetaballSystem.metaballs.Add(this);

        // Disallow render target creation on servers.
        if (Main.dedServ)
            return;

        // Generate render targets.
        Main.QueueMainThreadAction(() =>
        {
            for(int i = 0; i < DrawLayers.Length; i++)
                metaballDrawLayerLeases.Add(ScreenspaceTargetPool.Shared.Rent(Main.instance.GraphicsDevice, (width, height) => (width + 4, height + 4)));
        });

        FillDrawCalls = new List<MetaballDrawCommand>[DrawLayers.Length];

        for (int i = 0; i < DrawLayers.Length; i++)
        {
            FillDrawCalls[i] = [];
            FillDrawCalls[i].EnsureCapacity(300);
        }

        EdgeDrawCalls = new List<MetaballDrawCommand>[DrawLayers.Length];
        for (int i = 0; i < DrawLayers.Length; i++)
            EdgeDrawCalls[i] = [];
    }

    internal List<RenderTargetLease> metaballDrawLayerLeases = [];

    internal List<MetaballDrawCommand>[] FillDrawCalls = [];
    internal List<MetaballDrawCommand>[] EdgeDrawCalls = [];

    public abstract bool ShouldRender
    {
        get;
    }

    // This uses a Texture2D rather than an Asset to allow for the usage of render targets when working with metaballs.
    public abstract IEnumerable<Texture2D> LayerTextures
    {
        get;
    }

    public virtual Vector2 CalculateManualOffsetForLayer(int layerIndex) => Vector2.Zero;

    public abstract Color EdgeColor(int drawLayer);

    /// <summary>
    /// Whether the layer overlay contents from <see cref="Layers"/> should be fixed to the screen.<br></br>
    /// When true, the texture will be statically drawn to the screen with no respect for world position.
    /// </summary>
    public virtual bool FixedToScreen => false;
    
    public abstract MetaballDrawLayer[] DrawLayers { get; }

    public virtual Color DrawLayerColor(int layer) => Color.White;

    internal void AddMetaballFill(MetaballDrawCommand command, int layer) => FillDrawCalls[layer].Add(command);
    internal void AddMetaballEdge(MetaballDrawCommand command, int layer) => EdgeDrawCalls[layer].Add(command);

    internal virtual void Update() { }

    internal virtual void PreDrawCallContent() { }

    internal virtual void PostDrawCallContent(SpriteBatch spritebatch, int layer) { }

    internal virtual void ClearInstances() { }
}
