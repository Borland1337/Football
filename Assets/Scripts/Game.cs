using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviourPunCallbacks
{
    [SerializeField] private List<Qustions> Data = new List<Qustions>();
    [SerializeField] private TextMeshProUGUI QuestionText;
    [SerializeField] private TextMeshProUGUI[] Variants;
    [SerializeField] private Button[] VariantButtons;
    [SerializeField] private Transform ContentPage;
    [SerializeField] private List<Player> PlayerList = new List<Player>();
    [SerializeField] private Client Client;
    [SerializeField] private TextMeshProUGUI TimerText;
    [SerializeField] private GameObject EndGame;
    [SerializeField] private TextMeshProUGUI EndGameText;
    public TextMeshProUGUI[] GetVariants => Variants;
    private int CurrentQuestionID;
    private bool GameStarted = false;
    private float timer = 3f;
    private int BallPosition = 2;

    private void Start()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            PlayerList.Add(player);
        }
        string score = PlayerList[0].NickName + " " + PlayerList[0].Score + " : " + PlayerList[1].Score + " " + PlayerList[0].NickName;
        photonView.RPC("UpdateScore", RpcTarget.All, score);

        if (PhotonNetwork.IsMasterClient)
        {
            StartGame();
        }
    }

    private void StartGame()
    {
        CurrentQuestionID = Random.Range(0, Data.Count);
        photonView.RPC("NextQuestionClient", RpcTarget.All, CurrentQuestionID);
        GameStarted = true;
    }

    [PunRPC]
    private void NextQuestionClient(int seed)
    {
        QuestionText.text = Data[seed].Qustion;

        foreach (Button item in VariantButtons)
        {
            item.interactable = true;
        }

        Variants[0].text = Data[seed].Variants[0];
        Variants[1].text = Data[seed].Variants[1];
        Variants[2].text = Data[seed].Variants[2];
        Variants[3].text = Data[seed].Variants[3];

        Variants[0].color = Color.white;
        Variants[1].color = Color.white;
        Variants[2].color = Color.white;
        Variants[3].color = Color.white;
    }

    private void Update()
    {
        Master();
        TimerText.text = "Время на ответ " + (int)timer;
        if (timer >= 0)
        {
            timer -= Time.deltaTime;
        }
    }

    [PunRPC]
    public void Timer(float time)
    {
        timer = time;
    }

    private void Master()
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        if (GameStarted)
        {
            if (timer <= 0)
            {
                MoveBall();
                StartGame();
                string score = PlayerList[0].NickName + " " + PlayerList[0].Score + " : " + PlayerList[1].Score + " " + PlayerList[1].NickName;
                photonView.RPC("UpdateScore", RpcTarget.All, score);
                photonView.RPC("Timer", RpcTarget.All, 10f);
                timer = 10f;
            }
        }
    }

    private void MoveBall()
    {
        if (PlayerList.Count == 2)
        {
            if (PlayerList[0].isAnswered == false & PlayerList[1].isAnswered == false)
            {
                return;
            }
            if (PlayerList[0].isAnswered == true & PlayerList[1].isAnswered == true)
            {
                return;
            }
            if (PlayerList[0].isAnswered == true)
            {
                BallPosition += 1;
            }
            if (PlayerList[1].isAnswered == true)
            {
                BallPosition -= 1;
            }

            BallPositionIsInfinity();

            photonView.RPC("MoveBallClient", RpcTarget.All, BallPosition);

            PlayerList[0].isAnswered = false;
            PlayerList[1].isAnswered = false;
        }
    }

    private void BallPositionIsInfinity()
    {
        if (PlayerList.Count == 2)
        {
            if (BallPosition > 4)
            {
                PlayerList[1].Score++;
                photonView.RPC("GolAnnounce", RpcTarget.All, PlayerList[1].NickName + " забивает гол!");
                BallPosition = 2;
            }
            if (BallPosition < 0)
            {
                PlayerList[0].Score++;
                photonView.RPC("GolAnnounce", RpcTarget.All, PlayerList[0].NickName + " забивает гол!");
                BallPosition = 2;
            }
        }
    }

    [PunRPC]
    public void PlayerAnswer(int id, string nick)
    {
        if (PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        Player player = null;
        foreach (Player item in PlayerList)
        {
            if (item.NickName == nick)
            {
                player = item;
            }
        }
        if (player == null)
        {
            Debug.Log("Bug!!");
            return;
        }
        if (Data[CurrentQuestionID].Correct == id)
        {
            photonView.RPC("PlayerAnswer", player, true, id);
            player.isAnswered = true;
        }
        else
        {
            photonView.RPC("PlayerAnswer", player, false, id);
            player.isAnswered = false;
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        EndGame.gameObject.SetActive(true);
        EndGameText.text = "Матч завершён " + "игрок " + otherPlayer.NickName + " покинул игру";
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        EndGame.SetActive(true);
        EndGameText.text = "Ошибка " + cause.ToString() + " Перезапустите игру";
    }

    public void LoadLevel()
    {
        SceneManager.LoadScene(0);
    }

    public void ApplicationQuit()
    {
        Application.Quit();
    }
}
[System.Serializable]
public class Qustions
{
    public string Qustion;
    public string[] Variants;
    public int Correct;
}