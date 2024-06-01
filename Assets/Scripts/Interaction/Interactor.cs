using UnityEngine;

namespace Interaction
{
    public class Interactor : MonoBehaviour
    {
        [SerializeField] private Transform _interactionPoint;
        [SerializeField] private float _interactionPointRadius = 0.5f;
        [SerializeField] private LayerMask _interactableMask;
        private readonly Collider[] _colliders = new Collider[3];
        private int _numFound;

        private IInputHandler _inputHandler;
        private GameObject selectedObj;
        private bool interacting = false;

        private void Start()
        {
            var player = GetComponentInParent<Player>();
#if !UNITY_EDITOR
            if (!player.isLocalPlayer)
                return;
#endif

            _inputHandler = player.InputHandler;
            _inputHandler.OnInteract += CheckInteraction;
        }

        private void Update()
        {
            if (_inputHandler == null)
                return;

            _numFound = Physics.OverlapSphereNonAlloc(_interactionPoint.position, _interactionPointRadius, _colliders,
                _interactableMask);

            if (_numFound > 0)
            {
                var obj = _colliders[0].gameObject;
                if (obj != selectedObj)
                {
                    Deselect();
                    Select(obj);
                    ActivateMinimapIcon(obj);
                }
            }
            else
                Deselect();
        }

        private void ActivateMinimapIcon(GameObject obj)
        {
            if (!obj || obj == Player.Opponent?.gameObject || obj == Player.LocalPlayer?.gameObject) return;

            MinimapIcon icon = obj.GetComponentInChildren<MinimapIcon>();
            icon?.Show();
        }

        private void CheckInteraction(object sender, bool pressed)
        {
            if (selectedObj != null && selectedObj.TryGetComponent<IInteractable>(out var interactable))
            {
                if (pressed)
                    interacting = interactable?.StartInteraction(this) ?? false;
                else if (interacting)
                {
                    interactable?.StopInteraction(this);
                    interacting = false;
                }
            }
            else interacting = false;
        }

        private void Select(GameObject obj)
        {
            if (obj.TryGetComponent<ISelectable>(out var selectable))
                selectable.OnSelected();
            selectedObj = obj;
        }

        private void Deselect()
        {
            if (selectedObj)
            {
                if (interacting && selectedObj.TryGetComponent<IInteractable>(out var interactable))
                    interactable.StopInteraction(this);
                if (selectedObj.TryGetComponent<ISelectable>(out var selectable))
                    selectable.OnDeselected();
            }

            interacting = false;
            selectedObj = null;
        }
    }
}