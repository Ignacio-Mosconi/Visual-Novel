﻿using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] GameObject hudArea = default;
    [SerializeField] UIPrompt interactTextPrompt = default;
    [SerializeField] UIPrompt clueFoundPrompt = default;
    [SerializeField] UIPrompt investigationPhasePrompt = default;

    void Start()
    {
        Interactable[] interactables = FindObjectsOfType<Interactable>();

        foreach (Interactable interactable in interactables)
        {
            interactable.OnStartLookingAt.AddListener(ShowInteractTextPrompt);
            interactable.OnStopLookingAt.AddListener(HideInteractTextPrompt);
            interactable.OnInteraction.AddListener(DeactivateInteractTextPrompt);
        }

        PauseMenu pauseMenu = FindObjectOfType<PauseMenu>();
        pauseMenu.OnPaused.AddListener(HideHUD);
        pauseMenu.OnResume.AddListener(ShowHUD);

        DialogueManager.Instance.OnDialogueAreaEnable.AddListener(HideHUD);
        DialogueManager.Instance.OnDialogueAreaDisable.AddListener(ShowHUD);
        
        PlayerController playerController = CharacterManager.Instance.PlayerController;
        playerController.OnClueFound.AddListener(ShowClueFoundPrompt);
        playerController.OnStartedInvestigation.AddListener(ShowInvestigationPhasePrompt);
    }

    void ShowHUD()
    {
        hudArea.SetActive(true);
    }

    void HideHUD()
    {
        hudArea.SetActive(false);
    }

    void ShowInteractTextPrompt()
    {
        interactTextPrompt.Show();
    }

    void HideInteractTextPrompt()
    {
        interactTextPrompt.Hide();
    }

    void DeactivateInteractTextPrompt()
    {
        interactTextPrompt.Deactivate();
    }

    void ShowClueFoundPrompt()
    {
        clueFoundPrompt.Show();
    }

    void ShowInvestigationPhasePrompt()
    {
        investigationPhasePrompt.Show();
    }
}