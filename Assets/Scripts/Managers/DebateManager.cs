using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class DebateManager : MonoBehaviour
{
    #region Singleton

    static DebateManager instance;

    void Awake()
    {
        if (Instance != this)
            Destroy(gameObject);
    }

    public static DebateManager Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType<DebateManager>();
                if (!instance)
                    Debug.LogError("There is no 'Debate Manager' in the scene");
            }

            return instance;
        }
    }

    #endregion

    [Header("UI Elements")]
    [Header("Areas")]
    [SerializeField] GameObject debateArea;
    [SerializeField] GameObject speakerArea;
    [SerializeField] GameObject argumentAndSpeechArea;
    [Header("Panels")]
    [SerializeField] GameObject argumentPanel;
    [SerializeField] GameObject speechPanel;
    [SerializeField] GameObject debateOptionsPanel;
    [SerializeField] GameObject clueOptionsPanel;
    [Header("Layouts, Buttons & Texts")]
    [SerializeField] VerticalLayoutGroup clueOptionsLayout;
    [SerializeField] Button clueOptionsBackButton;
    [SerializeField] TextMeshProUGUI speakerText;
    [SerializeField] TextMeshProUGUI argumentText;
    [SerializeField] TextMeshProUGUI speechText;
    [Header("Debate Properties")]
    [SerializeField] [Range(30f, 60f)] float cameraRotSpeed = 60f;
    [SerializeField] [Range(1f, 2f)] float argumentPanelExpandScale = 2f;
    [SerializeField] [Range(0.5f, 1.5f)] float argumentPanelScaleDur = 1f;
    
    Camera debateCamera;
    DebateCharacterSprite[] debateCharactersSprites;
    DebateInfo currentDebateInfo;
    Argument currentArgument;
    Dialogue[] currentDialogueLines;
    DebateDialogue[] currentArgumentLines;
    Coroutine focusingRoutine;
    Coroutine expandindArgumentRoutine;
    Coroutine speakingRoutine;
    Quaternion currentCamTargetRot;
    CharacterName previousSpeaker = CharacterName.None;
    List<ClueInfo> caseClues;
    int lineIndex = 0;
    int[] regularCluesLayoutPadding = { 0, 0 };
    float regularCluesLayoutSpacing = 0f;
    float regularClueButtonHeight = 0f;
    float characterShowIntervals;
    float textSpeedMultiplier;
    int targetSpeechCharAmount;
    bool areInArgueingPhase = false;


    void Start()
    {
        DebateInitializer debateInitializer = FindObjectOfType<DebateInitializer>();
        
        debateCamera = debateInitializer.GetComponentInChildren<Camera>(includeInactive: true);
        debateCharactersSprites = debateInitializer.DebateCharactersSprites;

        regularCluesLayoutPadding[0] = clueOptionsLayout.padding.top;
        regularCluesLayoutPadding[1] = clueOptionsLayout.padding.bottom;
        regularCluesLayoutSpacing = clueOptionsLayout.spacing;

        Button clueOption = clueOptionsLayout.GetComponentInChildren<Button>(includeInactive: true);
        regularClueButtonHeight = clueOption.GetComponent<RectTransform>().rect.height;

        characterShowIntervals = 1f / GameManager.Instance.TargetFrameRate;
        textSpeedMultiplier = 1f / GameManager.Instance.TextSpeedMultiplier;

        enabled = false;
    }

    void Update()
    {
        if (Input.GetButtonDown("Continue"))
        {
            if (focusingRoutine != null)
            {
                FinishFocus();
                return;
            }

            if (expandindArgumentRoutine != null)
            {
                FinishArgumentExpansion();
                return;
            }

            if (speakingRoutine != null)
            {
                StopSpeaking();
                return;
            }

            lineIndex++;
            if (areInArgueingPhase)
            {
                if (lineIndex < currentArgumentLines.Length)
                {
                    ResetDebatePanelsStatuses();
                    Argue(currentArgumentLines[lineIndex].speakerName, 
                            currentArgumentLines[lineIndex].argument, 
                            currentArgumentLines[lineIndex].characterEmotion);
                }
            }
            else
            {
                if (lineIndex < currentDialogueLines.Length)
                {
                    ResetDebatePanelsStatuses();
                    Dialogue(currentDialogueLines[lineIndex].speakerName,
                            currentDialogueLines[lineIndex].speech,
                            currentDialogueLines[lineIndex].characterEmotion,
                            currentDialogueLines[lineIndex].playerThought);
                }
                else
                    StartArgumentPhase();
            }
        }
    }

    void SetDebateAreaAvailability(bool enableDebateArea)
    {
        debateArea.SetActive(enableDebateArea);
        enabled = enableDebateArea;
    }

    void ResetDebatePanelsStatuses()
    {
        speakerArea.SetActive(false);
        argumentAndSpeechArea.SetActive(false);

        argumentPanel.transform.localScale = new Vector3(1f, 1f, 1f);
    }

    void ShowDebateOptions()
    {
        lineIndex = 0;
        enabled = false;
        debateOptionsPanel.SetActive(true);
        GameManager.Instance.SetCursorAvailability(enable: true);
    }

    void StartArgumentPhase()
    {
        lineIndex = 0;
        speechPanel.SetActive(false);
        areInArgueingPhase = true;
        Argue(currentArgumentLines[0].speakerName, currentArgumentLines[0].argument, currentArgumentLines[0].characterEmotion);
    }

    void Argue(CharacterName speaker, string argument, CharacterEmotion speakerEmotion)
    {
        SpriteRenderer characterRenderer = Array.Find(debateCharactersSprites, cs => cs.characterName == speaker).spriteRenderer;

        if (speaker != previousSpeaker)
        {
            Vector3 charPosition = characterRenderer.transform.position;

            focusingRoutine = StartCoroutine(FocusOnCharacter(charPosition));     
            speakerText.text = speaker.ToString();
            previousSpeaker = speaker;
        }
        else
            SayArgument();

        argumentText.text = argument;
        characterRenderer.sprite = CharacterManager.Instance.GetCharacter(speaker).GetSprite(speakerEmotion);
    }

    void Dialogue(CharacterName speaker, string speech, CharacterEmotion speakerEmotion, bool playerThought)
    {
        SpriteRenderer characterRenderer = Array.Find(debateCharactersSprites, cs => cs.characterName == speaker).spriteRenderer;

        speechText.maxVisibleCharacters = 0;
        speechText.text = speech;
        targetSpeechCharAmount = speech.Length;
        // Temporary hack!
        if (speaker != CharacterName.Player)
            characterRenderer.sprite = CharacterManager.Instance.GetCharacter(speaker).GetSprite(speakerEmotion);

        if (speaker != previousSpeaker)
        {
            Vector3 charPosition = characterRenderer.transform.position;

            focusingRoutine = StartCoroutine(FocusOnCharacter(charPosition));
            speakerText.text = speaker.ToString();
            previousSpeaker = speaker;

            if (playerThought)
                speechText.color = GameManager.Instance.PlayerThinkingTextColor;
            else
                speechText.color = GameManager.Instance.NpcSpeakingTextColor;
        }
        else
            SayDialogue();
    }

    void SayArgument()
    {
        speakerArea.SetActive(true);
        argumentAndSpeechArea.SetActive(true);
        argumentPanel.SetActive(true);

        expandindArgumentRoutine = StartCoroutine(ExpandArgumentPanel());
    }

    void SayDialogue()
    {
        speakerArea.SetActive(true);
        argumentAndSpeechArea.SetActive(true);
        speechPanel.SetActive(true);

        speakingRoutine = StartCoroutine(Speak());
    }

    void FinishFocus()
    {
        if (focusingRoutine != null)
        {
            StopCoroutine(focusingRoutine);
            debateCamera.transform.rotation = currentCamTargetRot;
            if (areInArgueingPhase)
                SayArgument();
            else
                SayDialogue();
            focusingRoutine = null;
        }
    }

    void FinishArgumentExpansion()
    {
        if (expandindArgumentRoutine != null)
        {
            StopCoroutine(expandindArgumentRoutine);
            argumentPanel.transform.localScale = new Vector3(argumentPanelExpandScale, argumentPanelExpandScale, argumentPanelExpandScale);
            expandindArgumentRoutine = null;

            if (lineIndex == currentArgumentLines.Length - 1)
                ShowDebateOptions();
        }
    }

    void StopSpeaking()
    {
        if (speakingRoutine != null)
        {
            StopCoroutine(speakingRoutine);
            speechText.maxVisibleCharacters = targetSpeechCharAmount;
            speakingRoutine = null;
        }
    } 

    IEnumerator FocusOnCharacter(Vector3 characterPosition)
    {
        Vector3 diff = characterPosition - debateCamera.transform.position;

        Vector3 targetDir = new Vector3(diff.x, debateCamera.transform.forward.y, diff.z);
        Quaternion fromRot = debateCamera.transform.rotation;

        Debug.DrawRay(debateCamera.transform.position, targetDir.normalized * 5f, Color.blue, 5f);
        
        currentCamTargetRot = Quaternion.LookRotation(targetDir, debateArea.transform.up);

        float timer = 0f;
        float angleBetweenRots = Quaternion.Angle(fromRot, currentCamTargetRot);
        float rotDuration = angleBetweenRots / cameraRotSpeed;

        while (debateCamera.transform.rotation != currentCamTargetRot)
        {
            timer += Time.deltaTime;
            debateCamera.transform.rotation = Quaternion.Slerp(fromRot, currentCamTargetRot, timer / rotDuration);

            yield return new WaitForEndOfFrame();
        }

        focusingRoutine = null;

        if (areInArgueingPhase)
            SayArgument();
        else
            SayDialogue();
    }

    IEnumerator ExpandArgumentPanel()
    {
        Vector3 initialScale = argumentPanel.transform.localScale;
        Vector3 targetScale = argumentPanel.transform.localScale * argumentPanelExpandScale;

        float timer = 0f;

        while (argumentPanel.transform.localScale != targetScale)
        {
            timer += Time.deltaTime;
            argumentPanel.transform.localScale = Vector3.Lerp(initialScale, targetScale, timer / argumentPanelScaleDur);

            yield return new WaitForEndOfFrame();
        }

        if (lineIndex == currentArgumentLines.Length - 1)
            ShowDebateOptions();

        expandindArgumentRoutine = null;
    }

    IEnumerator Speak()
    {
        while (speechText.maxVisibleCharacters != targetSpeechCharAmount)
        {
            speechText.maxVisibleCharacters++;
            yield return new WaitForSecondsRealtime(characterShowIntervals * textSpeedMultiplier);
        }

        speakingRoutine = null;
    }

    public void TrustComment()
    {
        debateOptionsPanel.SetActive(false);
        Debug.Log("I Agree.");

        if (currentArgument.correctReaction == DebateReaction.Agree)
            Debug.Log("You selected the correct option.");
        else
            Debug.Log("You didn't select the correct option.");
    }

    public void RefuteComment()
    {
        debateOptionsPanel.SetActive(false);
        speakerArea.SetActive(false);
        argumentAndSpeechArea.SetActive(false);
        clueOptionsPanel.gameObject.SetActive(true);
    }

    public void AccuseWithEvidence(int optionIndex)
    {
        clueOptionsPanel.SetActive(false);
        
        Debug.Log("That's wrong!");
        
        if (currentArgument.correctReaction == DebateReaction.Disagree)
        {
            Debug.Log("You selected the correct option.");
            
            if (caseClues[optionIndex] == currentArgument.correctEvidence)
                Debug.Log("That's the correct evidence.");
            else
                Debug.Log("That's not the correct evidence.");
        }
        else
            Debug.Log("You didn't select the correct option.");
    }

    public void ReturnToDebateOptions()
    {
        debateOptionsPanel.SetActive(true);
        speakerArea.SetActive(true);
        argumentAndSpeechArea.SetActive(true);
        clueOptionsPanel.SetActive(false);
    }

    public void InitializeDebate(DebateInfo debateInfo, List<ClueInfo> playerClues)
    {
        debateCamera.gameObject.SetActive(true);

        currentDebateInfo = debateInfo;
        currentArgument = currentDebateInfo.arguments[0];
        currentDialogueLines = currentArgument.argumentIntroDialogue;
        currentArgumentLines = currentArgument.debateDialogue;

        Button[] clueOptions = clueOptionsLayout.GetComponentsInChildren<Button>(includeInactive: true);
                
        caseClues = playerClues;

        int i = 0;

        for (i = 0; i < caseClues.Count; i++)
        {
            clueOptions[i].gameObject.SetActive(true);

            TextMeshProUGUI clueText = clueOptions[i].gameObject.GetComponentInChildren<TextMeshProUGUI>();
            clueText.text = caseClues[i].clueName;
        }

        int paddMult = clueOptions.Length - i;
        float addPadding = (regularClueButtonHeight + regularCluesLayoutSpacing) * paddMult * 0.5f;

        clueOptionsLayout.padding.top = regularCluesLayoutPadding[0] + (int)addPadding;
        clueOptionsLayout.padding.bottom = regularCluesLayoutPadding[1] + (int)addPadding;

        SetDebateAreaAvailability(enableDebateArea: true);

        Dialogue(currentDialogueLines[0].speakerName, 
                    currentDialogueLines[0].speech, 
                    currentDialogueLines[0].characterEmotion, 
                    currentDialogueLines[0].playerThought);
    }  
}