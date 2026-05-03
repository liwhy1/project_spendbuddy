using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.Core;
using UnityEngine;

public class ApiManager : MonoBehaviour
{  
    [Header("Team Data")]
    public string teamID;
    public string teamScore;
    public string teamName;
    public string teamMembers;
    public string chatHistory;
    public string lastchatHistory;
    public string photoList;
    public string teamOwner;
    public string teamGoal;
    public string teamSave;

    [Header("Player Data")]
    public string setupState;
    public string avatarID;
    public string playerName;
    public string playerScore;
    public string playerBudget;
    public string votedIds;
    public string PlayerId;
    public string recievedVotes;
    public bool setupstateHandled;

    [Header("Reference Data")]
    [SerializeField] private OOBUIManager oobUIManager;
    [SerializeField] private GameUIManager gameUIManager;
    [SerializeField] private MessageManager messageManager;
    [SerializeField] private ImgurManager imgurManager;

    [Header("Misc Data")]
    [SerializeField] private GameObject spinnerUI;
    [SerializeField] private GameObject blockerUI;
    [SerializeField] private TMP_Text errorText;
    public GameObject errorPanel;
    private StringResultType playerResponse = null;
    private StringResultType teamResponse = null;

    private async void Awake()
    {
        oobUIManager.persistentUI.SetActive(true);
        oobUIManager.oobUI.SetActive(true);
        try
		{
            // Initialize services
			await UnityServices.InitializeAsync();
        }
        catch (Exception ex)
		{
			ErrorHandler("Error", "UnityServices failed to initialize, try again later!");
            Debug.Log(ex);
            SignOutAndClearSession();
            return;
		}
        // Warmup call
        var response = CloudCodeService.Instance.CallEndpointAsync<StringResultType>("TeamManager", new Dictionary<string, object> { { "action", "empty" } });
        Debug.Log("Attempted warmup call!");

        // Check if a cached player already exists by checking if the session token exists
        if (AuthenticationService.Instance.SessionTokenExists)
        {
            SpinnerHandler("enable");
            await SignInCachedUserAsync();
        }
        // No session token found, check for cached data
        else if (PlayerPrefs.GetString("cachedusername").Length > 0)
        {
            SpinnerHandler("enable");
            await SignInWithUsernamePasswordAsync(PlayerPrefs.GetString("cachedusername"), PlayerPrefs.GetString("cachedpassword"));
        }
        // Continuing to fresh startup
        else
        {
            spinnerUI.SetActive(false);
            blockerUI.SetActive(false);
            StartCoroutine(oobUIManager.MoveUI(oobUIManager.startupPage));
        }
    }

    private async Task SignInCachedUserAsync()
    {
        // Sign in Anonymously
        // This call will sign in the cached player.
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Cached sign in succeeded!");
            PlayerId = AuthenticationService.Instance.PlayerId;
            Debug.Log("PlayerID: " + PlayerId);
            await FetchPlayerDataAsync(true);
        }
        catch (Exception ex)
        {
            // Forward player back to signin upon failed cached signin
            ErrorHandler("Notice","You have been signed out!");
            Debug.Log(ex);
            SignOutAndClearSession();
        }
    }

    public async Task SignUpWithUsernamePasswordAsync(string username, string password)
    {
        SpinnerHandler("enable");
        string autocomply = "X@";
        try
        {
            // Attempt user creation with username and password
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password + autocomply);
            try
            {
                // Attempt player clouddata setup
                var data = new Dictionary<string, object>
                {
                    { "action", "clean" },
                };
                var response = await CloudCodeService.Instance.CallEndpointAsync<StringResultType>("UserManager", data);
                Debug.Log("SignUp is successful!");
                PlayerId = AuthenticationService.Instance.PlayerId;
                Debug.Log("PlayerID: " + PlayerId);
            }
            catch (Exception ex)
            {
                ErrorHandler("Error","Failed to authenticate, try again later!");
                Debug.Log(ex);
            }
        }
        catch (Exception ex)
        {
            if (ex.ToString().Contains("not in the correct format"))
            {
                ErrorHandler("Notice","Username and/or password are not in the correct format!");
            }
            if (ex.ToString().Contains("Username does not match requirements"))
            {
                ErrorHandler("Notice","Username must be atleast 3 letters long!");
            }            
            if (ex.ToString().Contains("Password does not match requirements"))
            {
                ErrorHandler("Notice","Password must be atleast 6 letters long and contain a digit!");
            }            
            if (ex.ToString().Contains("username already exists"))
            {
                ErrorHandler("Notice","Username already in use!");
            }
        }
        SpinnerHandler("disable");
    }

    public async Task SignInWithUsernamePasswordAsync(string username, string password)
    {
        SpinnerHandler("enable");
        string autocomply = "X@";
        try
        {
            // Attempt authentication with username and password
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password + autocomply);
            Debug.Log("SignIn is successful!");
            PlayerId = AuthenticationService.Instance.PlayerId;
            Debug.Log("PlayerID: " + PlayerId);
        }
        catch (Exception ex)
        {
            if (ex.ToString().Contains("not in the correct format"))
            {
                ErrorHandler("Notice","Username and/or password is not in the correct format!");
            }
            if (ex.ToString().Contains("Invalid username or password"))
            {
                ErrorHandler("Error","Invalid username or password!");
            }
            return;
        }
        await FetchPlayerDataAsync(true);
    }

    public void SignOutAndClearSession()
    {
        // Clear cached data
        PlayerPrefs.DeleteAll();
        AuthenticationService.Instance.SignOut(true);
        
        oobUIManager.passwordField.text = "";
        oobUIManager.newpasswordField.text = "";
        oobUIManager.passwordconfField.text = "";
        teamID = "";
        teamScore = "";
        teamName = "";
        teamMembers = "";
        chatHistory = "";
        teamOwner = "";
        photoList = "";
        setupState = "";
        avatarID = "";
        playerName = "";
        playerScore = "";
        playerBudget = "";
        votedIds = "";
        PlayerId = "";
        teamGoal = "";
        teamSave = "";
        gameUIManager.memberdata = "";
        imgurManager.lastPhotoList = "";
        imgurManager.teamIds.Clear();
        imgurManager.allIds.Clear();
        imgurManager.ownIds.Clear();
        imgurManager.currentImageId = "";
        //gameUIManager.previousDatasum = "";
        //gameUIManager.previousglobalDatasum = "";
        gameUIManager.previousmemberList = "";
        gameUIManager.previousphotosList = "";
        imgurManager.selectedFilter = "team";
        imgurManager.currentphotoData = "";
        lastchatHistory = "";
        recievedVotes = "";
        setupstateHandled = false;
        /*foreach (GameObject item in gameUIManager.teamBubbles)
        {
            Destroy(item);
        }
        foreach (GameObject item in gameUIManager.globalBubbles)
        {
            Destroy(item);
        }*/
        foreach (GameObject item in messageManager.messageBubbles)
        {
            Destroy(item);
        }
        foreach (Transform child in gameUIManager.memberView.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in gameUIManager.photosView.transform)
        {
            Destroy(child.gameObject);
        }
        //gameUIManager.teamBubbles.Clear();
        //gameUIManager.globalBubbles.Clear();
        messageManager.messageBubbles.Clear();

        // Signout player and delete session token
        oobUIManager.StopAllCoroutines();
        gameUIManager.StopAllCoroutines();
        messageManager.StopAllCoroutines();
        imgurManager.StopAllCoroutines();
        SpinnerHandler("disable");

        // Reset UI
        gameUIManager.gameUI.SetActive(false);
        oobUIManager.oobUI.SetActive(true);
        gameUIManager.navbarUI.SetActive(false);
        StartCoroutine(oobUIManager.MoveUI(oobUIManager.startupPage));
        Debug.Log("SignOut is successful!");
    }

    public async Task FetchPlayerDataAsync(bool logging)
    {
        // Listen for fetch timeout
        StartCoroutine(PlayerFetchTimeoutHandler());

        // Fetch player data from the js script
        try
        {
            var newData = new Dictionary<string, object>
            {
                { "action", "fetch" },
            };

            playerResponse = await CloudCodeService.Instance.CallEndpointAsync<StringResultType>("UserManager", newData);
            avatarID = Regex.Match(playerResponse.message, @"avatarid:(.*?):avatarid").Groups[1].Value;
            playerBudget = Regex.Match(playerResponse.message, @"budget:(.*?):budget").Groups[1].Value;
            playerName = Regex.Match(playerResponse.message, @"displayname:(.*?):displayname").Groups[1].Value;
            playerScore = Regex.Match(playerResponse.message, @"score:(.*?):score").Groups[1].Value;
            setupState = Regex.Match(playerResponse.message, @"setupstate:(.*?):setupstate").Groups[1].Value;
            teamID = Regex.Match(playerResponse.message, @"teamid:(.*?):teamid").Groups[1].Value;
            votedIds = Regex.Match(playerResponse.message, @"votedids:(.*?):votedids").Groups[1].Value;
            recievedVotes = Regex.Match(playerResponse.message, @"recievedvotes:(.*?):recievedvotes").Groups[1].Value;

            // Cache recieved data
            if (!string.IsNullOrEmpty(playerName))
            {
                PlayerPrefs.SetString("cachedteamid", teamID);
                if (logging)
                {
                    Debug.Log("Player data successfuly fetched!");
                }

                // If we are in oob, handle ui based on setupstate
                if (oobUIManager.oobUI.activeSelf && !setupstateHandled)
                {
                    StartCoroutine(SetupStateHandler());
                    return;
                }
            }
            else
            {
                Debug.Log("Incorrent player data!");
                ErrorHandler("Error","Incorrent player data!");
                SignOutAndClearSession();
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
            ErrorHandler("Error","Failed to fetch player data!");
            SignOutAndClearSession();
        }
        SpinnerHandler("disable");
    }

    private IEnumerator PlayerFetchTimeoutHandler()
    {
        playerResponse = null;
        yield return new WaitForSeconds(3.0f);
        if (playerResponse == null)
        {
            Debug.Log("Timeout!");
            var respone = FetchPlayerDataAsync(true);
        }
    }

    public async Task FetchTeamDataAsync(string teamid, bool logging)
    {
        // Listen for fetch timeout
        StartCoroutine(TeamFetchTimeoutHandler());

        if (teamid == "0")
        {
            teamid = "";
        }

        // Fetch team data from JS script
        try
        {            
            var newData = new Dictionary<string, object>
            {
                { "action", "fetch" },
                { "teamId", teamid }
            };

            teamResponse = await CloudCodeService.Instance.CallEndpointAsync<StringResultType>("TeamManager", newData);

            // Check for unsuccessful response
            if (teamResponse.message.Contains("doesn't exist") || teamResponse.message.Contains("can't be empty"))
            {
                ErrorHandler("Notice","Your team no longer exists, or you are no longer part of it!");
                teamID = "";
                PlayerPrefs.SetString("cachedteamid", "");
                await UpdatePlayerDataAsync("setteamid","0");
                setupState = "1";
                await UpdatePlayerDataAsync("setsetupstate","1");
                StartCoroutine(oobUIManager.MoveUI(oobUIManager.teamStartupPage));
                oobUIManager.oobUI.SetActive(true);
                gameUIManager.gameUI.SetActive(false);
                gameUIManager.navbarUI.SetActive(false);
                SpinnerHandler("disable");
                return;
            }

            // Continue on successful response
            if (!string.IsNullOrEmpty(teamResponse.message))
            {
                teamMembers = Regex.Match(teamResponse.message, @"members:(.*?):members").Groups[1].Value;
                teamName = Regex.Match(teamResponse.message, @"name:(.*?):name").Groups[1].Value;
                teamScore = Regex.Match(teamResponse.message, @"score:(.*?):score").Groups[1].Value;
                chatHistory = Regex.Match(teamResponse.message, @"chat:(.*?):chat").Groups[1].Value;
                teamOwner = Regex.Match(teamResponse.message, @"owner:(.*?):owner").Groups[1].Value;
                teamGoal = Regex.Match(teamResponse.message, @"goal:(.*?):goal").Groups[1].Value;
                teamSave = Regex.Match(teamResponse.message, @"ve:(.*?):save").Groups[1].Value;

                if (!teamMembers.Contains(PlayerId))
                {
                    ErrorHandler("Notice","You are no longer in this team!");
                    await UpdatePlayerDataAsync("setteamid", "0");
                    await UpdatePlayerDataAsync("setsetupstate", "1");
                    SignOutAndClearSession(); // Desperate times, desperate measures x2
                    return;
                }

                string newdata = "";
                if (!oobUIManager.oobUI.activeSelf && !string.IsNullOrEmpty(lastchatHistory))
                {
                    // Calculate difference between new and last chat history to detect changes
                    if (lastchatHistory.Length < chatHistory.Length)
                    {
                        newdata = chatHistory.Replace(lastchatHistory, "");
                        StartCoroutine(messageManager.MessageHistoryHandler(newdata));
                        gameUIManager.TeamMemberListHandler();
                    }
                    else if (messageManager.Content.transform.childCount == 0)
                    {
                        StartCoroutine(messageManager.MessageHistoryHandler(chatHistory));
                        gameUIManager.TeamMemberListHandler();
                    }
                    lastchatHistory = chatHistory;
                }

                lastchatHistory = chatHistory;

                if (logging)
                {
                    Debug.Log("Team data successfully fetched!");
                }

                // If we are in oob, handle ui based on setupstate
                if (oobUIManager.oobUI.activeSelf && !setupstateHandled)
                {
                    StartCoroutine(SetupStateHandler());
                }
            }
            else
            {
                Debug.Log("Bad response!");
            }
            SpinnerHandler("disable");
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
            SpinnerHandler("disable");
        }
    }

    private IEnumerator TeamFetchTimeoutHandler()
    {
        teamResponse = null;
        yield return new WaitForSeconds(3.0f);
        if (teamResponse == null)
        {
            Debug.Log("Timeout!");
            var respone = FetchTeamDataAsync(!string.IsNullOrEmpty(teamID) ? teamID : PlayerPrefs.GetString("cachedteamid"), true);
        }
    }

    private IEnumerator SetupStateHandler()
    {
        if (gameUIManager.gameUI.activeSelf) yield break;
        setupstateHandled = true;
        SpinnerHandler("enable");
        // Handle UI based on setupstate progress
        switch (setupState)
        {
            case "0": // No setup has been done
            StartCoroutine(oobUIManager.MoveUI(oobUIManager.avatarPage));
            Debug.Log("Setupstate handled!");
            SpinnerHandler("disable");
            break;
            case "1": // Avatar setup has been done
            StartCoroutine(oobUIManager.MoveUI(oobUIManager.teamStartupPage));
            Debug.Log("Setupstate handled!");
            SpinnerHandler("disable");
            break;
            case "2": // Team setup has been done
            if (string.IsNullOrEmpty(teamScore))
            {
                SpinnerHandler("enable");
                var respone = FetchTeamDataAsync(!string.IsNullOrEmpty(teamID) ? teamID : PlayerPrefs.GetString("cachedteamid"), true);
                yield return new WaitForSeconds(1);
                if (string.IsNullOrEmpty(teamScore)) // don't wait for exception
                {
                    yield return new WaitForSeconds(2);
                    if (string.IsNullOrEmpty(teamScore)) setupstateHandled = false; yield break; // Fetch failed, abort mission
                }
            }
            oobUIManager.AvatarLoadHandler();
            Task task = FetchPhotoListAsync();
            yield return new WaitUntil(() => task.IsCompleted);
            oobUIManager.logouticonButton.gameObject.SetActive(false);
            gameUIManager.Startup();
            Debug.Log("Setupstate handled!");
            SpinnerHandler("disable");
            break;
        }
    }

    public async Task<string> FetchMemberDataAsync(string playerid)
    {
        // Fetch player cloudsave data with js script
        try
        {
            var newData = new Dictionary<string, object>
            {
                { "action", "customfetch" },
                { "customstring", playerid },
            };
            var response = await CloudCodeService.Instance.CallEndpointAsync<FetchResultType>("UserManager", newData);
            var responseList = string.Join(Environment.NewLine, response.message[response.message.Keys.ToList()[1]]);
            string[] responeValues = ExtractValues(responseList); //avatarid, budget, displayname, setupstate, score, teamid, votedids - order
            return responeValues[0] + ";" + responeValues[2] + ";" + responeValues[4]; // avatar, name, score
        }
        catch (Exception ex)
        {
            Debug.Log("Failed to fetch custom player data!");
            Debug.Log(ex);
            return null;
        }
    }

    public async Task<List<string>> FetchTeamScoreAsync()
    {
        // Fetch team cloudsave data with js script
        try
        {
            var newData = new Dictionary<string, object>
            {
                { "action", "fetchteamscore" },
            };
            var response = await CloudCodeService.Instance.CallEndpointAsync<ListResultType>("TeamManager", newData);
            return response.message;
        }
        catch (Exception ex)
        {
            Debug.Log("Failed to fetch team score!");
            Debug.Log(ex);
            return null;
        }
    }

    public async Task UpdatePlayerDataAsync(string action, string customstring)
    {
        // Update player cloudsave data with js script
        try
        {
            var data = new Dictionary<string, object>
            {
                { "action", action },
                { "customstring", customstring }
            };
            await CloudCodeService.Instance.CallEndpointAsync<StringResultType>("UserManager", data);
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }

    public async void CallTeamManagerAsync(string teamId, string action, string custom)
    {
        // Call teammanager js script
        if (action == "create" || action == "join")
        {
            SpinnerHandler("enable");
            if (action == "join" && string.IsNullOrEmpty(teamId))
            {
                ErrorHandler("Notice","Team code can't be empty!");
                SpinnerHandler("disable");
                return;
            }
            else if (action == "create" && string.IsNullOrEmpty(custom))
            {
                ErrorHandler("Notice","Team name can't be empty!");
                SpinnerHandler("disable");
                return;
            }
            if (action == "leave" && string.IsNullOrEmpty(custom)) // js can't handle it being empty correctly so we fill it
            {
                custom = "a";
            }
        }

        try 
        {
            var data = new Dictionary<string, object>
            {
                { "teamId", teamId },
                { "action", action },
                { "customstring", custom }
            };
            var response = await CloudCodeService.Instance.CallEndpointAsync<StringResultType>("TeamManager", data);
            string[] errorCode = {"0","1","2","3","4","5"};
            // Handle returned error code
            if (!errorCode.Contains(response.message))
            {
                ErrorHandler("Error",response.message);
                SpinnerHandler("disable");
                return;
            }
            else if (action == "join" || action == "create")
            {
                if (action == "create") ErrorHandler("Notice", "Your team code is: " + teamId + ".\nShare it with your friends!");
                Debug.Log("Team created/joined with id: " + teamId);
                teamID = teamId;
                await UpdatePlayerDataAsync("setsetupstate","2"); // Await this or shit hits the fan
                await UpdatePlayerDataAsync("setteamid", teamId); 
                setupstateHandled = true;
                await FetchPlayerDataAsync(true);
                await FetchTeamDataAsync(!string.IsNullOrEmpty(teamID) ? teamID : PlayerPrefs.GetString("cachedteamid"), false);
                await FetchPhotoListAsync();
                gameUIManager.Startup();
            }
        }
        catch (CloudCodeException ex)
        {
            Debug.Log(ex);
            ErrorHandler("Error","Something went wrong, try again later!");
            SpinnerHandler("disable");
            return;
        }
    }

    public async Task FetchPhotoListAsync()
    {
        // Attempt to call teammanager with js script
        try 
        {
            var data = new Dictionary<string, object>
            {
                { "teamId", "" },
                { "action", "fetchphotos" },
                { "customstring", "" }
            };
            var response = await CloudCodeService.Instance.CallEndpointAsync<StringResultType>("TeamManager", data);
            photoList = response.message;
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }

    public string GenerateRandomString(int length)
    {
        // Helper script for team creation
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        char[] result = new char[length];
        byte[] randomBytes = new byte[length];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        for (int i = 0; i < length; i++)
        {
            result[i] = chars[randomBytes[i] % chars.Length];
        }
        return new string(result);
    }

    private static string[] ExtractValues(string jsonString)
    {
        // Helper script for playerdata fetch
        List<string> values = new List<string>();
        MatchCollection matches = Regex.Matches(jsonString, @"""value"":\s*""(.*?)""");
        foreach (Match match in matches)
        {
            values.Add(match.Groups[1].Value);
        }
        return values.ToArray();
    }

    public void SpinnerHandler(string state)
    {
        // Loading UI handler
        if (state == "enable")
        {
            spinnerUI.SetActive(true);
            blockerUI.SetActive(true);
            spinnerUI.GetComponent<Animator>().Play("Spinning");
        }
        else
        {
            spinnerUI.SetActive(false);
            blockerUI.SetActive(false);
        }
    }

    public void ErrorHandler(string title, string message)
    {
        // Error message handler
        SpinnerHandler("disable");
        errorPanel.SetActive(true);
        errorText.text = message;
        errorPanel.transform.Find("ErrorHeader").GetComponent<TMP_Text>().text = title;
    }

    public void DeleteAccount()
    {
        AuthenticationService.Instance.DeleteAccountAsync();
        CallTeamManagerAsync(teamID, "leave", PlayerId);
        SignOutAndClearSession();
    }

    // Js script fetch with expected string result
    public class StringResultType
    {
        public string message;
    }

    // Js script fetch with expected dictionary result
    public class FetchResultType
    {
        public Dictionary<string, object> message;
    }

    // Js script fetch with expected list result
    public class ListResultType
    {
        public List<string> message;
    }
}
