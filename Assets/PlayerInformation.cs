using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;


public class PlayerInformation : MonoBehaviourPun, IPunObservable
{
    public GameObject nextCheckpoint, resetPoint;
    public float distance;
    public int laps, checkpointIndex;
    public float delaydistance, delayCP, delaylap;
    public bool islocal;
    public string DataValue;
    public int MaxLaps, MaxCP;
    public Text SayMyName;

    public List<float> Laptimer;
    public float Besttimer;
    
    //public string playerId;

    private void Start()
    {
        //playerId = PhotonNetwork.LocalPlayer.UserId;
        islocal = base.photonView.IsMine;
        if (islocal)
        {
            base.photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
            StartCoroutine(DelayDistaceCheck());
            SayMyName.text = PhotonNetwork.LocalPlayer.NickName;
            photonView.RPC("setSayMyname", RpcTarget.All, SayMyName.text);
        }
        RaceManager.Instance.AddInoformation(this, islocal);

    }
    private void Update()
    {
        if (!islocal) return;
        if (RaceManager.Instance.RaceStarting && nextCheckpoint == null) return;
        distance = GetDistanceToNextCheckpoint();
        DataValue = $"{MaxLaps - laps}:{MaxCP - checkpointIndex}:{distance}";

    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(distance);
            stream.SendNext(laps);
            stream.SendNext(checkpointIndex);
            stream.SendNext(DataValue);
        }
        else if (stream.IsReading)//read need to check
        {
            distance = (float)stream.ReceiveNext();
            laps = (int)stream.ReceiveNext();
            checkpointIndex = (int)stream.ReceiveNext();
            DataValue = (string)stream.ReceiveNext();
        }
    }
    [PunRPC]
    public void setSayMyname(string name)
    {
        SayMyName.text = name;
    }
    IEnumerator DelayDistaceCheck()
    {
        while (true)
        {
            yield return new WaitForSeconds(2);
            if (islocal && !RaceManager.Instance.RaceStarting && nextCheckpoint != null)
            {
                if (delaydistance < distance)
                {
                    if (delayCP == checkpointIndex)
                        RaceManager.Instance.WrongWay(true);
                }
                else
                    RaceManager.Instance.WrongWay(false);
                delaydistance = GetDistanceToNextCheckpoint() + 5f;
                delayCP = checkpointIndex;
                delaylap = laps;
                //if (DelayDataValue == null)
                //{
                //    DelayDataValue = DataValue;
                //}
                //else
                //{
                //    if (DelayDataValue < DataValue)
                //    {
                //        RaceManager.Instance.WrongWay(true);
                //    }
                //    else
                //    {
                //        RaceManager.Instance.WrongWay(false);
                //    }
                //}
            }
        }
    }
    public void SetStaticData(int maxLap, int maxCheckpoint)
    {
        MaxLaps = maxLap;
        MaxCP = maxCheckpoint;
    }
    public void SetResetPoint(GameObject _nextCheckpoint, GameObject _resetPoint, int _laps, int _checkpointIndex)
    {
        nextCheckpoint = _nextCheckpoint;
        resetPoint = _resetPoint;
        laps = _laps;
        checkpointIndex = _checkpointIndex;

    }
    public void AddLapTimer(float time)
    {
        Laptimer.Add(time);
        foreach(var t in Laptimer)
        {
            if (Besttimer == 0)
            {
                Besttimer = t;
            }
            else
            {
                if (Besttimer > t)
                    Besttimer = t;
            }
        }
        RaceManager.Instance.displayBestLap();

    }

    //float GetTotalDistance() => Vector3.Distance(nextCheckpoint.transform.position,resetPoint.transform.position);
    float GetDistanceToNextCheckpoint() => Vector3.Distance(this.transform.position, nextCheckpoint.transform.position);

}
