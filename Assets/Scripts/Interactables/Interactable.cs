﻿using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class StartLookingEvent : UnityEvent<string> {}

public abstract class Interactable : MonoBehaviour
{
    [SerializeField] protected Transform interactionPoint = default;
    [SerializeField] [Range(1f, 5f)] float interactionRadius = default;
    [SerializeField] bool hasToBeFaced = false;

    protected PlayerController playerController;
    
    Transform cameraTransform;
    bool isPlayerLookingAt = false;

    StartLookingEvent onStartLookingAt = new StartLookingEvent();
    UnityEvent onStopLookingAt = new UnityEvent();
    UnityEvent onInteraction = new UnityEvent();

    protected virtual void Start()
    {
        playerController = CharacterManager.Instance.PlayerController;
        cameraTransform = playerController.GetComponentInChildren<Camera>().transform;

        DialogueManager.Instance.OnDialogueAreaDisable.AddListener(EnableInteraction);
    }

    void Update()
    {
        if (!playerController.CanInteract)
            return;

        Vector3 diff = interactionPoint.position - cameraTransform.position;

        if (diff.sqrMagnitude < interactionRadius * interactionRadius)
        {
            float angleBetweenObjs = Vector3.Angle(cameraTransform.forward, diff);
            float viewRange = playerController.FirstPersonCamera.InteractionFOV * 0.5f / diff.magnitude;
            float interactableFwdOrient = playerController.transform.InverseTransformDirection(transform.forward).z;

            if (angleBetweenObjs < viewRange && (!hasToBeFaced || interactableFwdOrient < -0.5f))
            {
                if (!isPlayerLookingAt)
                    StartLookingAt();
                if (Input.GetButtonDown("Interact"))
                {
                    isPlayerLookingAt = false;
                    Interact();
                    onInteraction.Invoke();
                }
            }
            else
            {
                if (isPlayerLookingAt)
                    StopLookingAt();
            }
        }
        else
            if (isPlayerLookingAt)
                StopLookingAt();
    }

    void StartLookingAt()
    {
        isPlayerLookingAt = true;
        onStartLookingAt.Invoke(GetInteractionKind());
    }

    void StopLookingAt()
    {
        isPlayerLookingAt = false;
        onStopLookingAt.Invoke();
    }

    public virtual void EnableInteraction()
    {
        enabled = true;
    }

    public virtual void DisableInteraction()
    {
        enabled = false;
    }

    public virtual string GetInteractionKind()
    {
        return "interact";
    }

    public abstract void Interact();

    #region Properties

    public Vector3 InteractionPosition
    {
        get { return interactionPoint.position; }
    }

    public StartLookingEvent OnStartLookingAt
    {
        get { return onStartLookingAt; }
    }

    public UnityEvent OnStopLookingAt
    {
        get { return onStopLookingAt; }
    }

    public UnityEvent OnInteraction
    {
        get { return onInteraction; }
    }
    
    #endregion
}