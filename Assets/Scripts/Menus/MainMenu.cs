using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject langingPagePanel = null;

    public void HostLobby()
    {
        langingPagePanel.SetActive(false);

        NetworkManager.singleton.StartHost();
    }

}
