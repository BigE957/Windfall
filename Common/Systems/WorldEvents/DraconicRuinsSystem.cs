using Luminance.Core.Graphics;
using Terraria.ModLoader.IO;
using Windfall.Content.NPCs.TravellingNPCs;
using Windfall.Content.NPCs.WorldEvents.LunarCult;

namespace Windfall.Common.Systems.WorldEvents;
public class DraconicRuinsSystem : ModSystem
{
    public static Point DraconicRuinsLocation = new(-1,-1);
    public static bool FacingLeft = false;
    public static Point RuinsEntrance => new(DraconicRuinsLocation.X + (FacingLeft ? -19 : 19), DraconicRuinsLocation.Y - 20);
    public static Point TabletRoom => new(DraconicRuinsLocation.X + (FacingLeft ? 9 : -9), DraconicRuinsLocation.Y + 46);

    public enum CutsceneState
    {
        Arrival,
        Fumble,
        End
    }
    public static CutsceneState State = CutsceneState.Arrival;

    public static bool CutsceneActive = false;
    public static bool ZoomActive = false;

    private static int CutsceneTime = 0;
    private static int CameraTime = 0;

    public override void ClearWorld()
    {
        DraconicRuinsLocation = new(-1, -1);
        FacingLeft = false;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        tag["DraconicRuinsLocation"] = DraconicRuinsLocation;
        tag["FacingLeft"] = FacingLeft;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        DraconicRuinsLocation = tag.Get<Point>("DraconicRuinsLocation");
        FacingLeft = tag.GetBool("FacingLeft");
    }

    public override void PreUpdateWorld()
    {
        //Dust.NewDustPerfect(RuinsEntrance.ToWorldCoordinates(), DustID.Terra, Vector2.Zero);
        //Dust.NewDustPerfect(TabletRoom.ToWorldCoordinates(), DustID.Shadowflame, Vector2.Zero);

        if(!NPC.AnyNPCs(ModContent.NPCType<SealingTablet>()))
                NPC.NewNPC(Entity.GetSource_None(), TabletRoom.X * 16 + 8, TabletRoom.Y * 16 - 160, ModContent.NPCType<SealingTablet>());

        if (ZoomActive)
        {
            if (CameraTime < 60)
                CameraTime++;
            else
                CameraTime = 60;

            if (CameraTime > 0)
                switch (State)
                {
                    case CutsceneState.Arrival:
                        CameraPanSystem.PanTowards(RuinsEntrance.ToWorldCoordinates() - new Vector2(0, 120), CircOutEasing(CameraTime / 60f, 1));

                        CameraPanSystem.Zoom = CircOutEasing(CameraTime / 60f, 1) / 2f;

                        NPC tc = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<TravellingCultist>())];

                        switch(CutsceneTime)
                        {
                            case 60:
                                DisplayMessage("Wow...", tc, Color.Orange, 60);
                                break;
                            case 180:
                                DisplayMessage("I can't believe I'm really here.", tc, Color.Orange, 90);
                                break;
                            case 300:
                                DisplayMessage("These ruins...", tc, Color.Orange, 60);
                                break;
                            case 390:
                                DisplayMessage("I can't wait to learn their secrets!", tc, Color.Orange, 90);
                                break;
                            case 510:
                                DisplayMessage("Let's head inside.", tc, Color.Orange, 60);
                                break;
                            case 570:
                                ZoomActive = false;
                                CutsceneActive = false;
                                CutsceneTime = 0;
                                CameraTime = 0;
                                State = CutsceneState.Fumble;
                                break;
                        }
                        break;
                    case CutsceneState.Fumble:
                        CameraPanSystem.PanTowards(TabletRoom.ToWorldCoordinates() - new Vector2(0, 120), CircOutEasing(CameraTime / 60f, 1));

                        CameraPanSystem.Zoom = CircOutEasing(CameraTime / 60f, 1) / 2f;

                        if (CutsceneTime > 240)
                        {
                            ZoomActive = false;
                            CutsceneActive = false;
                            CutsceneTime = 0;
                            CameraTime = 0;
                            State = CutsceneState.End;
                        }
                        break;
                }
        }

        if (CutsceneActive)
            CutsceneTime++;
    }

    public static void StartCutscene()
    {
        ZoomActive = true;
        CutsceneActive = true;
        CutsceneTime = 0;
        CameraTime = 0;
    }

    private static void DisplayMessage(string key, Rectangle location, Color color, int upTime)
    {
        int index = CombatText.NewText(location, color, GetWindfallTextValue($"Dialogue.{key}"), true);
        if (index == 100)
            return;
        CombatText MyDialogue = Main.combatText[index];
        MyDialogue.lifeTime = upTime;
        MyDialogue.velocity /= 1.5f;
    }

    private static void DisplayMessage(string key, NPC NPC, Color color, int upTime)
    {
        Rectangle location = new((int)NPC.position.X, (int)NPC.position.Y, NPC.width, NPC.width);
        int index = CombatText.NewText(location, color, key, true);//GetWindfallTextValue($"Dialogue.{key}"), true);
        if (index == 100)
            return;
        CombatText MyDialogue = Main.combatText[index];
        MyDialogue.lifeTime = upTime;
        MyDialogue.velocity /= 1.5f;
    }
}
