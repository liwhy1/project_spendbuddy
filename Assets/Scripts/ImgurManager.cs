using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class ImgurManager : MonoBehaviour
{

    [SerializeField] private ApiManager apiManager;
    [SerializeField] private GameUIManager gameUIManager;
    [SerializeField] private TMP_Text uploaderName;
    [SerializeField] private TMP_Text photoDescription;
    [SerializeField] private TMP_Text photoPrice;
    public string selectedFilter;
    public string currentImageId;
    private string photoData;
    public string currentphotoData;
    public List<string> teamIds = new();
    public List<string> allIds = new();
    public List<string> ownIds = new();
    public string lastPhotoList = "";
    private Texture2D firstphoto = null;
    private Texture2D secondphoto = null;
    private string previousFilter;

    private string clientID = "344aa732555befc"; // Replace with your Imgur Client ID
    private string apiURL = "https://api.imgur.com/3/image/";

    private void Awake()
    {
        selectedFilter = "team";
        currentImageId = "1";
        photoData = null;
        gameUIManager.feedImage.gameObject.SetActive(false);
    }

    public IEnumerator UploadImage(string description, byte[] imageBytes, Action<string> callback)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", imageBytes);
        form.AddField("description", "id:" + apiManager.PlayerId + ":id" + "name:" + apiManager.playerName + ":name" + "teamid:" + apiManager.teamID + ":teamid" + "description:" + description + ":description" + (!string.IsNullOrEmpty(Regex.Match(description, @"price:(.*?):price").Groups[1].Value) ? "price:" + Regex.Match(description, @"price:(.*?):price").Groups[1].Value + ":price" : ""));

        UnityWebRequest request = UnityWebRequest.Post(apiURL, form);
        request.SetRequestHeader("Authorization", "Client-ID " + clientID);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Imgur Upload Failed: " + request.error);
            callback(null);
        }
        else
        {
            string jsonResult = request.downloadHandler.text;

            // Extract the image URL from the JSON response
            ImgurResponse response = JsonUtility.FromJson<ImgurResponse>(jsonResult);
            callback(response.data.link);

            string[] parts = response.data.link.Split('/');
            string filename = parts[parts.Length - 1];
            string imageHash = filename.Split('.')[0];
            apiManager.CallTeamManagerAsync("","updatephotos", apiManager.PlayerId + ":" + imageHash + ":" + (gameUIManager.cameraTeamToggle.isOn ? "private" : "public"));
        }
    }

    public async Task GetImageInfo(string imageUrl)
    {
        photoData = null;
        if (string.IsNullOrEmpty(imageUrl))
        {
            Debug.LogError("Invalid Imgur URL");
        }
        
        using (UnityWebRequest request1 = UnityWebRequest.Get(apiURL + imageUrl))
        {
            request1.SetRequestHeader("Authorization", "Client-ID " + clientID);
            await request1.SendWebRequest();
            if (request1.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error Fetching Image Info: " + request1.error);
                if (!apiManager.errorPanel.activeSelf)
                {
                    apiManager.ErrorHandler("Error","Can't load more images at this time, try again later!");                    
                }
            }
            else
            {
                string jsonResult = request1.downloadHandler.text;

                // Parse the JSON response to extract description
                ImgurResponse response = JsonUtility.FromJson<ImgurResponse>(jsonResult);
                photoData = response.data.description;
            }
        }
    }
    
    public async Task<Texture2D> DownloadImageAsync(string url)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture("https://i.imgur.com/" + url + ".png"))
        {
            // Send the request asynchronously and wait for completion
            UnityWebRequestAsyncOperation asyncOp = request.SendWebRequest();
            await asyncOp;

            // Check for errors in the request
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error downloading image: {request.error}");
                apiManager.ErrorHandler("Error", "Can't Load images, try again later!");
                return null;
            }
            else
            {
                // Successfully downloaded the image, now get the texture
                return DownloadHandlerTexture.GetContent(request);
            }
        }
    }

    [Serializable]
    private class ImgurResponse
    {
        public ImgurData data;
    }

    [Serializable]
    private class ImgurData
    {
        public string description;
        public string link;
    }

    public void StartUpload(byte[] data, string description)
    {
        byte[] imageBytes = data;
        StartCoroutine(UploadImage(description, imageBytes, (imgurLink) =>
        {
            if (!string.IsNullOrEmpty(imgurLink))
            {
                Debug.Log("Uploaded Image URL: " + imgurLink);
            }
        }));
    }
    public void GetContentsListAsync(string filter)
    {
        selectedFilter = filter;
        var respone = apiManager.FetchPhotoListAsync();
        if (string.IsNullOrEmpty(apiManager.photoList) && gameUIManager.currentPage == gameUIManager.feedPage)
        {
            //apiManager.ErrorHandler("Error", "Failed to load image list!");
            return;
        }
        
        if (apiManager.photoList != lastPhotoList)
        {
            Debug.Log("New photos available!");
            lastPhotoList = apiManager.photoList;
            foreach (var photo in apiManager.photoList.Split(";"))
            {
                if (string.IsNullOrEmpty(photo) || apiManager.votedIds.Contains(photo.Split(":")[1])) // skip irrelevant photos
                    continue;
                
                if (photo.Contains(apiManager.PlayerId)) // filter out own photos
                {    
                    ownIds.Add(photo.Split(":")[1]);
                    continue;
                }
                List<string> empty = new();
                if (apiManager.teamMembers.Contains(photo.Split(":")[0]))
                {
                    teamIds.Add(photo.Split(":")[1]);
                }
                else if (photo.Count(c => c == ':') == 1 || photo.Contains("public"))
                {              
                    allIds.Add(photo.Split(":")[1]);
                }
                if (teamIds.Count > 0 && gameUIManager.feedImage.sprite == null) // try to load the first image as soon as it has been initalized
                {
                    //LoadNewContent(selectedFilter);
                }
            }
        }
        LoadNewContent(selectedFilter);
    }

    public async void LoadNewContent(string filter)
    {
        List<string> targetIds = filter == "team" ? teamIds : allIds;
        if (targetIds.Count == 0) // No photos available, disable view
        {
            gameUIManager.feedImage.gameObject.SetActive(false);
            gameUIManager.feedImage.sprite = null;
            currentImageId = null;
            uploaderName.text = "";
            secondphoto = null;
            gameUIManager.imageInfo.SetActive(false);
            gameUIManager.feedhint.SetActive(true);
            return;
        }

        if (previousFilter != filter) // filter changed, remove preloaded image and start from 0
        {
            secondphoto = null;
        }

        // image already loaded, but not voted yet, don't load next unless there was a filter change
        if (gameUIManager.feedImage.sprite != null && !apiManager.votedIds.Contains(currentImageId) && previousFilter == filter)
        {
            previousFilter = filter;
            return;
        }

        previousFilter = filter;

        // Load new image or use preloaded one
        firstphoto = secondphoto != null ? secondphoto : await DownloadImageAsync(targetIds[0]);

        // Preload the next image
        secondphoto = targetIds.Count > 1 ? await DownloadImageAsync(targetIds[1]) : null;

        if (firstphoto != null)
        {
            await GetImageInfo(targetIds[0]);
            currentphotoData = photoData;
            gameUIManager.feedImage.gameObject.SetActive(true);
            gameUIManager.imageInfo.SetActive(true);
            gameUIManager.feedhint.SetActive(false);
            currentImageId = targetIds[0];
            if (targetIds == teamIds)
            {
                uploaderName.text = string.IsNullOrEmpty(Regex.Match(photoData, @"name:(.*?):name").Groups[1].Value) ? photoData.Split(":")[0] : Regex.Match(photoData, @"name:(.*?):name").Groups[1].Value + (string.IsNullOrEmpty(Regex.Match(photoData, @"budget:(.*?):budget").Groups[1].Value) ? " Budget: " + Regex.Match(photoData, @"budget:(.*?):budget").Groups[1].Value + "€" : ""); // Allow loading legacy data
            }
            else
            {
                uploaderName.text = "";
            }
            photoDescription.text = !string.IsNullOrEmpty(Regex.Match(photoData, @"description:(.*?):description").Groups[1].Value) && Regex.Match(photoData, @"description:(.*?):description").Groups[1].Value.Contains("price") ? Regex.Match(photoData, @"description:(.*?):description").Groups[1].Value.Split(":")[0].Substring(0, Regex.Match(photoData, @"description:(.*?):description").Groups[1].Value.IndexOf("price")) : ""; // What is even this lol
            photoPrice.text = string.IsNullOrEmpty(Regex.Match(photoData, @"price:(.*?):price").Groups[1].Value) ? "0€" : Regex.Match(photoData, @"price:(.*?):price").Groups[1].Value + "€";
            var sprite = Sprite.Create(
            texture: firstphoto,
            rect: new Rect(0.0f, 0.0f, firstphoto.width, firstphoto.height),
            pivot: new Vector2(0.5f, 0.5f),
            pixelsPerUnit: 100.0f);
            gameUIManager.feedImage.sprite = sprite;
            gameUIManager.feedImage.preserveAspect = true;
        }
    }
}
