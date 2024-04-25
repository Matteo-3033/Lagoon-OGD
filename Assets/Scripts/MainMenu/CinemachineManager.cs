using System;
using Cinemachine;
using UnityEngine;

namespace MainMenu
{
    public class CinemachineManager: MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera connectionMenuCinemachine;
        [SerializeField] private CinemachineVirtualCamera mainMenuCinemachine;

        private void Awake()
        {
            ShowConnectionMenu();
        }

        public void ShowMainMenu()
        {
            connectionMenuCinemachine.gameObject.SetActive(false);
            mainMenuCinemachine.gameObject.SetActive(true);
        }

        public void ShowConnectionMenu()
        {
            connectionMenuCinemachine.gameObject.SetActive(true);
            mainMenuCinemachine.gameObject.SetActive(false);
        }
    }
}