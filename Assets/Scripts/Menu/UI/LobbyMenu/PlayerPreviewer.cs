using System.Collections.Generic;
using Network;
using Network.Messages;
using UnityEngine;

namespace Menu.UI.LobbyMenu
{
    public class PlayerPreviewer : MonoBehaviour
    {
        [SerializeField] private PlayerPreview previewTemplate;

        private MatchController match;
        private readonly Dictionary<string, PlayerPreview> previews = new();

        private void Awake()
        {
            previewTemplate.gameObject.SetActive(false);
        }

        public void ShowPlayerPreviews(IEnumerable<PlayerInfo> players)
        {
            foreach (var player in players)
            {
                if (!previews.TryGetValue(player.username, out var preview))
                {
                    preview = Instantiate(previewTemplate, transform);
                    preview.gameObject.SetActive(true);
                    previews[player.username] = preview;
                }
                preview.SetPlayer(player);
            }
        }

        public void Clear()
        {
            foreach (var preview in previews.Values)
            {
                Destroy(preview.gameObject);
            }
            previews.Clear();
        }
    }
}