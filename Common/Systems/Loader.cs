using CalamityMod.UI;
using MonoMod.Utils;
using System.Reflection;
using Terraria.Graphics.Effects;
using Windfall.Common.Systems.WorldEvents;
using Windfall.Content.Skies;
using Windfall.Content.Skies.CorruptCommunion;
using Windfall.Content.Skies.CrimsonCommunion;
using Windfall.Content.Skies.SlimyCommunion;
using DialogueHelper.Content.UI.Dialogue;
using Windfall.Content.UI.Events;

namespace Windfall.Common.Systems
{
    public class Loading : ModSystem
    {
        public override void Load()
        {
            #region Custom Skies
            SkyManager.Instance["Windfall:Orator"] = new OratorSky();

            Filters.Scene["Windfall:CorruptCommunion"] = new Filter(new CorruptCommunionScreenShaderData("FilterMiniTower").UseColor(0.6f, 0.2f, 0.6f).UseOpacity(0.5f), EffectPriority.VeryHigh);
            SkyManager.Instance["Windfall:CorruptCommunion"] = new CorruptCommunionSky();

            Filters.Scene["Windfall:CrimsonCommunion"] = new Filter(new CrimsonCommunionScreenShaderData("FilterMiniTower").UseColor(0.6f, 0.2f, 0.2f).UseOpacity(0.5f), EffectPriority.VeryHigh);
            SkyManager.Instance["Windfall:CrimsonCommunion"] = new CrimsonCommunionSky();


            Filters.Scene["Windfall:SlimyCommunion"] = new Filter(new SlimyCommunionScreenShaderData("FilterMiniTower").UseColor(0.4f, 0.4f, 0.6f).UseOpacity(0.5f), EffectPriority.VeryHigh);
            SkyManager.Instance["Windfall:SlimyCommunion"] = new SlimyCommunionSky();
            #endregion
        }
        public override void PostSetupContent()
        {
            #region Dialogue System
            string LocalizationPath = "Mods.Windfall.Dialogue.DialogueTrees.";
            DialogueHolder.CharacterAssetPathes.Add($"{nameof(Windfall)}", $"{nameof(Windfall)}/Content/UI/Dialogue/CharacterAssets");

            Dictionary<string, Character> Characters = new()
            {
                { "Windfall/TheCalamity", new Character("[c/FF0000:The Calamity]", [new Expression("Normal", 1, 0), new Expression("Finality", 1, 0)], textDelay: 3, primaryColor: Color.Black, secondaryColor: Color.Red)},
            };

            DialogueHolder.Characters.AddRange(Characters);

            Mod calamityMusic = ModLoader.GetMod("CalamityModMusic");
            Dictionary<string, DialogueTree> dialogueTrees = new()
            {
                #region The Calamity Trees
                {
                    "Windfall/Calamitous",
                    new DialogueTree(
                    [
                        new Dialogue
                        (
                            [
                                new Response("What", 1, Main.LocalPlayer.direction == 1),
                                new Response("Huh", 1, Main.LocalPlayer.direction == -1),
                            ],
                            expressionIndex: 0,
                            musicID: MusicLoader.GetMusicSlot(calamityMusic, "Sounds/Music/CalamitasClone")
                        ),
                        new Dialogue
                        (
                            [
                                new Response("Okay", -2)
                            ],
                            expressionIndex: 1,
                            musicID: MusicLoader.GetMusicSlot(calamityMusic, "Sounds/Music/CalamitasClone")
                        ),
                        new Dialogue
                        (
                            expressionIndex: 1,
                            musicID: MusicLoader.GetMusicSlot(calamityMusic, "Sounds/Music/CalamitasClone")
                        ),
                        new Dialogue
                        (
                            expressionIndex: 1,
                            musicID: MusicLoader.GetMusicSlot(calamityMusic, "Sounds/Music/CalamitasClone")
                        ),
                    ],
                    [
                        "Windfall/TheCalamity"
                    ],
                    LocalizationPath)
                },
                #endregion
                #region The Chef Trees
                {
                    "Windfall/CafeteriaActivityStart",
                    new DialogueTree(
                    [
                        new Dialogue
                        (
                            [
                                new Response("No"),
                                new Response("Yes"),
                            ]
                        ),
                    ],
                    [
                        "Windfall/TheCalamity"
                    ],
                    LocalizationPath)
                },
                {
                    "Windfall/FoodSelection",
                    new DialogueTree(
                    [
                        new Dialogue
                        (
                            LunarCultBaseSystem.GetMenuResponses()
                        ),
                    ],
                    [
                        "Windfall/TheCalamity"
                    ],
                    LocalizationPath)
                },
                #endregion
                #region Lunar Bishop Trees
                {
                    "Windfall/SelenicChat",
                    new DialogueTree(
                    [
                        new Dialogue
                        (
                            [
                                new Response("Knowledge", 1),
                                new Response("Balance", 2),
                            ]
                        ),
                        new Dialogue
                        (
                            [
                                new Response("Me", 3),
                                new Response("Goal", 4),
                            ]
                        ),
                        new Dialogue
                        (
                            [
                                new Response("Me", 3),
                                new Response("Goal", 4),
                            ]
                        ),
                        new Dialogue
                        (
                            [
                                new Response("Tablet", 5),
                            ]
                        ),
                        new Dialogue
                        (
                            [
                                new Response("Tablet", 5),
                            ]
                        ),
                        new Dialogue
                        (
                            [
                                new Response("IC", 6),
                                new Response("Cool", 6),
                            ]
                        ),
                        new Dialogue
                        (
                            [
                                new Response("Bye"),
                                new Response("Finally"),
                            ]
                        ),
                    ],
                    [
                        "Windfall/TheCalamity"
                    ],
                    LocalizationPath)
                },
                #endregion
                #region The Orator Trees
                {
                    "Windfall/TutorialChat",
                    new DialogueTree(
                    [
                        new Dialogue
                        (
                            musicID: -1
                        ),
                        new Dialogue
                        (
                            [
                                new Response("Chef", 2),
                                new Response("Seamstress", 3),
                            ]
                        ),
                        new Dialogue
                        (
                            [
                                new Response("IC", 4),
                            ]
                        ),
                        new Dialogue
                        (
                            [
                                new Response("IC", 4),
                            ]
                        ),
                        new Dialogue
                        (
                            [
                                new Response("Chef", 2),
                                new Response("Seamstress", 3),
                                new Response("You", 5),
                                new Response("No", 6),
                            ]
                        ),
                        new Dialogue
                        (
                            [
                                new Response("IC", 4)
                            ]
                        ),
                        new Dialogue
                        (
                            musicID: -1
                        ),
                        new Dialogue
                        (
                            musicID: -1
                        ),
                        new Dialogue
                        (
                            musicID: -1
                        ),
                        new Dialogue
                        (
                            musicID: -1
                        ),
                        new Dialogue
                        (
                            musicID: -1
                        ),
                    ],
                    [
                        "Windfall/TheCalamity"
                    ],
                    LocalizationPath)
                },
                {
                    "Windfall/RitualEvent",
                    new DialogueTree(
                    [
                        new Dialogue
                        (
                            [
                                new Response("No"),
                                new Response("Yes"),
                            ]
                        ),
                    ],
                    [
                        "Windfall/TheCalamity"
                    ],
                    LocalizationPath)
                },
                #endregion
                #region Communion Trees
                {
                    "Windfall/CorruptCommunion1",
                    new DialogueTree(
                    [
                        new Dialogue
                        (
                            musicID: MusicID.Corruption
                        ),
                        new Dialogue
                        (
                            musicID: MusicID.Corruption
                        ),
                        new Dialogue
                        (
                            musicID: MusicID.Corruption
                        ),
                        new Dialogue
                        (
                            musicID: MusicID.Corruption
                        ),
                        new Dialogue
                        (
                            musicID: MusicID.Corruption
                        ),
                    ],
                    [
                        "Windfall/TheCalamity"
                    ],
                    LocalizationPath)
                },
                {
                    "Windfall/CorruptCommunion2",
                    new DialogueTree(
                    [
                        new Dialogue
                        (
                            musicID: MusicID.Corruption
                        ),
                        new Dialogue
                        (
                            musicID: MusicID.Corruption
                        ),
                        new Dialogue
                        (
                            musicID: MusicID.Corruption
                        ),
                        new Dialogue
                        (
                            musicID: MusicID.Corruption
                        ),
                    ],
                    [
                        "Windfall/TheCalamity"
                    ],
                    LocalizationPath)
                },
                {
                    "Windfall/CrimsonCommunion1",
                    new DialogueTree(
                    [
                        new Dialogue
                        (
                            musicID: MusicID.Crimson
                        ),
                        new Dialogue
                        (
                            musicID: MusicID.Crimson
                        ),
                        new Dialogue
                        (
                            musicID: MusicID.Crimson
                        ),
                        new Dialogue
                        (
                            musicID: MusicID.Crimson
                        ),
                        new Dialogue
                        (
                            musicID: MusicID.Crimson
                        ),
                    ],
                    [
                        "Windfall/TheCalamity"
                    ],
                    LocalizationPath)
                },
                {
                    "Windfall/CrimsonCommunion2",
                    new DialogueTree(
                    [
                        new Dialogue
                        (
                            musicID: MusicID.Crimson
                        ),
                        new Dialogue
                        (
                            musicID: MusicID.Crimson
                        ),
                        new Dialogue
                        (
                            musicID: MusicID.Crimson
                        ),
                        new Dialogue
                        (
                            musicID: MusicID.Crimson
                        ),
                    ],
                    [
                        "Windfall/TheCalamity"
                    ],
                    LocalizationPath)
                },
                {
                    "Windfall/SlimyCommunion",
                    new DialogueTree(
                    [
                        new Dialogue
                        (
                            musicID: MusicID.Eerie
                        ),
                        new Dialogue
                        (
                            musicID: MusicID.Eerie
                        ),
                        new Dialogue
                        (
                            musicID: MusicID.Eerie
                        ),
                        new Dialogue
                        (
                            musicID: MusicID.Eerie
                        ),
                    ],
                    [
                        "Windfall/TheCalamity"
                    ],
                    LocalizationPath)
                },
                #endregion
            };

            DialogueHolder.DialogueTrees.AddRange(dialogueTrees);
            #endregion

            #region Event Bars
            ModLoader.GetMod("CalamityMod").Call("RegisterModCooldowns", this);
            FieldInfo InvasionGUIsFieldInfo = typeof(InvasionProgressUIManager).GetField("gUIs", BindingFlags.NonPublic | BindingFlags.Static);
            List<InvasionProgressUI> guis = ((List<InvasionProgressUI>)InvasionGUIsFieldInfo.GetValue(null));
            guis.Add(Activator.CreateInstance(typeof(TailorEventBar)) as InvasionProgressUI);
            guis.Add(Activator.CreateInstance(typeof(CafeteriaEventBar)) as InvasionProgressUI);
            guis.Add(Activator.CreateInstance(typeof(RitualEventBar)) as InvasionProgressUI);
            #endregion
        }
    }
}
