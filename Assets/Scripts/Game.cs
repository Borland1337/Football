using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviourPunCallbacks
{
    public List<Qustions> Data = new List<Qustions>();
    public List<QustionsAdditional> DataAdditional = new List<QustionsAdditional>();
    [SerializeField] private Transform ContentPage;
    [SerializeField] private List<Player> PlayerList = new List<Player>();
    [SerializeField] private Client Client;
    [SerializeField] private TextMeshProUGUI TimerText;
    [SerializeField] private TextMeshProUGUI GlobalTimerText;
    [SerializeField] private GameObject EndGame;
    [SerializeField] private TextMeshProUGUI EndGameText;
    [SerializeField] private float MathTime = 500f;

    private int CurrentQuestionID;
    private int CurrentQuestionAdditionalID;
    private bool GameStarted = false;
    private float GlobalTimer = 500f;
    private float timer = 15f;
    private int BallPosition = 2;
    private bool AdditionalQuestPause;
    private bool AdditionalQuestUnpause;
    private string FastPlayerAnswer = string.Empty;

    private void Start()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            PlayerList.Add(player);
        }
        string score = PlayerList[0].NickName + " " + PlayerList[0].Score + " : " + PlayerList[1].Score + " " + PlayerList[1].NickName;
        photonView.RPC("UpdateScore", RpcTarget.All, score);
        photonView.RPC("GlobalTimerSync", RpcTarget.All, MathTime);

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

    private void Update()
    {
        Master();
        TimerText.text = "����� �� ����� " + (int)timer + " ������";
        GlobalTimerText.text = "����� �� ����� ����� " + (int)GlobalTimer + " ������";
        if (timer >= 0)
        {
            timer -= Time.deltaTime;
        }
        if (GlobalTimer >= 0)
        {
            GlobalTimer -= Time.deltaTime;
        }
        if (PhotonNetwork.IsMasterClient)
        {
            if (GlobalTimer <= 0)
            {
                GameStarted = false;
                EndGame.SetActive(true);
                if (PlayerList[0].Score == PlayerList[1].Score)
                {
                    EndGameText.text = "�����";
                }
                if (PlayerList[0].Score < PlayerList[1].Score)
                {
                    EndGameText.text = "������ ������ " + PlayerList[1].NickName;
                }
                else
                {
                    EndGameText.text = "������ ������ " + PlayerList[0].NickName;
                }
            }
        }
    }

    [PunRPC]
    public void TimerSync(float time)
    {
        timer = time;
    }
    [PunRPC]
    public void GlobalTimerSync(float time)
    {
        GlobalTimer = time;
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
                if (AdditionalQuestPause)
                {
                    AdditionalQuestUnpause = true;
                    AdditionalQuestPause = false;
                    timer = 15f;
                    return;
                }
                if (AdditionalQuestUnpause)
                {
                    AdditionalQuestionVariants();
                }
                StartGame();
                string score = PlayerList[0].NickName + " " + PlayerList[0].Score + " : " + PlayerList[1].Score + " " + PlayerList[1].NickName;
                photonView.RPC("UpdateScore", RpcTarget.All, score);
                photonView.RPC("TimerSync", RpcTarget.All, 15f);
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
                AdditionalQuestion();
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

            MoveBallAndReset();
        }
    }

    private void MoveBallAndReset()
    {
        BallPositionIsInfinity();

        photonView.RPC("MoveBallClient", RpcTarget.All, BallPosition);

        PlayerList[0].isAnswered = false;
        PlayerList[1].isAnswered = false;

        PlayerList[0].isAnsweredAdditional = false;
        PlayerList[1].isAnsweredAdditional = false;

        PlayerList[0].Additional = 0;
        PlayerList[1].Additional = 0;
    }

    private void BallPositionIsInfinity()
    {
        if (PlayerList.Count == 2)
        {
            if (BallPosition > 4)
            {
                PlayerList[0].Score++;
                photonView.RPC("Announce", RpcTarget.All, PlayerList[0].NickName + " �������� ���!");
                BallPosition = 2;
            }
            if (BallPosition < 0)
            {
                PlayerList[1].Score++;
                photonView.RPC("Announce", RpcTarget.All, PlayerList[1].NickName + " �������� ���!");
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

    [PunRPC]
    public void PlayerAnswerAdditional(int id, string nick)
    {
        if (FastPlayerAnswer == string.Empty)
        {
            FastPlayerAnswer = nick;
        }
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
        player.Additional = id;
        player.isAnsweredAdditional = true;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        EndGame.gameObject.SetActive(true);
        EndGameText.text = "���� �������� " + "����� " + otherPlayer.NickName + " ������� ����";
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        EndGame.SetActive(true);
        EndGameText.text = "������ " + cause.ToString() + " ������������� ����";
    }

    public void LoadLevel()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }

    public void ApplicationQuit()
    {
        Application.Quit();
    }
    private void AdditionalQuestion()
    {
        PlayerList[0].isAnswered = false;
        PlayerList[1].isAnswered = false;
        CurrentQuestionAdditionalID = Random.Range(0, DataAdditional.Count);
        AdditionalQuestPause = true;
        photonView.RPC("Announce", RpcTarget.All, "��� ������ �������� ���������. ��������������� ������!");
        photonView.RPC("TimerSync", RpcTarget.All, 30f);
        photonView.RPC("NextQuestionAdditionalClient", RpcTarget.All, CurrentQuestionAdditionalID);
    }
    private void AdditionalQuestionVariants()
    {
        string answersLog = string.Empty;
        int player1 = PlayerList[0].Additional;
        int player2 = PlayerList[1].Additional;
        int correct = DataAdditional[CurrentQuestionAdditionalID].Correct;
        if (PlayerList[0].isAnsweredAdditional == false)
        {
            answersLog = "�����" + PlayerList[0].NickName + "����������� �� ������. ���� ��������� ������ " + PlayerList[1].NickName;
            Result(true, answersLog);
            return;
        }
        if (PlayerList[1].isAnsweredAdditional == false)
        {
            answersLog = "�����" + PlayerList[1].NickName + "����������� �� ������. ���� ��������� ������ " + PlayerList[0].NickName;
            Result(false, answersLog);
            return;
        }
        if (player1 == correct & player2 == correct)
        {
            if (PlayerList[0].NickName == FastPlayerAnswer)
            {
                answersLog = "�����" + PlayerList[0].NickName + "������ � ������� �������. ���� ��������� ������ " + PlayerList[0].NickName;
                Result(true, answersLog);
                return;
            }
            if (PlayerList[1].NickName == FastPlayerAnswer)
            {
                answersLog = "�����" + PlayerList[1].NickName + "������ � ������� �������. ���� ��������� ������ " + PlayerList[1].NickName;
                Result(false, answersLog);
                return;
            }
        }
        if (player1 == correct)
        {
            answersLog = "���� ��������� ������ " + PlayerList[0].NickName;
            Result(true, answersLog);
            return;
        }
        if (player2 == correct)
        {
            answersLog = "���� ��������� ������ " + PlayerList[1].NickName;
            Result(false, answersLog);
            return;
        }
        int sum1 = correct -= player1;
        int sum2 = correct -= player2;
        if (sum1 < sum2)
        {
            answersLog = "���� ��������� ������ " + PlayerList[0].NickName;
            Result(true, answersLog);
            return;
        }
        else
        {
            answersLog = "���� ��������� ������ " + PlayerList[1].NickName;
            Result(false, answersLog);
            return;
        }
    }
    private void Result(bool firstplayer, string answersLog)
    {
        if (firstplayer)
        {
            BallPosition += 1;
        }
        else
        {
            BallPosition -= 1;
        }
        FastPlayerAnswer = string.Empty;
        MoveBallAndReset();
        photonView.RPC("Announce", RpcTarget.All, answersLog);
        AdditionalQuestUnpause = false;
    }
}
[System.Serializable]
public class Qustions
{
    public string Qustion;
    public string[] Variants;
    public int Correct;
}
[System.Serializable]
public class QustionsAdditional
{
    public string Qustion;
    public int Correct;
}