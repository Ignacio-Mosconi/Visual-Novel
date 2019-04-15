﻿using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] GameObject hudArea;
    [SerializeField] GameObject interactTextPanel;

    void Start()
    {
        HideInteractTextPanel();

        Interactable[] interactables = FindObjectsOfType<Interactable>();

        foreach (Interactable interactable in interactables)
        {
            interactable.OnStartLookingAt.AddListener(ShowInteractTextPanel);
            interactable.OnStopLookingAt.AddListener(HideInteractTextPanel);
            interactable.OnInteraction.AddListener(HideInteractTextPanel);
        }

        DialogueManager.Instance.OnDialogueAreaEnable.AddListener(HideHUD);
        DialogueManager.Instance.OnDialogueAreaDisable.AddListener(ShowHUD);
    }

    void ShowHUD()
    {
        hudArea.SetActive(true);
    }

    void HideHUD()
    {
        hudArea.SetActive(false);
    }

    void ShowInteractTextPanel()
    {
        interactTextPanel.SetActive(true);
    }

    void HideInteractTextPanel()
    {
        interactTextPanel.SetActive(false);
    }
}