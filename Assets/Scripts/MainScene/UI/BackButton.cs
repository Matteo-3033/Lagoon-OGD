using UnityEngine;
using UnityEngine.UI;

namespace MainScene
{
    public class BackButton : MonoBehaviour
    {
        [SerializeField] private UIManager.Menu to;
        
        private void Start()
        {
            var button = gameObject.GetComponent<Button>();
            button.onClick.AddListener(() => UIManager.Instance.ShowMenu(to));
        }
    }
}