using System.Collections.Generic;
using System.Linq;
using TrapModifiers;
using UnityEngine;
using UnityEngine.UI;

namespace Round.UI.Main
{
    public class TrapWheel: MonoBehaviour
    {
        [SerializeField] private Image selectedTrapImage;
        [SerializeField] private Image previousTrapImage;
        [SerializeField] private Image nextTrapImage;

        [SerializeField] private Sprite noTrapSprite;
        
        private List<TrapModifier> Traps => Player.LocalPlayer.Inventory.Traps.ToList();
        private int selectedIndex = -1;

        private void Awake()
        {
            Player.LocalPlayer.TrapSelector.OnTrapSelected += OnTrapSelected;
            Player.LocalPlayer.Inventory.OnTrapsUpdated += OnTrapsUpdated;
        }
        
        private void OnTrapSelected(object sender, TrapSelector.OnTrapSelectedArgs args)
        {
            selectedIndex = args.Index;
            UpdateSprites(args.Index);
        }
        
        private void OnTrapsUpdated(object sender, Inventory.OnTrapsUpdatedArgs args)
        {
            UpdateSprites(selectedIndex);
        }

        private void UpdateSprites(int index)
        {
            if (index < 0)
            {
                selectedTrapImage.sprite = GetSprite(index);
                previousTrapImage.sprite = GetSprite(Traps.Count - 1);
                nextTrapImage.sprite = GetSprite(0);
            }
            else
            {
                selectedTrapImage.sprite = GetSprite(index);
                previousTrapImage.sprite = GetSprite(index - 1);
                nextTrapImage.sprite = GetSprite(index + 1);
            }
        }

        private Sprite GetSprite(int index)
        {
            if (index < 0 || Traps.Count <= index)
                return noTrapSprite;

            return Traps[index].icon;
        }
    }
}
