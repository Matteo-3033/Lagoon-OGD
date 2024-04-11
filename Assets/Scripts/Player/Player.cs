using Mirror;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class Player : NetworkBehaviour
    {
        public static Player LocalPlayer { get; private set;  }
        public static Player Opponent { get; private set;  }

        public override void OnStartClient()
        {
            base.OnStartClient();
            
            var identity = gameObject.GetComponent<NetworkIdentity>();
            if (!identity.isLocalPlayer)
                OnStartingOpponent();
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
     
            if (isLocalPlayer)
                LocalPlayer = this;
            
            gameObject.layer = LayerMask.NameToLayer("FieldOfView");
        }
        
        private void OnStartingOpponent()
        {
            Opponent = this;
            
            gameObject.layer = LayerMask.NameToLayer("Behind-FieldOfView");
        }
    }

}
