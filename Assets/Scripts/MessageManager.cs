using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Collections;

public class MessageManager : MonoBehaviour
{
    [SerializeField] private ApiManager apiManager;
    [SerializeField] private TMP_InputField messageInput;
    [SerializeField] private Button messageButton;
    [SerializeField] public TMP_Text chatText;
    [SerializeField] private GameObject NewMessage;
    [SerializeField] private GameObject NewOwnMessage;
    public GameObject Content;
    public List<GameObject> messageBubbles = new();

    private void OnEnable()
    {
        messageButton.onClick.AddListener(SendMessageAsync);
    }

    private void OnDisable()
    {
        messageButton.onClick.RemoveListener(SendMessageAsync);
    }

    public IEnumerator MessageHistoryHandler(string text)
    {
        List<string> messageList = new List<string>();
        List<string> clearmessageList = new List<string>();
        List<string> data = new List<string>(string.IsNullOrEmpty(text) == true ? apiManager.chatHistory.Split(";") : text.Split(";"));
        foreach(var message in data)
        {
            if (message.Contains(":date"))
            {
                messageList.Add(message);
            }
        } 
        foreach (var entry in messageList)
        {
            var parts = entry.Split(new string[] { ":date" }, StringSplitOptions.None);
            clearmessageList.Add(parts[1]);
        }

        foreach (var message in clearmessageList)
        {
            if (message.Split(":")[0].Contains(apiManager.playerName)) // hacky method since we don't currently send playerid with message
            {
                StartCoroutine(GenerateBubble(NewOwnMessage, message));
                yield return new WaitForEndOfFrame();
            }
            else
            {
                StartCoroutine(GenerateBubble(NewMessage, message));
                yield return new WaitForEndOfFrame();
            }
        }
        yield return new WaitForEndOfFrame(); 
        Content.transform.parent.transform.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(0, 0);
    }

    private IEnumerator GenerateBubble(GameObject objectType, string message)
    {
        GameObject Bubble = Instantiate(objectType, objectType.transform.localScale, objectType.transform.localRotation);
        if (objectType == NewMessage) // Handle different hierarchy type of message objects
        {
            Bubble.transform.Find("Message").GetComponent<TMP_Text>().text = message.Split(":")[1]; // Set bubble message
            Bubble.transform.Find("ElementHolder").transform.Find("Name").GetComponent<TMP_Text>().text = message.Split(":")[0]; // Set bubble name
        }
        else 
        {
            Bubble.transform.Find("Message").transform.Find("Message").GetComponent<TMP_Text>().text = message.Split(":")[1];
            Bubble.transform.Find("Message").transform.Find("ElementHolder").transform.Find("Name").GetComponent<TMP_Text>().text = "You"; // Set bubble name
        }
        yield return new WaitForEndOfFrame();
        Bubble.transform.SetParent(Content.transform);
        Bubble.transform.localScale = Vector3.one;
        messageBubbles.Add(Bubble);
    }

    public void SendMessageAsync()
    {
        if (string.IsNullOrEmpty(messageInput.text))
        {
            return;
        }
        DateTime dt = DateTime.Now;
        string date = dt.Hour + ":" + dt.Minute + ":" + dt.Second + ":date";
        string newData = date + apiManager.playerName + ": " + messageInput.text;
        apiManager.CallTeamManagerAsync(apiManager.teamID, "updatehistory", newData);
        _ = apiManager.FetchTeamDataAsync(apiManager.teamID, false);
        messageInput.text = string.Empty;
    }
}
