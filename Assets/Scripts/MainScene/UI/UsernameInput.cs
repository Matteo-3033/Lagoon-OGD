using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MainScene
{
    public class UsernameInput : MonoBehaviour
    {
        private TMP_InputField nameInputField;
        [SerializeField] private Button connectButton;
        
        private void Awake()
        {
            nameInputField = GetComponent<TMP_InputField>();
            InitInputField();
            nameInputField.onValueChanged.AddListener(SetUsername);
        }

        private void InitInputField()
        {
            if (!PlayerPrefs.HasKey(Utils.PlayerPrefsKeys.PlayerName))
                SetUsername("");
            else
            {
                var playerName = PlayerPrefs.GetString(Utils.PlayerPrefsKeys.PlayerName);
                nameInputField.text = playerName;
                SetUsername(playerName);
            }
        }

        private void SetUsername(string playerName)
        {
            connectButton.interactable = !string.IsNullOrEmpty(playerName);
            PlayerPrefs.SetString(Utils.PlayerPrefsKeys.PlayerName, playerName);
        }
    }
}