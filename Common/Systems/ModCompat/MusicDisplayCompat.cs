namespace Windfall.Common.Systems.ModCompat
{
    public class MusicDisplayCompat : ModSystem
    {
        public override void PostAddRecipes()
        {
            if (!ModLoader.TryGetMod("MusicDisplay", out Mod display))
                return;

            LocalizedText modName = Language.GetText("Mods.Windfall.MusicDisplay.ModName");

            void AddMusic(string path, string name)
            {
                LocalizedText author = Language.GetText("Mods.Windfall.MusicDisplay." + name + ".Author");
                LocalizedText displayName = Language.GetText("Mods.Windfall.MusicDisplay." + name + ".DisplayName");
                display.Call("AddMusic", (short)MusicLoader.GetMusicSlot(Windfall.Instance, path), displayName, author, modName);
            }

            AddMusic("Assets/Music/Orator", "Orator");
        }
    }
}
