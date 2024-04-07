using System.Collections;
using System.Collections.Generic;
using Mirror;
using Network;
using Network.Messages;
using UnityEngine;
using UnityEngine.SceneManagement;
using ClientMatchOperation = Network.Messages.ClientMatchOperation;
using ServerMatchOperation = Network.Messages.ServerMatchOperation;

namespace Menu.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GameObject main;
        [SerializeField] private GameObject search;
        [SerializeField] private LobbyMenu.LobbyMenu lobby;
        
        private readonly Dictionary<MenuKey, GameObject> menus = new();
        private GameObject currentMenu;
        private bool searchingMatch;

        public enum MenuKey
        {
            MainMenu,
            SearchingMatch,
            Lobby
        }
        
        public static UIManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("UIManager: Instance already exists.");
                Destroy(this);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (Application.isBatchMode)
                OnStartServer();
            else OnStartClient();
        }

        private void OnStartServer()
        {
            NetworkServer.RegisterHandler<ToServerMatchMessage>(OnToServerMatchMessage);
        }

        private void OnStartClient()
        {
            if (main == null)
            {
                Debug.LogError("UIManager: MainMenu is not set.");
                return;
            }

            AddMenu(MenuKey.MainMenu, main);
            AddMenu(MenuKey.SearchingMatch, search);
            AddMenu(MenuKey.Lobby, lobby.gameObject);
            
            ShowMenu(MenuKey.MainMenu);
            
            NetworkClient.RegisterHandler<ToClientMatchMessage>(OnToClientMatchMessage);
        }
        
        [ClientCallback]
        private void AddMenu(MenuKey key, GameObject value)
        {
            if (value != null)
                menus.Add(key, value);
            value.gameObject.SetActive(false);
        }

        [ClientCallback]
        private void ShowMenu(MenuKey menuKey)
        {
            if (currentMenu != null)
                currentMenu.SetActive(false);
            currentMenu = menus[menuKey];
            currentMenu.SetActive(true);
        }

        private void HideAll()
        {
            foreach (var menu in menus.Values)
                menu.SetActive(false);
        }
        


        private void OnToClientMatchMessage(ToClientMatchMessage msg)
        {
             switch (msg.Op)
             {
                 case ClientMatchOperation.MatchFound:
                 {
                     StopSearch();
                     ShowMenu(MenuKey.Lobby);
                     lobby.SetPlayers(msg.PlayerInfos);
                     NetworkClient.Send(new ToServerMatchMessage { Op = ServerMatchOperation.Ready, MatchId = msg.MatchId });
                     break;
                 }
                 case ClientMatchOperation.OpponentJoined:
                 {
                     lobby.SetPlayers(msg.PlayerInfos);
                     break;
                 } 
                 case ClientMatchOperation.Started: 
                 { 
                     HideAll();
                     SceneManager.LoadScene(Utils.Scenes.Round, LoadSceneMode.Additive);
                     break;
                 } 
                 case ClientMatchOperation.Ended: 
                 { 
                     break; 
                 } 
                 case ClientMatchOperation.None: 
                 default: 
                     Debug.LogWarning("Missing ClientMatchOperation"); 
                     break; 
             }
        }

        public void SearchMatch()
        {
            ShowMenu(MenuKey.SearchingMatch);
            StartCoroutine(DoSearchMatch(1));
        }
        
        private IEnumerator DoSearchMatch(float waitTime)
        {
            searchingMatch = true;
            
            while (searchingMatch)
            {
                Debug.Log("Searching for match...");
                NetworkClient.Send(new ToServerMatchMessage { Op = ServerMatchOperation.Search });
                yield return new WaitForSeconds(waitTime);
            }
        }

        public void StopSearch()
        {
            searchingMatch = false;
        }
        
        
        
        
        private void OnToServerMatchMessage(NetworkConnectionToClient conn, ToServerMatchMessage msg)
        {
            switch (msg.Op)
            {
                case ServerMatchOperation.Search:
                {
                    GetMatch(conn);
                    break;
                }
                case ServerMatchOperation.Ready:
                {
                    MatchMaker.Instance.SetReady(conn, msg.MatchId);
                    break;
                }
                case ServerMatchOperation.None: 
                default: 
                    Debug.LogWarning("Missing ClientMatchOperation"); 
                    break; 
            }
        }

        private void GetMatch(NetworkConnectionToClient conn)
        {
            var found = MatchMaker.Instance.SearchGame(conn, out var match, out var players);
        
            if (found)
            {
                conn.Send(new ToClientMatchMessage { Op = ClientMatchOperation.MatchFound, MatchId = match.matchId, PlayerInfos = players });
                
                if (match.players == 2)
                {
                    var otherConn = MatchMaker.Instance.OtherPlayerConn(match.matchId, conn);
                    otherConn.Send(new ToClientMatchMessage { Op = ClientMatchOperation.OpponentJoined, MatchId = match.matchId, PlayerInfos = players });
                }

                Debug.Log($"Match found: {match.matchId}");
            }
        }
    }
}