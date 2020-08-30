using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Unity.Editor;
using Firebase.Database;

public class DataBridge : MonoBehaviour
{
    public Text nameInput, emailInput, passwordInput, confirmPasswordInput, errorInput;

    [Header("Values")]
    // [SerializeField] private Text points = null;
    [SerializeField] private Text username = null;
    private PlayerData playerData;
    private DatabaseReference databaseReference;
    void Start()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://orbital-pacman.firebaseio.com/");

        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void RegisterAndSaveData() {

        if (emailInput.text.Equals("") || passwordInput.text.Equals("") || nameInput.text.Equals("")) {
            errorInput.text = "Please enter a valid username, email or password!";
            return;
        }

        if (!passwordInput.text.Equals(confirmPasswordInput.text)) {
            errorInput.text = "Please check that the password is entered correctly!";
            return;
        }

        FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(emailInput.text, passwordInput.text).
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
                errorInput.color = Color.green;
                errorInput.text = "Registration Complete!";
            }

        });

        PlayerData data = new PlayerData(nameInput.text, emailInput.text, passwordInput.text);
        string json = JsonUtility.ToJson(data);

        databaseReference.Child("Users").Child(emailInput.text).SetRawJsonValueAsync(json);
    }

    public void LoadPoints() {}

    public void LoadName() {
        Debug.Log("Loading name");
        
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
            case AuthError.EmailAlreadyInUse:
                errorInput.text = "Email already exists in database.";
                break;
            case AuthError.MissingPassword:
                errorInput.text = "Please enter a valid password.";
                break;
            case AuthError.WrongPassword:
                errorInput.text = "Wrong Password!";
                break;
            case AuthError.WeakPassword:
                errorInput.text = "Please enter a password with more than 6 characters.";
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
