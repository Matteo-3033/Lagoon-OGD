﻿using UnityEngine;

namespace Menu.UI
{
    public class Menu: MonoBehaviour
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