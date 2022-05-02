using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Checkpoint : MonoBehaviour
{
    private CheckpointHandler checkpointHandler;


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Player"))
        {
            if (other.GetComponentInParent<PlayerInformation>().islocal)
            {
                if (checkpointHandler.PlayerPassedCheckpoint(this))
                {
                    Debug.Log($"here in checkpoint checking {PhotonNetwork.LocalPlayer.UserId} {checkpointHandler.lapNumber} {checkpointHandler.GetCheckpointIndex(this)}");
                    other.GetComponentInParent<PlayerInformation>().SetResetPoint(checkpointHandler.GetNextPoint(this).gameObject, this.transform.gameObject, checkpointHandler.lapNumber, checkpointHandler.GetCheckpointIndex(this));
                }

            }
        }
    }

    public void InitCheckPoint(CheckpointHandler checkpointHandler)
    {
        this.checkpointHandler = checkpointHandler;
    }

}
