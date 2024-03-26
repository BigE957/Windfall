using CalamityMod.Cooldowns;

namespace Windfall.Content.Buffs.Cooldowns
{
    public class ParryWeapon : CooldownHandler
    {
        public static new string ID => "ParryWeapon";
        public override bool ShouldDisplay => true;
        public override LocalizedText DisplayName => Language.GetOrRegister($"Mods.{nameof(Windfall)}.UI.Cooldowns.{ID}");
        public override string Texture => "Windfall/Assets/UI/Cooldowns/ParryWeapon";
        public override Color OutlineColor => Color.Lerp(new Color(173, 66, 203), new Color(252, 109, 202), instance.Completion);
        public override Color CooldownStartColor => new(252, 109, 202);
        public override Color CooldownEndColor => new(119, 254, 254);
    }
}
