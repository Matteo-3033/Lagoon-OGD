using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragBehaviour : MovementBehaviour
{
    public float deacceleration;

    public override Vector3 GetVector(MovementStatus status)
    {
        float t = Time.fixedDeltaTime;
        if (status.inputDirection.magnitude <= .1f)
        {
            if (status.currentSpeed > deacceleration * t)
            {
                return - status.currentMovement.normalized * deacceleration;
            }
        }

        return Vector3.zero;
    }
}
