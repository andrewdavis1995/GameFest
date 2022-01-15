using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

enum ChefAction { NotStarted, FacingBoard, FacingGrill, SelectingBread, ChoppingBread, SelectingBurger, SlidingBurgerBoard, Serving, ClearingPlate, Confirmation, ViewingOrders };

public class LicenseToGrillInputHandler : GenericInputHandler
{
    // constants
    const uint MAX_BURGER_ITEMS = 7;
    const float SPAWN_POINT_OFFSET = 18f;
    const int WASTE_POINTS_BURGER = 20;
    const int WASTE_POINTS_VEG = 10;
    const int WASTE_POINTS_SAUCE = 5;
    const int WASTE_POINTS_BUN = 10;

    // components
    ChefScript Chef;

    // status variables
    uint _burgerItemIndex = 0;
    int _selectedPattyIndex = -1;
    int _flippedPattyIndex = -1;
    List<Transform> _burgerElements = new List<Transform>();
    List<object> _burgerSelections = new List<object>();
    List<CookingSelectionObject> _currentItem = new List<CookingSelectionObject>();
    ChefAction _action = ChefAction.NotStarted;
    ChefAction _actionCopy = ChefAction.NotStarted;
    int _knifeTarget = 0;
    SelectionType _selectedBread;
    CustomerHandler _customerHandler;
    Action _confirmCallback;
    int _wastePoints;
    Coroutine _errorRoutine;

    /// <summary>
    /// Called once on startup
    /// </summary>
    private void Start()
    {
        // get the Chef corresponding to this player
        Chef = LicenseToGrillController.Instance.Chefs[GetPlayerIndex()];
        Chef.gameObject.SetActive(true);

        // set appearance of items which change colour based on player
        Chef.ChoppingBoardColour.color = ColourFetcher.GetColour(GetPlayerIndex());
        Chef.SelectionHandColour.color = ColourFetcher.GetColour(GetPlayerIndex());

        // create order list
        _customerHandler = new CustomerHandler();
        Chef.DisplayOrders(_customerHandler.GetNextOrders(5));

        // assign callbacks for when the selection hand enters a trigger
        Chef.AddItemSelectionCallbacks(TriggerEntered_, TriggerExited_);
    }

    /// <summary>
    /// Get how many points have been lost from throwing away food
    /// </summary>
    /// <returns></returns>
    internal int GetWastePoints()
    {
        return _wastePoints;
    }

    /// <summary>
    /// Disables the camera associated with the player
    /// </summary>
    internal void DisableCamera()
    {
        Chef.ChefCamera.gameObject.SetActive(false);
    }

    /// <summary>
    /// Get the customers for this player
    /// </summary>
    /// <returns>List of all customer orders</returns>
    internal List<CustomerOrder> GetCustomers()
    {
        return _customerHandler.GetAllOrders();
    }

    /// <summary>
    /// Starts the player
    /// </summary>
    public void Activate()
    {
        Debug.Log("Activated");
        _action = ChefAction.FacingBoard;
    }

    /// <summary>
    /// Stops the player
    /// </summary>
    public void Finished()
    {
        _action = ChefAction.NotStarted;
    }

    /// <summary>
    /// Called when a trigger is entered
    /// </summary>
    /// <param name="cso">The selection object that was collided with</param>
    void TriggerEntered_(CookingSelectionObject cso)
    {
        _currentItem.Add(cso);

        // if entered a grill zone, keep track of it
        if (_currentItem[0].ObjectType == SelectionType.GrillZone)
        {
            _selectedPattyIndex = _currentItem[0].Index;
        }

        _currentItem[0].Selected();
    }

    /// <summary>
    /// Called when a trigger is left
    /// </summary>
    /// <param name="cso">The selection object that was collided with</param>
    void TriggerExited_(CookingSelectionObject cso)
    {
        _currentItem.Remove(cso);
        cso.Unselected();

        // if left the grill zone, we are no longer in that zone
        if ((cso.ObjectType == SelectionType.GrillZone) && (_currentItem.Count == 0))
        {
            _selectedPattyIndex = -1;
        }

        // next item in list is now selected
        if (_currentItem.Count > 0)
        {
            _currentItem[0].Selected();
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

        if (Input.GetKeyDown(KeyCode.M))
        {
            // move the camera based on the current action
            switch (_action)
            {
                case ChefAction.FacingGrill:
                case ChefAction.FacingBoard:
                case ChefAction.SelectingBread:
                case ChefAction.SelectingBurger:
                case ChefAction.ChoppingBread:
                case ChefAction.ClearingPlate:
                    _actionCopy = _action;
                    _action = ChefAction.ViewingOrders;
                    Chef.OrderList.gameObject.SetActive(true);
                    break;
                case ChefAction.ViewingOrders:
                    _action = _actionCopy;
                    Chef.OrderList.gameObject.SetActive(false);
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
                    {
                        if (Chef.Burgers[_selectedPattyIndex].isActiveAndEnabled)
                        {
                            // flip
                            if (!(Chef.Burgers.Any(b => b.Flipping())))
                            {
                                _flippedPattyIndex = _selectedPattyIndex;
                                Chef.Burgers[_selectedPattyIndex].Flip(() => { StartCoroutine(Chef.Burgers[_flippedPattyIndex].StartNewSide()); });
                            }
                        }
                        else
                        {
                            // select burger
                            _action = ChefAction.SlidingBurgerBoard;
                            StartCoroutine(SlideBurgersUp_());
                        }
                    }
                    break;
                case ChefAction.FacingBoard:
                {
                    if (_currentItem.Count > 0)
                    {
                        switch (_currentItem[0].ObjectType)
                        {
                            case SelectionType.Lettuce:
                            case SelectionType.Tomato:
                            case SelectionType.Pickle:
                                SpawnVeg_();
                                break;
                            case SelectionType.BriocheBunTop:
                            case SelectionType.SesameBunTop:
                            case SelectionType.BrownBunTop:
                                SpawnBread_(true);
                                Chef.TopBun.gameObject.SetActive(false);
                                break;
                            case SelectionType.BreadBin:
                                if (_burgerItemIndex == 0)
                                {
                                    _action = ChefAction.SelectingBread;
                                    ShowBreadOptions_(true);
                                }
                                else
                                {
                                    ShowErrorMessage_("Bread option has already been selected");
                                }
                                break;
                            case SelectionType.Plate:
                                Confirm_(Serve_, "Are you sure you wish to serve this burger?");
                                break;
                        }
                    }
                    break;
                }
                case ChefAction.SelectingBread:
                {
                    if (_currentItem.Count > 0)
                    {
                        switch (_currentItem[0].ObjectType)
                        {
                            case SelectionType.BriocheBun:
                            case SelectionType.SesameBun:
                            case SelectionType.BrownBunTop:
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
                    {
                        if (Chef.TopBun.gameObject.activeInHierarchy)
                        {
                            Chef.Burgers[_selectedPattyIndex].Flip(() =>
                            {
                                _flippedPattyIndex = _selectedPattyIndex;
                                SpawnBurger_();
                                Chef.Burgers[_flippedPattyIndex].gameObject.SetActive(false);
                                Chef.Burgers[_flippedPattyIndex].ResetBurger();
                            });
                        }
                        else
                        {
                            ShowErrorMessage_("Can't serve burger without a bun on the plate");
                        }
                    }
                    break;
            }
        }

        // TEMP
        if (_action == ChefAction.SelectingBurger)
        {
            int index = -1;

            if (Input.GetKeyDown(KeyCode.T)) index = 0;
            if (Input.GetKeyDown(KeyCode.KeypadEnter)) index = 1;
            if (Input.GetKeyDown(KeyCode.A)) index = 2;

            if (index > -1) DropBurger(index);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            switch (_action)
            {
                case ChefAction.SelectingBurger:
                    StartCoroutine(SlideBurgersDown_());
                    _action = ChefAction.FacingGrill;
                    break;
                case ChefAction.SelectingBread:
                    ShowBreadOptions_(false);
                    _action = ChefAction.FacingBoard;
                    break;
            }
        }

        // TEMP
        if (_action == ChefAction.Confirmation)
        {
            if (Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                _confirmCallback?.Invoke();
                Chef.ConfirmPopup.SetActive(false);
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _action = _actionCopy;
                Chef.ConfirmPopup.SetActive(false);
            }
        }

        // TEMP
        if (Input.GetKeyDown(KeyCode.T))
        {
            switch (_action)
            {
                case ChefAction.FacingBoard:
                {
                    if (_currentItem.Count > 0)
                    {
                        switch (_currentItem[0].ObjectType)
                        {
                            case SelectionType.Plate:
                                if (_burgerItemIndex > 0)
                                    Confirm_(DisposePlate_, "Are you sure you wish to throw away your creation? You will lose points");
                                break;
                        }
                    }
                    break;
                }
                case ChefAction.FacingGrill:
                {
                    if (_currentItem.Count > 0)
                    {
                        switch (_currentItem[0].ObjectType)
                        {
                            case SelectionType.GrillZone:
                                if (Chef.Burgers[_selectedPattyIndex].isActiveAndEnabled)
                                    Confirm_(() => StartCoroutine(DisposePatty_()), "Are you sure you wish to throw away this patty? You will lose points");
                                break;
                        }
                    }
                    break;
                }
            }
        }

        // TEMP:
        CheckMove_();
    }

    /// <summary>
    /// Shows the specified error message
    /// </summary>
    /// <param name="msg">The message to show</param>
    private void ShowErrorMessage_(string msg)
    {
        Chef.ErrorPopupText.text = msg;
        if (_errorRoutine != null) StopCoroutine(_errorRoutine);
        _errorRoutine = StartCoroutine(ShowErrorMessage_());
    }

    /// <summary>
    /// Shows the specified error message
    /// </summary>
    private IEnumerator ShowErrorMessage_()
    {
        Chef.ErrorPopup.SetActive(true);
        yield return new WaitForSeconds(2f);
        Chef.ErrorPopup.SetActive(true);
    }

    /// <summary>
    /// Display the confirmation popup
    /// </summary>
    /// <param name="confirmCallback">The callback to call upon completion</param>
    /// <param name="message">The message to display</param>
    void Confirm_(Action confirmCallback, string message)
    {
        // set actions
        _actionCopy = _action;
        _confirmCallback = confirmCallback;
        _action = ChefAction.Confirmation;

        // show popup
        Chef.ConfirmPopup.SetActive(true);
        Chef.ConfirmPopupText.text = message;
    }

    /// <summary>
    /// Throws a patty away
    /// </summary>
    private IEnumerator DisposePatty_()
    {
        // only do this if the burger is active
        if (Chef.Burgers[_selectedPattyIndex].gameObject.activeInHierarchy)
        {
            var targetPosition = Chef.BinY;

            // continue until reached the top
            while (Chef.BinPatty.localPosition.y < targetPosition)
            {
                yield return new WaitForSeconds(0.001f);
                Chef.BinPatty.Translate(new Vector3(0, 0.2f, 0));
            }

            Chef.Burgers[_selectedPattyIndex].CookedBar.SetActive(false);

            // disable collider
            Chef.Burgers[_selectedPattyIndex].GetComponent<BoxCollider2D>().enabled = false;

            // lose points
            _wastePoints -= WASTE_POINTS_BURGER;
            Chef.BinPattyText.text = "-" + WASTE_POINTS_BURGER;

            // fade away the message
            for (float i = 1f; i >= 0; i -= 0.1f)
            {
                Chef.BinPattyText.color = new Color(1, 1, 1, i);
                yield return new WaitForSeconds(0.1f);
            }
            Chef.BinPattyText.color = new Color(1, 1, 1, 0);

            // reset action
            _action = ChefAction.FacingGrill;

            // move bin down
            targetPosition = Chef.BinY - 8f;
            while (Chef.BinPatty.localPosition.y > targetPosition)
            {
                yield return new WaitForSeconds(0.001f);
                Chef.BinPatty.Translate(new Vector3(0, -0.2f, 0));
            }

            // reset the burger
            Chef.Burgers[_selectedPattyIndex].ResetBurger();
        }
    }

    /// <summary>
    /// Drops a burger ont the grill
    /// </summary>
    /// <param name="burgerIndex">Type of burger to drop</param>
    void DropBurger(int burgerIndex)
    {
        var burger = Chef.Burgers[_selectedPattyIndex];

        // set burger appearance
        burger.Renderer.sprite = LicenseToGrillController.Instance.Burgers[burgerIndex];
        burger.RendererUnder.sprite = LicenseToGrillController.Instance.BurgerBottoms[burgerIndex];

        // show urger
        burger.SetBurgerType(burgerIndex);
        burger.gameObject.SetActive(true);

        // remove options
        StartCoroutine(SlideBurgersDown_());
    }

    /// <summary>
    /// Slides the burger choices on to the screen
    /// </summary>
    private IEnumerator SlideBurgersUp_()
    {
        var targetPosition = Chef.BurgerTrayY;

        // continue until reached the top
        while (Chef.BurgerTray.localPosition.y < targetPosition)
        {
            yield return new WaitForSeconds(0.001f);
            Chef.BurgerTray.Translate(new Vector3(0, 0.2f, 0));
        }

        // next action
        _action = ChefAction.SelectingBurger;
    }

    /// <summary>
    /// Slides the burger choices off the screen
    /// </summary>
    private IEnumerator SlideBurgersDown_()
    {
        _action = ChefAction.SlidingBurgerBoard;

        // move sufficiently off the screen
        var targetPosition = Chef.BurgerTrayY - 8;

        // continue until reached the top
        while (Chef.BurgerTray.localPosition.y > targetPosition)
        {
            yield return new WaitForSeconds(0.001f);
            Chef.BurgerTray.Translate(new Vector3(0, -0.2f, 0));
        }

        // next action
        _action = ChefAction.FacingGrill;
    }

    /// <summary>
    /// Show the behaviour for chopping bread
    /// </summary>
    private void ChopBread_()
    {
        // onnly do it if the hand is in a valid location
        if (_currentItem.Count > 0)
        {
            _selectedBread = _currentItem[0].ObjectType;

            // show the knife and hide the bread
            Chef.Knife.gameObject.SetActive(true);
            Chef.Knife.localPosition = new Vector3(3f, -1.25f, -0.14f);
            ShowBreadOptions_(false);

            // show big bread
            Chef.ChopBunRendererBottom.gameObject.SetActive(true);

            // set bread image
            SetBunChopImage_();

            // start chopping
            Chef.SelectionHand.gameObject.SetActive(false);
            _action = ChefAction.ChoppingBread;
            _knifeTarget = -1;
        }
    }

    /// <summary>
    /// Set the image of the bun to be chopped, based on current bread type
    /// </summary>
    private void SetBunChopImage_()
    {
        // determine which image to use
        int index = 0;
        switch (_selectedBread)
        {
            // brioche
            case SelectionType.BriocheBun:
                index = 0;
                break;
            // sesame
            case SelectionType.SesameBun:
                index = 1;
                break;
            // brown
            case SelectionType.BrownBun:
                index = 2;
                break;
        }

        // set image
        Chef.TopBun.sprite = LicenseToGrillController.Instance.BreadTop[index];
        Chef.ChopBunRendererTop.sprite = LicenseToGrillController.Instance.BreadTop[index];
        Chef.ChopBunRendererBottom.sprite = LicenseToGrillController.Instance.BreadBottoms[index];
    }

    /// <summary>
    /// Serves the burger to the customer
    /// </summary>
    private void Serve_()
    {
        for (int i = 0; i < _burgerSelections.Count; i++)
        {
            // find all burger
            if (_burgerSelections[i] is BurgerPatty)
            {
                // get temperaature of burger
                var patty = _burgerSelections[i] as BurgerPatty;
                var tempTracker = (_burgerElements[i].GetComponent<TemperatureTracker>());

                // set the heat
                patty.SetHeat(tempTracker.Temperature());
                _burgerSelections[i] = patty;   // need to overwrite the list with the local variable to stop it going out of scope
            }
        }

        StartCoroutine(ServeToCustomer_());
    }

    /// <summary>
    /// Clears the plate
    /// </summary>
    private void DisposePlate_()
    {
        StartCoroutine(TipPlate_());
    }

    /// <summary>
    /// Clears all items from the plate
    /// </summary>
    IEnumerator TipPlate_()
    {
        _action = ChefAction.ClearingPlate;

        var targetPosition = Chef.BinY;

        // slide bin in
        while (Chef.Bin.localPosition.y < targetPosition)
        {
            yield return new WaitForSeconds(0.001f);
            Chef.Bin.Translate(new Vector3(0, 0.2f, 0));
        }

        // make all elements fall
        foreach (var item in _burgerElements)
        {
            item.GetComponent<BoxCollider2D>().enabled = false;
        }

        // get waste points for all items wasted
        var wastePointsPrevious = _wastePoints;
        foreach (var item in _burgerSelections)
        {
            if (item is BurgerPatty) _wastePoints -= WASTE_POINTS_BURGER;
            if (item is BurgerVeg) _wastePoints -= WASTE_POINTS_VEG;
            if (item is BurgerBun) _wastePoints -= WASTE_POINTS_BUN;
            if (item is BurgerSauce) _wastePoints -= WASTE_POINTS_SAUCE;
        }

        // display wasted points
        var difference = wastePointsPrevious - _wastePoints;
        Chef.BinText.text = "-" + difference;

        // fade out wasted points text
        for (float i = 1f; i >= 0; i -= 0.1f)
        {
            Chef.BinText.color = new Color(1, 1, 1, i);
            yield return new WaitForSeconds(0.1f);
        }
        Chef.BinText.color = new Color(1, 1, 1, 0);

        yield return new WaitForSeconds(1f);

        // remove elements from plate
        ClearPlate_();

        targetPosition = Chef.BinY - 8f;

        // slide bin out
        while (Chef.Bin.localPosition.y > targetPosition)
        {
            yield return new WaitForSeconds(0.001f);
            Chef.Bin.Translate(new Vector3(0, -0.2f, 0));
        }

        _action = ChefAction.FacingBoard;
    }

    /// <summary>
    /// Moves the plate off the screen and serves the burger
    /// </summary>
    IEnumerator ServeToCustomer_()
    {
        _action = ChefAction.Serving;

        var rightPosition = Chef.PlatePosition() + 12f;

        // move plate right
        while (Chef.Plate.localPosition.x < rightPosition)
        {
            Chef.Plate.Translate(new Vector3(0.25f, 0, 0));
            yield return new WaitForSeconds(0.01f);
        }

        // give the burger to the customer
        _customerHandler.CustomerServed(_burgerSelections);

        // update UI with next 10 orders
        Chef.DisplayOrders(_customerHandler.GetNextOrders(10));

        // remove unused items
        ClearPlate_();

        yield return new WaitForSeconds(0.25f);

        // move plate left
        while (Chef.Plate.localPosition.x > Chef.PlatePosition())
        {
            Chef.Plate.Translate(new Vector3(-0.25f, 0, 0));
            yield return new WaitForSeconds(0.01f);
        }
        Chef.Plate.localPosition = new Vector3(Chef.PlatePosition(), Chef.Plate.localPosition.y, Chef.Plate.localPosition.z);

        // next action
        _action = ChefAction.FacingBoard;
    }

    /// <summary>
    /// Serves the burger to the customer
    /// </summary>
    private void ClearPlate_()
    {
        _burgerItemIndex = 0;

        // destroy all burger elements
        foreach (var item in _burgerElements)
        {
            Destroy(item.gameObject);
        }

        // no more items
        _burgerElements.Clear();
        _burgerSelections.Clear();

        StartCoroutine(FadeOutTopBun_());
    }

    /// <summary>
    /// Fades out the top burger
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadeOutTopBun_()
    {
        var a = 1f;
        while (a > 0)
        {
            // decrease the visibility of the image
            Chef.TopBun.color = new Color(1, 1, 1, a);
            yield return new WaitForSeconds(0.01f);
            a -= 0.1f;
        }

        // reset bun
        Chef.TopBun.gameObject.SetActive(false);
        yield return new WaitForSeconds(1);
        Chef.TopBun.color = new Color(1, 1, 1, 1);
    }

    /// <summary>
    /// Spawn bread
    /// </summary>
    void SpawnBread_(bool top)
    {
        Sprite sprite = null;
        BunType bunType = BunType.Brioche;

        switch (_selectedBread)
        {
            // spawn brioche
            case SelectionType.BriocheBun:
                sprite = top ? LicenseToGrillController.Instance.BreadTop[0] : LicenseToGrillController.Instance.BreadBottoms[0];
                bunType = BunType.Brioche;
                break;
            // spawn sesame
            case SelectionType.SesameBun:
                sprite = top ? LicenseToGrillController.Instance.BreadTop[1] : LicenseToGrillController.Instance.BreadBottoms[1];
                bunType = BunType.Sesame;
                break;
            // spawn brown
            case SelectionType.BrownBun:
                sprite = top ? LicenseToGrillController.Instance.BreadTop[2] : LicenseToGrillController.Instance.BreadBottoms[2];
                bunType = BunType.Brown;
                break;
        }

        // spawn an item
        if (sprite != null)
        {
            SpawnSomething_(LicenseToGrillController.Instance.FoodPlateItemPrefab, sprite, new BurgerBun(bunType), false);
            ShowBreadOptions_(false);
            _action = ChefAction.FacingBoard;
        }
    }

    /// <summary>
    /// Spawn vegetables (or fruit)
    /// </summary>
    void SpawnVeg_()
    {
        // check item is ok
        if (_currentItem.Count == 0) return;
        if (!Chef.TopBun.gameObject.activeInHierarchy) return;

        Sprite sprite = null;
        BurgerVegType vegType = BurgerVegType.Lettuce;

        if (_currentItem.Count > 0)
        {
            switch (_currentItem[0].ObjectType)
            {
                // spawn tomato
                case SelectionType.Tomato:
                    sprite = LicenseToGrillController.Instance.TomatoSlices;
                    vegType = BurgerVegType.Tomato;
                    break;
                // spawn lettuce
                case SelectionType.Lettuce:
                    sprite = LicenseToGrillController.Instance.LettuceSlice;
                    vegType = BurgerVegType.Lettuce;
                    break;
                // spawn pickle
                case SelectionType.Pickle:
                    sprite = LicenseToGrillController.Instance.PickleSlices;
                    vegType = BurgerVegType.Pickle;
                    break;
            }
        }

        // spawn an item
        if (sprite != null)
            SpawnSomething_(LicenseToGrillController.Instance.FoodPlateItemPrefab, sprite, new BurgerVeg(vegType));
    }

    /// <summary>
    /// Spawn a patty
    /// </summary>
    void SpawnBurger_()
    {
        var activeBurger = Chef.Burgers[_selectedPattyIndex];

        // temperature gets done at the end
        BurgerPatty patty = new BurgerPatty(activeBurger.GetBurgerType(), new float[] { activeBurger.GetBurgerColour().r, activeBurger.GetBurgerColourBack().r }, 0, activeBurger.GetBurgerColour());
        var spawned = SpawnSomething_(LicenseToGrillController.Instance.FoodPlateBurgerPrefab, LicenseToGrillController.Instance.Burgers[0], patty, false);

        // check the index is valid
        if (_selectedPattyIndex > -1)
        {
            // set colour of the burger
            var renderers = spawned.GetComponentsInChildren<SpriteRenderer>();
            renderers[0].color = activeBurger.GetBurgerColour();
            renderers[1].color = activeBurger.GetBurgerColourBack();
            renderers[2].color = activeBurger.GetBurgerColourGrill();

            // set sprite of the burger
            renderers[0].sprite = activeBurger.Renderer.sprite;
            renderers[1].sprite = activeBurger.RendererUnder.sprite;
        }
    }

    /// <summary>
    /// Spawns an item
    /// </summary>
    /// <param name="prefab">The prefab to use</param>
    /// <param name="sprite">The sprite to use</param>
    /// <param name="colour">The colour to set the sprite as</param>
    Transform SpawnSomething_(Transform prefab, Sprite sprite, object burgerItem, bool includedInCount = true)
    {
        Transform spawned = null;

        // limit to certain number items
        if ((_burgerItemIndex > MAX_BURGER_ITEMS) && includedInCount)
        {
            // cannot add any more
            ShowErrorMessage_("Too many items on the plate");
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
            _burgerSelections.Add(burgerItem);
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

        // move the hand, spatula or bread
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

    /// <summary>
    /// Controls knife movement
    /// </summary>
    /// <param name="x">X positino of the knife</param>
    private void CheckKnife_(float x)
    {
        const float KNIFE_MOVEMENT = 0.25f;

        // if right, and meant to be going right
        if (x > 0.9f && _knifeTarget > 0)
        {
            Chef.Knife.Translate(new Vector3(KNIFE_MOVEMENT / 5, 0, 0));
            _knifeTarget = -1;
        }
        // if left, and meant to be going left
        else if (x < -0.9f && _knifeTarget < 0)
        {
            Chef.Knife.Translate(new Vector3(-KNIFE_MOVEMENT, 0, 0));
            _knifeTarget = 1;

            // once reached the end
            if (Chef.Knife.localPosition.x <= -3.25f)
            {
                // hide bread, knife
                ShowBreadOptions_(false);
                Chef.Knife.gameObject.SetActive(false);
                _action = ChefAction.FacingBoard;

                //move to next stage
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
        // set state of each bread option
        foreach (var bread in Chef.BreadOptions)
        {
            bread.gameObject.SetActive(state);
        }
    }
}