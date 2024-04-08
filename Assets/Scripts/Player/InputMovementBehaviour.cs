using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputMovementBehaviour : MovementBehaviour
{
    public float acceleration;
    
    public override Vector3 GetVector(MovementStatus status)
    {
        return status.inputDirection * acceleration;
    }
}
