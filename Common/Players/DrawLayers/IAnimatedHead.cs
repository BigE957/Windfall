namespace Windfall.Common.Players.DrawLayers;
public interface IAnimatedHead
{
    Asset<Texture2D> headTexture { get; set; }

    int AnimationLength { get; }
    int AnimationDelay { get; }

    Color? CustomDrawColor => null;

    bool PreDraw(PlayerDrawSet drawInfo) => true;
}
