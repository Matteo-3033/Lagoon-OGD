using UnityEngine;
using UnityEngine.UI;

namespace Round.UI.Winner
{
    [RequireComponent(typeof(Image))]
    public class WinnerAvatar : MonoBehaviour
    {
        [SerializeField] private Sprite mangiagalliSprite;
        [SerializeField] private Sprite golgiSprite;

        private void Awake()
        {
            var image = GetComponent<Image>();
            
            var winner = RoundController.Instance.Winner;
            
            image.sprite = winner.IsMangiagalli ? mangiagalliSprite : golgiSprite;
        }
    }
}