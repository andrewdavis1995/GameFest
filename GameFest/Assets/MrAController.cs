using System;
using System.Collections;
using UnityEngine;

public class MrAController : MonoBehaviour
{
    private bool _floating = true;
    private bool _flying = true;
    private float _flyEnd = 0;
    Action _callback;

    public Animator Animator;
    SpriteRenderer _renderer;

    private void Start()
    {
        _renderer = GetComponentInChildren<SpriteRenderer>();
    }

    IEnumerator Float_()
    {
        SetAnimation_("Float");
        _callback?.Invoke();

        while (_floating)
        {
            for (var i = 0; i < 12; i++)
            {
                transform.position -= new Vector3(0, 0.01f, 0);
                yield return new WaitForSeconds(0.08f);
            }
            for (var i = 0; i < 12; i++)
            {
                transform.position += new Vector3(0, 0.01f, 0);
                yield return new WaitForSeconds(0.08f);
            }
        }
    }

    public void Fly(float position, Action callback)
    {
        _flyEnd = position;
        _flying = true;
        _callback = callback;

        SetAnimation_("Fly");

        if (transform.position.x > _flyEnd)
        {
            StartCoroutine(FlyLeft_());
        }
        else
        {
            StartCoroutine(FlyRight_());
        }
    }

    IEnumerator FlyLeft_()
    {
        _renderer.flipX = true;

        while (_flying && transform.position.x > _flyEnd)
        {
            transform.position -= new Vector3(10f * Time.deltaTime, 0, 0);
            yield return new WaitForSeconds(0.01f);
        }
        StartCoroutine(Float_());
    }

    IEnumerator FlyRight_()
    {
        _renderer.flipX = false;

        while (_flying && transform.position.x < _flyEnd)
        {
            transform.position += new Vector3(10f * Time.deltaTime, 0, 0);
            yield return new WaitForSeconds(0.01f);
        }
        StartCoroutine(Float_());
    }

    void SetAnimation_(string animation)
    {
        Animator.ResetTrigger("Float");
        Animator.ResetTrigger("Fly");
        Animator.SetTrigger(animation);
    }
}
