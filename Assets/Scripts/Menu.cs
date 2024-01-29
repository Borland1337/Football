using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviourPunCallbacks
{
    [SerializeField] private TextMeshProUGUI TextSearchGame;
    [SerializeField] private Button[] MenuButtons;
    [SerializeField] private GameObject Connecttomaster;
    [SerializeField] private TextMeshProUGUI Log;
    private bool isSearch;
    private float serchTime;
    private string RoomName;

    private void Start()
    {
        Connecttomaster.SetActive(true);
        Log.text = "�����������...";
        PhotonNetwork.LocalPlayer.NickName = "�����" + Random.Range(0, 55);
        PhotonNetwork.AutomaticallySyncScene = true;
        foreach (Button item in MenuButtons)
        {
            item.interactable = false;
        }
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Connecttomaster.SetActive(false);
        PhotonNetwork.JoinLobby();
        DisplayButtons();
    }

    private void DisplayButtons()
    {
        foreach (Button item in MenuButtons)
        {
            item.interactable = true;
        }
    }

    public void CreateRoom()
    {
        isSearch = false;
        RoomOptions roomOptions = new RoomOptions()
        {
            MaxPlayers = 2
        };
        RoomName = "������� " + Random.Range(0, 500);
        PhotonNetwork.CreateRoom(RoomName, roomOptions);
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        Connecttomaster.SetActive(true);
        Log.text = "������� �������. �������� ������� ������";
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Connecttomaster.SetActive(true);
        Log.text = "������ �������� �������. ������������� ����";
    }

    public void JoinRoomRandom()
    {
        if (PhotonNetwork.InRoom)
        {
            return;
        }
        isSearch = !isSearch;
        if (isSearch)
        {
            StartCoroutine(JoinRoomCoroutine());
        }
        else
        {
            StopCoroutine(JoinRoomCoroutine());
            serchTime = 0;
            TextSearchGame.text = string.Empty;
        }
    }

    private IEnumerator JoinRoomCoroutine()
    {
        while (!PhotonNetwork.InRoom)
        {
            yield return new WaitForSecondsRealtime(1f);
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnJoinedRoom()
    {
        isSearch = false;
        base.OnJoinedRoom();
        isSearch = false;
        serchTime = 0;
        TextSearchGame.text = string.Empty;
        Connecttomaster.SetActive(true);
        Log.text = "������������ � �������. �������� ������� ������";
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        if (isSearch)
        {
            return;
        }
        base.OnJoinRandomFailed(returnCode, message);
        Connecttomaster.SetActive(true);
        Log.text = "������ ����������� � �������. ������������� ����";
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        StartCoroutine(ChangeLevel(3f));
    }

    private void Update()
    {
        if (isSearch)
        {
            serchTime += Time.deltaTime;
            TextSearchGame.text = "����� �������.. " + (int)serchTime + " ������ ������";
        }
    }

    public void ApplicationQuit()
    {
        Application.Quit();
    }

    private IEnumerator ChangeLevel(float value)
    {
        yield return new WaitForSecondsRealtime(value);
        PhotonNetwork.LoadLevel(1);
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Connecttomaster.SetActive(true);
        Log.text = "������ " + cause.ToString() + " ������������� ����";
    }
}