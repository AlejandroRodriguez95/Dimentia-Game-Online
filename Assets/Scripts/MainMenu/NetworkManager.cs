using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    #region Serialized Fields
    [SerializeField] GameObject findGamePanel;
    [SerializeField] GameObject connectingPanel;
    #endregion


    #region PhotonCallBacks
    //public override void OnJoinedRoom()
    //{
    //    int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
    //    Debug.Log($"Joined room. total players in room: {playerCount}");
    //}

    public override void OnCreatedRoom()
    {
        SceneManager.LoadScene("WaitingRoom");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log($"Connected to master server");
        JoinRandomRoom();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log($"Joined main lobby");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        PhotonNetwork.CreateRoom(null, new RoomOptions());
    }


    #endregion


    #region Custom Methods
    public void SetPlayerName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            if (PhotonNetwork.IsMasterClient && PhotonNetwork.IsConnected)
                PhotonNetwork.NickName = "Player 1";
            else
                PhotonNetwork.NickName = "Player 2";
        }

        else
            PhotonNetwork.NickName = name;

        Debug.Log($"Player name set to: {PhotonNetwork.NickName}");
    }
    public void ConnectToServer()
    {
        if (PhotonNetwork.IsConnected)
            return;

        if (string.IsNullOrEmpty(PhotonNetwork.NickName))
            PhotonNetwork.NickName = "Anonymous player";

        findGamePanel.SetActive(false);
        connectingPanel.SetActive(true);
        
        PhotonNetwork.ConnectUsingSettings();

        if (PhotonNetwork.IsConnected)
            Debug.Log("Successfully connected to server");
    }

    private void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    #endregion


    #region Unity Callbacks

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    #endregion
}
