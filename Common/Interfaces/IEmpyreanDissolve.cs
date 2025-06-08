using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windfall.Common.Interfaces;
public interface IEmpyreanDissolve
{
    float DissolveIntensity { get; set; }
    Vector2 sampleOffset { get; set; }
    void DrawOverlay(SpriteBatch sb);
}
