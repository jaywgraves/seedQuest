﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace SeedQuest.Interactables
{
    public enum InteractableUIMode { NextPrevSelect, GridSelect, ListSelect, Dialogue };

    [System.Serializable]
    public class InteractableUI
    { 
        public string name = "";
        public int fontSize = 36;
        public float scaleSize = 1;
        public InteractableUIMode mode;
        public bool useRotateToCamera = true;
        public Vector3 rotationOffset = new Vector3(0, 0, 0);
        public Vector3 positionOffset = new Vector3(0, 0, 0);
        public Vector3 buttonOffset = new Vector3(0, 0, 0);
        public Vector3 labelOffset = new Vector3(0, 0, 0);
        public float scalingCoefficient = 0f;
        public GameObject debugActionUI = null;

        private Interactable parent;
        private GameObject actionUI = null;
        private TMPro.TextMeshProUGUI persistentLabel;
        private Button labelButton;
        private Button[] actionButtons;
        private Button checkButton;
        private Image[] checkImages;
        private Image[] actionButtonImages;
        private TMPro.TextMeshProUGUI actionUITextMesh;
        private RectTransform actionUIRect;
        private RectTransform actionTextRect;
        private RectTransform progressButtonRect;
        private RectTransform actionButtonRect1;
        private RectTransform actionButtonRect2;
        private RectTransform parentCanvasRect;
        private RectTransform labelRect;
        private RectTransform trackerRect;
        private Transform playerTransform;

        private Vector3 progressPosition;
        private Vector3 actionPosition1;
        private Vector3 actionPosition2;
        private Vector3 labelPosition;
        private Vector3 trackerPosition = new Vector3(-123, 0, 0);
        private Vector3 labelScale = new Vector3(1, 1, 1);
        private Vector3 buttonScale = new Vector3(1.5f, 1.5f, 1f);
        private Vector3 trackerScale = new Vector3(0.5f, 0.5f, 0.5f);

        private UIScaler scaler;

        private Canvas parentCanvas;
        private static HUD.ScreenspaceActionUI screenspaceAction;
        private bool useScaleToCamera = false;

        Camera c;

        private ProgressButton progressButton;

        private bool dialogueSelected = false;

        public void Update() {
            if (isReady()) {
                if (InteractableManager.Instance.useSeparatedUI)
                    ResetScreenspaceCanvas();
                SetScale();
                SetPosition();
                SetRotation();
            }

            if (InteractableManager.Instance.useInteractableNames)
            {
                if (persistentLabel.text != actionUITextMesh.text)
                {
                    persistentLabel.gameObject.SetActive(true);
                    if (InteractableManager.Instance.useSeparatedUI && (GameManager.State == GameState.Interact || GameManager.State == GameState.Play))
                        SetCanvasToScreenspace();

                }
                else
                {
                    if (persistentLabel.gameObject.activeSelf)
                    {
                        persistentLabel.gameObject.SetActive(false);
                        if (InteractableManager.Instance.useSeparatedUI)
                            screenspaceAction.deactivate();

                        Color temp = actionUI.GetComponentInChildren<Image>().color;
                        temp.a = 0.0f;
                        actionUI.GetComponentInChildren<Image>().color = temp;
                    }
                }
            }
        }

        public GameObject ActionUI
        {
            get { return actionUI; }
        }

        public RectTransform ActionUIRect
        {
            get { return actionUIRect; }
        }

        /// <summary> Initialize Interactable UI with Prompt Text and Buttons </summary>
        /// <param name="interactable">Parent Interactable Object</param>
        public void Initialize(Interactable interactable) {
            parent = interactable;

            if (interactable.flagDeleteUI)
                return;

            int modeIndex = 0;
            modeIndex = mode == InteractableUIMode.GridSelect ? 1 : modeIndex;
            modeIndex = mode == InteractableUIMode.ListSelect ? 2 : modeIndex;
            modeIndex = mode == InteractableUIMode.Dialogue ? 3 : modeIndex;

            Transform UIContainer;
            if (!GameObject.Find("InteractableUIContainer")) {
                UIContainer = new GameObject("InteractableUIContainer").transform;
                UIContainer.parent = InteractableManager.Instance.transform;
            }
            else  {
                UIContainer = GameObject.Find("InteractableUIContainer").transform;
            }

            actionUI = GameObject.Instantiate(InteractableManager.Instance.actionSpotIcons[modeIndex], UIContainer);
            debugActionUI = actionUI;

            initializeComponentRefs();
            SetScale();
            SetupLabel();
            SetupActionButtons();
            SetupActionComponentRefs();
            SetupCheckButton();
            SetPosition();

            if (parent == InteractablePath.NextInteractable)
                ToggleTracker(true);
        }

        /// <summary> Ready Status of InteractableUI </summary>
        public bool isReady() {
            return actionUI != null;
        }

        /// <summary> Delete UI Object </summary>
        public void DeleteUI() {
            GameObject.Destroy(actionUI);
        }

        public void initializeComponentRefs()
        {
            progressButton = actionUI.GetComponentInChildren<ProgressButton>();
            labelButton = actionUI.GetComponentInChildren<Button>();
            c = Camera.main;
            actionUIRect = actionUI.GetComponent<RectTransform>();
            var textList = actionUI.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
            actionUITextMesh = textList[0];
            persistentLabel = textList[1];
            labelRect = persistentLabel.gameObject.GetComponent<RectTransform>();
            labelPosition = labelRect.anchoredPosition;
            actionTextRect = actionUITextMesh.gameObject.GetComponent<RectTransform>();
            parentCanvas = actionUI.GetComponentInParent<Canvas>();
            parentCanvasRect = parentCanvas.gameObject.GetComponent<RectTransform>();
            screenspaceAction = HUDManager.Instance.GetComponentInChildren<HUD.ScreenspaceActionUI>();
            progressButtonRect = progressButton.gameObject.GetComponent<RectTransform>();
            progressPosition = new Vector3(0, 0, 0);
            trackerRect = actionUI.gameObject.GetComponentsInChildren<Canvas>(true)[2].gameObject.GetComponent<RectTransform>();
            scaler = actionUI.GetComponentInChildren<UIScaler>();
            playerTransform = GameObject.FindWithTag("Player").transform;
        }

        public void SetupActionComponentRefs()
        {
            actionButtonRect1 = actionButtons[0].gameObject.GetComponent<RectTransform>();
            actionButtonRect2 = actionButtons[1].gameObject.GetComponent<RectTransform>();

            actionPosition1 = new Vector3(132, 0, 0);
            actionPosition2 = new Vector3(-132, 0, 0);
        }

        /// <summary> Intialize and Setupt Label Button </summary>
        public void SetupLabel() {
            actionUITextMesh.text = parent.Name;
            if (InteractableManager.Instance.useInteractableNames) persistentLabel.text = parent.Name;
            persistentLabel.gameObject.SetActive(false);

            labelButton.onClick.AddListener(delegate { onClickLabel(); });

            var textList = actionUI.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
            foreach (TMPro.TextMeshProUGUI text in textList)
                text.fontSize = fontSize;
        }

        /// <summary> Intialize and Setup Action Buttons </summary>
        public void SetupActionButtons() {
            Button[] buttons = actionUI.GetComponentsInChildren<Button>();

            if (mode == InteractableUIMode.NextPrevSelect)  {
                actionButtons = new Button[buttons.Length - 2];
            }
            else if (mode == InteractableUIMode.GridSelect || mode == InteractableUIMode.ListSelect || mode == InteractableUIMode.Dialogue) {
                actionButtons = new Button[buttons.Length - 1];
                checkImages = new Image[buttons.Length - 1];
            }

            System.Array.Copy(buttons, 1, actionButtons, 0, actionButtons.Length);

            if (mode == InteractableUIMode.NextPrevSelect) {
                actionButtons[0].onClick.AddListener(parent.NextAction);
                actionButtons[1].onClick.AddListener(parent.PrevAction);
            }

            else if (mode == InteractableUIMode.GridSelect || mode == InteractableUIMode.ListSelect || mode == InteractableUIMode.Dialogue) {
                for (int i = 0; i < 4; i++) {
                    var actionText = actionButtons[i].GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    actionText.text = parent.stateData.getStateName(i);
                    checkImages[i] = actionButtons[i].gameObject.GetComponentsInChildren<Image>()[1];
                }

                if (mode == InteractableUIMode.Dialogue) {
                    for (int i = 0; i < 4; i++) {
                        checkImages[i].gameObject.SetActive(false);
                    }
                    actionButtons[4].GetComponentInChildren<TMPro.TextMeshProUGUI>().text = parent.stateData.getPrompt();
                }
                else {
                    foreach (Image image in checkImages) {
                        image.gameObject.SetActive(false);
                    }
                }

                actionButtons[0].onClick.AddListener(delegate { ClickActionButton(0); });
                actionButtons[1].onClick.AddListener(delegate { ClickActionButton(1); });
                actionButtons[2].onClick.AddListener(delegate { ClickActionButton(2); });
                actionButtons[3].onClick.AddListener(delegate { ClickActionButton(3); });
            }

            hideActions();
        }

        /// <summary> Setup Checkmark Button for use with NextPrevSelect Button only </summary>
        public void SetupCheckButton() {
            if (mode == InteractableUIMode.NextPrevSelect) {
                Button[] buttons = actionUI.GetComponentsInChildren<Button>();
                checkButton = buttons[1];
                checkButton.onClick.AddListener(onClickCheck);
                checkButton.gameObject.SetActive(false);
            }
        }
        
        /// <summary> Handles Clicking the Label Button </summary>
        public void onClickLabel() {
            //parent.NextAction();
        }

        /// <summary> Handles Clicking an Action Button </summary>
        public void ClickActionButton(int actionIndex) {
            parent.DoAction(actionIndex);

            if (mode == InteractableUIMode.GridSelect || mode == InteractableUIMode.ListSelect)
                hideActions();

            else if (mode == InteractableUIMode.Dialogue) {
                actionButtons[5].transform.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = actionButtons[actionIndex].GetComponentInChildren<TMPro.TextMeshProUGUI>().text;
                dialogueSelected = true;
                hideActions();
            }

            if(GameManager.Mode == GameMode.Rehearsal) {
                if (actionIndex == InteractablePath.NextInteractable.ID.actionID)
                {
                    InteractableManager.SetActiveInteractable(parent, parent.ActionIndex);
                    InteractableLog.Add(parent, parent.ActionIndex);
                    InteractablePath.GoToNextInteractable();
                }
            }
            else if (GameManager.Mode == GameMode.Recall)
                InteractableLog.Add(parent, parent.ActionIndex);
        }

        /// <summary> Handles Clicking a Checkmark Button </summary>
        public void onClickCheck() {
            SetCheckButtonActive(false);

            if (GameManager.Mode == GameMode.Rehearsal &&
                parent == InteractablePath.NextInteractable &&
                parent.ActionIndex == InteractablePath.NextAction)
            {
                InteractableManager.SetActiveInteractable(parent, parent.ActionIndex);
                progressButton.checkmarkAnimate();
                InteractableLog.Add(parent, parent.ActionIndex);
                InteractablePath.GoToNextInteractable();

                if (mode == InteractableUIMode.NextPrevSelect)
                {
                    progressButton.SetActive(false);
                }
            }
            else if (GameManager.Mode == GameMode.Rehearsal)
            {
                progressButton.exAnimate();
            }
            else if (GameManager.Mode == GameMode.Recall)
            {
                progressButton.checkmarkAnimate();
                InteractableLog.Add(parent, parent.ActionIndex);
            }

        }

        /// <summary> Sets Label Text to Current Action and Activates Checkmark if necessary </summary>
        public void SetActionUI(int actionIndex) {
            Debug.Log(actionIndex);
            InteractableState state = parent.stateData.states[actionIndex];
            //SetText(parent.Name + ":\n "+ state.actionName);
            SetText(state.actionName);
            SetCheckmark(actionIndex);
        }

        /// <summary> Sets Label Text </summary>
        public void SetText(string text) {
            if (actionUI == null) return;
            actionUITextMesh.text = text;
        }

        /// <summary> Gets Label Text </summary>
        public string GetText() {
            return actionUITextMesh.text;
        }

        /// <summary> Show Action Button UI and Set Checkmark for Rehearsal Mode for ListSelect and GridSelect UI  </summary>
        public void showCurrentActions() {
            InteractableManager.hideAllInteractableUI();
            showActions();
            SetCheckImageActive();

            string label = GetText();
            InteractableManager.resetInteractableUIText();
            SetText(label);
        }

        /// <summary> Toogles Action Buttons </summary>
        public void toggleActions()  {
            bool isShown = actionButtons[0].gameObject.activeSelf;
            if (isShown)
                hideActions();
            else
                showActions();
        }

        /// <summary> Hide Action Button UI </summary>
        public void hideActions() {
            if (actionButtons == null) return;

            if (mode == InteractableUIMode.Dialogue) {
                for (int i = 0; i < 6; i++) {
                    if (i == 4) {
                        if (dialogueSelected) {
                            actionButtons[i].transform.localPosition = new Vector3(actionButtons[4].transform.localPosition.x, 125, 0);
                            actionButtons[i].transform.GetComponent<Image>().color = Color.gray;
                            actionButtons[5].transform.gameObject.SetActive(true);
                            return;
                        }
                        continue;
                    }
                    actionButtons[i].transform.gameObject.SetActive(false);
                }
            }

            else {
                foreach (Button button in actionButtons)
                {
                    button.transform.gameObject.SetActive(false);
                }
            }
        }

        /// <summary> Show Action Button UI </summary>
        public void showActions()  {
            if (actionButtons == null) return;

            if (mode == InteractableUIMode.Dialogue) {
                for (int i=0; i < 5; i++) {
                    actionButtons[i].transform.gameObject.SetActive(true);
                }

                if (dialogueSelected) {
                    dialogueSelected = false;
                    actionButtons[4].transform.localPosition = new Vector3(actionButtons[4].transform.localPosition.x, 70, 0);
                    actionButtons[4].transform.GetComponent<Image>().color = Color.white;
                    actionButtons[5].transform.gameObject.SetActive(false);
                }
            }
            else{
                foreach (Button button in actionButtons)
                    button.transform.gameObject.SetActive(true);
            }

        }

        /// <summary> Handles hovering over UI </summary>
        public void onHoverUI() {
            GameManager.State = GameState.Interact;
            showCurrentActions();
            InteractableManager.SetActiveInteractable(parent, parent.ActionIndex);
        }

        /// <summary> Handles exiting hovering UI </summary>
        public void offHoverUI() {
            GameManager.State = GameState.Play;
        }

        /// <summary> Sets UI Size Scale </summary>
        public void SetScale() {
            actionUIRect.localScale = new Vector3(-0.01f * scaleSize, 0.01f * scaleSize, 0.01f * scaleSize);
        }


        public void SetFontSize() {
            var textList = actionUI.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
            foreach (TMPro.TextMeshProUGUI text in textList)
                text.fontSize = fontSize;
        }

        /// <summary> Sets UI Position </summary>
        public void SetPosition() {
            actionUITextMesh.gameObject.SetActive(true);
            Vector3 labelPositionOffset = Vector3.zero;
            if (parent.stateData != null) labelPositionOffset = parent.stateData.labelPosOffset;
            Vector3 position = parent.transform.position + labelPositionOffset + positionOffset;
            actionUIRect.position = position;

            parentCanvas.renderMode = RenderMode.WorldSpace;
        }

        /*
        public void SetScreenspaceAction()
        {
            Vector2 lowPosVector2 = new Vector2(0, 0);
            Vector2 relativePos = c.WorldToViewportPoint(actionUI.transform.position);
            if (lowPosVector2 != new Vector2(0, 0))
                screenspaceAction.setAction(actionUITextMesh.text, relativePos, lowPosVector2);
            else
            {
                screenspaceAction.setAction(actionUITextMesh.text, relativePos, new Vector2(0, -80));
            }

            actionUITextMesh.gameObject.SetActive(false);
        }
        */

        public void SetCanvasToScreenspace()
        {
            Vector3 labelPositionOffset = Vector3.zero;
            if (parent.stateData != null) labelPositionOffset = parent.stateData.labelPosOffset;
            if (buttonOffset == new Vector3(0, 0, 0))
                buttonOffset = new Vector3(0, -200, 0);
            if (labelOffset == new Vector3(0, 0, 0))
                labelOffset = new Vector3(0, 50, 0);
            
            Vector2 worldPosition = c.WorldToViewportPoint(actionUI.transform.position);

            parentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            Vector3 relativePos = new Vector3(
                ((worldPosition.x * parentCanvasRect.sizeDelta.x) - (parentCanvasRect.sizeDelta.x * 0.5f)),
                ((worldPosition.y * parentCanvasRect.sizeDelta.y) - (parentCanvasRect.sizeDelta.y * 0.5f)), 0);

            relativePos += buttonOffset;

            labelRect.anchoredPosition = labelOffset - buttonOffset;
            progressButtonRect.anchoredPosition = relativePos;
            actionButtonRect1.anchoredPosition = relativePos + actionPosition1;
            actionButtonRect2.anchoredPosition = relativePos + actionPosition2;
            trackerRect.anchoredPosition = relativePos + trackerPosition  + labelOffset - buttonOffset;

            float scale = calculateScale();
            if (useScaleToCamera)
                setNewScale(scale);
        }

        public void ResetScreenspaceCanvas()
        {
            progressButtonRect.anchoredPosition = new Vector3(0, 0, 0);
            actionButtonRect1.anchoredPosition = new Vector3(0, 0, 0);
            actionButtonRect2.anchoredPosition = new Vector3(0, 0, 0);
            labelRect.anchoredPosition = labelPosition;
            actionButtonRect1.position = actionPosition1;
            actionButtonRect2.position = actionPosition2;
            trackerRect.anchoredPosition = trackerPosition;
            if (useScaleToCamera)
                resetScale();
        }

        /// <summary> Sets UI Rotation </summary>
        public void SetRotation()  {
            if (useRotateToCamera) {
                BillboardInteractable();
                actionUIRect.Rotate(rotationOffset);
            }
            else {
                actionUIRect.rotation = Quaternion.Euler(rotationOffset);
            }
        }

        /// <summary> Sets Billboarding for UI i.e. so UI follows camera </summary>
        public void BillboardInteractable() {
            Vector3 targetPosition = c.transform.position - (100 * c.transform.forward ) ;
            Vector3 interactablePosition = actionUI.transform.position;
            Vector3 lookAtDir = targetPosition - interactablePosition;

            Quaternion rotate = Quaternion.LookRotation(lookAtDir);
            actionUI.transform.rotation = rotate;
        }

        /// <summary> Activates Checkmark Button for use with NextPrevSelect </summary>
        private void SetCheckButtonActive(bool active) {
            if (mode == InteractableUIMode.NextPrevSelect) {
                //checkButton.gameObject.SetActive(active);
                progressButton.SetShow(true, 3.0f);
                progressButton.ProgressCompleteAction = onClickCheck;
            }
        }

        public void StartProgress() {
            if (mode == InteractableUIMode.NextPrevSelect) {
                if(progressButton.IsActive) {
                    progressButton.startProgress();
                }
            }
        }

        public void CheckProgress() {
            if (mode == InteractableUIMode.NextPrevSelect)  {
                if (progressButton.IsActive) {
                    progressButton.checkProgress();
                }
            }
        }

        public float ProgressTime {
            get {
                if (mode == InteractableUIMode.NextPrevSelect) {
                    return progressButton.ProgressTime;
                }
                else
                    return 0;
            }
        }

        public bool ProgressComplete { 
            get {
                if (mode == InteractableUIMode.NextPrevSelect)
                    return progressButton.ProgressComplete;
                else
                    return false;
            }
        }

        /// <summary> Activates Checkmark on GridSelect and ListSelect Buttons </summary>
        private void SetCheckImageActive() {
            if (mode == InteractableUIMode.GridSelect || mode == InteractableUIMode.ListSelect || mode == InteractableUIMode.Dialogue) {
                if (InteractablePath.isNextInteractable(parent))
                    checkImages[InteractablePath.NextInteractable.ID.actionID].gameObject.SetActive(true);
                else {
                    checkImages[0].gameObject.SetActive(false);
                    checkImages[1].gameObject.SetActive(false);
                    checkImages[2].gameObject.SetActive(false);
                    checkImages[3].gameObject.SetActive(false);
                }
            }
        }

        public float calculateScale()
        {
            float dist = Vector3.Distance(c.gameObject.transform.position, parent.gameObject.transform.position);
            float cameraToPlayer = Vector3.Distance(c.gameObject.transform.position, playerTransform.position);
            float scaleFloat = 0.2f + ((cameraToPlayer + scalingCoefficient) / dist);
            if (scaleFloat < .5)
                scaleFloat = 0.5f;
            else if (scaleFloat > 1.5)
                scaleFloat = 1.5f;

            //Debug.Log("Calculated scale: " + scaleFloat);

            return scaleFloat;
        }

        public void setNewScale(float scale)
        {
            scaler.setScale(scale);
        }

        public void resetScale()
        {
            scaler.resetScale();
        }

        /// <summary> Activates Checkmarks for Rehearal Mode </summary>
        public void SetCheckmark(int actionIndex) {
            if (GameManager.Mode == GameMode.Rehearsal) {
                SetCheckImageActive();
                
                if (InteractablePath.isNextInteractable(parent) && actionIndex == InteractablePath.NextInteractable.ID.actionID) 
                    SetCheckButtonActive(true);
                else
                    SetCheckButtonActive(false);
            }
            else if (GameManager.Mode == GameMode.Recall) {
                SetCheckButtonActive(true);
            }
        }

        public Bounds actionUiBox()
        {
            if (actionUI != null)
            {
                if (actionUI.GetComponent<Collider>() != null)
                    return actionUI.GetComponent<Collider>().bounds;
                else if (actionUI.GetComponent<Mesh>() != null)
                    return actionUI.GetComponent<Mesh>().bounds;
                else if (actionUI.GetComponent<Renderer>() != null)
                    return actionUI.GetComponent<Renderer>().bounds;
            }
            Debug.Log("Action UI is null or has some other issue");
            Bounds returnBounds = new Bounds(new Vector3(-997, -997, -997), new Vector3(0,0,0));
            return returnBounds;
        }

        public void ToggleTracker(bool active) {
            if (actionUI == null) return;

            Canvas tracker = actionUI.gameObject.GetComponentsInChildren<Canvas>(true)[2];
            tracker.gameObject.SetActive(active);

            RectTransform rect = actionUI.GetComponentsInChildren<Button>(true)[2].GetComponentInChildren<RectTransform>();
            rect.localPosition = new Vector3(-169, 0, 0);
        }

        public bool IsOnHover() {
            bool hover = false;

            if (actionButtons != null)
            {
                foreach (Button button in actionButtons)
                {
                    InteractButton interactButton = button.GetComponentInChildren<InteractButton>();
                    if (interactButton != null)
                    {
                        if (interactButton.IsOnHover)
                            hover = true;
                    }
                }
            }

            if(progressButton != null) {
                if (progressButton.IsOnHover)
                    hover = true;
            }

            return hover;
        }

    }
}