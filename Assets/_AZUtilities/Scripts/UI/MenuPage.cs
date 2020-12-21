using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MenuPage : MonoBehaviour
{
    public Selectable selectOnShow;
    public bool selectOnEnable = false;

    public bool enablePreSequence = true;
    public float fadeDuration = 0.4f;

    public event Action onShowing;
    public event Action onHiding;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnEnable()
    {
        if (selectOnEnable)
        {
            this.WaitForFrameAndExecute(SelectItemOnShow);
        }
    }

    public void Show()
    {
        DOTween.Kill(gameObject);
        var selectables = gameObject.GetInteractiveSelectables();
        foreach (var selectable in selectables)
        {
            selectable.interactable = false;
        }

        var showSeq = gameObject.DOFade(1, fadeDuration);

        TweenCallback playShowSeqCallback = () =>
        {
            gameObject.SetActive(true);
            SelectItemOnShow();

            showSeq.AppendCallback(() =>
            {
                foreach (var selectable in selectables)
                {
                    selectable.interactable = true;
                }
            }).Play();
        };

        if (enablePreSequence)
        {
            var preSeq = gameObject.DOFade(0, 0);
            preSeq.AppendCallback(playShowSeqCallback);
            preSeq.Play();
        }
        else
        {
            playShowSeqCallback();
        }

        onShowing?.Invoke();
    }

    public void Hide()
    {
        DOTween.Kill(gameObject);

        var selectables = gameObject.GetInteractiveSelectables();
        foreach (var selectable in selectables)
        {
            selectable.interactable = false;
        }

        var hideSeq = gameObject.DOFade(0, fadeDuration);
        hideSeq.AppendCallback(() =>
        {
            gameObject.SetActive(false);

            foreach (var selectable in selectables)
            {
                selectable.interactable = true;
            }
        });

        TweenCallback playHideSeqCallback = () => { hideSeq.Play(); };

        if (enablePreSequence)
        {
            gameObject.SetActive(true);
            var preSeq = gameObject.DOFade(1, 0);
            preSeq.AppendCallback(playHideSeqCallback);
            preSeq.Play();
        }
        else
        {
            playHideSeqCallback();
        }

        onHiding?.Invoke();
    }

    void SelectItemOnShow()
    {
        selectOnShow?.Select();
    }
}