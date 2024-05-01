using Mirror;
using UnityEngine;

namespace Interaction
{
    [RequireComponent(typeof(Animator)), RequireComponent(typeof(NetworkIdentity))]
    public class DoorInteractable : NetworkBehaviour, IInteractable
    {
        [SerializeField] private bool defaultOpen;
        [SerializeField] private GameObject invisibleWall;
        
        public string InteractionPrompt => "Door";

        [SyncVar(hook = nameof(OnStateChanged))]
        private bool open;

        private Animator animator;
        private static readonly int Open = Animator.StringToHash("Open");

        public override void OnStartServer()
        {
            base.OnStartServer();
            open = defaultOpen;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            
            animator = GetComponent<Animator>();
        }

        public bool StartInteraction(Interactor interactor)
        {
            CmdInteract();
            return true;
        }

        [Command(requiresAuthority = false)]
        private void CmdInteract()
        {
            open = !open;
        }

        private void OnStateChanged(bool oldValue, bool newValue)
        {
            invisibleWall.SetActive(!newValue);
            animator.SetBool(Open, newValue);
        }
    }
}