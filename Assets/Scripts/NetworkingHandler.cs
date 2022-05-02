using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class NetworkingHandler : MonoBehaviourPunCallbacks
{
    [Header("AllMainPanel")]
    [SerializeField] GameObject Creat_joinRoomPanel, RoomPanel;

    [SerializeField] GameObject MultiplayerMenuPanel, LoadingPanel,MainMenuPanel,SettingPanel;
    [SerializeField] InputField CreateRoomName, JoinRoomName,username;
    [SerializeField] Button StartMultiplayer;

    [Header("Silder for Room Details")]
    [SerializeField] GameObject sliderprefab, sliderparent; 

    [Space]
    [Header("Lobby Details")]
    [SerializeField] bool IslobbyReq;
    [SerializeField] GameObject LobbyPanel;
    [Range(2,4)]
    [SerializeField] int MaxPlayerinLobby;
    [SerializeField] Button Startbtn;
    [Header("Silder for Room Details")]
    [SerializeField] GameObject Playernameprefab, Playernameparent;
    List<GameObject> playerListTxt;
    string playername;

    [Header("Loading Scene")]
    public int LoadScene_idx;
    private void Start()
    {        
        MainMenuPanel.SetActive(true);
        SettingPanel.SetActive(false);
        MultiplayerMenuPanel.SetActive(false);
        LoadingPanel.SetActive(false);
        if (username.text == "")
        {
            StartMultiplayer.interactable = false;
        }       
    }
    public void Checkval(Text val)
    {        
         StartMultiplayer.interactable = val.text != "";        
    }
    public void MultiplayerStart()
    {
        playerListTxt = new List<GameObject>();
        playername = username.text;
        Debug.Log(playername);
        MainMenuPanel.SetActive(false);
        MultiplayerMenuPanel.SetActive(false);
        LoadingPanel.SetActive(true);
        
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.SerializationRate = 20;
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log($" connnected to {PhotonNetwork.Server}");       
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        MultiplayerMenuPanel.SetActive(true);
        LoadingPanel.SetActive(false);
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {

        foreach (var room in roomList)
        {
            var t = Instantiate(sliderprefab, sliderparent.transform);
            t.GetComponentInChildren<Text>().text = room.Name;
            t.GetComponent<Button>().onClick.AddListener(() => JoinRoom(t.GetComponentInChildren<Text>().text));
        }
    }

    public void CreateRoom()
    {
        RoomOptions room=new RoomOptions();
        room.MaxPlayers = (byte)MaxPlayerinLobby;
        room.PublishUserId = true;        
        if (CreateRoomName.text.Length > 0)
            PhotonNetwork.CreateRoom(CreateRoomName.text,room);
    }
    public void JoinRoom()
    {   

        PhotonNetwork.LocalPlayer.NickName = playername;
        if (JoinRoomName.text.Length > 0)
            PhotonNetwork.JoinRoom(JoinRoomName.text);
    }
    public void JoinRoom(string name)
    {
        PhotonNetwork.LocalPlayer.NickName =  playername;
        Debug.Log("Calling Room");
        PhotonNetwork.JoinRoom(name);
    }
    public override void OnCreatedRoom()
    {
        Debug.Log("Created Room Successfully");
        Debug.Log($"Max Player:{PhotonNetwork.CurrentRoom.MaxPlayers}");
    }
    public override void OnJoinedRoom()
    {

        PhotonNetwork.LocalPlayer.NickName = playername;
        Debug.Log("Joined Room Successfully");
        Debug.Log(PhotonNetwork.CurrentRoom.PlayerCount);
        if (!IslobbyReq)
        {
        if (PhotonNetwork.CurrentRoom.PlayerCount != PhotonNetwork.CurrentRoom.MaxPlayers)        
        {
            LoadingPanel.SetActive(true);
            MultiplayerMenuPanel.SetActive(false);
            LoadingPanel.GetComponentInChildren<Text>().text = "Waiting for players";
        }
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            PhotonNetwork.LoadLevel(LoadScene_idx);
        }
        }
        else
        {
            RoomPanel.SetActive(false);
            Creat_joinRoomPanel.SetActive(false);
            LobbyPanel.SetActive(true);
            Startbtn.gameObject.SetActive(PhotonNetwork.IsMasterClient);
            Startbtn.interactable = PhotonNetwork.CurrentRoom.PlayerCount >= 2;
            foreach(var Playername in PhotonNetwork.CurrentRoom.Players)
            {
                //var t = Instantiate(Playernameprefab, Playernameparent.transform);
                //t.GetComponent<Text>().text = Playername.Value.UserId;
                //playerListTxt.Add(t);
                InstantiatePlayerNameCard(Playername.Value.NickName);
            }
        }
    }
    void InstantiatePlayerNameCard(string playerName)
    {
        var t = Instantiate(Playernameprefab, Playernameparent.transform);
        t.GetComponent<Text>().text = playerName;
        playerListTxt.Add(t);
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Check");
        if (!IslobbyReq)
        {
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            PhotonNetwork.LoadLevel(LoadScene_idx);
        }
        }
        else
        {
            if(PhotonNetwork.IsMasterClient)
            Startbtn.interactable = PhotonNetwork.CurrentRoom.PlayerCount >= 2;
            Debug.Log($"{newPlayer.NickName}joined");
            InstantiatePlayerNameCard(newPlayer.NickName);
        }
    }
    public void StartTheGame()
    {
        PhotonNetwork.LoadLevel(LoadScene_idx);
    }
    //Disconnect Leave Room
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        RoomPanel.SetActive(false);
        LobbyPanel.SetActive(false);
        Creat_joinRoomPanel.SetActive(true);
        foreach (var ti in playerListTxt)
            Destroy(ti);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        Startbtn.interactable = PhotonNetwork.CurrentRoom.PlayerCount >= 2;
        foreach (var t in playerListTxt)
        {
            if (t.GetComponent<Text>().text == otherPlayer.UserId)
            {
                Destroy(t);
            }
        }
    }
    public void backtoMainMenu()
    {
        MainMenuPanel.SetActive(true);
        SettingPanel.SetActive(false);
        MultiplayerMenuPanel.SetActive(false);
        
    }
    public void EXit()
    {
        Application.Quit();
    }
}

