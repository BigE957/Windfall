namespace Windfall.Common.Utils;

public static partial class WindfallUtils
{
    /// <param name="key">The language key. This will have "Mods.Windfall." appended behind it.</param>
    /// <returns>
    /// A <see cref="LocalizedText"/> instance found using the provided key with "Mods.Windfall." appended behind it. 
    /// </returns>
    public static LocalizedText GetWindfallLocalText(string key) => Language.GetOrRegister("Mods.Windfall." + key);

    /// <param name="key">The language key. This will have "Mods.Windfall." appended behind it.</param>
    /// <returns>
    /// A <see cref="string"/> instance found using the provided key with "Mods.Windfall." appended behind it.
    /// </returns>
    public static string GetWindfallTextValue(string key) => Language.GetTextValue("Mods.Windfall." + key);
}
