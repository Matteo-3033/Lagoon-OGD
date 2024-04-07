using UnityEngine;

namespace Menu.UI
{
    public class Menu : MonoBehaviour
    {
        public virtual void OnFocus()
        {
            gameObject.SetActive(true);
        }
        
        public virtual void OnUnfocus()
        {
            gameObject.SetActive(false);
        }
        
        public void SetFocus(bool focus)
        {
            if (focus)
                OnFocus();
            else
                OnUnfocus();
        }
    }
}