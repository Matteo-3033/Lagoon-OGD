using System.Collections;
using MasterServerToolkit.MasterServer;
using MasterServerToolkit.Networking;
using Network.Master;
using Network.Messages;
using UnityEngine;

namespace Network.Client
{
    public class MatchmakingBehaviour: MasterServerToolkit.Bridges.MatchmakingBehaviour
    {
        [SerializeField] private float searchMatchInterval = 5F;
        
        public new static MatchmakingBehaviour Instance => MasterServerToolkit.Bridges.MatchmakingBehaviour.Instance as MatchmakingBehaviour;
        
        private bool searchingMatch;

        protected override void Awake()
        {
            if (Mst.Server.Spawners.IsSpawnedProccess)
            {
                Destroy(gameObject);
                return;
            }
            
            base.Awake();
        }

        public void SearchMatch(int rounds)
        {
            StartCoroutine(DoSearchMatch(rounds));
        }
        
        private IEnumerator DoSearchMatch(int rounds)
        {
            searchingMatch = true;
            
            while (searchingMatch)
            {
                Debug.Log("Searching for match...");
                SendGetMatchRequest(rounds);
                yield return new WaitForSeconds(searchMatchInterval);
            }
        }

        private void SendGetMatchRequest(int rounds)
        {
            var matchRequestPacket = new MatchRequestPacket { RoundsCnt = rounds };

            Connection.SendMessage(OpCodes.GetMatch, matchRequestPacket, (status, response) =>
            {
                if (status != ResponseStatus.Success)
                {
                    Debug.Log("Failed to get match: " + status);
                    return;
                }
                
                Debug.Log("Match found");
                
                StopSearch();
                var access = response.AsPacket<RoomAccessPacket>();
                Mst.Client.Rooms.TriggerAccessReceivedEvent(access);
            });
        }

        public void StopSearch()
        {
            searchingMatch = false;
        }
    }
}