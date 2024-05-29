using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Round.Obstacles.TrapPressurePlate
{
    public class TrapPressurePlateAnimator: MonoBehaviour
    {
        private static readonly int PressedParameter = Animator.StringToHash("Pressed");
        [SerializeField] private UnityEngine.Renderer[] trapRenderers;
        
        private Animator animator;
        private ParticleSystem particles;

        private bool disappearing;

        public event EventHandler<EventArgs> OnDisappearAnimationDone;

        private void Awake()
        {
            //animator = gameObject.GetComponent<Animator>();
            particles = gameObject.GetComponentInChildren<ParticleSystem>(); 

            TrapPressurePlate.OnStateChanged += OnStateChanged;
        }

        private void OnStateChanged(object sender, bool pressed)
        {
            var trapPressurePlate = GetComponent<TrapPressurePlate>();
            if ((TrapPressurePlate) sender != trapPressurePlate)
                return;
            
            //animator.SetBool(PressedParameter, pressed);
            
            if (pressed && !disappearing)
                StartCoroutine(Disappear());
        }

        private IEnumerator Disappear()
        {
            disappearing = true;
            
            var halfDuration = (particles.main.duration + particles.main.startLifetime.constant) / 2;
            var materials = trapRenderers.Select(r => r.material).ToList();
         
            particles.Play();
            
            var elapsedTime = 0F;
            while (elapsedTime < halfDuration)
            {
                elapsedTime += Time.deltaTime;
                
                var alpha = Mathf.Lerp(1, 0, elapsedTime / halfDuration);
                foreach (var m in materials)
                {
                    var color = m.color;
                    color.a = alpha;
                    m.color = color;
                }
                
                yield return null;
            }
            
            yield return new WaitForSeconds(halfDuration);
            OnDisappearAnimationDone?.Invoke(this, EventArgs.Empty);
        }

        private void OnDestroy()
        {
            TrapPressurePlate.OnStateChanged -= OnStateChanged;
        }
    }
}