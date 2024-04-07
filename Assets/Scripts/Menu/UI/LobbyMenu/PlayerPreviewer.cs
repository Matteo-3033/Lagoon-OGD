using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Network;
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

        private void ShowPlayerPreviews()
        {
            foreach (var player in match.Players)
            {
                var preview = Instantiate(previewTemplate, transform);
                preview.gameObject.SetActive(true);
                previews[player.username] = preview;
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

        public void SetMatch(MatchController matchController)
        {
            match = matchController;
            match.Players.Callback += OnPlayerChanged;
        }

        private void OnPlayerChanged(SyncList<Player>.Operation op, int itemindex, Player olditem, Player newitem)
        {
            ShowPlayerPreviews();
        }

        private void OnDestroy()
        {
            if (match)
                match.Players.Callback -= OnPlayerChanged;
        }
    }
}