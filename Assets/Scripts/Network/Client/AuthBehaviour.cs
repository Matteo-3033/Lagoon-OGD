using MasterServerToolkit.MasterServer;
using MasterServerToolkit.Networking;
using UnityEngine;
using UnityEngine.Events;

namespace Network
{
    public class AuthBehaviour : MasterServerToolkit.Bridges.AuthBehaviour
    {
        public new static AuthBehaviour Instance => MasterServerToolkit.Bridges.AuthBehaviour.Instance as AuthBehaviour;

        public UnityEvent OnSignInFailedEvent;

        protected override void Awake()
        {
            if (Mst.Server.Spawners.IsSpawnedProccess)
            {
                Destroy(gameObject);
                return;
            }
            
            base.Awake();
        }

        public void SignIn(string username)
        {
            Debug.Log("Signing in with username: " + username);

            MstTimer.WaitForSeconds(0.1f, () =>
            {
                Mst.Client.Auth.SignInWithLoginAndPassword(username, "no_password", (accountInfo, _) =>
                {
                    if (accountInfo == null)
                        OnSignInFailedEvent.Invoke();
                }, Connection);
            });
        }
    }
}