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
    [SerializeField] private Text passwordInput = null;
    [SerializeField] private Text errorInput = null;

    [Header("Register Panel UI")]
    [SerializeField] private Text nameInput = null;
    [SerializeField] private Text emailInput = null;
    [SerializeField] private Text passInput = null;
    [SerializeField] private Text confirmPassInput = null;

    [Header("Values")]
    // [SerializeField] private Text points = null;
    [SerializeField] private Text username = null;
    private PlayerData playerData;
    private DatabaseReference databaseReference;

    [Header("Panels")]
    [SerializeField] private GameObject accountPanel = null;
    [SerializeField] private GameObject mainSelectPanel = null;

    void Start() {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://orbital-pacman.firebaseio.com/");

        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
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
                Debug.Log("test");
            }

        });
    }

    public void LogOut() {
        if (FirebaseAuth.DefaultInstance.CurrentUser == null) return;
        FirebaseAuth.DefaultInstance.SignOut();
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

                accountPanel.SetActive(false);
                mainSelectPanel.SetActive(true);
            }

        });
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
                errorInput.color = Color.green;
                errorInput.text = "Registration Complete!";
            }

        });
        
    }

    void WriteNewUser() {
        PlayerData data = new PlayerData(nameInput.text, emailInput.text, passInput.text);
        string json = JsonUtility.ToJson(data);

        databaseReference.Child("Users").Child(emailInput.text).SetRawJsonValueAsync(json);
        Debug.Log("test");
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

    public void LoadPoints() {}

    public void LoadName() {
        Debug.Log("Loading name");

        accountPanel.SetActive(false);
        mainSelectPanel.SetActive(true);
        
        // get current user
        Firebase.Auth.FirebaseUser currentUser = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;

        Debug.Log(currentUser.Email);

        FirebaseDatabase.DefaultInstance.GetReference("/Users/" + currentUser.Email + "/username").GetValueAsync().
        ContinueWith(task => {
            if (task.IsFaulted) {
                Debug.Log("Name cannot be found!");
            }

            if (task.IsCompleted) {
                DataSnapshot snapshot = task.Result;
                string name = snapshot.GetRawJsonValue();

                username.text = name;
            }
        });
    }

    public void GetErrorMessage(AuthError err) {
        string message = "";
        message = err.ToString();
        Debug.Log(message);

        switch (err) {
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
