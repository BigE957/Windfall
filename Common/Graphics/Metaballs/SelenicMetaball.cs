using Terraria;
using Windfall.Common.Systems;
using static Windfall.Common.Graphics.Metaballs.MetaballSystem;

namespace Windfall.Common.Graphics.Metaballs;

public class SelenicMetaball : MyMetaball
{
    public override bool ShouldRender => true;

    public override IEnumerable<Texture2D> LayerTextures
    {
        get
        {
            for (int i = 0; i < layerAssets.Length; i++)
                yield return layerAssets[i].Value;
        }
    }

    public override Vector2 CalculateManualOffsetForLayer(int layerIndex)
    {
        switch (layerIndex)
        {
            case 0:
                return Vector2.UnitX * Main.GlobalTimeWrappedHourly * 30f;

            case 1:
                return Vector2.UnitX * Main.GlobalTimeWrappedHourly * 60f + Vector2.UnitY * (float)Math.Sin(Main.GlobalTimeWrappedHourly) * 30f;
            case 2:
                Vector2 offset = Vector2.One * (float)Math.Cos(Main.GlobalTimeWrappedHourly * 0.04f) * 1000f;
                offset = -offset.RotatedBy(Main.GlobalTimeWrappedHourly * 0.08f) * 0.76f;
                return offset;
            case 3:
                offset = Vector2.One * (float)Math.Cos(Main.GlobalTimeWrappedHourly * 0.045f) * 1000f;
                offset = -offset.RotatedBy(Main.GlobalTimeWrappedHourly * 0.08f) * 0.84f;
                return offset;
            case 4:
                offset = Vector2.One * (float)Math.Cos(Main.GlobalTimeWrappedHourly * 0.05f) * 1000f;
                offset = -offset.RotatedBy(Main.GlobalTimeWrappedHourly * 0.08f) * 0.97f;
                return offset;
        }
        return Vector2.Zero;
    }


    public static float BorderLerpValue(float offset) => (MathF.Sin(Main.GlobalTimeWrappedHourly + offset) / 0.5f) + 0.5f;

    public static Color BorderColor(float offset) => Color.Lerp(new Color(117, 255, 159), new Color(255, 180, 80), BorderLerpValue(offset));

    public override Color EdgeColor(int layer) => BorderColor(layer == 0 ? 30 : 0).MultiplyRGB(layer == 1 ? Color.White : Color.Gray);

    public override Color DrawLayerColor(int layer) => layer == 1 ? Color.White : Color.Gray;

    public override MetaballSystem.MetaballDrawLayer[] DrawLayers => [MetaballDrawLayer.BeforeAllTiles, MetaballDrawLayer.AfterEverything];

    private static Asset<Texture2D>[] layerAssets = null;

    public override void Load()
    {
        if (Main.dedServ)
            return;

        layerAssets = new Asset<Texture2D>[4];

        for (int i = 0; i < 4; i++)
            layerAssets[i] = (ModContent.Request<Texture2D>($"Windfall/Assets/Graphics/Metaballs/Empyrean_Metaball/EmpyreanLayer{i + 1}"));

        base.Load();
    }

    internal override void Update()
    {
        foreach (var p in SelenicMetaballParticle.SelenicParticles)
            p.Update();

        SelenicMetaballParticle.SelenicParticles.RemoveAll(p => p.Size <= 0.05f);
    }

    internal override void PreDrawCallContent()
    {
        foreach (var p in SelenicMetaballParticle.SelenicParticles)
            p.Draw();
    }

    internal override void ClearInstances()
    {
        SelenicMetaballParticle.SelenicParticles.Clear();
    }

    public static float SumofSines(float sineOff, float interp, float wavelength, float speed)
    {
        float time = Main.GlobalTimeWrappedHourly;
        float x = sineOff;
        float a = interp / 2f;
        float w = 2 / wavelength;
        float s = speed * w;
        return (a * 2f) * (float)Math.Sin(x * w + time * (s * 0.25f)) + a * (float)Math.Sin(x * (w * 0.75f) + time * (s * 2f)) + (a * 0.5f) * (float)Math.Sin(x * (w * 1.5f) + time * s);
    }
}

public class SelenicMetaballParticle
{
    internal static List<SelenicMetaballParticle> SelenicParticles { get; private set; } = [];

    public float Size;

    public Vector2 Velocity;

    public Vector2 Center;

    public byte Layer;

    public float SpeedDecay;

    public float SizeDecay;

    private SelenicMetaballParticle(Vector2 center, Vector2 velocity, float size, byte layer, float speedDecay, float sizeDecay)
    {
        this.Center = center;
        this.Velocity = velocity;
        this.Size = size / 60f;
        this.Layer = layer;
        SpeedDecay = speedDecay;
        SizeDecay = sizeDecay;
    }

    public static void SpawnParticle(Vector2 center, Vector2 velocity, float size, byte layer = 1, float speedDecay = 0.99f, float sizeDecay = 0.9f)
    {
        SelenicParticles.Add(new(center, velocity, size, layer, speedDecay, sizeDecay));
    }

    public void Update()
    {
        Velocity *= 0.99f;
        Size *= 0.9f;
        Center += Velocity;
    }

    public void Draw()
    {
        ModContent.GetInstance<SelenicMetaball>().AddMetaballFill(new(LoadSystem.Circle.Value, Center - Main.screenPosition, null, 0f, LoadSystem.Circle.Size() * 0.5f, Size, 0), Layer);
    }
}

