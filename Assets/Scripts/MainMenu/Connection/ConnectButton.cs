using Network;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.UI;

namespace MainMenu.Connection
{
    public class ConnectButton: ChangeFontOnClickButton
    {
        [SerializeField] private ClientConnector connector;

        protected override void OnClick()
        {
            connector.InitClient();
        }
    }
}