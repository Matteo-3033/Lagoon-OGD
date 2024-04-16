using UnityEngine;

namespace Utils
{
    public class Screen: MonoBehaviour
    {
        public virtual void OnFocus()
        {
            if (gameObject != null)
                gameObject.SetActive(true);
        }
        
        public virtual void OnUnfocus()
        {
            if (gameObject != null)
                gameObject.SetActive(false);
        }
    }
}