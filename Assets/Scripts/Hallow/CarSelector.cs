using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public class CarSelector : MonoBehaviourPun, IPunObservable
{
    [SerializeField]
    List<Button> button;
    [SerializeField]
    Button Confirm;
    Button CurrentSelected;


    //List of the Cars
    [SerializeField]
    List<GameObject> Cars;
    //Current Selected car 
    [SerializeField]
    string CurrentCar;
    [SerializeField]
    List<string> CurrentSelectedCars;
    [SerializeField]
    List<string> CurrentSelectedCars2;


    //Stats Images
    [SerializeField]
    List<Sprite> statImages;
    [SerializeField]
    Image selectedImage;
    
    //Timer Variables
    [SerializeField]
    [Range(10f, 60f)]
    float SelectionLobbyTimer = 10f;

    float time;
    bool flag;

    //Player Ready system
    int readyplayers;

    void Start()
    {
        CurrentSelectedCars = new List<string>();
        CurrentSelectedCars2 = new List<string>();
        Confirm.interactable = false;
        readyplayers = 0;
        
        //StartCoroutine
        if (PhotonNetwork.IsMasterClient)
        {
            time = SelectionLobbyTimer;
            StartCoroutine(StartTimer());
        }

    }
    public void SetData()
    {
        CurrentSelectedCars2 = new List<string>(CurrentSelectedCars);
        //CurrentSelectedCars2 = CurrentSelectedCars;
        if (CurrentSelectedCars2.Count > 0)
        {
            foreach (var t in CurrentSelectedCars)
            {
                if (t == CurrentSelected.gameObject.name)
                {
                    //invalid Selections;
                    return;
                }
                else
                {
                    //Valid Selections;
                    //Rpc call here
                    base.photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
                    Debug.Log("Validation done inside if");
                    CurrentSelectedCars2.Add(CurrentCar);
                    Confirm.interactable = false;
                    Confirm.gameObject.SetActive(false);
                    DNDOLData.Instance.MyCar = CurrentCar;
                    base.photonView.RPC("SetInActiveButton", RpcTarget.All, CurrentSelected.gameObject.name);
                    SetAllButtonInActive();
                }
            }
        }
        else
        {
            //Transfer ownership
            base.photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
            Debug.Log("Validation done outside if");
            CurrentSelectedCars2.Add(CurrentCar);
            Confirm.interactable = false;
            Confirm.gameObject.SetActive(false);
            DNDOLData.Instance.MyCar = CurrentCar;
            base.photonView.RPC("SetInActiveButton", RpcTarget.All, CurrentSelected.gameObject.name);
            SetAllButtonInActive();
        }
        //Confirm the Selection and Assign it to the player data
        //Set Button InActive Across the server
    }
    void SetAllButtonInActive()
    {
        foreach (var t in button)
        {
            t.gameObject.SetActive(false);
        }
    }
    [PunRPC]
    public void SetInActiveButton(string btn)
    {
         
        readyplayers++;
        foreach (var t in button)
        {
            if (t.gameObject.name == btn)
            {
                t.interactable = false;
                if (CurrentSelected == t)
                {
                    t.GetComponent<Image>().color = Color.white;
                    CurrentSelected = null;
                }
            }
        }
    }
    private void Update()
    {
        if (readyplayers == PhotonNetwork.PlayerList.Length && !flag && PhotonNetwork.IsMasterClient)
        {
            Debug.Log("ChangeScene)");
            PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().buildIndex + 1);
            flag = true;
        }
    }
    public void SetCurrentSelector(GameObject _button)
    {
        Confirm.interactable = true;
        foreach (var t in button)
            if (t == _button.GetComponent<Button>())
            {
                if (t != CurrentSelected)
                {
                    if (CurrentSelected != null)
                        CurrentSelected.GetComponent<Image>().color = Color.white;
                    CurrentSelected = t;
                    CurrentCar = t.gameObject.name;
                    t.GetComponent<Image>().color = Color.green;

                }
            }
    }
    public void CarDisplay(int idx)
    {
        //Car Model display here

        //CurrentSelected Will be assigned her

        if (!selectedImage.gameObject.activeSelf)
            selectedImage.gameObject.SetActive(true);

        selectedImage.sprite = statImages[idx];



    }
    IEnumerator StartTimer()
    {
        while (time > -1)
        {
            yield return new WaitForSeconds(1f);
            time--;

        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        Debug.Log(info.Sender);
        if (stream.IsWriting)
        {
            stream.SendNext(time);
           
            stream.SendNext(CurrentSelectedCars2.ToArray());
        }
        else
        if (stream.IsReading)
        {
            time = (float)stream.ReceiveNext();
            
            var t = (string[])stream.ReceiveNext();
            CurrentSelectedCars = t.ToList();
            Debug.Log("here stream" + CurrentSelectedCars.Count);
        }
        else
        {
            Debug.Log("NoData Send Or Recived");
        }
    }

}

//Singleton Lib
public class Singleton<T> : MonoBehaviour where T : Component
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            return _instance;
        }
    }
    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }
    public virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this);
        }
    }
}
