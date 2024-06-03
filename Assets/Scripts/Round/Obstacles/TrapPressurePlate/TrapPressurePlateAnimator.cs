using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Round.Obstacles.TrapPressurePlate
{
    public class TrapPressurePlateAnimator: MonoBehaviour
    {
        private static readonly int PressedParameter = Animator.StringToHash("Pressed");
        private MeshRenderer[] _trapRenderers;
        
        private Animator _animator;
        private ParticleSystem _particles;

        private bool _disappearing;

        public event EventHandler<EventArgs> OnDisappearAnimationDone;

        private void Awake()
        {
            _animator = gameObject.GetComponentInChildren<Animator>();
            _particles = gameObject.GetComponentInChildren<ParticleSystem>();

            TrapPressurePlate.OnStateChanged += OnStateChanged;
        }

        private void OnStateChanged(object sender, bool pressed)
        {
            var trapPressurePlate = GetComponent<TrapPressurePlate>();
            if ((TrapPressurePlate) sender != trapPressurePlate)
                return;
            
            _animator.SetBool(PressedParameter, pressed);
            
            if (pressed && !_disappearing)
                StartCoroutine(Disappear());
        }

        private IEnumerator Disappear()
        {
            _trapRenderers = GetComponentsInChildren<MeshRenderer>();
            _disappearing = true;
            
            var halfDuration = (_particles.main.duration + _particles.main.startLifetime.constant) / 2;
            var materials = _trapRenderers.Select(r => r.material).ToList();
         
            _particles.Play();
            
            var elapsedTime = 0F;
            while (elapsedTime < halfDuration)
            {
                elapsedTime += Time.deltaTime;
                
                var alpha = Mathf.Lerp(1, 0, elapsedTime / halfDuration);
                foreach (var m in materials)
                {
                    var color = m.color;
                    m.color = new Color(color.r, color.g, color.b, alpha);
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