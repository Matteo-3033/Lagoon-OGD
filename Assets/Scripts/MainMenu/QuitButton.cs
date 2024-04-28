using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.UI;

namespace MainMenu
{
    public class QuitButton : ChangeFontOnClickButton
    {
        protected override void OnClick()
        {
            Application.Quit();
        }
    }
}