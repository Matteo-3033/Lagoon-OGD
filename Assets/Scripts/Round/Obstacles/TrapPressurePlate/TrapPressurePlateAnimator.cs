using UnityEngine;

namespace Round.Obstacles.TrapPressurePlate
{
    public class TrapPressurePlateAnimator: MonoBehaviour
    {
        private static readonly int PressedParameter = Animator.StringToHash("Pressed");
        
        private Animator animator;

        private void Awake()
        {
            animator = gameObject.GetComponent<Animator>();

            var trapPressurePlate = gameObject.GetComponent<TrapPressurePlate>();
            trapPressurePlate.OnStateChanged += OnStateChanged;
        }

        private void OnStateChanged(object sender, bool pressed)
        {
            animator.SetBool(PressedParameter, pressed);
        }
    }
}