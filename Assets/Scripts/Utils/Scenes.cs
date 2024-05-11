using UnityEngine.SceneManagement;

namespace Utils
{
    public static class Scenes
    {
        public const string Master = "Assets/Scenes/Master/Master.unity";
        public const string Menu = "Assets/Scenes/Client/MainMenu.unity";
        public const string Lobby = "Assets/Scenes/Client/Lobby.unity";
        public const string Round1 = "Assets/Scenes/Client/Round1.unity";

        public static bool IsIn(string scene)
        {
            return SceneManager.GetActiveScene().name == scene;
        }
    }
}