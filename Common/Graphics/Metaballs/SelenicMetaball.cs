using CalamityMod.Graphics.Metaballs;

namespace Windfall.Common.Graphics.Metaballs
{
    public class SelenicMetaball : Metaball
    {
        public class SelenicParticle
        {
            public float Size;

            public Vector2 Velocity;

            public Vector2 Center;

            public SelenicParticle(Vector2 center, Vector2 velocity, float size)
            {
                Center = center;
                Velocity = velocity;
                Size = size;
            }

            public void Update()
            {
                Size *= 0.94f;
                Center += Velocity;
                Velocity *= 0.96f;
            }
        }

        private static List<Asset<Texture2D>> layerAssets;

        public static List<SelenicParticle> SelenicParticles
        {
            get;
            private set;
        } = new();

        // Check if there are any extraneous particles or if the Gruesome Eminence projectile is present when deciding if this particle should be drawn.
        public override bool AnythingToDraw => SelenicParticles.Any();

        public override IEnumerable<Texture2D> Layers
        {
            get
            {
                for (int i = 0; i < layerAssets.Count; i++)
                    yield return layerAssets[i].Value;
            }
        }

        public override MetaballDrawLayer DrawContext => MetaballDrawLayer.AfterProjectiles;

        public override Color EdgeColor => new(132, 225, 211);

        public override void Load()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            // Load layer assets.
            layerAssets = new()
            {
                ModContent.Request<Texture2D>($"Windfall/Assets/Graphics/Metaballs/Selenic_Metaball/Selenic_Mist_Layer1", AssetRequestMode.ImmediateLoad)
            };
        }

        public override void ClearInstances() => SelenicParticles.Clear();

        public static void SpawnParticle(Vector2 position, Vector2 velocity, float size) =>
            SelenicParticles.Add(new(position, velocity, size));

        public override void Update()
        {
            // Update all particle instances.
            // Once sufficiently small, they vanish.
            foreach (SelenicParticle particle in SelenicParticles)
            {
                particle.Velocity *= 0.99f;
                particle.Size *= 0.9f;
                particle.Center += particle.Velocity;
            }
            SelenicParticles.RemoveAll(p => p.Size <= 2.5f);
        }

        public override Vector2 CalculateManualOffsetForLayer(int layerIndex) => Vector2.Zero;

        public override void DrawInstances()
        {
            Texture2D tex = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/BasicCircle").Value;

            // Draw all particles.
            foreach (SelenicParticle particle in SelenicParticles)
            {
                Vector2 drawPosition = particle.Center - Main.screenPosition;
                Vector2 origin = tex.Size() * 0.5f;
                Vector2 scale = Vector2.One * particle.Size / tex.Size();
                Main.spriteBatch.Draw(tex, drawPosition, null, Color.White, 0f, origin, scale, 0, 0f);
            }
        }
    }
}
