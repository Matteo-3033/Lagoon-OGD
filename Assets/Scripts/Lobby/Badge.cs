using UnityEngine;
using UnityEngine.UI;

namespace Lobby
{
 
    [RequireComponent(typeof(Image))]
    public class Badge: MonoBehaviour
    {
        [SerializeField] private Color bronze = new Color(205, 127, 50);
        [SerializeField] private Color silver = new Color(211, 211, 211);
        [SerializeField] private Color gold = new Color(255, 215, 0);
        
        private Image image;

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        public void SetScore(int score)
        {
            image.color = score switch
            {
                <= 100 => bronze,
                <= 200 => silver,
                _ => gold,
            };
        }
    }
}