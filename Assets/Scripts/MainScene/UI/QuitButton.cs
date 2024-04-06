using UnityEngine;
using UnityEngine.UI;

namespace MainScene
{
    public class QuitButton : MonoBehaviour
    {
        private void Start()
        {
            var button = gameObject.GetComponent<Button>();
            button.onClick.AddListener(Application.Quit);
        }
    }
}