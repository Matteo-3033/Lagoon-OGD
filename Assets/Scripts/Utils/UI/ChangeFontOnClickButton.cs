using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Utils.UI
{
    [RequireComponent(typeof(Button))]
    public abstract class ChangeFontOnClickButton: MonoBehaviour
    {
        [SerializeField] private TMP_FontAsset font;
        [SerializeField] private float secondsDelay = 0.5F;
        
        private Button button;
        private TextMeshProUGUI textField;
        private bool fontChanged;

        protected virtual void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(DoOnClick);
            
            textField = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        private void DoOnClick()
        {
            if (fontChanged)
            {
                OnClick();
                return;
            }
            
            textField.font = font;
            fontChanged = true;
            
            StartCoroutine(OnClickCoroutine());
        }

        private IEnumerator OnClickCoroutine()
        {
            yield return new WaitForSeconds(secondsDelay);
            OnClick();
        }

        protected abstract void OnClick();
    }
}