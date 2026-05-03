using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class OOBUIManager : MonoBehaviour
{
    [Header("Startup UI Data")]
    public GameObject startupPage;
    public GameObject onboard1Page;
    public GameObject onboard2Page;
    public GameObject onboard3Page;
    [SerializeField] private Button startsigninButton;
    [SerializeField] private Button finishonboardButton;
    [SerializeField] public Button onboardstartButton;
    [SerializeField] public Button onboardnext1Button;
    [SerializeField] public Button onboardnext2Button;
    [SerializeField] public Button onboardback1Button;
    [SerializeField] public Button onboardback2Button;
    [SerializeField] public Button onboardback3Button;
    [SerializeField] public Button onboardskip1Button;
    [SerializeField] public Button onboardskip2Button;

    [Header("Signin UI Data")]
    public GameObject signinPage;
    [SerializeField] private Button signinButton;
    [SerializeField] private Button newplayerButton;
    [SerializeField] private Button signinbackButton;
    [SerializeField] private TMP_InputField usernameField;
    public TMP_InputField passwordField;

    [Header("Signup UI Data")]
    public GameObject signupPage;
    [SerializeField] private Button signupButton;
    [SerializeField] private Button signupbackButton;
    [SerializeField] private TMP_InputField newusernameField;
    public TMP_InputField newpasswordField;
    public TMP_InputField passwordconfField;
    [SerializeField] private TMP_InputField teamidField;
    [SerializeField] private TMP_InputField teamnameField;
    [SerializeField] private TMP_InputField teamgoalField;
    [SerializeField] private TMP_InputField teamsaveField;

    [Header("Avatar UI Data")]
    public GameObject avatarPage;
    [SerializeField] private Button avatarnextButton;
    [SerializeField] private TMP_InputField displaynameField;

    [Header("Team startup UI Data")]
    public GameObject teamStartupPage;
    [SerializeField] private Button teamstartupjoinButton;
    [SerializeField] private Button teamstartupcreateButton;

    [Header("Team UI Data")]
    public GameObject teamJoinPage;
    [SerializeField] public GameObject teamCreatePage;
    [SerializeField] private Button teamcreateButton;
    [SerializeField] private Button teamjoinButton;
    [SerializeField] private Button teamjoinbackButton;
    [SerializeField] public Button teamcreatebackButton;
    [SerializeField] private Button newteamButton;

    [Header("Misc UI Data")]
    public GameObject oobUI;
    [SerializeField] private Button errorokButton;
    public Button logouticonButton;
    public GameObject persistentUI;

    [Header("Reference Data")]
    [SerializeField] private ApiManager apiManager;
    [SerializeField] private GameUIManager gameUIManager;

    private void OnEnable()
    {
        signupButton.onClick.AddListener(SignupButtonPressed);
        signinButton.onClick.AddListener(SigninButtonPressed);
        newplayerButton.onClick.AddListener(() => StartCoroutine(MoveUI(signupPage)));
        signupbackButton.onClick.AddListener(() => StartCoroutine(MoveUI(startupPage)));
        signinbackButton.onClick.AddListener(() => StartCoroutine(MoveUI(startupPage)));
        startsigninButton.onClick.AddListener(() => StartCoroutine(MoveUI(signinPage)));
        logouticonButton.onClick.AddListener(() => apiManager.SignOutAndClearSession());
        teamstartupcreateButton.onClick.AddListener(() => StartCoroutine(MoveUI(teamCreatePage)));
        teamstartupjoinButton.onClick.AddListener(() => StartCoroutine(MoveUI(teamJoinPage)));
        teamcreateButton.onClick.AddListener(() => TeamCreateHandler());
        teamjoinButton.onClick.AddListener(() => apiManager.CallTeamManagerAsync(teamidField.text, "join", ""));
        teamjoinbackButton.onClick.AddListener(() => StartCoroutine(MoveUI(teamStartupPage)));
        teamcreatebackButton.onClick.AddListener(() => StartCoroutine(MoveUI(teamStartupPage)));
        avatarnextButton.onClick.AddListener(() => AvatarHandler());
        newteamButton.onClick.AddListener(() => StartCoroutine(MoveUI(teamCreatePage)));
        errorokButton.onClick.AddListener(() => apiManager.errorPanel.SetActive(false));
        onboardstartButton.onClick.AddListener(() => StartCoroutine(MoveUI(onboard1Page)));
        onboardnext1Button.onClick.AddListener(() => StartCoroutine(MoveUI(onboard2Page)));
        onboardnext2Button.onClick.AddListener(() => StartCoroutine(MoveUI(onboard3Page)));
        onboardback1Button.onClick.AddListener(() => StartCoroutine(MoveUI(startupPage)));
        onboardback2Button.onClick.AddListener(() => StartCoroutine(MoveUI(onboard1Page)));
        onboardback3Button.onClick.AddListener(() => StartCoroutine(MoveUI(onboard2Page)));
        onboardskip1Button.onClick.AddListener(() => StartCoroutine(MoveUI(signupPage)));
        onboardskip2Button.onClick.AddListener(() => StartCoroutine(MoveUI(signupPage)));
        finishonboardButton.onClick.AddListener(() => StartCoroutine(MoveUI(signupPage)));
    }

    private void OnDisable()
    {
        signupButton.onClick.RemoveAllListeners();
        signinButton.onClick.RemoveAllListeners();
        startsigninButton.onClick.RemoveAllListeners();
        newplayerButton.onClick.RemoveAllListeners();
        signupbackButton.onClick.RemoveAllListeners();
        signinbackButton.onClick.RemoveAllListeners();
        logouticonButton.onClick.RemoveAllListeners();
        teamcreateButton.onClick.RemoveAllListeners();
        teamstartupcreateButton.onClick.RemoveAllListeners();
        teamstartupjoinButton.onClick.RemoveAllListeners();
        teamjoinButton.onClick.RemoveAllListeners();
        teamjoinbackButton.onClick.RemoveAllListeners();
        teamcreatebackButton.onClick.RemoveAllListeners();
        avatarnextButton.onClick.RemoveAllListeners();
        newteamButton.onClick.RemoveAllListeners();
        errorokButton.onClick.RemoveAllListeners();
        onboardstartButton.onClick.RemoveAllListeners();
        onboardnext1Button.onClick.RemoveAllListeners();
        onboardnext2Button.onClick.RemoveAllListeners();
        onboardback1Button.onClick.RemoveAllListeners();
        onboardback2Button.onClick.RemoveAllListeners();
        onboardback3Button.onClick.RemoveAllListeners();
        onboardskip1Button.onClick.RemoveAllListeners();
        onboardskip2Button.onClick.RemoveAllListeners();
        finishonboardButton.onClick.RemoveAllListeners();
    }

    private void Awake()
    {
        // Reset every UI element
        usernameField.text = "";
        passwordField.text = "";
        newpasswordField.text = "";
        newusernameField.text = "";
        passwordconfField.text = "";
        teamidField.text = "";
        teamnameField.text = "";
        displaynameField.text = "";
        gameUIManager.gameUI.SetActive(false);
        gameUIManager.navbarUI.SetActive(false);
        GenderSwitchHandler("male");
    }

    private async void SignupButtonPressed()
    {
        // Check if password fields match
        if (newpasswordField.text != passwordconfField.text)
        {
            apiManager.ErrorHandler("Notice","Password fields don't match.");
            return;
        }

        await apiManager.SignUpWithUsernamePasswordAsync(newusernameField.text, newpasswordField.text);
        if (!string.IsNullOrEmpty(apiManager.PlayerId))
        {
            PlayerPrefs.SetString("cachedusername", newusernameField.text);
            PlayerPrefs.SetString("cachedpassword", newpasswordField.text);
            StartCoroutine(MoveUI(avatarPage));
        }
    }

    private async void SigninButtonPressed()
    {
        await apiManager.SignInWithUsernamePasswordAsync(usernameField.text, passwordField.text);
        if (!string.IsNullOrEmpty(apiManager.PlayerId))
        {
            PlayerPrefs.SetString("cachedusername", usernameField.text);
            PlayerPrefs.SetString("cachedpassword", passwordField.text); // Probably unsecure af, but this will never be technically in prod
        }
    }

    public IEnumerator MoveUI(GameObject target) 
    {
        yield return new WaitForEndOfFrame();
        float currentLerpTime = 0;
        float lerpTime = .5f;
        if (target != gameUIManager.profilePage && target != gameUIManager.teamPage)
        {
            Color color = Color.black;
            Camera.main.backgroundColor = ColorUtility.TryParseHtmlString("#181818", out color) ? color : Color.black;
        }

        if (target != startupPage && target != signupPage && target != signinPage && target != onboard1Page && target != onboard2Page && target != onboard3Page && target != gameUIManager.profilePage && target != gameUIManager.teamPage)
        {
            logouticonButton.gameObject.SetActive(true);
        }
        else
        {
            logouticonButton.gameObject.SetActive(false);
        }
        if (target == signupPage)
        {
            apiManager.ErrorHandler("Notice", "Welcome to the prototype, have fun, if you're stuck try reloading :)");
        }

        Vector3 targetpos = new Vector3(target.transform.position.x, target.transform.position.y, Camera.main.transform.position.z);
        while (currentLerpTime<lerpTime)
        {
            currentLerpTime += Time.deltaTime;
            float t = currentLerpTime / lerpTime;
            t = Mathf.Sin(t * Mathf.PI * 0.5f);
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, targetpos, t);
            yield return new WaitForEndOfFrame();
        }
    }

    private void TeamCreateHandler()
    {
        if (!gameUIManager.gameUI.activeSelf)
        {
            apiManager.CallTeamManagerAsync(apiManager.GenerateRandomString(5), "create", teamnameField.text + ":" + teamgoalField.text + ":" + teamsaveField.text);
        }
        else
        {
            teamcreatebackButton.gameObject.SetActive(true);
            apiManager.CallTeamManagerAsync(apiManager.teamID, "setteamgoal", teamgoalField.text + ":" + teamsaveField.text + ":" + teamnameField.text);
            oobUI.SetActive(false);
            apiManager.teamGoal = teamgoalField.text;
            apiManager.teamSave = teamsaveField.text;
            apiManager.teamName = teamnameField.text;
            gameUIManager.teamgoalText.text = "Team goal:\n" +apiManager.teamGoal;
            gameUIManager.teamsaveText.text = "Budget: \n" + apiManager.teamSave;
            gameUIManager.teamnameText.text = "Team name:\n" +apiManager.teamName;
            logouticonButton.gameObject.SetActive(false);
            Color color = Color.blue;
            Camera.main.backgroundColor = ColorUtility.TryParseHtmlString("#00b8d4", out color) ? color : Color.blue;
            StartCoroutine(MoveUI(gameUIManager.teamPage));
        }
    }

    public void CodeFieldHandler()
    {
        // Limit length to 5 characters, and capitalize characters
        if (teamidField.text.Length > 5)
        {
            teamidField.text = teamidField.text.Remove(teamidField.text.Length - 1);
        }
        teamidField.text = teamidField.text.ToUpper();
    }

    public void DisplayNameHandler()
    {
        if (displaynameField.text.Length > 8)
        {
            displaynameField.text = displaynameField.text.Remove(displaynameField.text.Length - 1);
        }
    }

    private void AvatarHandler()
    {
        // Check if a display name has been entered
        if (string.IsNullOrEmpty(displaynameField.text))
        {
            apiManager.ErrorHandler("Notice","Display name can't be empty!");
            return;
        }
        var respone = apiManager.UpdatePlayerDataAsync("setdisplayname", displaynameField.text);
        Debug.Log((characterFemale.activeSelf ? "f;" : "m;") + ColorUtility.ToHtmlStringRGB(eyeColor.color) + ";" + currenthair + ":" + ColorUtility.ToHtmlStringRGB(hairColor.color) + ";" + currentshirt + ":" + ColorUtility.ToHtmlStringRGB(shirtColor.color) + ";" + currentpants + ":" + ColorUtility.ToHtmlStringRGB(pantsColor.color));
        string avatardata = (characterFemale.activeSelf ? "f;" : "m;") + ColorUtility.ToHtmlStringRGB(eyeColor.color) + ";" + currenthair + ";" + ColorUtility.ToHtmlStringRGB(hairColor.color) + ";" + currentshirt + ";" + ColorUtility.ToHtmlStringRGB(shirtColor.color) + ";" + currentpants + ";" + ColorUtility.ToHtmlStringRGB(pantsColor.color);
        respone = apiManager.UpdatePlayerDataAsync("setavatarid", avatardata);
        apiManager.avatarID = avatardata;
        apiManager.playerName = displaynameField.text;
        if (!gameUIManager.gameUI.activeSelf)
        {
            respone = apiManager.UpdatePlayerDataAsync("setsetupstate","1");
            StartCoroutine(MoveUI(teamStartupPage));
        }
        else
        {
            oobUI.SetActive(false);
            StartCoroutine(MoveUI(gameUIManager.profilePage));
            gameUIManager.nameText.text = apiManager.playerName;
            logouticonButton.gameObject.SetActive(false);
            Color color = Color.blue;
            Camera.main.backgroundColor = ColorUtility.TryParseHtmlString("#00b8d4", out color) ? color : Color.blue;
            gameUIManager.AvatarHandler();
        }

    }

    public void AvatarLoadHandler()
    {
        //f;14DE79;fh1:000000;fs1:146BDE;fp1:DE7914
        if (string.IsNullOrEmpty(apiManager.avatarID) || apiManager.avatarID == "0" || apiManager.avatarID == "1")
        {
            apiManager.avatarID = "m;14DE79;mh1;000000;ms1;146BDE;mp1;DE7914";
            var response = apiManager.UpdatePlayerDataAsync("setavatarid", apiManager.avatarID);
        }
        GenderSwitchHandler(apiManager.avatarID.Split(";")[0] == "m" ? "male" : "female");
        Color color1 = Color.black;
        eyeColor.color = ColorUtility.TryParseHtmlString("#" + apiManager.avatarID.Split(";")[1], out color1) ? color1 : Color.black;
        hairColor.color = ColorUtility.TryParseHtmlString("#" + apiManager.avatarID.Split(";")[3], out color1) ? color1 : Color.black;
        shirtColor.color = ColorUtility.TryParseHtmlString("#" + apiManager.avatarID.Split(";")[5], out color1) ? color1 : Color.black;
        pantsColor.color = ColorUtility.TryParseHtmlString("#" + apiManager.avatarID.Split(";")[7], out color1) ? color1 : Color.black;
        
        AvatarEditorHandler(apiManager.avatarID.Split(";")[2]);
        AvatarEditorHandler(apiManager.avatarID.Split(";")[4]);
        AvatarEditorHandler(apiManager.avatarID.Split(";")[6]);
        Debug.Log("Avatar loaded!");
    }

    public GameObject avatar;
    public float rotationMultiplier;
    private bool isDragging = false;
    private Vector3 lastInputPosition;
    private float lastDeltaX;

    void Update()
    {
        if (avatarPage.activeSelf)
        {
            if (Input.GetMouseButtonDown(0))
            {
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
        if (characterMale.activeSelf)
        {
            avatar = characterMale;
        }
        else
        {
            avatar = characterFemale;
        }
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(inputPosition);

        if (Physics.Raycast(ray, out hit) && hit.transform == avatar.transform)
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

        Vector3 localTorque = avatar.transform.TransformDirection(Vector3.up) * -rotationAmount;
        avatar.GetComponent<Rigidbody>().AddTorque(localTorque, ForceMode.VelocityChange);
        lastDeltaX = rotationAmount;
        lastInputPosition = inputPosition;
    }

    void ApplyMomentum()
    {
        if (Mathf.Abs(lastDeltaX) > 0.1f)
        {
            Vector3 localTorque = avatar.transform.TransformDirection(Vector3.up) * -lastDeltaX;
            avatar.GetComponent<Rigidbody>().AddTorque(localTorque, ForceMode.Impulse);
        }
    }


    // Dear God forgive me if I've sinned with the following code
    [SerializeField] private GameObject customizationpageMale;
    [SerializeField] private GameObject customizationpageFemale;
    [SerializeField] public GameObject characterMale;
    [SerializeField] public GameObject characterFemale;
    [SerializeField] private GameObject editorHair;
    [SerializeField] private GameObject editorEye;
    [SerializeField] private GameObject editorShirt;
    [SerializeField] private GameObject editorPants;
    [SerializeField] private GameObject editorHair2;
    [SerializeField] private GameObject editorEye2;
    [SerializeField] private GameObject editorShirt2;
    [SerializeField] private GameObject editorPants2;
    [SerializeField] private GameObject colorPickerButton;
    [SerializeField] private GameObject colorPickerUI;
    [SerializeField] private Material hairColor;
    [SerializeField] Material eyeColor;
    [SerializeField] Material shirtColor;
    [SerializeField] Material pantsColor;
    [SerializeField] string currentPage = "hair";
    [SerializeField] private GameObject fh1;
    [SerializeField] private GameObject fh2;
    [SerializeField] private GameObject fh3;
    [SerializeField] private GameObject fh4;
    [SerializeField] private GameObject fs1;
    [SerializeField] private GameObject fs2;
    [SerializeField] private GameObject fp1;    
    [SerializeField] private GameObject fp2;
    [SerializeField] private GameObject mh1;
    [SerializeField] private GameObject mh2;
    [SerializeField] private GameObject mh3;
    [SerializeField] private GameObject mh4;
    [SerializeField] private GameObject ms1;
    [SerializeField] private GameObject ms2;

    private void ResetEditor()
    {
        fh1.SetActive(false);
        fh2.SetActive(false);
        fh3.SetActive(false);
        fh4.SetActive(false);
        mh1.SetActive(false);
        mh2.SetActive(false);
        mh3.SetActive(false);
        mh4.SetActive(false);
        fs1.SetActive(false);
        fs2.SetActive(false);
        ms1.SetActive(false);
        ms2.SetActive(false);
        fp1.SetActive(false);
        fp2.SetActive(false);
        colorPickerUI.SetActive(false);
        currentPage = "hair";
        fh1.SetActive(true);
        mh1.SetActive(true);
        fs1.SetActive(true);
        ms1.SetActive(true);
        fp1.SetActive(true);
        Color color1 = Color.black;
        string color = "#000000";
        hairColor.color = ColorUtility.TryParseHtmlString(color, out color1) ? color1 : Color.black;
        color = "#14de79";
        eyeColor.color = ColorUtility.TryParseHtmlString(color, out color1) ? color1 : Color.black;
        color = "#146bde";
        shirtColor.color = ColorUtility.TryParseHtmlString(color, out color1) ? color1 : Color.black;
        color = "#de7914";
        pantsColor.color = ColorUtility.TryParseHtmlString(color, out color1) ? color1 : Color.black;
        EditorPageHandler("hair");
    }

    public void GenderSwitchHandler(string target)
    {
        if (target == "male")
        {
            customizationpageFemale.SetActive(false);
            customizationpageMale.SetActive(true);
            characterMale.SetActive(true);
            characterFemale.SetActive(false);
            ResetEditor();
            AvatarEditorHandler("mh1");
            currenthair = "mh1";
            currentpants = "mp1";
            currentshirt = "ms1";
        }
        else
        {
            customizationpageFemale.SetActive(true);
            customizationpageMale.SetActive(false);
            characterMale.SetActive(false);
            characterFemale.SetActive(true);
            ResetEditor();
            AvatarEditorHandler("fh1");
            currenthair = "fh1";
            currentpants = "fp1";
            currentshirt = "fs1";
        }
    }

    string currenthair;
    string currentshirt;
    string currentpants;

    public void AvatarEditorHandler(string cloth)
    {
        if (cloth.Contains("h"))
        {
            fh1.SetActive(false);
            fh2.SetActive(false);
            fh3.SetActive(false);
            fh4.SetActive(false);
            mh1.SetActive(false);
            mh2.SetActive(false);
            mh3.SetActive(false);
            mh4.SetActive(false);
            currenthair = cloth;
        }
        else if (cloth.Contains("s"))
        {
            fs1.SetActive(false);
            fs2.SetActive(false);
            ms1.SetActive(false);
            ms2.SetActive(false);
            currentshirt = cloth;
        }
        else if (cloth.Contains("p"))
        {
            fp1.SetActive(false);
            fp2.SetActive(false);
            currentpants = cloth;
        }

        switch (cloth)
        {
            case "fh1":
            fh1.SetActive(true);
            break;
            case "fh2":
            fh2.SetActive(true);
            break;
            case "fh3":
            fh3.SetActive(true);
            break;
            case "fh4":
            fh4.SetActive(true);
            break;
            case "fs1":
            fs1.SetActive(true);
            break;
            case "fs2":
            fs2.SetActive(true);
            break;
            case "fp1":
            fp1.SetActive(true);
            break;
            case "fp2":
            fp2.SetActive(true);
            break;
            case "mh1":
            mh1.SetActive(true);
            break;
            case "mh2":
            mh2.SetActive(true);
            break;
            case "mh3":
            mh3.SetActive(true);
            break;
            case "mh4":
            mh4.SetActive(true);
            break;
            case "ms1":
            ms1.SetActive(true);
            break;
            case "ms2":
            ms2.SetActive(true);
            break;
        }
    }

    public void OnColorChange(string color)
    {
        Color color1 = Color.black;
        switch (currentPage)
        {
            case "hair":
            hairColor.color = ColorUtility.TryParseHtmlString(color, out color1) ? color1 : Color.black;
            break;
            case "eyes":
            eyeColor.color = ColorUtility.TryParseHtmlString(color, out color1) ? color1 : Color.black;
            break;
            case "shirt":
            shirtColor.color = ColorUtility.TryParseHtmlString(color, out color1) ? color1 : Color.black;
            break;
            case "pants":
            pantsColor.color = ColorUtility.TryParseHtmlString(color, out color1) ? color1 : Color.black;
            break;
        }
    }

    public void ColorPickerHandler()
    {
        if (colorPickerUI.activeSelf)
        {
            colorPickerUI.SetActive(false);
        }
        else
        {
            colorPickerUI.SetActive(true);
        }
    }

    public void EditorPageHandler(string page)
    {
        editorEye.SetActive(false);
        editorHair.SetActive(false);
        editorShirt.SetActive(false);
        editorPants.SetActive(false);
        editorEye2.SetActive(false);
        editorHair2.SetActive(false);
        editorShirt2.SetActive(false);
        editorPants2.SetActive(false);
        currentPage = page;
        switch (page)
        {
            case "hair":
            if (customizationpageFemale.activeSelf) editorHair.SetActive(true); else editorHair2.SetActive(true);
            break;
            case "eyes":
            if (customizationpageFemale.activeSelf) editorEye.SetActive(true); else editorEye2.SetActive(true);
            break;
            case "shirt":
            if (customizationpageFemale.activeSelf) editorShirt.SetActive(true); else editorShirt2.SetActive(true);
            break;
            case "pants":
            if (customizationpageFemale.activeSelf) editorPants.SetActive(true); else editorPants2.SetActive(true);
            break;
        }
    }
}
