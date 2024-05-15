using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windfall.Common.Utils
{
    public static partial class WindfallUtils
    {
        public static void NPCAntiClump(this NPC npc, float pushForce = 0.05f)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc2 = Main.npc[i];
                if (!npc2.active || i == npc.whoAmI)
                {
                    continue;
                }
                bool shareType = npc2.type == npc.type;
                float proximity = Math.Abs(npc.position.X - npc2.position.X) + Math.Abs(npc.position.Y - npc2.position.Y);
                DisplayLocalizedText($"{shareType && proximity < (float)npc.width}");
                if (shareType && proximity < (float)npc.width)
                {
                    if (npc.position.X < npc2.position.X)
                    {
                        npc.velocity.X -= pushForce;
                    }
                    else
                    {
                        npc.velocity.X += pushForce;
                    }

                    if (npc.position.Y < npc2.position.Y)
                    {
                        npc.velocity.Y -= pushForce;
                    }
                    else
                    {
                        npc.velocity.Y += pushForce;
                    }
                }
            }
        }
    }
}
