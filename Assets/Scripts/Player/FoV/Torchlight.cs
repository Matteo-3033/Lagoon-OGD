using UnityEngine;

[RequireComponent(typeof(Light))]
public class Torchlight : MonoBehaviour
{
    [SerializeField] private FieldOfView fieldOfView;
    
    private Light spotLight;
    
    private void Start()
    {
        if (!GetComponentInParent<Player>().isLocalPlayer)
            gameObject.SetActive(false);
        else
        {
            spotLight = GetComponent<Light>();
            fieldOfView.OnFieldOfViewChanged += FieldOfView_OnFieldOfViewChanged;   
        }
    }

    private void FieldOfView_OnFieldOfViewChanged(object sender, FieldOfView.FieldOfViewArgs args)
    {
        spotLight.spotAngle = args.FieldOfViewDegree;
        spotLight.innerSpotAngle = args.FieldOfViewDegree / 2;
    }
}
