using UnityEngine;

[RequireComponent(typeof(Light))]
public class Torchlight : MonoBehaviour
{
    [SerializeField] private FieldOfVIew fieldOfView;
    
    private Light spotLight;
    
    private void Start()
    {
        spotLight = GetComponent<Light>();
        fieldOfView.OnFieldOfViewChanged += FieldOfView_OnFieldOfViewChanged;
    }

    private void FieldOfView_OnFieldOfViewChanged(object sender, FieldOfVIew.FieldOfViewArgs args)
    {
        spotLight.spotAngle = args.FieldOfViewDegree;
        spotLight.innerSpotAngle = args.FieldOfViewDegree / 2;
    }
}
