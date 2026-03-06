using SandboxPlatform.SAMANDA;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrevPlayerInfo : MonoBehaviour
{
    [SerializeField] ServerSelector TestServer;
    [SerializeField] ServerSelector QAServer;
    [SerializeField] ServerSelector LiveServer;

    [SerializeField] GameObject CloneTarget;
    void Start()
    {
        string savedJWT = PlayerPrefs.GetString("JWT_FOR_EDITOR");
        List<string> jwtArray = new List<string>(savedJWT.Split(','));

        CloneTarget.SetActive(true);

        foreach (string info in jwtArray)
        {
            GameObject listItem = Instantiate(CloneTarget);
            listItem.transform.SetParent(CloneTarget.transform.parent);
            listItem.transform.localScale = Vector3.one;

            Text text = listItem.transform.Find("Text").GetComponent<Text>();
            text.text = info;
            string serverFlag = info.Split('/')[0];
            if (serverFlag == "TEST")
            {
                text.color = new Color(0.6470588f, 0.8862746f, 0.5137255f);
            }
            else if(serverFlag == "QA")
            {
                text.color = new Color(0.9450981f, 0.9725491f, 0.0f);
            }
            else
            {
                text.color = new Color(1, 0.427451f, 1);
            }

            listItem.GetComponent<Button>().onClick.AddListener(() => {
                OnSelectInfo(listItem); 
            });

            listItem.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() => {
                OnDeleteInfo(listItem);
            });
        }

        CloneTarget.SetActive(false);
    }

    public void OnSelectInfo(GameObject item)
    {
        Text text = item.transform.Find("Text").GetComponent<Text>();
        string info = text.text;
        string[] data = info.Split('/');

        PlayerPrefs.SetString("account_token", data[1]);
        PlayerPrefs.SetString("account_ano", data[2]);
        string serverFlag = data[0];
        if (serverFlag == "TEST")
        {
            TestServer.OnClick();
        }
        else if(serverFlag == "QA")
        {
            QAServer.OnClick();
        }
        else
        {
            LiveServer.OnClick();
        }
    }

    public void OnDeleteInfo(GameObject item)
    {
        Text text = item.transform.Find("Text").GetComponent<Text>();
        string info = text.text;
        
        string savedJWT = PlayerPrefs.GetString("JWT_FOR_EDITOR");
        List<string> jwtArray = new List<string>(savedJWT.Split(','));
        jwtArray.Remove(info);

        PlayerPrefs.SetString("JWT_FOR_EDITOR", string.Join(",", jwtArray));

        Destroy(item);
    }
}
