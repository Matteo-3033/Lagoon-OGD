using System;
using TMPro;
using UnityEngine;

namespace MainMenu.Connection
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class ConnectionInfoText: MonoBehaviour
    {
        private TextMeshProUGUI textField;

        private void Awake()
        {
            textField = GetComponent<TextMeshProUGUI>();
        }
        
        public void Hide()
        {
            SetText("");
        }
        
        private void SetText(string text)
        {
            textField.text = $"<color=red>{text}</color>";
        }

        public void ShowNoConnection()
        {
            SetText("Could not connect to the server");
        }

        public void ShowConnecting()
        {
            SetText("Connecting to the server...");
        }

        public void ShowConnectionLoss()
        {
            SetText("Connection to the server was lost");
        }
    }
}