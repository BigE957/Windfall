using CalamityMod;
using CalamityMod.Items.Accessories;
using CalamityMod.NPCs.DesertScourge;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Windfall.Common.Utilities;
using Windfall.Content.Items.Fishing;
using Windfall.Content.Items.Utility;
using Windfall.Content.Items.Weapons.Misc;

namespace Windfall.Content.NPCs.WanderingNPCs
{
    public class IlmeranPaladinKnocked : ModNPC
    {
        public override string Texture => "Windfall/Assets/NPCs/WanderingNPCs/IlmeranPaladinKnocked";
        public override void SetStaticDefaults()
        {
            this.HideFromBestiary();
            NPCID.Sets.ActsLikeTownNPC[Type] = true;
            NPCID.Sets.NoTownNPCHappiness[Type] = true;
        }
        public override void SetDefaults()
        {
            NPC.friendly = true; // NPC Will not attack player
            NPC.width = 18;
            NPC.height = 40;
            NPC.aiStyle = -1;
            NPC.damage = 10;
            NPC.defense = 15;
            NPC.lifeMax = 250;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 1f;
            NPC.immortal = true;

            AnimationType = NPCID.BartenderUnconscious;
        }
        public override bool CanChat() => true;

        public override string GetChat()
        {
            NPC.Transform(ModContent.NPCType<IlmeranPaladin>());
            return Language.GetOrRegister($"Mods.{nameof(Windfall)}.Dialogue.IlmeranPaladin.Chat.Saved").Value; // chat is implicitly cast to a string.
        }
        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            if (firstButton)
            {
                shop = "Shop";
            }
            else
            {
                Utilities.QuestDialogueHelper(Main.npc[NPC.whoAmI]);
            }
        }

        public override void AddShops()
        {
            new NPCShop(Type)
                .AddWithCustomValue<AmidiasSpark>(5000)
                .AddWithCustomValue<Cnidrisnack>(50)
                .AddWithCustomValue<AncientIlmeranRod>(1000, WindfallConditions.ScoogHunt1ActiveOrCompleted)
                .AddWithCustomValue<IlmeranHorn>(2000, WindfallConditions.ScoogHunt1Completed)
                .Register();
        }

        public override bool CheckActive()
        {
            if (NPC.AnyNPCs(ModContent.NPCType<DesertScourgeHead>()))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
