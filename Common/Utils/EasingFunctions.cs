using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windfall.Common.Utils;
public static partial class WindfallUtils
{
    public static float CircOutEasing(float amount) => (float)Math.Sqrt(1.0 - Math.Pow(amount - 1f, 2.0));

    public static float ExpInEasing(float amount)
    {
        if (amount != 0f)
        {
            return (float)Math.Pow(2.0, 10f * amount - 10f);
        }

        return 0f;
    }
    public static float ExpOutEasing(float amount)
    {
        if (amount != 1f)
        {
            return 1f - (float)Math.Pow(2.0, -10f * amount);
        }

        return 1f;
    }

    public static float SineInEasing(float amount) => 1f - (float)Math.Cos(amount * MathF.PI / 2f); 
    public static float SineOutEasing(float amount) => (float)Math.Sin(amount * MathF.PI / 2f);
    public static float SineInOutEasing(float amount) => (0f - ((float)Math.Cos(amount * MathF.PI) - 1f)) / 2f;
    public static float SineBumpEasing(float amount) => (float)Math.Sin(amount * MathF.PI);
}
