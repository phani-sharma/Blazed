using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nitrous : MonoBehaviour
{
    public Renderer nosMesh;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag.Equals("Player"))
        {
            other.GetComponentInParent<RCC_CarControllerV3>().EnableNos();
            nosMesh.enabled = false;
            GetComponent<BoxCollider>().enabled = false;
            Invoke("ReenableDisabled", 3f);
            
        }
    }

    void ReenableDisabled()
    {
        nosMesh.enabled = true;
        GetComponent<BoxCollider>().enabled = true;
    }
}
