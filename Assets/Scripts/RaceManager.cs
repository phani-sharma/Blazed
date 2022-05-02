using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class RaceManager : MonoBehaviourPun, IPunObservable
{
    public static RaceManager Instance;
    public GameObject PlayerRaceCar;
    public RCC_Camera CarCam;
    public PlayersRacingCarDetails PlayerDetails;
    public bool RaceStarting, RaceEnding;
    public int StartTime = 3;
    public DateTime StopWatch;
    public int CurrentReadyPlayers = 0;
    public List<GameObject> Trap, N0S;

    public List<PlayerInformation> playerInformation;
    public PlayerInformation myinfo;
    public int myTrackPosition;

    public float timer = 0;
    public float laptimer = 0;
    public List<string> playerFinishDetail;


    public GameObject racepanel;
    public List<Text> Sno;
    public List<Text> playernames;
    public List<Text> Bestlapstime;
    public List<Text> Racetime;

    public Text trackPositionText, lapCountText, lapTimeText, Bestlaptime, RaceStatus;
    public Button MainMenu;

    private void Awake()
    {
        Instance = Instance == null ? this : Instance;
    }
    private void Start()
    {
        //PlayerDetails = new PlayersRacingCarDetails();//Temp for checking
        playerFinishDetail = new List<string>();
        racepanel.SetActive(false);
        RaceStarting = true;
        RaceEnding = false;
        if (PhotonNetwork.IsMasterClient)
        {
            PlayerDetails.AssignPos(base.photonView);
            //base.photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
        }
        for(int i = 0; i < 4; i++)
        {
            Sno[i].text = "";
            playernames[i].text = "";
            Bestlapstime[i].text = "";
            Racetime[i].text = "";
        }
        //CarCam.SetTarget(PlayerDetails.MyCar);
    }
    public void AddInoformation(PlayerInformation pi, bool ismine)
    {
        if (ismine)
        {
            myinfo = pi;
        }
        playerInformation.Add(pi);


    }
    [PunRPC]
    public void playerReadyStateToMasterClient()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        CurrentReadyPlayers++;
        if (CurrentReadyPlayers == PhotonNetwork.PlayerList.Length)
            StartCoroutine(raceStarting());
    }
    [PunRPC]
    public void SpawnCar(string id, int idx)
    {
        PlayerDetails.SpawnCars(idx, id, PlayerRaceCar,
            (GameObject go) =>
            {
                Debug.Log("Setting Car " + go.name);
                CarCam.RemoveTarget();
                CarCam.playerCar = go.GetComponent<RCC_CarControllerV3>();
                base.photonView.RPC("playerReadyStateToMasterClient", RpcTarget.MasterClient);
            });
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(RaceStarting);
        }
        else
        if (stream.IsReading)
        {
            RaceStarting = (bool)stream.ReceiveNext();
        }
    }
    IEnumerator raceStarting()
    {

        int timer = StartTime;
        while (timer > -1)
        {
            Debug.Log("check timer" + timer);
            yield return new WaitForSeconds(1f);
            //Visual Rpc Functions
            base.photonView.RPC("RaceStartingVisuals", RpcTarget.All, timer);
            timer--;
        }
        RaceStarting = false;
        photonView.RPC("setAllStaticValues", RpcTarget.All);
    }
    [PunRPC]
    void setAllStaticValues()
    {
        var t = GameObject.FindObjectOfType<CheckpointHandler>();
        t.SetFirstCheckpoint();
        StartCoroutine(Timer());
    }
    IEnumerator delay(Action methode, float time)
    {
        yield return new WaitForSeconds(time);
        methode();
    }
    [PunRPC]
    public void RaceStartingVisuals(int value)
    {
        string Displaytext = value == 0 ? "Go!" : $"{value}";
        Debug.Log("DisplayText:" + Displaytext);
        RaceStatus.text = Displaytext;
        if (value == 0)
            StartCoroutine(delay(() => RaceStatus.gameObject.SetActive(false), 2));
    }
    public void WrongWay(bool val)
    {
        if (RaceStarting) return;
        RaceStatus.text = "Wrong Way";
        RaceStatus.gameObject.SetActive(val);
    }
    private void Update()
    {
        FindMyTrackPosition();
    }
    public void FindMyTrackPosition()
    {
        if (this.playerInformation.Count.Equals(0)) return;
        float temp;
        List<bool> trackpositions = new List<bool>();
        {
           
        }
        var sortbylaps = playerInformation.OrderBy(p => p.DataValue);
        foreach (var t in sortbylaps)
        {
            //Track Position Visual Stuff Here
            if (t.islocal)
            {
                myTrackPosition = sortbylaps.ToList().IndexOf(t) + 1;
                trackPositionText.text = $"{myTrackPosition}/{PhotonNetwork.PlayerList.Length}";
                lapCountText.text = $"{t.laps}/3";
                Debug.Log("Check my position" + myTrackPosition + lapCountText.text);

            }
        }
    }
    [PunRPC]
    public void StartTimer()
    {
        StartCoroutine(Timer());
    }
    IEnumerator Timer()
    {
        StopWatch = new DateTime();
        timer = 0;
        laptimer = 0;
        while (!RaceEnding)
        {
            Debug.Log("Checking the timer");
            yield return new WaitForSeconds(1);
            StopWatch.AddSeconds(1);
            Debug.Log(StopWatch.ToString("mm:ss"));
            timer++;
            laptimer++;
            string min = ((int)timer / 60).ToString("00");
            string sec = (timer % 60).ToString("00");
            lapTimeText.text = min + ":" + sec;
        }

    }
    public void recordLaptimer()
    {
        myinfo.AddLapTimer(laptimer);
        laptimer = 0f;
    }
    public void displayBestLap()
    {
        Bestlaptime.text = $"Best Lap: {((int)myinfo.Besttimer / 60).ToString("00")}: {(myinfo.Besttimer % 60).ToString("00")}";
    }
    [PunRPC]
    public void AddPlayertoFinish(string value)
    {
        playerFinishDetail.Add(value);
        for(int i = 0; i < playerFinishDetail.Count; i++)
        {
            var t = playerFinishDetail[i].Split('-');
            var te = i + 1;
            Sno[i].text =te.ToString();
            playernames[i].text =t[0];
            Bestlapstime[i].text =t[1];
            Racetime[i].text =t[2];
        }
        if (playerFinishDetail.Count < PhotonNetwork.PlayerList.Length)
        {
            MainMenu.interactable = false;
        }
        if(playerFinishDetail.Count == PhotonNetwork.PlayerList.Length){
            MainMenu.interactable = true;
        }
    }

    public void Gameover()
    {
        PlayerDetails.MyCar.SetActive(false);
        RaceEnding = true;
        racepanel.SetActive(true);
        string val = $"{PhotonNetwork.LocalPlayer.NickName}-{((int)myinfo.Besttimer / 60).ToString("00")}:{(myinfo.Besttimer % 60).ToString("00")}-{((int)timer / 60).ToString("00")}: {(timer % 60).ToString("00")}";
        photonView.RPC("AddPlayertoFinish", RpcTarget.AllBufferedViaServer, val);
    }
    public void Exit()
    {
        Application.Quit();
    }
    public void GoToMainMenu()
    { 
        PhotonNetwork.Disconnect();
        PhotonNetwork.LeaveLobby();
        PhotonNetwork.LoadLevel(0);
    } 

}


[System.Serializable]
public class PlayersRacingCarDetails
{

    public GameObject[] StartingPositions;
    public int myposidx;
    //[HideInInspector]
    public GameObject MyCar;


    public void SpawnCars(int posidx, string id, GameObject prefab, Action<GameObject> Setcam)
    {
        if (id == PhotonNetwork.LocalPlayer.UserId)
            if (posidx > -1 && posidx < StartingPositions.Length - 1)
            {
                Debug.Log("instan" + id + " " + posidx);
                if (!DNDOLData.Instance)
                    MyCar = PhotonNetwork.Instantiate(prefab.name, StartingPositions[posidx].transform.position, Quaternion.identity);
                else
                    MyCar = PhotonNetwork.Instantiate(DNDOLData.Instance.MyCar, StartingPositions[posidx].transform.position, Quaternion.identity);

                MyCar.name = id;
                Setcam(MyCar);
            }
    }
    public void AssignPos(PhotonView phv)
    {
        //var t =PhotonNetwork.PlayerList;
        if (PhotonNetwork.IsMasterClient)
        {
            int idx = 0;
            foreach (var t in PhotonNetwork.PlayerList)
            {
                Debug.Log(PhotonNetwork.PlayerList.Length);
                Debug.Log(t.UserId);
                myposidx = idx;
                phv.RPC("SpawnCar", RpcTarget.AllBufferedViaServer, t.UserId, idx);
                idx++;
            }
        }
    }
}

