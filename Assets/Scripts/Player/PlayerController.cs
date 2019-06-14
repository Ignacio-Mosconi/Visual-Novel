﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(FirstPersonCamera))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerController : MonoBehaviour, ICharacter
{
    [SerializeField] CharacterName playerName = default;
    [SerializeField] Sprite[] characterSprites = default;

    FirstPersonCamera firstPersonCamera;
    PlayerMovement playerMovement;
    Camera playerCamera;
    List<ClueInfo> cluesGathered = new List<ClueInfo>();
    bool canInteract = true;
    bool foundClueInLastDialogue = false;
    bool startedInvestigationInLastDialogue = false;

    UnityEvent onClueFound = new UnityEvent();
    UnityEvent onStartedInvestigation = new UnityEvent();

    void Awake()
    {
        firstPersonCamera = GetComponent<FirstPersonCamera>();
        playerMovement = GetComponent<PlayerMovement>();
        playerCamera = GetComponentInChildren<Camera>();
    }

    void Start()
    {
        DialogueManager.Instance.OnDialogueAreaEnable.AddListener(Disable);
        DialogueManager.Instance.OnDialogueAreaDisable.AddListener(Enable);
    }

    bool TriggeredNotification()
    {
        bool triggeredNotification = false;

        if (foundClueInLastDialogue)
        {
            triggeredNotification = true;
            foundClueInLastDialogue = false;
            onClueFound.Invoke();
        }

        if (startedInvestigationInLastDialogue)
        {
            triggeredNotification = true;
            startedInvestigationInLastDialogue = false;
            onStartedInvestigation.Invoke();
        }

        return triggeredNotification;
    }

    public void Enable()
    {
        firstPersonCamera.enabled = true;
        playerMovement.enabled = true;
        canInteract = !TriggeredNotification();
    }

    public void Disable()
    {
        firstPersonCamera.enabled = false;
        playerMovement.enabled = false;
        canInteract = false;
    }

    public void ReEnableInteractionDelayed()
    {
        canInteract = true;
    }

    public void DeactivateCamera()
    {
        playerCamera.gameObject.SetActive(false);
    }

    public void AddClue(ClueInfo clueInfo)
    {
        if (!cluesGathered.Contains(clueInfo))
        {
            foundClueInLastDialogue = true;
            cluesGathered.Add(clueInfo);
        }
    }

    public void StartInvestigation()
    {
        startedInvestigationInLastDialogue = true;
    }

    public bool HasClue(ref ClueInfo clueInfo)
    {    
        return (cluesGathered.Contains(clueInfo));
    }

    public bool IsMovementEnabled()
    {
        return playerMovement.enabled;
    }

    public CharacterName GetCharacterName()
    {
        return playerName;
    }

    public Sprite GetSprite(CharacterEmotion characterEmotion)
    {
        return characterSprites[(int)characterEmotion];
    }

    #region Properties

    public FirstPersonCamera FirstPersonCamera
    {
        get { return firstPersonCamera; }
    }

    public bool CanInteract
    {
        get { return canInteract; }
    }

    public List<ClueInfo> CluesGathered
    {
        get { return cluesGathered; }
    }

    public UnityEvent OnClueFound
    {
        get { return onClueFound; }
    }

    public UnityEvent OnStartedInvestigation
    {
        get { return onStartedInvestigation; }
    }
    
    #endregion
}