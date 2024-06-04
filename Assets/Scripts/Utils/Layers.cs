using UnityEngine;

namespace Utils
{
    public static class Layers
    {
        public static int Default => LayerMask.NameToLayer("Default");
        public static int FieldOfView => LayerMask.NameToLayer("FieldOfView");
        public static int BehindFieldOfView => LayerMask.NameToLayer("Behind-FieldOfView");
        public static int Minimap => LayerMask.NameToLayer("Minimap");
        public static int BehindMinimap => LayerMask.NameToLayer("Minimap-BehindFieldOfView");
        public static int Interactable => LayerMask.NameToLayer("Interactable");
        
        public static void SetLayerRecursively(GameObject obj, int layer)
        {
            if (obj == null)
                return;
       
            obj.layer = layer;
       
            foreach (Transform child in obj.transform)
            {
                if (child == null)
                    continue;
                SetLayerRecursively(child.gameObject, layer);
            }
        }
    }
}