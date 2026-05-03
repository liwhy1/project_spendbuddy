using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [Header("Feed UI Data")]
    public GameObject feedPage;
    [SerializeField] private Button feedButton;
    [SerializeField] private Button feedlikeButton;
    [SerializeField] private Button feeddislikeButton;
    [SerializeField] private Button feedglobalButton;
    [SerializeField] private Button feedteamButton;
    public GameObject imageInfo;
    public GameObject feedhint;
    public Image feedImage;

    [Header("Chat UI Data")]
    [SerializeField] private GameObject chatPage;
    [SerializeField] private Button chatButton;
    
    [Header("Camera UI Data")]
    [SerializeField] private GameObject cameraPage;
    [SerializeField] private Button cameraButton;
    [SerializeField] private Button camerasnapButton;
    [SerializeField] private Button cameraswapButton;
    [SerializeField] private Button cameracancelButton;
    [SerializeField] private Button camerauploadButton;
    [SerializeField] private AspectRatioFitter aspectFitter;
    [SerializeField] private string cameraState;
    [SerializeField] private TMP_InputField cameraDescription;
    [SerializeField] private TMP_InputField cameraPrice;
    [SerializeField] private TMP_InputField cameraBudget;
    public Toggle cameraTeamToggle;
    public byte[] currentImageData;
    public RawImage previewImage;
    private WebCamTexture frontcamTexture = null;
    private WebCamTexture backcamTexture = null;
    private WebCamTexture selectedCamera = null;

    /*[Header("Team UI Data")]
    [SerializeField] private GameObject leaderboardPage;
    [SerializeField] private GameObject leadteamView;
    [SerializeField] private GameObject leadglobalView;
    [SerializeField] private Button leaderboardButton;
    [SerializeField] private Button leaderboardteamButton;
    [SerializeField] private Button leaderboardglobalButton;
    [SerializeField] private GameObject NewInstance;
    public GameObject teamContent;
    public GameObject globalContent;
    public string previousDatasum = "";
    [SerializeField] private string datasum = "";
    public string previousglobalDatasum = "";
    [SerializeField] private string globaldatasum = "";
    public List<GameObject> teamBubbles = new List<GameObject>();
    public List<GameObject> globalBubbles = new List<GameObject>();*/

    [Header("Profile UI Data")]
    [SerializeField] public GameObject profilePage;
    [SerializeField] private GameObject avatarObject;
    [SerializeField] private Slider allRatio;
    [SerializeField] private Button profileButton;
    [SerializeField] public TMP_Text nameText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text budgetText;
    [SerializeField] private Button submenucloseButton;

    [Header("Settings UI Data")]
    [SerializeField] private GameObject settingsPage;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button signoutButton;

    [Header("Team UI Data")]
    [SerializeField] public GameObject teamPage;
    [SerializeField] private Button teamButton;
    public GameObject memberView;
    [SerializeField] private GameObject memberInstance;
    [SerializeField] private Button teamleaveButton;
    [SerializeField] private TMP_Text teammembersText;
    [SerializeField] private TMP_Text teamscoreText;
    [SerializeField] private TMP_Text teamidText;
    [SerializeField] public TMP_Text teamgoalText;
    [SerializeField] public TMP_Text teamnameText;
    [SerializeField] public TMP_Text teamsaveText;
    public string previousmemberList;

    [Header("Photos UI Data")]
    [SerializeField] private GameObject photosPage;
    [SerializeField] private Button photosButton;
    public GameObject photosView;
    [SerializeField] private GameObject photosInstance;
    public string previousphotosList;

    [Header("Reference Data")]
    [SerializeField] private ApiManager apiManager;
    [SerializeField] private OOBUIManager oobUIManager;
    [SerializeField] private MessageManager messageManager;
    [SerializeField] private ImgurManager imgurManager;

    [Header("Misc UI")]
    [SerializeField] private GameObject oobUI;
    public GameObject gameUI;
    public GameObject navbarUI;
    [SerializeField] Material invertMaterial;
    [SerializeField] GameObject selectUI;
    public GameObject currentPage;
    public string memberdata;

    private void OnEnable()
    {
        feedButton.onClick.AddListener(() => StartCoroutine(MoveUI(feedPage)));
        feedlikeButton.onClick.AddListener(() => FeedLikeHandler());
        feeddislikeButton.onClick.AddListener(() => FeedDislikeHandler());
        feedglobalButton.onClick.AddListener(() => FeedGlobalHandler());
        feedteamButton.onClick.AddListener(() => FeedTeamHandler());
        //leaderboardteamButton.onClick.AddListener(() => LeaderTeamButtonPressed());
        //leaderboardglobalButton.onClick.AddListener(() => LeaderGlobalButtonPressed());
        chatButton.onClick.AddListener(() => StartCoroutine(MoveUI(chatPage)));
        submenucloseButton.onClick.AddListener(() => CloseButtonPressed());
        cameraButton.onClick.AddListener(() => StartCoroutine(MoveUI(cameraPage)));
        teamButton.onClick.AddListener(() => StartCoroutine(MoveUI(teamPage)));
        profileButton.onClick.AddListener(() => StartCoroutine(MoveUI(profilePage)));
        camerasnapButton.onClick.AddListener(() => CameraSnapHandler());
        cameracancelButton.onClick.AddListener(() => CameraCancelHandler());
        camerauploadButton.onClick.AddListener(() => ImgurCallHandler());
        cameraswapButton.onClick.AddListener(() => CameraSwapHandler());
        settingsButton.onClick.AddListener(() => SettingsButtonPressed());
        photosButton.onClick.AddListener(() => PhotosButtonPressed());
        teamleaveButton.onClick.AddListener(() => TeamLeaveButtonPressed());
        signoutButton.onClick.AddListener(() => apiManager.SignOutAndClearSession());
    }

    private void OnDisable()
    {
        feedButton.onClick.RemoveAllListeners();
        feedlikeButton.onClick.RemoveAllListeners();
        feeddislikeButton.onClick.RemoveAllListeners();
        feedglobalButton.onClick.RemoveAllListeners();
        feedteamButton.onClick.RemoveAllListeners();
        //leaderboardteamButton.onClick.RemoveAllListeners();
        //leaderboardglobalButton.onClick.RemoveAllListeners();
        chatButton.onClick.RemoveAllListeners();
        cameraButton.onClick.RemoveAllListeners();
        submenucloseButton.onClick.RemoveAllListeners();
        teamButton.onClick.RemoveAllListeners();
        profileButton.onClick.RemoveAllListeners();
        camerasnapButton.onClick.RemoveAllListeners();
        cameracancelButton.onClick.RemoveAllListeners();
        camerauploadButton.onClick.RemoveAllListeners();
        cameraswapButton.onClick.RemoveAllListeners();
        settingsButton.onClick.RemoveAllListeners();
        photosButton.onClick.RemoveAllListeners();
        signoutButton.onClick.RemoveAllListeners();
    }

    public void Startup()
    {
        if (!apiManager.setupstateHandled) // UNDER NO CIRCUMSTANCE RUN STARTUP MORE THAN ONCE, PLEASE
        {
            Debug.Log("What the fuck");
            return;
        }
        oobUI.SetActive(false);
        gameUI.SetActive(true);
        navbarUI.SetActive(true);
        settingsPage.SetActive(false);
        photosPage.SetActive(false);
        oobUIManager.logouticonButton.gameObject.SetActive(false);
        submenucloseButton.gameObject.SetActive(false);
        cameraswapButton.gameObject.SetActive(true);
        camerasnapButton.gameObject.SetActive(true);
        cameracancelButton.gameObject.SetActive(false);
        camerauploadButton.gameObject.SetActive(false);
        cameraDescription.gameObject.SetActive(false);
        cameraPrice.gameObject.SetActive(false);
        cameraBudget.gameObject.SetActive(false);
        cameraTeamToggle.gameObject.SetActive(false);
        //leadglobalView.SetActive(false);
        //leadteamView.SetActive(true);
        nameText.text = apiManager.playerName;
        scoreText.text = "Score: " + apiManager.playerScore;
        budgetText.text = "Budget: " + apiManager.playerBudget;
        teamscoreText.text = "Team score: " + apiManager.teamScore;
        teamgoalText.text = !string.IsNullOrEmpty(apiManager.teamGoal) ? "Team goal:\n" +apiManager.teamGoal : teamgoalText.text;
        teamsaveText.text = !string.IsNullOrEmpty(apiManager.teamSave) ? "Team budget:\n" + apiManager.teamSave : "Budget:";
        teamnameText.text = !string.IsNullOrEmpty(apiManager.teamName) ? "Team name:\n" +apiManager.teamName : "Team";
        teamidText.text = !string.IsNullOrEmpty(apiManager.teamID) ? "Team code: " + apiManager.teamID : "Team code: " + PlayerPrefs.GetString("cachedteamid");
        cameraState = "back";
        recievedYes = 0;
        recievedNo = 0;
        //LeaderBoardHandler();
        imgurManager.GetContentsListAsync(imgurManager.selectedFilter);
        StartCoroutine(MoveUI(feedPage));
        StartCoroutine(FetchLoop());
        var respone = apiManager.FetchTeamDataAsync(!string.IsNullOrEmpty(apiManager.teamID) ? apiManager.teamID : PlayerPrefs.GetString("cachedteamid"), false); // Fire up message history
        AvatarHandler();
    }
    
    private IEnumerator FetchLoop()
    {
        yield return new WaitForEndOfFrame();
        if (gameUI.activeSelf)
        {
            if (currentPage == chatPage)
            {
                var response = apiManager.FetchTeamDataAsync(!string.IsNullOrEmpty(apiManager.teamID) ? apiManager.teamID : PlayerPrefs.GetString("cachedteamid"), false);
                yield return new WaitForSeconds(8);
            }
            else if (currentPage == profilePage)
            {
                var response = apiManager.FetchPlayerDataAsync(false);
                VoteRatioHandler();
                yield return new WaitForSeconds(8);
            }
            else if (currentPage == feedPage)
            {
                var response = apiManager.FetchPhotoListAsync();
                yield return new WaitForSeconds(7);
            }
            else if (currentPage == teamPage)
            {
                var response = apiManager.FetchTeamDataAsync(!string.IsNullOrEmpty(apiManager.teamID) ? apiManager.teamID : PlayerPrefs.GetString("cachedteamid"), false);
                //LeaderBoardHandler();
                TeamMemberListHandler();
                yield return new WaitForSeconds(8);
            }
            else
            {
                yield return new WaitForSeconds(5);
            }
            StartCoroutine(FetchLoop());
        }
    }

    private void TeamLeaveButtonPressed()
    {
        apiManager.CallTeamManagerAsync(apiManager.teamID,"leave","");
        var respone = apiManager.UpdatePlayerDataAsync("setteamid", "");
        respone = apiManager.UpdatePlayerDataAsync("setsetupstate", "1");
        apiManager.SignOutAndClearSession(); // Desperate times, desperate measures
    }

    private void CloseButtonPressed()
    {
        settingsPage.SetActive(false);
        photosPage.SetActive(false);
        submenucloseButton.gameObject.SetActive(false);
        newAvatar.SetActive(true);
    }
    
    private IEnumerator CameraHandler()
    {
        if (cameracancelButton.gameObject.activeSelf)
        {
            previewImage.gameObject.SetActive(true);
            yield break;
        }

        // Ask for camera permission
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
            if (!Application.HasUserAuthorization(UserAuthorization.WebCam)) yield break;
        }

        // Get available devices
        WebCamDevice[] devices;
        devices = WebCamTexture.devices;

        if (devices.Length == 0)
        {
            Debug.Log("No camera devices available!");
            yield break;
        }
        else
        {
            previewImage.texture = null;
        }

        // Select front and back cameras
        foreach (var camera in devices)
        {
            if (string.IsNullOrEmpty(camera.name)) continue;
            Debug.Log("Camera found: " + camera.name);

            if (camera.isFrontFacing && frontcamTexture == null)
            {
                frontcamTexture = new WebCamTexture(camera.name);
            }
            else if (!camera.isFrontFacing && backcamTexture == null)
            {
                backcamTexture = new WebCamTexture(camera.name);
            }
            else
            {
                backcamTexture = backcamTexture == null ? new WebCamTexture(camera.name) : backcamTexture;
            }
        }
        
        // Start camera
        if (frontcamTexture == null && backcamTexture == null) yield break; // failsafe to prevent swap loop if cameras fail
        selectedCamera = backcamTexture != null ? backcamTexture : frontcamTexture; // temporary assignment so if we need to swap it isn't null
        previewImage.texture = null;
        aspectFitter.aspectRatio = 0.75f;
        if (cameraState == "front" && frontcamTexture != null)
        {
            selectedCamera = frontcamTexture;
        }
        else if (cameraState == "back" && backcamTexture != null)
        {
            selectedCamera = backcamTexture;
        }
        else if (devices.Length > 1)
        {
            // Attempt to swap to the other camera if the current one fails
            CameraSwapHandler();
            yield break;
        }
        
        // Apply texture & start camera
        previewImage.gameObject.SetActive(false);
        previewImage.texture = selectedCamera;
        selectedCamera.Play();
        if (cameraState == "front") { previewImage.rectTransform.localScale = new Vector3(-1, 1, 1); } // Flip camera if necessary
        else { previewImage.rectTransform.localScale = new Vector3(1, 1, 1); }
        yield return new WaitForEndOfFrame();
        aspectFitter.aspectRatio = 0.75f; // 1.6f :/
        previewImage.gameObject.SetActive(true);
    }

    private void CameraSwapHandler()
    {
        if (cameraState == "front" && backcamTexture != null)
        {
            selectedCamera.Stop();
            cameraState = "back";
            previewImage.texture = null;
            previewImage.gameObject.SetActive(false);
            selectedCamera = backcamTexture;
            previewImage.texture = selectedCamera;
            previewImage.rectTransform.localScale = new Vector3(1, 1, 1);
            selectedCamera.Play();
            aspectFitter.aspectRatio = 0.75f;
            previewImage.gameObject.SetActive(true);
        }
        else if (cameraState == "back" && frontcamTexture != null)
        {
            selectedCamera.Stop();
            cameraState = "front";
            previewImage.texture = null;
            previewImage.gameObject.SetActive(false);
            selectedCamera = frontcamTexture;
            previewImage.texture = selectedCamera;
            previewImage.rectTransform.localScale = new Vector3(-1, 1, 1); // Flip preview
            selectedCamera.Play();
            aspectFitter.aspectRatio = 0.75f;
            previewImage.gameObject.SetActive(true);
        }
    }

    private void CameraSnapHandler()
    {
        cameraswapButton.gameObject.SetActive(false);
        camerasnapButton.gameObject.SetActive(false);
        cameracancelButton.gameObject.SetActive(true);
        camerauploadButton.gameObject.SetActive(true);
        cameraDescription.gameObject.SetActive(true);
        cameraPrice.gameObject.SetActive(true);
        cameraBudget.gameObject.SetActive(true);
        cameraTeamToggle.gameObject.SetActive(true);

        // Save image
        Texture2D texture = new Texture2D(previewImage.texture.width, previewImage.texture.height, TextureFormat.ARGB32, false);
        
        //Save the image to the Texture2D
        texture.SetPixels(selectedCamera.GetPixels());
        texture.Apply();

        //Encode it as a PNG.
        currentImageData = texture.EncodeToPNG();
        selectedCamera.Stop(); // "Freeze" camera
    }

    private void ImgurCallHandler()
    {
        imgurManager.StartUpload(currentImageData, cameraDescription.text + "price:" + cameraPrice.text + ":price" + "budget:" + cameraBudget.text + ":budget");
        selectedCamera.Play();
        cameraswapButton.gameObject.SetActive(true);
        camerasnapButton.gameObject.SetActive(true);
        cameracancelButton.gameObject.SetActive(false);
        camerauploadButton.gameObject.SetActive(false);
        cameraDescription.text = null;
        cameraDescription.gameObject.SetActive(false);
        cameraPrice.gameObject.SetActive(false);
        cameraBudget.gameObject.SetActive(false);
        cameraTeamToggle.gameObject.SetActive(false);
        currentImageData = null;
    }

    private void CameraCancelHandler()
    {
        selectedCamera.Play();
        cameraswapButton.gameObject.SetActive(true);
        camerasnapButton.gameObject.SetActive(true);
        cameracancelButton.gameObject.SetActive(false);
        camerauploadButton.gameObject.SetActive(false);
        cameraDescription.gameObject.SetActive(false);
        cameraDescription.text = "";
        cameraPrice.gameObject.SetActive(false);
        cameraBudget.gameObject.SetActive(false);
        cameraTeamToggle.gameObject.SetActive(false);
    }

    public void PhotoDescriptionHandler()
    {
        if (cameraDescription.text.Length > 59)
        {
            cameraDescription.text = cameraDescription.text.Remove(cameraDescription.text.Length - 1);
        }
    }

    public void PhotoPriceHandler()
    {
        string allowedChars = "0123456789,.-"; // Allowed characters
        cameraPrice.text = new string(cameraPrice.text.Where(c => allowedChars.Contains(c)).ToArray());
        if (cameraPrice.text.Length > 4)
        {
            cameraPrice.text = cameraPrice.text.Remove(cameraPrice.text.Length - 1);
        }
    }

    public void PhotoBudgetHandler()
    {
        string allowedChars = "0123456789,.-"; // Allowed characters
        cameraBudget.text = new string(cameraBudget.text.Where(c => allowedChars.Contains(c)).ToArray());
        if (cameraBudget.text.Length > 4)
        {
            cameraBudget.text = cameraBudget.text.Remove(cameraBudget.text.Length - 1);
        }
    }

    private void FeedTeamHandler()
    {
        Color color = Color.blue;
        feedteamButton.GetComponent<Image>().color = ColorUtility.TryParseHtmlString("#585858", out color) ? color : Color.blue;
        feedglobalButton.GetComponent<Image>().color = ColorUtility.TryParseHtmlString("#FFFFFF", out color) ? color : Color.blue;
        imgurManager.GetContentsListAsync("team");
    }

    private void FeedGlobalHandler()
    {
        Color color = Color.blue;
        feedglobalButton.GetComponent<Image>().color = ColorUtility.TryParseHtmlString("#585858", out color) ? color : Color.blue;
        feedteamButton.GetComponent<Image>().color = ColorUtility.TryParseHtmlString("#FFFFFF", out color) ? color : Color.blue;
        imgurManager.GetContentsListAsync("global");
    }

    private void SelectionChange()
    {
        Button button = null;
        if (currentPage == chatPage)
        {
            button = chatButton;
        }
        else if (currentPage == teamPage)
        {
            button = teamButton;
        }
        else if (currentPage == profilePage)
        {
            button = profileButton;
        }
        else if (currentPage == cameraPage)
        {
            button = cameraButton;
        }
        else if (currentPage == feedPage)
        {
            button = feedButton;
        }
        selectUI.transform.position = button.gameObject.transform.position;
        feedButton.image.material = null;
        chatButton.image.material = null;
        cameraButton.image.material = null;
        teamButton.image.material = null;
        profileButton.image.material = null;
        button.image.material = invertMaterial;
    }

    private void FeedLikeHandler()
    {
        if (!string.IsNullOrEmpty(imgurManager.currentImageId))
        {
            apiManager.votedIds = apiManager.votedIds + ";" + imgurManager.currentImageId;
            var response = apiManager.UpdatePlayerDataAsync("setvotedids", apiManager.votedIds);
            if (imgurManager.selectedFilter == "team")
            {  
                Debug.Log("Vote sent: " + imgurManager.currentImageId + ":team:yes:" + (string.IsNullOrEmpty(Regex.Match(imgurManager.currentphotoData, @"id:(.*?):id").Groups[1].Value) ? imgurManager.currentphotoData.Split(":")[1].Split(";")[1] : Regex.Match(imgurManager.currentphotoData, @"id:(.*?):id").Groups[1].Value) + ":" + imgurManager.currentImageId);
                response = apiManager.UpdatePlayerDataAsync("setrecievedvotes", imgurManager.currentImageId + ":team:yes:" + (string.IsNullOrEmpty(Regex.Match(imgurManager.currentphotoData, @"id:(.*?):id").Groups[1].Value) ? imgurManager.currentphotoData.Split(":")[1].Split(";")[1] : Regex.Match(imgurManager.currentphotoData, @"id:(.*?):id").Groups[1].Value) + ":" + imgurManager.currentImageId);
                imgurManager.teamIds.Remove(imgurManager.teamIds[0]);
            }
            else
            {
                response = apiManager.UpdatePlayerDataAsync("setrecievedvotes", imgurManager.currentImageId + ":anon:yes:" + (string.IsNullOrEmpty(Regex.Match(imgurManager.currentphotoData, @"id:(.*?):id").Groups[1].Value) ? imgurManager.currentphotoData.Split(":")[1].Split(";")[1] : Regex.Match(imgurManager.currentphotoData, @"id:(.*?):id").Groups[1].Value) + ":" + imgurManager.currentImageId);
                imgurManager.allIds.Remove(imgurManager.allIds[0]);
            }
            imgurManager.GetContentsListAsync(imgurManager.selectedFilter);
        }
    }

    private void FeedDislikeHandler()
    {
        if (!string.IsNullOrEmpty(imgurManager.currentImageId))
        {
            apiManager.votedIds = apiManager.votedIds + ";" + imgurManager.currentImageId;
            var response = apiManager.UpdatePlayerDataAsync("setvotedids", apiManager.votedIds);
            if (imgurManager.selectedFilter == "team")
            { 
                response = apiManager.UpdatePlayerDataAsync("setrecievedvotes", imgurManager.currentImageId + ":team:no:" + (string.IsNullOrEmpty(Regex.Match(imgurManager.currentphotoData, @"id:(.*?):id").Groups[1].Value) ? imgurManager.currentphotoData.Split(":")[1].Split(";")[1] : Regex.Match(imgurManager.currentphotoData, @"id:(.*?):id").Groups[1].Value) + ":" + imgurManager.currentImageId);
                imgurManager.teamIds.Remove(imgurManager.teamIds[0]);
            }
            else
            {
                response = apiManager.UpdatePlayerDataAsync("setrecievedvotes", imgurManager.currentImageId + ":anon:no:" + (string.IsNullOrEmpty(Regex.Match(imgurManager.currentphotoData, @"id:(.*?):id").Groups[1].Value) ? imgurManager.currentphotoData.Split(":")[1].Split(";")[1] : Regex.Match(imgurManager.currentphotoData, @"id:(.*?):id").Groups[1].Value) + ":" + imgurManager.currentImageId);
                imgurManager.allIds.Remove(imgurManager.allIds[0]);
            }
            imgurManager.GetContentsListAsync(imgurManager.selectedFilter);
        }
    }

    private void SettingsButtonPressed()
    {
        settingsPage.SetActive(true);
        submenucloseButton.gameObject.SetActive(true);
        newAvatar.SetActive(false);
    }


    private void PhotosButtonPressed()
    {
        photosPage.SetActive(true);
        newAvatar.SetActive(false);
        submenucloseButton.gameObject.SetActive(true);
        VoteRatioHandler();
    }

    int recievedYes;
    int recievedNo;
    private async void VoteRatioHandler()
    {
        if (imgurManager.ownIds.Count != 0)
        {
            if (previousphotosList != apiManager.photoList)
            {
                foreach (Transform child in photosView.transform) // Destroy old list
                {
                    Destroy(child.gameObject);
                }
                foreach (var id in imgurManager.ownIds)
                {
                    Texture2D photo = await imgurManager.DownloadImageAsync(id);
                    var sprite = Sprite.Create(
                    texture: photo,
                    rect: new Rect(0.0f, 0.0f, photo.width, photo.height),
                    pivot: new Vector2(0.5f, 0.5f),
                    pixelsPerUnit: 100.0f);

                    // Bruh
                    int num1 = Int32.Parse(Regex.Match(apiManager.recievedVotes, $@"{id}:(.*?):{id}").Groups[1].Value.Split(";")[0].Split(":")[1]); // no
                    int num2 = Int32.Parse(Regex.Match(apiManager.recievedVotes, $@"{id}:(.*?):{id}").Groups[1].Value.Split(";")[0].Split(":")[2]); // yes
                    int num3 = Int32.Parse(Regex.Match(apiManager.recievedVotes, $@"{id}:(.*?):{id}").Groups[1].Value.Split(";")[1].Split(":")[1]);
                    int num4 = Int32.Parse(Regex.Match(apiManager.recievedVotes, $@"{id}:(.*?):{id}").Groups[1].Value.Split(";")[1].Split(":")[2]);
                    recievedYes += num2;
                    recievedNo += num1;
                    float votedatateam = num1 == 0 && num2 == 0 ? 3 : (float)Math.Min(num1, num2) / Math.Max(num1, num2);
                    float votedataanon = num3 == 0 && num4 == 0 ? 3 : (float)Math.Min(num3, num4) / Math.Max(num3, num4);
                    votedatateam = num2 > num1 && num1 == 0 ? 1 : votedatateam; // if there are yesses but the nos are 0, still set a value
                    votedataanon = num4 > num3 && num4 == 0 ? 1 : votedataanon;
                    StartCoroutine(CreatePhotoInstance(sprite, votedatateam, votedataanon));
                }
                previousphotosList = apiManager.photoList;
            }
        }
        if (recievedYes == 0 && recievedNo == 0)
        {
            allRatio.gameObject.transform.Find("Background").GetComponent<Image>().color = Color.gray;
            allRatio.gameObject.transform.Find("Fill Area").transform.Find("Fill").GetComponent<Image>().color = Color.gray;
        }
        else
        {
            allRatio.gameObject.transform.Find("Background").GetComponent<Image>().color = Color.red;
            allRatio.gameObject.transform.Find("Fill Area").transform.Find("Fill").GetComponent<Image>().color = Color.green;
            float newRatio = (float)Math.Min(recievedYes, recievedNo) / Math.Max(recievedYes, recievedNo);
            newRatio = recievedNo > recievedYes && recievedYes == 0 ? 1 : newRatio;
            allRatio.value = newRatio;
        }
    }

    private IEnumerator CreatePhotoInstance(Sprite photo, float votedatateam, float votedataanon)
    {
        GameObject Bubble = Instantiate(photosInstance, photosInstance.transform.localScale, photosInstance.transform.localRotation);
        Bubble.transform.Find("Preview").GetComponent<Image>().sprite = photo; // Set bubble preview
        
        if (votedatateam == 3)
        {
            Bubble.transform.Find("TeamRatio").transform.Find("Background").GetComponent<Image>().color = Color.gray;
            Bubble.transform.Find("TeamRatio").transform.Find("Fill Area").transform.Find("Fill").GetComponent<Image>().color = Color.gray;
        }
        else
        {
            Bubble.transform.Find("TeamRatio").transform.Find("Background").GetComponent<Image>().color = Color.red;
            Bubble.transform.Find("TeamRatio").transform.Find("Fill Area").transform.Find("Fill").GetComponent<Image>().color = Color.green;
            Bubble.transform.Find("TeamRatio").GetComponent<Slider>().value = votedatateam; // Set bubble data
        }
        if (votedataanon == 3)
        {
            Bubble.transform.Find("AnonRatio").transform.Find("Background").GetComponent<Image>().color = Color.gray;
            Bubble.transform.Find("AnonRatio").transform.Find("Fill Area").transform.Find("Fill").GetComponent<Image>().color = Color.gray;
        }
        else
        {
            Bubble.transform.Find("AnonRatio").transform.Find("Background").GetComponent<Image>().color = Color.red;
            Bubble.transform.Find("AnonRatio").transform.Find("Fill Area").transform.Find("Fill").GetComponent<Image>().color = Color.green;
            Bubble.transform.Find("AnonRatio").GetComponent<Slider>().value = votedataanon; // Set bubble data
        }
        yield return new WaitForEndOfFrame();
        Bubble.transform.SetParent(photosView.transform);
        Bubble.transform.localScale = Vector3.one;
    }

    public async void TeamMemberListHandler()
    {
        string[] members = apiManager.teamMembers.Split(",");
        if (previousmemberList != apiManager.teamMembers)
        {
            Debug.Log("New memberdata available!");
            foreach (Transform child in memberView.transform) // Destroy old list
            {
                Destroy(child.gameObject);
            }
            foreach (var member in members) // Generate list
            {
                if (member != apiManager.PlayerId && !string.IsNullOrEmpty(member))
                {
                    var response = await apiManager.FetchMemberDataAsync(member);
                    if (string.IsNullOrEmpty(response) || !response.Contains(";")) continue;
                    string name = response.Split(";")[1];
                    StartCoroutine(CreateMemberListInstance(name, member));
                }
            }
        }
        else
        {
            return;
        }
        previousmemberList = apiManager.teamMembers;
    }
    
    private IEnumerator CreateMemberListInstance(string name, string id)
    {
        GameObject Bubble = Instantiate(memberInstance, memberInstance.transform.localScale, memberInstance.transform.localRotation);
        Bubble.transform.Find("Name").GetComponent<TMP_Text>().text = name; // Set bubble name
        if (apiManager.PlayerId != apiManager.teamOwner) // Disable kick button if the player is not the owner
        {
            Bubble.transform.Find("Kick").gameObject.SetActive(false);
        }
        else
        {
            Bubble.transform.Find("Kick").GetComponent<Button>().onClick.AddListener(() => StartCoroutine(MemberKickHandler(id)));
        }
        yield return new WaitForEndOfFrame();
        Bubble.transform.SetParent(memberView.transform);
        Bubble.transform.localScale = Vector3.one;
    }

    private IEnumerator MemberKickHandler(string id)
    {
        apiManager.CallTeamManagerAsync(apiManager.teamID, "leave", id);
        yield return new WaitForSeconds(2f);
        TeamMemberListHandler();
    }

    /*private async void LeaderBoardHandler()
    {
        string[] members = apiManager.teamMembers.Split(",");
        List<string> teamdata = new List<string>();
        List<string> globaldata = new List<string>();
        List<string> badData = new List<string>();
        List<string> sortedData = new List<string>();
        teammembersText.text = "Members:";
        datasum = "";
        globaldatasum = "";
        foreach (var member in members)
        {
            if (string.IsNullOrEmpty(member)) continue;
            memberdata = await apiManager.FetchMemberDataAsync(member);
            datasum = datasum + ":" + memberdata;
        }
        
        // TEAM
        if (previousDatasum != datasum)
        {
            teamdata.Clear();
            if (teamBubbles.Count > 0)
            {
                foreach (GameObject item in teamBubbles)
                {
                    Destroy(item);
                }
            }
            teamBubbles.Clear();
            foreach (var item in datasum.Split(":"))
            {
                teamdata.Add(item);
            }
            // Cleanup
            badData.Clear();
            foreach (var item in teamdata)
            {
                if (string.IsNullOrEmpty(item) || item == ":")
                {
                    badData.Add(item);
                }
            }
            foreach (var item in badData)
            {
                teamdata.Remove(item);
            }
            sortedData = teamdata
            .OrderByDescending(entry => int.Parse(entry.Split(';').Last()))
            .ToList();
            foreach (var item in sortedData)
            {
                StartCoroutine(CreateLeaderBoardInstance(teamContent, (sortedData.IndexOf(item) + 1).ToString(), item.Split(";")[1], item.Split(";")[2]));
            }
            teamContent.transform.parent.transform.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(1, 1);
            previousDatasum = datasum;
            datasum = "";
        }
        

        // GLOBAL
        var respone = await apiManager.FetchTeamScoreAsync();
        globaldatasum = "";
        foreach (var item in respone)
        {
            globaldatasum = globaldatasum + ";" + item;
        }
        
        if (previousglobalDatasum != globaldatasum)
        {
            globaldata.Clear();
            foreach (var item in globalBubbles)
            {
                Destroy(item);
            }
            globalBubbles.Clear();
            foreach (var item in globaldatasum.Split(";"))
            {
                globaldata.Add(item);
            }
            // Cleanup
            badData.Clear();
            foreach (var item in globaldata)
            {
                if (string.IsNullOrEmpty(item) || item == ";")
                {
                    badData.Add(item);
                }
            }
            foreach (var item in badData)
            {
                globaldata.Remove(item);
            }
            sortedData = globaldata
            .OrderByDescending(entry => int.Parse(entry.Split(':').Last()))
            .ToList();
            foreach (var item in sortedData)
            {
                StartCoroutine(CreateLeaderBoardInstance(globalContent, (sortedData.IndexOf(item) + 1).ToString(), item.Split(":")[0], item.Split(":")[1]));
            }
            globalContent.transform.parent.transform.parent.GetComponent<ScrollRect>().normalizedPosition = new Vector2(1, 1);
            previousglobalDatasum = globaldatasum;
            globaldatasum = "";
        }
    }*/

    /*private IEnumerator CreateLeaderBoardInstance(GameObject scrollView, string rank, string name, string score)
    {
        GameObject Bubble = Instantiate(NewInstance, NewInstance.transform.localScale, NewInstance.transform.localRotation);
        Bubble.transform.Find("Name").GetComponent<TMP_Text>().text = name; // Set bubble message
        Bubble.transform.Find("Score").GetComponent<TMP_Text>().text = score; // Set bubble name
        Bubble.transform.Find("Rank").GetComponent<TMP_Text>().text = rank; // Set bubble rank
        yield return new WaitForEndOfFrame();
        Bubble.transform.SetParent(scrollView.transform);
        Bubble.transform.localScale = Vector3.one;
        if (scrollView == teamContent.gameObject)
        {
            teamBubbles.Add(Bubble);
        }
        else
        {
            globalBubbles.Add(Bubble);
        }
    }*/

    /*private void LeaderTeamButtonPressed()
    {
        leadglobalView.SetActive(false);
        leadteamView.SetActive(true);
    }*/

    /*private void LeaderGlobalButtonPressed()
    {
        leadteamView.SetActive(false);
        leadglobalView.SetActive(true);
    }*/

    public void TeamEditHandler()
    {
        oobUI.SetActive(true);
        StartCoroutine(MoveUI(oobUIManager.teamCreatePage));
        oobUIManager.teamcreatebackButton.gameObject.SetActive(false);
    }

    public void AvatarEditHandler()
    {
        oobUI.SetActive(true);
        Destroy(newAvatar);
        StartCoroutine(MoveUI(oobUIManager.avatarPage));
    }

    GameObject newAvatar;
    public void AvatarHandler()
    {
        newAvatar = Instantiate(oobUIManager.characterMale.activeSelf ? oobUIManager.characterMale : oobUIManager.characterFemale, avatarObject.transform.position, oobUIManager.characterMale.activeSelf ? oobUIManager.characterMale.transform.rotation : avatarObject.transform.rotation);
        newAvatar.transform.localScale = oobUIManager.characterMale.activeSelf ? new Vector3(250,250,250) : new Vector3(100,100,100);
        newAvatar.transform.SetParent(profilePage.transform);
    }

    public float rotationMultiplier = 0.1f;
    private bool isDragging = false;
    private Vector3 lastInputPosition;
    private float lastDeltaX;

    void Update()
    {
        if (profilePage.activeSelf && !oobUI.activeSelf)
        {
            if (Input.GetMouseButtonDown(0))
            {
                rotationMultiplier = 0.1f;
                TryStartDragging(Input.mousePosition);
            }
            if (Input.GetMouseButton(0) && isDragging)
            {
                RotateObject(Input.mousePosition);
            }
            if (Input.GetMouseButtonUp(0))
            {
                ApplyMomentum();
                isDragging = false;
            }

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    TryStartDragging(touch.position);
                }
                else if (touch.phase == TouchPhase.Moved && isDragging)
                {
                    RotateObject(touch.position);
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    ApplyMomentum();
                    isDragging = false;
                }
            }
        }
    }

    void TryStartDragging(Vector3 inputPosition)
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(inputPosition);

        if (Physics.Raycast(ray, out hit) && hit.transform == newAvatar.transform)
        {
            isDragging = true;
            lastInputPosition = inputPosition;
            lastDeltaX = 0f;
        }
    }

    void RotateObject(Vector3 inputPosition)
    {
        Vector3 deltaInput = inputPosition - lastInputPosition;
        float rotationAmount = deltaInput.x * rotationMultiplier;

        Vector3 localTorque = newAvatar.transform.TransformDirection(Vector3.up) * -rotationAmount;
        newAvatar.GetComponent<Rigidbody>().AddTorque(localTorque, ForceMode.VelocityChange);
        lastDeltaX = rotationAmount;
        lastInputPosition = inputPosition;
    }

    void ApplyMomentum()
    {
        if (Mathf.Abs(lastDeltaX) > 0.1f)
        {
            Vector3 localTorque = newAvatar.transform.TransformDirection(Vector3.up) * -lastDeltaX;
            newAvatar.GetComponent<Rigidbody>().AddTorque(localTorque, ForceMode.Impulse);
        }
    }


    private IEnumerator MoveUI(GameObject target) 
    {
        yield return new WaitForEndOfFrame();
        float currentLerpTime = 0;
        float lerpTime = .5f;
        currentPage = target != oobUIManager.avatarPage && target != oobUIManager.teamCreatePage ? target : currentPage;
        Color color = Color.blue;
        Camera.main.backgroundColor = ColorUtility.TryParseHtmlString("#00b8d4", out color) ? color : Color.blue;

        if (target != cameraPage && selectedCamera != null) // Prevent runtime exception
        {
            selectedCamera.Stop();
            previewImage.gameObject.SetActive(false);
        }

        if (target == feedPage)
        {
            imgurManager.GetContentsListAsync(imgurManager.selectedFilter);
        }

        SelectionChange();

        Vector3 targetpos = new Vector3(target.transform.position.x, target.transform.position.y, Camera.main.transform.position.z);
        while (currentLerpTime<lerpTime)
        {
            currentLerpTime += Time.deltaTime;
            float t = currentLerpTime / lerpTime;
            t = Mathf.Sin(t * Mathf.PI * 0.5f);
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, targetpos, t);
            yield return new WaitForEndOfFrame();
        }

        if (target == cameraPage)
        {
            StartCoroutine(CameraHandler());
        }
    }
}
