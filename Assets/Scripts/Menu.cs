using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class Menu : MonoBehaviourPunCallbacks
{
    [Header("Screens")]
    public GameObject mainScreen;
    public GameObject lobbyScreen;

    [Header("Main Screen")]
    public Button createRoomButton;
    public Button joinRoomButton;

    [Header("Lobby Screen")]
    public TextMeshProUGUI playerListText;
    public Button startGameButton;

    void Start () 
    {
        // disables the buttons on start up, since we are not yet connected to the server
        createRoomButton.interactable = false;
        joinRoomButton.interactable = false;
    }

    public override void OnConnectedToMaster()
    {
        // enables the buttons, since are already connected to the server
        createRoomButton.interactable = true;
        joinRoomButton.interactable = true;
    }

    void SetScreen (GameObject screen)
    {
        // deactivates all the screens
        mainScreen.SetActive(false);
        lobbyScreen.SetActive(false);

        // enables the requested screen
        screen.SetActive(true);
    }

    // called when the create room button is pressed
    public void OnCreateRoomButton (TMP_InputField roomNameInput)
    {
        NetworkManager.instance.CreateRoom(roomNameInput.text);
    }

    // called when the join room button is pressed
    public void OnJoinRoomButton (TMP_InputField roomNameInput)
    {
        NetworkManager.instance.JoinRoom(roomNameInput.text);
    }

    // called when the player name is updated
    public void OnPlayerNameUpdate (TMP_InputField playerNameInput)
    {
        PhotonNetwork.NickName = playerNameInput.text;
    }

    // called when the player joins the room
    public override void OnJoinedRoom()
    {
        SetScreen(lobbyScreen); // changes the active menu screen to the lobby one

        photonView.RPC("UpdateLobbyUI", RpcTarget.All); // since there is a player on the lobby, tell everyone to update the lobby screen
    }

    // called when the player leaves the room
    public override void OnPlayerLeftRoom (Player otherPlayer)
    {
        // we don't run PUN RPC here like we did in join the lobby because OnPlayerLeftRoom gets called for all the clients in the room, and not just for the one who joined
        UpdateLobbyUI();
    }

    // updates the lobby UI to show the player list and host options
    [PunRPC]
    public void UpdateLobbyUI ()
    {
        playerListText.text = "";

        // displays all the players currently in the lobby
        foreach(Player player in PhotonNetwork.PlayerList)
        {
            playerListText.text += player.NickName + "\n"; // adds a new line after each one
        }

        // only the host can start the game
        if(PhotonNetwork.IsMasterClient)
            startGameButton.interactable = true;
        else
            startGameButton.interactable = false;
    }

    // called when the player clicks the leave lobby button
    public void OnLeaveLobbyButton ()
    {
        PhotonNetwork.LeaveRoom();
        SetScreen(mainScreen);
    }

    // called when the player clicks the start game button
    public void OnStartGameButton ()
    {
        NetworkManager.instance.photonView.RPC("ChangeScene", RpcTarget.All, "Game");
    }
}
