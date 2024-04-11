using UnityEngine;

namespace Utils
{
    [RequireComponent(typeof(RectTransform))]
    public class LoadingSpinner : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 200f;
        
        private RectTransform rect;

        private void Start()
        {
            rect = GetComponent<RectTransform>();
        }

        private void Update()
        {
            rect.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);
        }
    }
}