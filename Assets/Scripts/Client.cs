using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    [SerializeField] private PhotonView photonView;
    [SerializeField] private Button[] Buttons;
    [SerializeField] private Game Game;
    [SerializeField] private Transform[] Positions;
    [SerializeField] private Transform Ball;
    [SerializeField] private TextMeshProUGUI Score;
    [SerializeField] private TextMeshProUGUI GolText;
    [SerializeField] private TextMeshProUGUI QuestionText;
    [SerializeField] private TextMeshProUGUI[] Variants;
    [SerializeField] private Button[] VariantButtons;
    [SerializeField] private GameObject Questions;
    [SerializeField] private GameObject QuestionsAdditional;
    [SerializeField] private TMP_InputField CodeField;
    [SerializeField] private TextMeshProUGUI QuestionAdditionalText;
    private int BallPosition;

    private void Start()
    {
        if (photonView == null)
        {
            photonView = GetComponent<PhotonView>();
        }
    }

    public void QuesionVariantID(int variantID)
    {
        photonView.RPC("PlayerAnswer", RpcTarget.All, variantID, PhotonNetwork.LocalPlayer.NickName);
        HideButtons();
    }

    [PunRPC]
    public void PlayerAnswer(bool iscorrect, int id)
    {
        if (iscorrect)
        {
            Variants[id].color = Color.green;
        }
        else
        {
            Variants[id].color = Color.red;
        }
    }

    private void HideButtons()
    {
        foreach (Button item in Buttons)
        {
            item.interactable = false;
        }
    }

    [PunRPC]
    public void MoveBallClient(int newspep)
    {
        Ball.position = Positions[newspep].position;
    }

    [PunRPC]
    private void UpdateScore(string text)
    {
        if (Score != null)
        {
            Score.text = text;
        }
    }

    [PunRPC]
    private void Announce(string text)
    {
        GolText.text = text;
        StartCoroutine(GolAnnouceEnum());
    }

    private IEnumerator GolAnnouceEnum()
    {
        yield return new WaitForSecondsRealtime(3f);
        GolText.text = string.Empty;
    }
    [PunRPC]
    private void NextQuestionClient(int num)
    {
        Questions.SetActive(true);
        QuestionsAdditional.SetActive(false);
        QuestionText.text = Game.Data[num].Qustion;

        foreach (Button item in VariantButtons)
        {
            item.interactable = true;
        }

        Variants[0].text = Game.Data[num].Variants[0];
        Variants[1].text = Game.Data[num].Variants[1];
        Variants[2].text = Game.Data[num].Variants[2];
        Variants[3].text = Game.Data[num].Variants[3];

        Variants[0].color = Color.white;
        Variants[1].color = Color.white;
        Variants[2].color = Color.white;
        Variants[3].color = Color.white;
    }

    [PunRPC]
    public void NextQuestionAdditionalClient(int num)
    {
        Questions.SetActive(false);
        QuestionsAdditional.SetActive(true);
        QuestionAdditionalText.text = Game.DataAdditional[num].Qustion;
    }

    public void EnterCode(int code)
    {
        if (CodeField.text.Length < 4)
        {
            CodeField.text += code;
        }
    }

    public void RemoveCode()
    {
        if (CodeField.text.Length != 0)
        {
            CodeField.text = CodeField.text.Remove(CodeField.text.Length - 1);
        }
    }

    public void SendCode()
    {
        photonView.RPC("PlayerAnswerAdditional", RpcTarget.All, int.Parse(CodeField.text), PhotonNetwork.LocalPlayer.NickName);
        QuestionsAdditional.SetActive(false);
        CodeField.text = string.Empty;
        Announce("Ваш ответ принят");
    }
}