using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenu.Connection
{
    [RequireComponent(typeof(TMP_InputField))]
    public class UsernameInput : MonoBehaviour
    {
        private TMP_InputField nameInputField;
        
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
            PlayerPrefs.SetString(Utils.PlayerPrefsKeys.PlayerName, username);
        }
    }
}