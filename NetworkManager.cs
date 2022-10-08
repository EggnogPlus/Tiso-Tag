using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button playButton;

    // Start is called before the first frame update
    void Start()
    {
        Connect();
        SetInputsEnabled(false);

        // Restore player name
        nameInput.text = PlayerPrefs.GetString("playername");

        // Config Photon
        PhotonNetwork.AutomaticallySyncScene = true;
        //follows host to new scenes if I want to expand

        SetStatus("");
    }

    private void SetInputsEnabled(bool enabled)
    {
        nameInput.interactable = enabled;
        playButton.interactable = enabled;
    }

    private void SetStatus(string message)
    {
        statusText.text = message;
    }

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public void Connect()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public void Play() // asign this to public match button (and change random) and asign a different thing for private 1v1 room when/if we ge to it
    {

        if (string.IsNullOrEmpty(nameInput.text))
        {
            SetStatus("Please enter a name");
            return;
        }

        // set player nickname
        PhotonNetwork.NickName = nameInput.text;

        // store name for next run
        PlayerPrefs.SetString("playername", nameInput.text);

        // join random room
        PhotonNetwork.JoinRandomRoom();
    }


    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Tried to join a room and failed");
        //most likely because there is no room
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 20 }); //adjust for maximum amount as 20 people maximum for ALL ROOMS ### 'null' is room name btw or something idk
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("joined a room- yay!");
        if (PhotonNetwork.IsMasterClient) //if we're host
        {
            PhotonNetwork.LoadLevel(1);
        }
    }

    public override void OnConnectedToMaster()
    {
        SetStatus("Connected to master");

        SetInputsEnabled(true);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        SetStatus($"Disconnected: {cause}");
    }



    // Update is called once per frame
    void Update()
    {
        
    }

    

}
