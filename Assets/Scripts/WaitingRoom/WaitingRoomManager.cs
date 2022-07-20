using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Realtime;
using TMPro;

public class WaitingRoomManager : MonoBehaviourPunCallbacks
{

    #region Serialized Fields

    [SerializeField] private TextMeshProUGUI player1;
    [SerializeField] private TextMeshProUGUI player2;

    #endregion


    #region Private Variables

    #endregion


    #region Unity Callbacks
        private void Awake()
    {
        player1.text = PhotonNetwork.NickName;
    }

    #endregion


    #region Photon Callbacks
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        player2.text = newPlayer.NickName;

        if (playerCount == 2 && PhotonNetwork.IsMasterClient)
        {
            Debug.Log($"Both players ready and connected. \nStarting game...");

            StartCoroutine(LoadNextScene());
        }
    }

    public override void OnJoinedRoom()
    {
        player2.text = PhotonNetwork.MasterClient.NickName;
    }
    #endregion


    #region Custom Methods
    IEnumerator LoadNextScene()

    {
        yield return new WaitForSeconds(3);
        PhotonNetwork.LoadLevel("Game");
    }
    #endregion
}
