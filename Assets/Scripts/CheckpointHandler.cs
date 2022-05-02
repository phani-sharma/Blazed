using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CheckpointHandler : MonoBehaviour
{
    public List<Checkpoint> checkpointList;
    private int checkPointIndexList;

    public int lapNumber = 0;
    public int maxLap = 3;




    private void Start()
    {
        foreach (Checkpoint c in checkpointList)
        {
            c.InitCheckPoint(this);
        }
        
    }
    

    public bool PlayerPassedCheckpoint(Checkpoint checkpoint)
    {
        //if (!PhotonNetwork.LocalPlayer.IsLocal) return;
        if (checkPointIndexList.Equals(checkpointList.IndexOf(checkpoint)))
        {
            Debug.Log("Correct:  " + checkpointList.IndexOf(checkpoint));
            checkPointIndexList = checkPointIndexList==checkpointList.Count-1?0: checkPointIndexList+ 1;
            if (checkpointList.IndexOf(checkpoint).Equals(0))
            {
                
                Debug.Log("Lap ++");
                lapNumber++;
                if(lapNumber>1)
                RaceManager.Instance.recordLaptimer();
                if (lapNumber > maxLap)
                {
                    //Game Over here for this player
                    RaceManager.Instance.Gameover();
                }
                //checkPointIndexList[playerindex] = 0;
            }
            return true;
        }
        else
        {
            Debug.Log("Wrong");
            return false;
        }


    }

    public Transform GetNextPoint(Checkpoint currentCheckpoint)
    {
        int temp = checkpointList.IndexOf(currentCheckpoint) == checkpointList.Count - 1 ? 0: checkpointList.IndexOf(currentCheckpoint) + 1;
        return checkpointList[temp].gameObject.transform;
    }
    public int GetCheckpointIndex(Checkpoint cp) => checkpointList.IndexOf(cp);

    public void SetFirstCheckpoint()
    {
        foreach(var t in RaceManager.Instance.playerInformation)
        {
            
            t.SetResetPoint(checkpointList[checkpointList.Count - 1].gameObject, checkpointList[0].gameObject, lapNumber, 0);
            t.SetStaticData(maxLap, checkpointList.Count - 1);
        }
    }

}
