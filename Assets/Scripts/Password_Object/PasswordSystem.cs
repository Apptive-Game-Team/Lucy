using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Password_Object
{
    public class PasswordSystem : MonoBehaviour
    {
        [SerializeField] private string currentPassword = "";

        private string password = "";

        PasswordObject passwordObject;

        Canvas passwordCanvas;

        TMP_Text text;

        [SerializeField] List<Button> buttons;

        private void Awake()
        {
            // includeInactive: the password canvas is disabled most of the time.
            text = gameObject.GetComponentInChildren<TMP_Text>(true);
            passwordCanvas = gameObject.GetComponentInChildren<Canvas>(true);
        }

        void Start()
        {
            InitButtonsOnClick();
            passwordCanvas.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            currentPassword = "";
            text.SetText(currentPassword);
        }

        public void ClosePasswordPage()
        {
            passwordCanvas.gameObject.SetActive(false);
        }

        public void SetPassword(string password, PasswordObject passwordObject)
        {
            this.passwordObject = passwordObject;
            passwordCanvas.gameObject.SetActive(true);
            this.password = password;
            currentPassword = "";
            text.SetText(currentPassword);
        }

        private void InitButtonsOnClick()
        {
            if (buttons.Count < 12)
            {
                Debug.LogError("PasswordSystem needs 12 buttons: 0-9, clear, confirm");
                return;
            }

            for (int i = 0; i < 10; i++) {
                int index = i;
                buttons[i].onClick.AddListener(() =>
                {
                    currentPassword += index.ToString();
                    text.SetText(currentPassword);
                
                });
            }

            buttons[10].onClick.AddListener(() =>
            {
                currentPassword = "";
                text.SetText(currentPassword);
            });

            buttons[11].onClick.AddListener(() =>
            {
                if (password == currentPassword && passwordObject != null)
                {
                    passwordObject.Unlock();
                    passwordCanvas.gameObject.SetActive(false);
                } else
                {
                    currentPassword = "";
                    text.SetText(currentPassword);
                }
            });
        }
    }
}
