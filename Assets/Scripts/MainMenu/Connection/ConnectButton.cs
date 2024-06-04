using Network;
using TMPro;
using UnityEngine;
using Utils.UI;

namespace MainMenu.Connection
{
    public class ConnectButton: ChangeFontOnClickButton
    {
        [SerializeField] private ClientConnector connector;
        [SerializeField] private TMP_InputField usernameInput;
        [SerializeField] private TMP_InputField serverIpInput;

        private string Username => PlayerPrefs.GetString(Utils.PlayerPrefsKeys.PlayerName, "");
        private string ServerIp => PlayerPrefs.GetString(Utils.PlayerPrefsKeys.ServerIp, "");
        
        protected override void Awake()
        {
            base.Awake();
            
            usernameInput.onValueChanged.AddListener(ValidateUsername);
            serverIpInput.onValueChanged.AddListener(ValidateServerIp);
        }
        
        private void ValidateUsername(string username)
        {
            ValidateForm(username, ServerIp);
        }
        
        private void ValidateServerIp(string serverIp)
        {
            ValidateForm(Username, serverIp);
        }

        private void ValidateForm(string username, string serverIp)
        {
            Button.interactable = !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(serverIp);
        }

        protected override void OnClick()
        {
            connector.InitClient();
        }
    }
}