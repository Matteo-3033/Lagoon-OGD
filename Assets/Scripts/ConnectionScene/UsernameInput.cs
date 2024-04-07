using Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MainScene
{
    [RequireComponent(typeof(TMP_InputField))]
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

        private void SetUsername(string username)
        {
            connectButton.interactable = !string.IsNullOrEmpty(username);
            PlayerPrefs.SetString(Utils.PlayerPrefsKeys.PlayerName, username);
        }
    }
}