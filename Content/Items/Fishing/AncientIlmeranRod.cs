using CalamityMod.Events;
using CalamityMod.Items;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Windfall.Content.Projectiles.Fishing;
using Windfall.Content.Projectiles.NPCAnimations;
using Windfall.Content.NPCs.WanderingNPCs;
using Windfall.Common.Systems;
using System.Collections.Generic;
using Windfall.Common.Utilities;
using CalamityMod;
using System;

namespace Windfall.Content.Items.Fishing
{
    public class AncientIlmeranRod : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Fishing";
        public override string Texture => "Windfall/Assets/Items/Fishing/AncientIlmeranRod";

        public static readonly SoundStyle Roar = new("Calamitymod/Sounds/Custom/DesertScourgeRoar");

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.useAnimation = 8;
            Item.useTime = 8;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item1;
            Item.rare = ItemRarityID.Blue;
            Item.fishingPole = 10;
            Item.shootSpeed = 10f;
            Item.shoot = ModContent.ProjectileType<AncientIlmeranBobber>();
            Item.value = CalamityGlobalItem.Rarity1BuyPrice;
        }
        internal bool isCast = false;
        internal int scoogCounter = 0;
        internal struct dialogue
        {
            internal string text;
            internal int delay;
        }
        internal static List<dialogue> PaladinDialogue = new()
        { 
            //First Time Dialogue
            new dialogue {text = "Salutations!" , delay = 7},
            new dialogue {text = "I see you've got your line cast...", delay = 4},
            new dialogue {text = "You're ready for battle then.", delay = 3},
            new dialogue {text = "Ah, this takes me back...", delay = 6},
            new dialogue {text = "Hunting a Scourge was once a right of passage for Paladin's in training.", delay = 3},
            new dialogue {text = "I still remember mine like it was yesterday...", delay = 3},
            /*
            new dialogue {text = "Its a real shame you won't get to see one in its prime, cause let me tell you...", delay = 5},
            new dialogue {text = "The Scourges were some of the most magestic creatures the Ilmeran Sea had to offer.", delay = 3},
            new dialogue {text = "Now all thats left of them is a dried husk, driven mad with hunger...", delay = 3},
            */
            new dialogue {text = "If you are to emerge victorious, you wouldn't be far off from being a paladin yourself.", delay = 5},
            new dialogue {text = "It'd makes me happy to know I'm not the last...", delay = 3},
            new dialogue {text = "However, to truly become a Paladin, you'd need to be recognized by the King.", delay = 3},
            new dialogue {text = "And... Well...", delay = 3},
            new dialogue {text = "No one knows what became of him after the Incineration...", delay = 2},
            new dialogue {text = "I still hold out hope he may one day be found.", delay = 3},
            new dialogue {text = "If he were to return, perhaps there'd be hope for-", delay = 3},
            new dialogue {text = "?!", delay = 2},
            new dialogue {text = "The Scourge draws near...", delay = 5},
            new dialogue {text = "...ready yourself.", delay = 3},
            //Marks end of this set of dialogue
            new dialogue {text = "I should never say this :3", delay = 20},

            //Subsequent Time Dialogue
            new dialogue {text = "Alright!" , delay = 6},
            new dialogue {text = "Let's give this another go." , delay = 3},
            new dialogue {text = "?!" , delay = 6},
            new dialogue {text = "Here she comes!" , delay = 3},

            //Here only for error checking
            new dialogue {text = "YOU'RE OUT OF BOUNDS DUMBASS!!!", delay = 20},
        };
        int scoogWait = 60;
        int dialogueCounter = 0;
        static int shakeCounter = 0;
        bool startLeft = false;
        public override void HoldItem(Player player)
        {
            isCast = false;
            for (int i = 0; i < Main.projectile.Length; i++)
            {
                if (Main.projectile[i].type == ModContent.ProjectileType<AncientIlmeranBobber>())
                {
                    isCast = true;
                    break;
                }
            }
            if (isCast && player.ZoneDesert && !NPC.AnyNPCs(ModContent.NPCType<DesertScourgeHead>()) && !BossRushEvent.BossRushActive)
            {
                //Determines how long the wait for Desert Scourge will be based on ScoogFished
                if (scoogCounter == 0)
                {
                    shakeCounter = 0;
                    startLeft = Main.rand.NextBool();
                    if (WorldSaveSystem.ScoogFished)
                    {
                        scoogWait = Main.rand.Next(30, 31);
                        dialogueCounter = 17;
                    }
                    else
                    {
                        scoogWait = Main.rand.Next(68, 69);
                        dialogueCounter = 0;
                    }
                }
                scoogCounter++;
                
                //Spawns Paladin after 5 Seconds
                if (scoogCounter == 60 * 5 && !NPC.AnyNPCs(ModContent.NPCType<IlmeranPaladin>()) && !NPC.AnyNPCs(ModContent.NPCType<IlmeranPaladinKnocked>()))
                {
                    Projectile.NewProjectile(null, new Vector2(player.Center.X - 80 * player.direction, player.Center.Y + 100), new Vector2(0, -8), ModContent.ProjectileType<IlmeranPaladinDig>(), 0, 0);
                }

                //Ilmeran Paladin Dialogue during the wait for Desert Scourge
                
                if (NPC.AnyNPCs(ModContent.NPCType<IlmeranPaladin>()))
                {
                    NPC Paladin = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<IlmeranPaladin>())];
                    int Delay = 0;
                    if (!WorldSaveSystem.ScoogFished)
                    {
                        for (int i = dialogueCounter; i >= 0; i--)
                        {
                            Delay += PaladinDialogue[i].delay;
                        }
                    }
                    else
                    {
                        for (int i = dialogueCounter; i >= 17; i--)
                        {
                            Delay += PaladinDialogue[i].delay;
                        }

                    }
                    if (scoogCounter == (60 * (Delay)))
                    {
                        PaladinMessage(PaladinDialogue[dialogueCounter].text, Paladin);
                        dialogueCounter++;
                    }
                }

                //Does the ambiant Scourge Sound and Screen Shake
                if (!WorldSaveSystem.ScoogFished)
                {
                    if(scoogCounter >= 60 * 49 && scoogCounter <= 60 * 55)
                    {
                        ScoogShake(player, scoogCounter, 60 * 52, startLeft, 0.25f);
                    }
                    if (scoogCounter >= 60 * 60 && scoogCounter <= 60 * 65)
                    {
                        ScoogShake(player, scoogCounter, 60 * 65, !startLeft, 0.5f);
                    }
                }
                else
                {
                    if (scoogCounter >= 60 * 14 && scoogCounter <= 60 * 20)
                    {
                        ScoogShake(player, scoogCounter, 60 * 16, startLeft, 0.25f);
                    }
                    if (scoogCounter >= 60 * 22 && scoogCounter <= 60 * 28)
                    {
                        ScoogShake(player, scoogCounter, 60 * 28, !startLeft, 0.5f);
                    }
                }

                //Spawns Desert Scourge after "scoogWait" seconds
                if (scoogCounter >= 60 * scoogWait)
                {
                    if (NPC.AnyNPCs(ModContent.NPCType<IlmeranPaladin>()))
                    {
                        NPC Paladin = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<IlmeranPaladin>())];
                        PaladinMessage("En garde!!", Paladin);
                    }
                    WorldSaveSystem.ScoogFished = true;
                    SoundEngine.PlaySound(SoundID.Roar, player.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<DesertScourgeHead>());
                    else
                        NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, -1, -1, null, player.whoAmI, ModContent.NPCType<DesertScourgeHead>());

                    if (CalamityWorld.revenge)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<DesertNuisanceHead>());
                        else
                            NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, -1, -1, null, player.whoAmI, ModContent.NPCType<DesertNuisanceHead>());

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                            NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<DesertNuisanceHead>());
                        else
                            NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, -1, -1, null, player.whoAmI, ModContent.NPCType<DesertNuisanceHead>());
                    }
                }
            }
            else
            {
                if (scoogCounter != 0 && NPC.AnyNPCs(ModContent.NPCType<IlmeranPaladin>()) && !NPC.AnyNPCs(ModContent.NPCType<DesertScourgeHead>()))
                {
                    NPC Paladin = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<IlmeranPaladin>())];
                    PaladinMessage("Oh, nevermind...", Paladin);
                }
                scoogCounter = 0;
            }
        }
        internal static void PaladinMessage(string text, NPC Paladin)
        {
            Rectangle location = new((int)Paladin.Center.X, (int)Paladin.Center.Y, Paladin.width, Paladin.width);
            CombatText.NewText(location, Color.SandyBrown, text, true);
        }
        internal static void ScoogShake(Player target, int scoogTimer, int midpoint, bool leftSide, float volume)
        {
            float groundShakeTime = 270f;

            // Make the ground shake and the ground create rising sand particles on the ground at first.
            float groundShakeInterpolant = (float)shakeCounter / groundShakeTime;

            for (int i = 0; i < 3; i++)
            {
                if (Main.rand.NextFloat() >= groundShakeInterpolant + 0.2f)
                    continue;

                Vector2 particleSpawnPosition = Utilities.GetGroundPositionFrom(target.Center + new Vector2(Main.rand.NextFloatDirection() * 1200f, -560f));
                bool sandBelow = CalamityUtils.ParanoidTileRetrieval((int)(particleSpawnPosition.X / 16f), (int)(particleSpawnPosition.Y / 16f)).TileType == TileID.Sand;
                if (sandBelow)
                    Dust.NewDustPerfect(particleSpawnPosition + new Vector2(Main.rand.NextFloatDirection() * 8f, -8f), 32, Main.rand.NextVector2Circular(1.5f, 1.5f) - Vector2.UnitY * 1.5f);
            }
            // Create screen shake effects.
            target.Windfall_Camera().CurrentScreenShakePower = (float)(MathF.Pow(groundShakeInterpolant, 1.81f) * 10f);
            if (scoogTimer == midpoint)
                if (leftSide)
                {
                    SoundEngine.PlaySound(Roar with { Volume = volume }, target.Center + new Vector2(Main.rand.Next(-300, -200), 150));
                }
                else
                {
                    SoundEngine.PlaySound(Roar with { Volume = volume }, target.Center + new Vector2(Main.rand.Next(200, 300), 150));
                }
            else if (scoogTimer > midpoint)
                shakeCounter--;
            else
                shakeCounter++;
        }
    }
}
