using System.Collections.Generic;
using UnityEngine;

namespace Lobby
{
    public class PlayerPreviewer : MonoBehaviour
    {
        [SerializeField] private PlayerPreview previewTemplate;
        [SerializeField] private GameObject loadingSpinner;

        private readonly Dictionary<string, PlayerPreview> previews = new();

        private void Awake()
        {
            previewTemplate.gameObject.SetActive(false);
            loadingSpinner.SetActive(true);
            
            Player.OnPlayerSpawned += (localPlayer) => ShowPreview(localPlayer ? Player.LocalPlayer : Player.Opponent);
            Player.OnPlayerDespawned += (localPlayer) => DeletePreview(localPlayer ? Player.LocalPlayer : Player.Opponent);
        }

        private void ShowPreview(Player player)
        {  
            if (!previews.TryGetValue(player.Username, out var preview))
            {
                if (previews.Count == 1)
                    loadingSpinner.SetActive(false);
                
                preview = Instantiate(previewTemplate, previewTemplate.transform.parent);
                if (previews.Count == 0)
                    preview.transform.SetAsFirstSibling();
                preview.gameObject.SetActive(true);
                
                previews[player.Username] = preview;
            }
            
            preview.SetPlayer(player);
        }
        
        
        private void DeletePreview(Player player)
        {
            if (previews.TryGetValue(player.Username, out var preview))
            {
                Destroy(preview.gameObject);
                previews.Remove(player.Username);
            }
        }
    }
}