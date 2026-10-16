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
        private PasswordObject passwordObject;
        private Canvas passwordCanvas;
        private TMP_Text text;
        [SerializeField] private List<Button> buttons;

        private void Awake()
        {
            text = GetComponentInChildren<TMP_Text>(true);
            passwordCanvas = GetComponentInChildren<Canvas>(true);
            InitButtonsOnClick();
            ClosePasswordPage();
        }

        private void OnEnable() { ClearInput(); }
        private void OnDisable() { passwordObject = null; ClearInput(); }

        private void ClearInput()
        {
            currentPassword = "";
            if (text != null) text.SetText(currentPassword);
        }

        public void ClosePasswordPage()
        {
            passwordObject = null;
            password = "";
            ClearInput();
            if (passwordCanvas != null) passwordCanvas.gameObject.SetActive(false);
        }

        public void SetPassword(string password, PasswordObject target)
        {
            if (target == null || passwordCanvas == null || string.IsNullOrEmpty(password)) return;
            this.password = password;
            passwordCanvas.gameObject.SetActive(true);
            passwordObject = target;
            ClearInput();
        }

        private bool CanEnter => passwordObject != null && passwordCanvas != null &&
            passwordCanvas.gameObject.activeInHierarchy && !Lucy.Presentation.GameSession.BlocksGameplayInput;

        private void InitButtonsOnClick()
        {
            if (buttons == null) return;
            for (int i = 0; i < 10 && i < buttons.Count; i++)
            {
                int digit = i;
                if (buttons[i] == null) continue;
                buttons[i].onClick.AddListener(() =>
                {
                    if (!CanEnter || currentPassword.Length >= password.Length) return;
                    currentPassword += digit.ToString();
                    if (text != null) text.SetText(currentPassword);
                });
            }
            if (buttons.Count > 10 && buttons[10] != null)
                buttons[10].onClick.AddListener(() => { if (CanEnter) ClearInput(); });
            if (buttons.Count > 11 && buttons[11] != null)
                buttons[11].onClick.AddListener(() =>
                {
                    if (!CanEnter) return;
                    if (password != currentPassword) { ClearInput(); return; }
                    var target = passwordObject;
                    ClosePasswordPage();
                    if (target != null) target.Unlock();
                });
        }
    }
}
