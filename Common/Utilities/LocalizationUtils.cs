namespace Windfall.Common.Utilities
{
    public static partial class Utilities
    {
        /// <param name="key">The language key. This will have "Mods.Windfall." appended behind it.</param>
        /// <returns>
        /// A <see cref="LocalizedText"/> instance found using the provided key with "Mods.Windfall." appended behind it. 
        /// <para>NOTE: Modded translations are not loaded until after PostSetupContent.</para>Caching the result is suggested.
        /// </returns>
        public static LocalizedText GetLocalText(string key) => Language.GetOrRegister("Mods.Windfall." + key);

        /// <param name="key">The language key. This will have "Mods.Windfall." appended behind it.</param>
        /// <returns>
        /// A <see cref="string"/> instance found using the provided key with "Mods.Windfall." appended behind it.
        /// <para>NOTE: Modded translations are not loaded until after PostSetupContent.</para>Caching the result is suggested.
        /// </returns>
        public static string GetWindfallTextValue(string key) => Language.GetTextValue("Mods.Windfall." + key);
    }
}
