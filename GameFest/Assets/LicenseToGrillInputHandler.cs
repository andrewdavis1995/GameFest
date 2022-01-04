using System;
using System.Collections.Generic;
using UnityEngine;

enum ChefAction { FacingBoard, FacingGrill, SelectingBread, ChoppingBread };

public class LicenseToGrillInputHandler : GenericInputHandler
{
    // constants
    const uint MAX_BURGER_ITEMS = 7;
    const float SPAWN_POINT_OFFSET = 18f;

    // components
    ChefScript Chef;

    // status variables
    uint _burgerItemIndex = 0;
    int _selectedPattyIndex = -1;
    List<Transform> _burgerElements = new List<Transform>();
    CookingSelectionObject _currentItem = null;
    ChefAction _action = ChefAction.FacingBoard;
    int _knifeTarget = 0;
    SelectionType _selectedBread;

    /// <summary>
    /// Called once on startup
    /// </summary>
    private void Start()
    {
        // TEMP
        Chef = LicenseToGrillController.Instance.Chefs[0];

        // assign callbacks for when the selection hand enters a trigger
        Chef.AddItemSelectionCallbacks(TriggerEntered_, TriggerExited_);
    }

    /// <summary>
    /// Called when a trigger is entered
    /// </summary>
    /// <param name="cso">The selection object that was collided with</param>
    void TriggerEntered_(CookingSelectionObject cso)
    {
        _currentItem = cso;

        // if entered a grill zone, keep track of it
        if (_currentItem.ObjectType == SelectionType.GrillZone)
        {
            _selectedPattyIndex = _currentItem.Index;
        }
    }

    /// <summary>
    /// Called when a trigger is left
    /// </summary>
    /// <param name="cso">The selection object that was collided with</param>
    void TriggerExited_(CookingSelectionObject cso)
    {
        _currentItem = null;

        // if left the grill zone, we are no longer in that zone
        if (cso.ObjectType == SelectionType.GrillZone)
        {
            _selectedPattyIndex = -1;
        }
    }

    /// <summary>
    /// Called once per frame
    /// </summary>
    private void Update()
    {
        // TEMP
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // move the camera based on the current action
            switch (_action)
            {
                case ChefAction.FacingBoard:
                    Chef.CameraLeft_();
                    _action = ChefAction.FacingGrill;
                    break;
            }
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            // move the camera based on the current action
            switch (_action)
            {
                case ChefAction.FacingGrill:
                    Chef.CameraRight_();
                    _action = ChefAction.FacingBoard;
                    break;
            }
        }

        // current item was selected
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // perform the required action
            switch (_action)
            {
                case ChefAction.FacingGrill:
                    if (_selectedPattyIndex > -1)
                        Chef.Burgers[_selectedPattyIndex].Flip(() => { StartCoroutine(Chef.Burgers[_selectedPattyIndex].StartNewSide()); });
                    break;
                case ChefAction.FacingBoard:
                {
                    if (_currentItem != null)
                    {
                        switch (_currentItem.ObjectType)
                        {
                            case SelectionType.Lettuce:
                            case SelectionType.Tomato:
                                SpawnVeg_();
                                break;
                            case SelectionType.BriocheBunTop:
                            case SelectionType.SesameBunTop:
                                SpawnBread_(true);
                                Chef.TopBun.gameObject.SetActive(false);
                                break;
                            case SelectionType.BreadBin:
                                _action = ChefAction.SelectingBread;
                                ShowBreadOptions_(true);
                                break;
                        }
                    }
                    break;
                }
                case ChefAction.SelectingBread:
                {
                    if (_currentItem != null)
                    {
                        switch (_currentItem.ObjectType)
                        {
                            case SelectionType.BriocheBun:
                            case SelectionType.SesameBun:
                                ChopBread_();
                                break;
                        }
                    }
                    break;
                }
            }
        }

        // current item was selected
        if (Input.GetKeyDown(KeyCode.S))
        {
            // perform the required action
            switch (_action)
            {
                case ChefAction.FacingGrill:
                    if (_selectedPattyIndex > -1)
                        Chef.Burgers[_selectedPattyIndex].Flip(() =>
                        {
                            SpawnBurger_();
                            Chef.Burgers[_selectedPattyIndex].gameObject.SetActive(false);
                            Chef.Burgers[_selectedPattyIndex].ResetBurger();
                        });
                    break;
                case ChefAction.FacingBoard:
                    Serve_();
                    break;
            }
        }

        // TEMP:
        CheckMove_();
    }

    /// <summary>
    /// Show the behaviour for chopping bread
    /// </summary>
    private void ChopBread_()
    {
        _selectedBread = _currentItem.ObjectType;

        Chef.Knife.gameObject.SetActive(true);
        Chef.Knife.localPosition = new Vector3(3f, -1.25f, -0.14f);
        ShowBreadOptions_(false);

        // show big bread
        Chef.ChopBunRendererBottom.gameObject.SetActive(true);

        // set bread image
        SetBunChopImage_();

        Chef.SelectionHand.gameObject.SetActive(false);
        _action = ChefAction.ChoppingBread;
        _knifeTarget = -1;
    }

    private void SetBunChopImage_()
    {
        int index = 0;
        switch (_selectedBread)
        {
            case SelectionType.SesameBun:
                index = 1;
                break;
        }

        Chef.TopBun.sprite = LicenseToGrillController.Instance.BreadTop[index];
        Chef.ChopBunRendererTop.sprite = LicenseToGrillController.Instance.BreadTop[index];
        Chef.ChopBunRendererBottom.sprite = LicenseToGrillController.Instance.BreadBottoms[index];
    }

    /// <summary>
    /// Serves the burger to the customer
    /// </summary>
    private void Serve_()
    {
        // TODO: Add animation

        _burgerItemIndex = 0;

        // destroy all burger elements
        foreach (var item in _burgerElements)
        {
            Destroy(item.gameObject);
        }

        // no more items
        _burgerElements.Clear();
    }

    /// <summary>
    /// Spawn bread
    /// </summary>
    void SpawnBread_(bool top)
    {
        Sprite sprite = null;

        switch (_selectedBread)
        {
            // spawn brioche
            case SelectionType.BriocheBun:
                sprite = top ? LicenseToGrillController.Instance.BreadTop[0] : LicenseToGrillController.Instance.BreadBottoms[0];
                break;
            // spawn sesame
            case SelectionType.SesameBun:
                sprite = top ? LicenseToGrillController.Instance.BreadTop[1] : LicenseToGrillController.Instance.BreadBottoms[1];
                break;
        }

        // spawn an item
        if (sprite != null)
        {
            SpawnSomething_(LicenseToGrillController.Instance.FoodPlateItemPrefab, sprite, false);
            ShowBreadOptions_(false);
            _action = ChefAction.FacingBoard;
        }
    }

    /// <summary>
    /// Spawn vegetables (or fruit)
    /// </summary>
    void SpawnVeg_()
    {
        if (_currentItem == null) return;
        if (!Chef.TopBun.gameObject.activeInHierarchy) return;

        Sprite sprite = null;

        switch (_currentItem.ObjectType)
        {
            // spawn tomato
            case SelectionType.Tomato:
                sprite = LicenseToGrillController.Instance.TomatoSlices;
                break;
            // spawn lettuce
            case SelectionType.Lettuce:
                sprite = LicenseToGrillController.Instance.LettuceSlice;
                break;
        }

        // spawn an item
        if (sprite != null)
            SpawnSomething_(LicenseToGrillController.Instance.FoodPlateItemPrefab, sprite);
    }

    /// <summary>
    /// Spawn a patty
    /// </summary>
    void SpawnBurger_()
    {
        Sprite sprite = LicenseToGrillController.Instance.Burgers[0];
        // TODO: get type of burger from the one that was selected
        // TODO: change colour based on the one that was selected
        var spawned = SpawnSomething_(LicenseToGrillController.Instance.FoodPlateBurgerPrefab, sprite, false);

        if (_selectedPattyIndex > -1)
        {
            var renderers = spawned.GetComponentsInChildren<SpriteRenderer>();
            renderers[0].color = Chef.Burgers[_selectedPattyIndex].GetBurgerColour();
            renderers[1].color = Chef.Burgers[_selectedPattyIndex].GetBurgerColourBack();
            renderers[2].color = Chef.Burgers[_selectedPattyIndex].GetBurgerColourGrill();
        }
    }

    /// <summary>
    /// Spawns an item
    /// </summary>
    /// <param name="prefab">The prefab to use</param>
    /// <param name="sprite">The sprite to use</param>
    /// <param name="colour">The colour to set the sprite as</param>
    Transform SpawnSomething_(Transform prefab, Sprite sprite, bool includedInCount = true)
    {
        Transform spawned = null;

        // limit to certain number items
        if ((_burgerItemIndex > MAX_BURGER_ITEMS) && includedInCount)
        {
            // cannot add any more
            // TODO: display message
        }
        else
        {
            // spawn an item
            spawned = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity, Chef.Plate);
            spawned.localPosition = new Vector3(0, SPAWN_POINT_OFFSET, -0.1f - (0.1f * _burgerItemIndex));
            spawned.localScale = new Vector3(1, 1, 1);

            // update appearance
            var renderer = spawned.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;

            // adjust collider
            var collider = spawned.GetComponent<BoxCollider2D>();
            collider.offset = new Vector2(collider.offset.x, 0.6f - (_burgerItemIndex * 0.325f));
            collider.size = new Vector2(collider.size.x, 4.4f + (_burgerItemIndex * 0.02f));

            // move to next position
            _burgerItemIndex++;

            _burgerElements.Add(spawned);
        }

        return spawned;
    }

    /// <summary>
    /// Check if the player is moving
    /// </summary>
    private void CheckMove_()
    {
        float x = 0f, y = 0f;

        // TEMP
        if (Input.GetKey(KeyCode.UpArrow))
        {
            y = 1;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            y = -1;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            x = -1;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            x = 1;
        }

        // move the hand
        switch (_action)
        {
            case ChefAction.FacingBoard:
            case ChefAction.SelectingBread:
                Chef.MoveHand(x, y);
                break;
            case ChefAction.FacingGrill:
                Chef.MoveSpatula(x, y);
                break;
            case ChefAction.ChoppingBread:
                CheckKnife_(x);
                break;
        }
    }

    private void CheckKnife_(float x)
    {
        const float KNIFE_MOVEMENT = 0.25f;

        if (x > 0.9f && _knifeTarget > 0)
        {
            Chef.Knife.Translate(new Vector3(KNIFE_MOVEMENT / 5, 0, 0));
            _knifeTarget = -1;
        }
        else if (x < -0.9f && _knifeTarget < 0)
        {
            Chef.Knife.Translate(new Vector3(-KNIFE_MOVEMENT, 0, 0));
            _knifeTarget = 1;

            if (Chef.Knife.localPosition.x <= -3.25f)
            {
                ShowBreadOptions_(false);
                Chef.Knife.gameObject.SetActive(false);
                _action = ChefAction.FacingBoard;
                Chef.SelectionHand.gameObject.SetActive(true);
                Chef.ChopBunRendererBottom.gameObject.SetActive(false);
                Chef.TopBun.gameObject.SetActive(true);
                SpawnBread_(false);
            }
        }
    }

    /// <summary>
    /// Show the options for which bread to use
    /// </summary>
    private void ShowBreadOptions_(bool state)
    {
        foreach (var bread in Chef.BreadOptions)
        {
            bread.gameObject.SetActive(state);
        }
    }
}