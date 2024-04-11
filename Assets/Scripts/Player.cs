using Mirror;

public class Player : NetworkBehaviour
{
    public static Player LocalPlayer { get; private set;  }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
     
        if (isLocalPlayer)
            LocalPlayer = this;
    }
}
