// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkerInGame.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class WorkerInGame : Photon.MonoBehaviour
{
    public Transform playerPrefab;
    private GameObject newPlayer;
    public List<GameObject> playerList;
    public void Awake()
    {
        // in case we started this demo with the wrong scene being active, simply load the menu scene
        if (!PhotonNetwork.connected)
        {
            SceneManager.LoadScene(WorkerMenu.SceneNameMenu);
            return;
        }

        Vector3 spawnPoint = new Vector3(Random.Range(-2f, 2f), 10, Random.Range(-2f, 2f));

        // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
        newPlayer = PhotonNetwork.Instantiate(this.playerPrefab.name, spawnPoint, Quaternion.identity, 0);
        
    }

    public void Start()
    {

        if (newPlayer.GetComponent<PhotonView>().owner.IsMasterClient)
        {
            if (PlayerControl.gameStartTime <= 0)
            {
                newPlayer.GetComponent<PhotonView>().RPC("EspectadorPos", PhotonTargets.All);
            }
        }
    }

    public void OnGUI()
    {
        if (GUILayout.Button("Return to Lobby"))
        {
            PhotonNetwork.LeaveRoom();  // we will load the menu level when we successfully left the room
        }
    }

    public void OnMasterClientSwitched(PhotonPlayer player)
    {
        Debug.Log("OnMasterClientSwitched: " + player);

        string message;
        InRoomChat chatComponent = GetComponent<InRoomChat>();  // if we find a InRoomChat component, we print out a short message

        if (chatComponent != null)
        {
            // to check if this client is the new master...
            if (player.IsLocal)
            {
                message = "You are Master Client now.";
            }
            else
            {
                message = player.NickName + " is Master Client now.";
            }


            chatComponent.AddLine(message); // the Chat method is a RPC. as we don't want to send an RPC and neither create a PhotonMessageInfo, lets call AddLine()
        }
    }

    public void OnLeftRoom()
    {
        Debug.Log("OnLeftRoom (local)");

        // back to main menu
        SceneManager.LoadScene(WorkerMenu.SceneNameMenu);
    }

    public void OnDisconnectedFromPhoton()
    {
        Debug.Log("OnDisconnectedFromPhoton");

        // back to main menu
        SceneManager.LoadScene(WorkerMenu.SceneNameMenu);
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        Debug.Log("OnPhotonInstantiate " + info.sender);    // you could use this info to store this or react
        
    }

    public void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        Debug.Log("OnPhotonPlayerConnected: " + player);
        newPlayer.GetComponent<PlayerControl>().DelayUI();
    }

    public void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        Debug.Log("OnPlayerDisconneced: " + player);
        //----
        newPlayer.GetComponent<PlayerControl>().DelayUI();
    }

    public void OnFailedToConnectToPhoton()
    {
        Debug.Log("OnFailedToConnectToPhoton");

        // back to main menu
        SceneManager.LoadScene(WorkerMenu.SceneNameMenu);
    }
}