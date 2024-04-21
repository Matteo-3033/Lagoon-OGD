using System;
using System.Collections.Generic;
using System.Linq;
using LinqToDB.Tools;
using TrapModifiers;
using UnityEngine;
using UnityEngine.UI;

namespace Round.UI.Main.TrapWheel
{
    public class TrapWheelAnimator: MonoBehaviour
    {
        [SerializeField] private Image selectedTrapImage;
        [SerializeField] private Image previousTrapImage;
        [SerializeField] private Image nextTrapImage;
        [SerializeField] private Image hiddenTrapImage;

        [SerializeField] private Sprite noTrapSprite;
        
        private Animator animator;
        private static readonly int RotateClockwise = Animator.StringToHash("RotateClockwise");
        private static readonly int RotateCounterClockwise = Animator.StringToHash("RotateCounterClockwise");

        private List<Sprite> Sprites = new();

        private void UpdateSpritesList()
        {
            Sprites = Player.LocalPlayer.Inventory.Traps.Select(t => t.icon).ToList();
            Sprites.Insert(0, noTrapSprite);
        }
        
        private int selectedIndex;
        

        private void Awake()
        {
            selectedIndex = 0;
            
            animator = GetComponent<Animator>();

            Player.LocalPlayer.TrapSelector.OnSelectedTrapIndexChanged += OnSelectedTrapIndexChanged;
            Player.LocalPlayer.Inventory.OnTrapsUpdated += OnTrapsUpdated;
            TrapWheelRotation.OnRotationCompleted += OnRotationCompleted;

            UpdateSpritesList();
            UpdateSprites();
        }

        private void OnSelectedTrapIndexChanged(object sender, TrapSelector.OnSelectedTrapIndexChangedArgs args)
        {
            var newIndex = args.Index < 0 ? 0 : args.Index + 1;     // +1 because of the no trap sprite
            if (newIndex == selectedIndex)
                return;

            hiddenTrapImage.sprite = GetSprite(args.IndexIncreased ? newIndex + 1 : newIndex - 1);
            animator.SetTrigger(args.IndexIncreased ? RotateCounterClockwise : RotateClockwise);

            selectedIndex = newIndex;
        }
        
        private void OnRotationCompleted(object sender, EventArgs args)
        {
            UpdateSprites();
        }
        
        private void OnTrapsUpdated(object sender, Inventory.OnTrapsUpdatedArgs args)
        {
            UpdateSpritesList();
            UpdateSprites();
        }

        private void UpdateSprites()
        {
            previousTrapImage.sprite = GetSprite(selectedIndex - 1);
            selectedTrapImage.sprite = GetSprite(selectedIndex);
            nextTrapImage.sprite = GetSprite(selectedIndex + 1);
        }

        private Sprite GetSprite(int index)
        {
            index = (index + Sprites.Count) % Sprites.Count;
            return Sprites[index];
        }
    }
}
