using TMPro;
using UnityEngine;

namespace MainMenu.Connection
{
    [RequireComponent(typeof(TMP_InputField))]
    public class ServerIpInput : MonoBehaviour
    {
        private TMP_InputField serverIpInputField;
        
        private void Awake()
        {
            serverIpInputField = GetComponent<TMP_InputField>();
            InitInputField();
            serverIpInputField.onValueChanged.AddListener(SetServerIp);
        }

        private void InitInputField()
        {
            if (!PlayerPrefs.HasKey(Utils.PlayerPrefsKeys.ServerIp))
                SetServerIp("");
            else
            {
                var serverIp = PlayerPrefs.GetString(Utils.PlayerPrefsKeys.ServerIp);
                serverIpInputField.text = serverIp;
                SetServerIp(serverIp);
            }
        }

        private void SetServerIp(string serverIp)
        {
            PlayerPrefs.SetString(Utils.PlayerPrefsKeys.ServerIp, serverIp);
        }
    }
}
