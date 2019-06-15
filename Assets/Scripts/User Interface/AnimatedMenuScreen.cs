using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class AnimatedMenuScreen : MonoBehaviour
{
    [SerializeField] Button backButton = default;

    Animator screenAnimator;
    float showAnimationDuration;
    float hideAnimationDuration;

    public void Awake()
    {
        screenAnimator = GetComponent<Animator>();

        AnimationClip[] animations = screenAnimator.runtimeAnimatorController.animationClips;
        AnimationClip showAnim = Array.Find(animations, a => a.name.ToLower().Contains("show"));
        AnimationClip hideAnim = Array.Find(animations, a => a.name.ToLower().Contains("hide"));

        showAnimationDuration = showAnim.length;
        hideAnimationDuration = hideAnim.length;
    }

    IEnumerator InvokeDeactivationRealTime()
    {
        yield return new WaitForSecondsRealtime(hideAnimationDuration);
        Deactivate();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        if (backButton)
            backButton.interactable = true;
    }

    public void Hide()
    {
        screenAnimator.SetTrigger("Hide");
        StartCoroutine(InvokeDeactivationRealTime());
        if (backButton)
            backButton.interactable = false;
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}