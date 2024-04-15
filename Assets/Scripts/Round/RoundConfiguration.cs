using Mirror;
using UnityEngine;

namespace Utils
{
    [CreateAssetMenu(menuName = "ScriptableObjects/RoundConfiguration", fileName = "RoundConfiguration")]
    public class RoundConfiguration : ScriptableObject
    {
        [Scene] public string scene;
        public int keyFragments;
    }

    public static class RoundConfigurationSerializer 
    {
        public static void WriteRoundConfiguration(this NetworkWriter writer, RoundConfiguration config)
        {
            writer.WriteString(config == null ? "" : config.name);
        }

        public static RoundConfiguration ReadRoundConfiguration(this NetworkReader reader)
        {
            var roundName = reader.ReadString();
            return string.IsNullOrEmpty(roundName) ? null : Resources.Load<RoundConfiguration>($"Rounds/{roundName}");
        }
    }
}