using System;
using UnityEngine;
using Utils.UI;


namespace Audio
{
    public class UISoundManager : SoundManager
    {
        [SerializeField] private UIAudioClips audioClips;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            ChangeFontOnClickButton.OnBeforeClick += OnBeforeButtonClick;
            ChangeFontOnClickButton.OnAfterClick += OnAfterButtonClick;
        }

        private void OnBeforeButtonClick(object sender, EventArgs e)
        {
            PlayClipAtPoint(audioClips.beforeButtonClick, Vector3.zero);
        }
        
        private void OnAfterButtonClick(object sender, EventArgs e)
        {
            PlayClipAtPoint(audioClips.afterButtonClick, Vector3.zero);
        }
    }
}