using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChopItem : MonoBehaviour
{
    public SpriteRenderer[] Slices;
    public Transform Knife;
    public AudioSource VegChopSound;

    Vector3 _knifeRestPosition;
    Vector3 _startPosition;
    float _knifeLeft;
    List<Vector3> _slicePositions = new List<Vector3>();
    Action<BurgerVegType> _completeCallback;

    int _index = 0;
    bool _available = false;
    BurgerVegType _type;

    private void Start()
    {
        _startPosition = transform.localPosition;
        foreach (var slice in Slices)
        {
            _slicePositions.Add(slice.transform.localPosition);
        }
        _knifeRestPosition = Knife.localPosition;
        _knifeLeft = _knifeRestPosition.x * -1;
    }

    public void Initialise(BurgerVegType veg, Action<BurgerVegType> action)
    {
        _type = veg;
        _completeCallback = action;
    }

    public bool Available()
    {
        return _available;
    }

    public void Slice()
    {
        if (!_available) return;

        StartCoroutine(Slice_());
    }

    private IEnumerator Slice_()
    {
        VegChopSound.Play();

        _available = false;

        ++_index;

        Knife.SetParent(Slices[_index].transform);
        Knife.localPosition = new Vector3(Knife.localPosition.x, Knife.localPosition.y, -0.001f);
        Knife.eulerAngles = Vector3.zero;

        while (Knife.transform.localPosition.x > 0)
        {
            Knife.transform.Translate(new Vector3(-0.1f, 0f, 0));
            yield return new WaitForSeconds(0.01f);
        }

        for (int i = 0; i < _index; i++)
        {
            var move = i != (_index - 1) ? 0.05f : 0.75f;

            var constraints = Slices[i].GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
            Slices[i].transform.Translate(new Vector3(-move, 0, 0));
        }

        while (Knife.transform.localPosition.x > _knifeLeft)
        {
            Knife.transform.Translate(new Vector3(-0.18f, 0f, 0));
            yield return new WaitForSeconds(0.001f);
        }

        Knife.transform.Translate(new Vector3(0, -0.1f, 0));
        yield return new WaitForSeconds(0.4f);

        Knife.SetParent(transform);
        Knife.transform.localPosition = new Vector3(Knife.transform.localPosition.x, Knife.transform.localPosition.y, -0.5f);

        while (Knife.transform.localPosition.x < (-1 * _knifeLeft))
        {
            Knife.transform.Translate(new Vector3(0.25f, 0f, 0));
            yield return new WaitForSeconds(0.001f);
        }

        if (_index >= Slices.Length - 1)
        {
            _completeCallback?.Invoke(_type);
            ResetItem();
        }
        else
            _available = true;
    }

    public void OnEnable()
    {
        StartCoroutine(Activate_());
    }

    public bool CanCancel()
    {
        return _index == 0 && _available;
    }

    private IEnumerator Activate_()
    {
        yield return new WaitForSeconds(1f);

        Knife.gameObject.SetActive(true);
        _available = true;
    }

    public void ResetItem()
    {
        gameObject.SetActive(false);
        transform.localPosition = _startPosition;
        _available = false;
        _index = 0;
        Knife.gameObject.SetActive(false);
        Knife.transform.localPosition = _knifeRestPosition;

        for (int i = 0; i < Slices.Length; i++)
        {
            Slices[i].transform.eulerAngles = Vector3.zero;
            Slices[i].transform.localPosition = _slicePositions[i];
            Slices[i].GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX;
        }
    }
}
