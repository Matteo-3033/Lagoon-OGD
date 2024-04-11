﻿using Network;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.UI.Connection
{
    [RequireComponent(typeof(Button))]
    public class ConnectButton: MonoBehaviour
    {
        [SerializeField] private ClientConnector connector;
        
        private void Start()
        {
            var button = gameObject.GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }
        
        private void OnClick()
        {
            connector.InitClient();
        }
    }
}