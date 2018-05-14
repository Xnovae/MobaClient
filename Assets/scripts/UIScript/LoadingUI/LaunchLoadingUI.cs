using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace MyLib
{
    public class LaunchLoadingUI : IUserInterface
    {
        public enum ConfirmResultType
        {
            None,
            Ok,
            Cancel,
        }

        protected class LaodProgressTask
        {
            public float ProgressDelta;
            public float CurrentProgress;
        }

        public static LaunchLoadingUI Instance { get; set; }

        private UILabel m_LoadingBarTip;
 
        private UISprite m_LoadingBarFg;
        private float m_LoadingBarWidth = 335;

        private GameObject m_ConfirmPanel;
        private UILabel m_ConfirmPanelContent;

        private UIButton m_CancelButton;
        private UILabel m_CancelButtonText;
        private UIButton m_OkButton;
        private UILabel m_OkButtonText;

        private ConfirmResultType m_ConfirmResult = ConfirmResultType.None;
        private Stack<LaodProgressTask> m_loadTaskProgressDelta = new Stack<LaodProgressTask>();

        public ConfirmResultType ConfirmResult
        {
            get { return m_ConfirmResult; }
        }

        private LaunchLoadingUI()
        {
        }

        private void Awake()
        {
            Instance = this;
            m_LoadingBarTip = GetLabel("LoadingBarTip"); ;
            m_LoadingBarFg = GetSprite("LoadingBarFg");

            m_ConfirmPanel = GetName("ConfirmPanel");
            m_ConfirmPanelContent = GetLabel("Content");

            m_CancelButton = GetName("CancelButton").GetComponent<UIButton>();
            m_CancelButtonText = GetLabel("CancelText");
            m_OkButton = GetName("OkButton").GetComponent<UIButton>();
            m_OkButtonText = GetLabel("OkText");
        }

        public void ShowLoadingTip(string tip)
        {
            m_LoadingBarTip.text = tip;
        }

        public void SetLoadingBarProgressDelta(float progress)
        {
            var progressTask = m_loadTaskProgressDelta.Peek();
            progressTask.CurrentProgress += progress * m_LoadingBarWidth * progressTask.ProgressDelta;
            m_LoadingBarFg.width = (int)Mathf.Clamp(progressTask.CurrentProgress, 0, m_LoadingBarWidth);
        }

        public void SetLoadingBarProgress(float progress)
        {
            var progressTask = m_loadTaskProgressDelta.Peek();
            progressTask.CurrentProgress = progress * m_LoadingBarWidth * progressTask.ProgressDelta;
            m_LoadingBarFg.width = (int)Mathf.Clamp(progressTask.CurrentProgress, 0, m_LoadingBarWidth);
        }

        public IEnumerator ShowConfirmPanel(string content, string cancelText, Action cancelAction, string okText,
            Action okAction)
        {
            m_ConfirmResult = ConfirmResultType.None;

            bool hasCancelBtn = true;
            if (string.IsNullOrEmpty(cancelText) || cancelAction == null)
            {
                hasCancelBtn = false;
                m_CancelButton.gameObject.SetActive(false);
            }
            else
            {
                m_CancelButton.gameObject.SetActive(true);
            }

            bool hasOkBtn = true;
            if (string.IsNullOrEmpty(okText) || okAction == null)
            {
                hasOkBtn = false;
                m_OkButton.gameObject.SetActive(false);
            }
            else
            {
                m_OkButton.gameObject.SetActive(true);
            }

            if (hasCancelBtn && hasOkBtn)
            {
                m_CancelButton.transform.localPosition = new Vector3(-124.2f, -100.9781f, 0);
                m_OkButton.transform.localPosition = new Vector3(124.2f, -100.9781f, 0);
            }
            else
            {
                m_CancelButton.transform.localPosition = new Vector3(0, -100.9781f, 0);
                m_OkButton.transform.localPosition = new Vector3(0, -100.9781f, 0);
            }

            m_CancelButtonText.text = cancelText;
            m_OkButtonText.text = okText;
            m_ConfirmPanelContent.text = content == null ? "" : content;

            m_CancelButton.onClick.Clear();
            m_OkButton.onClick.Clear();

            SetCallback("CancelButton", () =>
            {
                if (cancelAction != null)
                    cancelAction();
                m_ConfirmResult = ConfirmResultType.Cancel;
                m_ConfirmPanel.SetActive(false);
            });

            SetCallback("OkButton", () =>
            {
                if (okAction != null)
                    okAction();
                m_ConfirmResult = ConfirmResultType.Ok;
                m_ConfirmPanel.SetActive(false);

            });
            m_ConfirmPanel.SetActive(true);

            while (m_ConfirmResult == ConfirmResultType.None)
            {
                yield return null;
            }
        }

        public void PushLoadTaskProgressDelta(float delta)
        {
            var progressTask = new LaodProgressTask();
            progressTask.CurrentProgress = 0;
            progressTask.ProgressDelta = delta;
            m_loadTaskProgressDelta.Push(progressTask);
        }

        public void PopLoadTaskProgressDelta()
        {
            m_loadTaskProgressDelta.Pop();
        }
    }
}