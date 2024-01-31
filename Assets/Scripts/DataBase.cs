using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class DataBase : MonoBehaviour
{
    private const string urlMain = "https://docs.google.com/spreadsheets/d/*/export?format=csv";
    [SerializeField] private GameObject Connecttomaster;
    [SerializeField] private TextMeshProUGUI Log;
    [HideInInspector] public List<Qustions> Qustions = new List<Qustions>();
    public List<QustionsAdditional> QustionsAdditional = new List<QustionsAdditional>();

    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    public void LoadQuesitons(bool isMain)
    {
        string actualUrl = string.Empty;
        if (isMain)
        {
            actualUrl = urlMain.Replace("*", "1PMyc7FM0Emte-Kgq_PkcWyss-xuLmusmPqTyo-c55GY");
        }
        else
        {
            actualUrl = urlMain.Replace("*", "1-TAIs3_o6WhYR1WSGH7YUSfXOMAdHi7DdIdxwoyPbsg");
        }
        StartCoroutine(DownloadRawCvsTable(actualUrl, isMain));
    }

    private IEnumerator DownloadRawCvsTable(string actualUrl, bool isMain)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(actualUrl))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError ||
                request.result == UnityWebRequest.Result.DataProcessingError)
            {
                Connecttomaster.SetActive(true);
                Log.text = "Ошибка загрузка дб " + request.error;
            }
            else
            {
                CreateQuestions(request.downloadHandler.text, isMain);
            }

        }
        yield return null;
    }
    private char GetPlatformSpecificLineEnd()
    {
        char lineEnding = '\n';
        return lineEnding;
    }

    private void CreateQuestions(string text, bool isMain)
    {
        char lineEnding = GetPlatformSpecificLineEnd();
        string[] split = text.Split(lineEnding);

        if (isMain)
        {
            for (int b = 0; b < split.Length; b++)
            {
                string[] questions = split[b].Split(',');
                for (int i = 0; i < questions.Length; i++)
                {
                    Qustions Newqustion = new Qustions
                    {
                        Qustion = questions[i],
                        Correct = 0,
                        Variants = new string[4]
                        {
                            questions[i + 1],
                            questions[i + 2],
                            questions[i + 3],
                            questions[i + 4]
                        }
                    };
                    i += 4;
                    Qustions.Add(Newqustion);
                }
            }
        }
        else
        {
            for (int b = 0; b < split.Length; b++)
            {
                string[] questions = split[b].Split(',');
                for (int i = 0; i < questions.Length; i++)
                {
                    QustionsAdditional qustionsAdditional = new QustionsAdditional
                    {
                        Qustion = questions[i],
                        Correct = int.Parse(questions[i+1])
                    };
                    i += 2;
                    QustionsAdditional.Add(qustionsAdditional);
                }
            }
        }
    }
}