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
            Game.GetVariants[id].color = Color.green;
        }
        else
        {
            Game.GetVariants[id].color = Color.red;
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
    private void GolAnnounce(string text)
    {
        GolText.text = text;
        StartCoroutine(GolAnnouceEnum());
    }

    private IEnumerator GolAnnouceEnum()
    {
        yield return new WaitForSecondsRealtime(3f);
        GolText.text = string.Empty;
    }

}