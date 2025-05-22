using System.Reflection;
using Terraria.Graphics.Shaders;

namespace Windfall.Common.Utils;
public static partial class WindfallUtils
{
    internal static readonly FieldInfo UImageFieldMisc0 = typeof(MiscShaderData).GetField("_uImage0", BindingFlags.Instance | BindingFlags.NonPublic);

    internal static readonly FieldInfo UImageFieldMisc1 = typeof(MiscShaderData).GetField("_uImage1", BindingFlags.Instance | BindingFlags.NonPublic);

    internal static readonly FieldInfo UImageFieldArmor = typeof(ArmorShaderData).GetField("_uImage", BindingFlags.Instance | BindingFlags.NonPublic);

    public static MiscShaderData SetTexture(this MiscShaderData shader, Asset<Texture2D> texture, int index = 1)
    {
        switch (index)
        {
            case 0:
                UImageFieldMisc0.SetValue(shader, texture);
                break;
            case 1:
                UImageFieldMisc1.SetValue(shader, texture);
                break;
        }

        return shader;
    }

    public static Color FromHexToColor(string hex)
    {
        System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(hex);
        int r = Convert.ToInt16(color.R);
        int g = Convert.ToInt16(color.G);
        int b = Convert.ToInt16(color.B);
        return new Color(r, g, b);
    }

    public static Vector3 FromHexToVec3(string hex)
    {
        System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(hex);
        int r = Convert.ToInt16(color.R);
        int g = Convert.ToInt16(color.G);
        int b = Convert.ToInt16(color.B);
        return new Color(r, g, b).ToVector3();
    }
}
