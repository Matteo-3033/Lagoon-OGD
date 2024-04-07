using MainScene;
using UnityEngine;

namespace Menu.UI.SearchMenu
{
    public class SearchMenu : Menu
    {
        [SerializeField] private MatchFinder matchFinder;
        
        public override void OnFocus()
        {
            base.OnFocus();
            matchFinder.SearchMatch();
        }
        
        public override void OnUnfocus()
        {
            base.OnUnfocus();
            matchFinder.StopSearch();
        }
    }
}