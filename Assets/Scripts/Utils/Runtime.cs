using UnityEditor;
using UnityEngine.Device;

namespace Utils
{
    public class Runtime
    {
        public static void Quit()
        {
            #if UNITY_EDITOR
                EditorApplication.isPlaying = false;
            #elif !UNITY_EDITOR && !UNITY_WEBGL
                UnityEngine.Application.Quit();
            #elif !UNITY_EDITOR && UNITY_WEBGL
                MstAlert(webGlQuitMessage);
                Logs.Info(webGlQuitMessage);
            #endif
        }
    }
}