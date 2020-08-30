using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Unity.Editor;
using Firebase.Database;

public class AccountManager : MonoBehaviour
{
    [Header("Log-In Panel UI")]
    [SerializeField] private Text accountInput = null;
    [SerializeField] private InputField passwordInput = null;
    [SerializeField] private Text errorInput = null;

    [Header("Register Panel UI")]
    [SerializeField] private Text nameInput = null;
    [SerializeField] private Text emailInput = null;
    [SerializeField] private InputField passInput = null;
    [SerializeField] private InputField confirmPassInput = null;

    [Header("Values")]
    [SerializeField] private Text points = null;
    [SerializeField] private Text username = null;
    private PlayerData playerData;
    private DatabaseReference databaseReference;

    [Header("UI")]
    [SerializeField] private Button enterButton = null;
    [SerializeField] private GameObject landingPagePanel = null;
    [SerializeField] private GameObject mainSelectPanel = null;
    [SerializeField] private GameObject mainPageAnimation = null;
    [SerializeField] private Text changeNameInput = null;
    [SerializeField] private InputField changePassInput = null;
    [SerializeField] private InputField confirmPasswordInput = null;
    [SerializeField] private Text errorMessageText = null;

    private Firebase.Auth.FirebaseUser currentUser = null;

    void Start() {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://orbital-pacman.firebaseio.com/");

        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        int prevScore = PlayerPrefs.GetInt("PlayerScore");

        if (FirebaseAuth.DefaultInstance.CurrentUser != null) {
            currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
            mainPageAnimation.SetActive(false);

            int totalPoints = PlayerPrefs.GetInt("CurrentPoints") + (prevScore/100);
            Debug.Log(totalPoints);
            UpdatePoints(totalPoints);

            PlayerPrefs.SetInt("PlayerScore", 0); // set to zero to prevent duplicate counting
            points.text = "" + totalPoints;
            SavePoints(); // to sync the points received up till now

            username.text = PlayerPrefs.GetString("UserName");
            landingPagePanel.SetActive(false);
            mainSelectPanel.SetActive(true);
            
        } else {
            PlayerPrefs.SetInt("CurrentPoints", 0);
        }
    }

    public void SaveName() {
        PlayerPrefs.SetString("UserName", username.text);
    }

    public void SavePoints() {
        PlayerPrefs.SetInt("CurrentPoints", Int32.Parse(points.text));
    }

    public void LogIn() {
        
        FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(accountInput.text, passwordInput.text).
        ContinueWith(task => {
            if (task.IsCanceled) {
                Debug.Log("Log-In cancelled");
                return;
            }
            if (task.IsFaulted) {
                Firebase.FirebaseException e = 
                task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;

                GetErrorMessage((AuthError) e.ErrorCode);

                return;
            }
            if (task.IsCompleted) {
                LoadName();
                LoadPoints();
                Debug.Log("test");
                enterButton.interactable = true;
            }

        });

        currentUser = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
        Debug.Log("test2");
    }

    public void LogOut() {
        if (currentUser == null) return;
        
        FirebaseAuth.DefaultInstance.SignOut();
        
        currentUser = null;

        // fix the UI
        points.text = "" + 0;
        username.text = "Guest";
    }

    public void LogInGuest() {
        FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync().ContinueWith(task => {
            if (task.IsCanceled) {
                return;
            }
            if (task.IsFaulted) {
                Firebase.FirebaseException e = 
                task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;

                GetErrorMessage((AuthError) e.ErrorCode);

                return;
            }
            if (task.IsCompleted) {
                Debug.Log("Successfully logged in!");
            }

        });

        currentUser = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
        Debug.Log("test2");
    }

    public void RegisterAndSaveData() {

        if (emailInput.text.Equals("") || passInput.text.Equals("") || nameInput.text.Equals("")) {
            errorInput.text = "Please enter a valid username, email or password!";
            return;
        }

        if (!passInput.text.Equals(confirmPassInput.text)) {
            errorInput.text = "Please check that the password is entered correctly!";
            return;
        }

        if (passInput.text.Length < 7) {
            errorInput.text = "Please use a password longer than 6 characters!";
            return;
        }

        FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(emailInput.text, passInput.text).
        ContinueWith(task => {
            if (task.IsCanceled) {
                Firebase.FirebaseException e = 
                task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;

                GetErrorMessage((AuthError) e.ErrorCode);

                return;
            }
            if (task.IsFaulted) {
                Firebase.FirebaseException e = 
                task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;

                GetErrorMessage((AuthError) e.ErrorCode);

                return;
            }
            if (task.IsCompleted) {
                WriteNewUser();
                errorInput.text = "Registration Complete!";
            }

        });
        
    }

    public void WriteNewUser() {
        string[] strList = emailInput.text.Split('.');
        string savedEmail = strList[0] + strList[1];
        Debug.Log(savedEmail);

        PlayerData data = new PlayerData(nameInput.text, emailInput.text, passInput.text, 0);
        Debug.Log("This player is " + emailInput.text + " ," + nameInput.text + ", " + passInput.text);
        string json = JsonUtility.ToJson(data);

        databaseReference.Child("Users").Child(savedEmail).SetRawJsonValueAsync(json);
    }

    public void ChangeUsername() {
        string[] strList = currentUser.Email.Split('.');
        string savedEmail = strList[0] + strList[1];
        Debug.Log(savedEmail);

        databaseReference.Child("Users").Child(savedEmail).Child("username").SetValueAsync(changeNameInput.text);
        LoadName();
    }

    public void ChangePassword() {
        string[] strList = currentUser.Email.Split('.');
        string savedEmail = strList[0] + strList[1];
        Debug.Log(savedEmail);

        if (changePassInput.text != confirmPasswordInput.text) {
            errorMessageText.text = "Please confirm that the inputs are the same!";
        } else {
            errorMessageText.color = Color.green;
            errorMessageText.text = "Password changed successfully!";
        }

        currentUser.UpdatePasswordAsync(changePassInput.text).ContinueWith(task => {
            if (task.IsFaulted) {
                Firebase.FirebaseException e = 
                task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;

                GetErrorMessage((AuthError) e.ErrorCode);

                return;
            }
            if (task.IsCanceled) {
                Firebase.FirebaseException e = 
                task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;

                GetErrorMessage((AuthError) e.ErrorCode);

                return;
            }
            if (task.IsCompleted) {
                databaseReference.Child("Users").Child(savedEmail).Child("password").SetValueAsync(changePassInput.text);
                Debug.Log("Password changed");
            }
        });
        
    }

    public void WriteNewItemStorage() {
        string[] strList = emailInput.text.Split('.');
        string savedEmail = strList[0] + strList[1];
        Debug.Log("Creating new item storage...");

        ItemData data = new ItemData(new bool[4]{true, true, true, true}, new bool[2]{true, true}, 
        new bool[3]{true, true, true}, new bool[4], new bool[2]);
        string json = JsonUtility.ToJson(data);

        databaseReference.Child("Items").Child(savedEmail).SetRawJsonValueAsync(json);
    }

    public void UpdatePoints(int totalPoints) {
        Debug.Log("The current email is " + currentUser.Email);
        string[] strList = currentUser.Email.Split('.');
        string savedEmail = strList[0] + strList[1];

        FirebaseDatabase.DefaultInstance.GetReference("/Users/" + savedEmail + "/points").SetValueAsync(totalPoints);
    }

    public void ClearRegistrationPage() {
        nameInput.text = null;
        emailInput.text = null;
        passInput.text = null;
        confirmPassInput.text = null;
    }

    public void ClearLogInPage() {
        accountInput.text = null;
        passwordInput.text = null;
    }

    public void LoadPoints() {
        Debug.Log("Loading points");

        // get current user
        Debug.Log(currentUser.Email);

        string[] strList = currentUser.Email.Split('.');
        string storedEmail = strList[0] + strList[1];

        FirebaseDatabase.DefaultInstance.GetReference("/Users/" + storedEmail + "/points").GetValueAsync().
        ContinueWith(task => {
            if (task.IsFaulted) {
                Debug.Log("Points cannot be found!");
            }

            if (task.IsCompleted) {
                DataSnapshot snapshot = task.Result;
                int pointCount = Int32.Parse(snapshot.GetRawJsonValue());
                
                Debug.Log(pointCount);
                points.text = "" + pointCount;
            }
        });
    }

    public void LoadName() {
        Debug.Log("Loading name");
        
        // get current user
        Debug.Log(currentUser.Email);

        string[] strList = currentUser.Email.Split('.');
        string storedEmail = strList[0] + strList[1];

        FirebaseDatabase.DefaultInstance.GetReference("/Users/" + storedEmail + "/username").GetValueAsync().
        ContinueWith(task => {
            if (task.IsFaulted) {
                Debug.Log("Name cannot be found!");
            }

            if (task.IsCompleted) {
                DataSnapshot snapshot = task.Result;
                string name = snapshot.GetRawJsonValue();
                
                Debug.Log(name);
                username.text = name;
            }
        });
    }

    public void GetErrorMessage(AuthError err) {
        string message = "";
        message = err.ToString();
        Debug.Log(message);

        switch (err) {
            case AuthError.MissingEmail:
                errorInput.text = "Please enter an email!";
                break;
            case AuthError.UserNotFound:
                errorInput.text = "This email is not registered yet!";
                break;
            case AuthError.AccountExistsWithDifferentCredentials:
                errorInput.text = "Email already exists in database.";
                break;
            case AuthError.MissingPassword:
                errorInput.text = "Please enter a valid password.";
                break;
            case AuthError.WrongPassword:
                errorInput.text = "Wrong Password!";
                break;
            case AuthError.InvalidEmail:
                errorInput.text = "Email is invalid!";
                break;
            default:
                errorInput.text = err.ToString();
                break;
        }
    }


}
