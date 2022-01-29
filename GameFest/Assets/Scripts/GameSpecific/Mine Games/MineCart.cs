using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MineItemDrop { None, Gold, Coal }

/// <summary>
/// Controls the behaviour of the carts
/// </summary>
public class MineCart : MonoBehaviour
{
    public Vector3 SpawnPointOffset;
    public Transform SpawnPrefab;
    public Transform RotationObject;
    public Sprite[] CoalImages;
    public Sprite[] GoldImages;

    SpriteRenderer _renderer;
    float _xPosition;
    MineItemDrop _contents;
    List<MineDropItem> _rocks = new List<MineDropItem>();
    bool _movingIn;
    bool _movingOut;

    const float TIPPING_POINT = -60;
    const int SPAWN_ITEMS = 15;
    const int MOVE_SPEED = 15;
    const float MOVEMENT_STOP = 30f;
    const float ROTATION_ANGLE_START = -10f;
    const float ROTATION_ANGLE_STOP = -75f;

    /// <summary>
    /// Called once on startup
    /// </summary>
    void Start()
    {
        // initialise components
        _renderer = GetComponent<SpriteRenderer>();
        _xPosition = transform.localPosition.x;
    }

    /// <summary>
    /// Called once per frame
    /// </summary>
    void Update()
    {
        if (_movingIn)
        {
            transform.localPosition += new Vector3(MOVE_SPEED * Time.deltaTime, 0, 0);

            if (transform.localPosition.x > _xPosition)
            {
                _movingIn = false;
                transform.localPosition = new Vector3(_xPosition, transform.localPosition.y, transform.localPosition.z);
            }
        }
        else if (_movingOut)
        {
            transform.localPosition += new Vector3(MOVE_SPEED * Time.deltaTime, 0, 0);

            if (transform.localPosition.x > (_xPosition + MOVEMENT_STOP))
            {
                _movingOut = false;
                transform.localPosition = new Vector3(_xPosition - MOVEMENT_STOP, transform.localPosition.y, transform.localPosition.z);
            }
        }
    }

    /// <summary>
    /// Sets the contents of the cart
    /// </summary>
    /// <param name="dropItem">The item to drop</param>
    public void SetContents(MineItemDrop dropItem)
    {
        _contents = dropItem;
    }

    /// <summary>
    /// Tips the cart to reveal its contents
    /// </summary>
    public void TipCart()
    {
        StartCoroutine(TipCart_());
    }

    /// <summary>
    /// Tips the cart to reveal its contents
    /// </summary>
    IEnumerator TipCart_()
    {
        // loop through each image
        for (float i = ROTATION_ANGLE_START; i >= ROTATION_ANGLE_STOP; i-=2)
        {
            RotationObject.transform.eulerAngles = new Vector3(i, 0, 0);

            // update image
            yield return new WaitForSeconds(0.01f);

            // if we have reached tipping point, spawn coal and gold
            if (i == TIPPING_POINT)
            {
                switch (_contents)
                {
                    case MineItemDrop.Gold:StartCoroutine(SpawnItems_(GoldImages)); break;
                    case MineItemDrop.Coal: StartCoroutine(SpawnItems_(CoalImages)); break;
                }
            }
        }

        yield return new WaitForSeconds(2);

        // loop back through each image
        for (float i = ROTATION_ANGLE_STOP; i <= ROTATION_ANGLE_START; i+=2)
        {
            RotationObject.transform.eulerAngles = new Vector3(i, 0, 0);

            // update image
            yield return new WaitForSeconds(0.01f);
        }

        yield return new WaitForSeconds(2);

        // remove items
        foreach (var rock in _rocks)
            rock.Finished();

        yield return new WaitForSeconds(1.4f);

        _rocks.Clear();
    }

    /// <summary>
    /// Spawns coal and rocks
    /// </summary>
    /// <param name="images">The images to choose from</param>
    IEnumerator SpawnItems_(Sprite[] images)
    {
        _rocks.Clear();

        for (int i = 0; i < SPAWN_ITEMS; i++)
        {
            var item = Instantiate(SpawnPrefab, transform.position + SpawnPointOffset, Quaternion.identity);
            // set image
            item.GetComponent<SpriteRenderer>().sprite = images[Random.Range(0, images.Length)];
            _rocks.Add(item.GetComponent<MineDropItem>());
            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// Moves the cart onto the screen
    /// </summary>
    public void MoveIn()
    {
        _movingIn = true;
        _movingOut = false;
    }

    /// <summary>
    /// Moves the cart off of the screen
    /// </summary>
    public void MoveOut()
    {
        _movingOut = true;
        _movingIn = false;
    }
}