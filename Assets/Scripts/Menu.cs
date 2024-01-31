using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviourPunCallbacks, IMatchmakingCallbacks
{
    [SerializeField] private TextMeshProUGUI TextSearchGame;
    [SerializeField] private Button[] MenuButtons;
    [SerializeField] private GameObject Connecttomaster;
    [SerializeField] private TextMeshProUGUI Log;
    [SerializeField] private TMP_InputField NicknamePanel;
    [SerializeField] private DataBase dataBase;

    private bool isSearch;
    private float serchTime;
    private string RoomName;


    private void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            Connecttomaster.SetActive(false);
            return;
        }
        Connecttomaster.SetActive(true);
        Log.text = "Подключение...";
        PhotonNetwork.AutomaticallySyncScene = true;
        foreach (Button item in MenuButtons)
        {
            item.interactable = false;
        }
        if (NicknamePanel.text == string.Empty)
        {
            PhotonNetwork.LocalPlayer.NickName = "Игрок" + Random.Range(0, 55);
        }
        else
        {
            PhotonNetwork.LocalPlayer.NickName = NicknamePanel.text;
        }
        PhotonNetwork.ConnectUsingSettings();
    }

    public void EnterNickname()
    {
        PhotonNetwork.LocalPlayer.NickName = NicknamePanel.text;
    }

    public override void OnConnectedToMaster()
    {
        Connecttomaster.SetActive(false);
        PhotonNetwork.JoinLobby();
        DisplayButtons();
        dataBase.LoadQuesitons(true);
        dataBase.LoadQuesitons(false);
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
        RoomName = "Комната " + Random.Range(0, 500);

        RoomOptions roomOptions = new RoomOptions() { MaxPlayers = 2, PublishUserId = true };
        PhotonNetwork.CreateRoom(RoomName, roomOptions, TypedLobby.Default);

    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        Connecttomaster.SetActive(true);
        Log.text = "Комната создана. Ожидайте другого игрока";
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Connecttomaster.SetActive(true);
        Log.text = "Ошибка создание комнаты. Перезапустите игру";
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
        Log.text = "Подключились к комнате. Ожидайте другого игрока";
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        if (isSearch)
        {
            return;
        }
        base.OnJoinRandomFailed(returnCode, message);
        Connecttomaster.SetActive(true);
        Log.text = "Ошибка подключение к комнате. Перезапустите игру";
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
            TextSearchGame.text = "Поиск комнаты.. " + (int)serchTime + " секунд прошло";
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
        Log.text = "Ошибка " + cause.ToString() + " Перезапустите игру";
    }
}