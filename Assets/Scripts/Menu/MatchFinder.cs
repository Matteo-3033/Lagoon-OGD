using System;
using System.Collections;
using Menu.UI;
using Mirror;
using UnityEngine;

namespace MainScene
{
    public class MatchFinder : NetworkBehaviour
    {
        private bool searching;
        
        [SerializeField] public float searchInterval = 1;

        public void SearchMatch ()
        {
            Player.LocalPlayer.OnMatchFound += OnMatchFound;
            StartCoroutine(Search());
        }

        public void StopSearch()
        {
            if (!searching) return;
            Player.LocalPlayer.OnMatchFound -= OnMatchFound;
            searching = false;
        }
        
        private void OnMatchFound(object sender, EventArgs eventArgs) 
        {
            if (!searching) return;
            Player.LocalPlayer.OnMatchFound -= OnMatchFound;
            searching = false;
            UIManager.Instance.ShowMenu(UIManager.MenuKey.Lobby);
        }

        private IEnumerator Search()
        {
            searching = true;

            float currentTime = 1;

            while (searching)
            {
                if (currentTime > 0)
                    currentTime -= Time.deltaTime;
                else
                {
                    Debug.Log("Searching for match...");
                    
                    currentTime = searchInterval;
                    Player.LocalPlayer.CmdSearchGame();
                }
                
                yield return null;
            }
        }
    }
}