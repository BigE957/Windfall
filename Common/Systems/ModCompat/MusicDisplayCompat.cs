namespace Windfall.Common.Systems.ModCompat
{
    public class MusicDisplayCompat : ModSystem
    {
        private static readonly string displayPath = "ModCompat.MusicDisplay.";
        public override void PostAddRecipes()
        {
            if (!ModLoader.TryGetMod("MusicDisplay", out Mod display))
                return;

            LocalizedText modName = GetWindfallLocalText(displayPath + "ModName");

            void AddMusic(string path, string name)
            {
                LocalizedText author = GetWindfallLocalText(displayPath + name + ".Author");
                LocalizedText displayName = GetWindfallLocalText(displayPath + name + ".DisplayName");
                display.Call("AddMusic", (short)MusicLoader.GetMusicSlot(Windfall.Instance, path), displayName, author, modName);
            }

            AddMusic("Assets/Music/Orator", "Orator");
            AddMusic("Assets/Music/DeathByGlamor", "Tailor");
            AddMusic("Assets/Music/CanYouReallyCallThisAHotelIDidntReceiveAMintOnMyPillowOrAnything", "Cafeteria");
            AddMusic("Assets/Music/ItsRainingSomewhereElse", "LunarCultBase");
            AddMusic("Assets/Music/Amalgam", "Ritual");
        }
    }
}
