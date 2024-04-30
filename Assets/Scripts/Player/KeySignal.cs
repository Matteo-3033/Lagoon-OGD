using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeySignal : MonoBehaviour
{
    private Inventory _inventory;
    private MinimapIcon _icon;

    // Start is called before the first frame update
    void Start()
    {
        _inventory = Player.LocalPlayer ? Player.LocalPlayer.Inventory : GetComponent<Inventory>();
        _icon = GetComponentInChildren<MinimapIcon>();
        _inventory.OnKeyFragmentUpdated += OnKeyFragmentUpdated;

        UpdateRippleEffect(_inventory.KeyFragments);
    }

    private void OnKeyFragmentUpdated(object sender, Inventory.OnKeyFragmentUpdatedArgs e)
    {
        UpdateRippleEffect(e.NewValue);
    }

    private void UpdateRippleEffect(int keys)
    {
        if (keys == 0)
        {
            _icon.StopRipple();
            return;
        }
        Debug.Log("Keys: " + keys);
        _icon.ShowRipple(2, 5f / keys, 250, Color.red);
    }
}