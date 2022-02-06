using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public enum ChefAction { NotStarted, FacingBoard, FacingGrill, SelectingBread, ChoppingBread, SelectingBurger, SlidingBurgerBoard, Serving, ClearingPlate, Confirmation, ViewingOrders, SauceIntoPosition, SquirtingSauce, SauceReturn, ChoppingTomato, ChoppingPickle };

public class LicenseToGrillInputHandler : GenericInputHandler
{
    // constants
    const uint MAX_BURGER_ITEMS = 7;
    const float SPAWN_POINT_OFFSET = 18f;
    const int WASTE_POINTS_BURGER = 20;
    const int WASTE_POINTS_VEG = 10;
    const int WASTE_POINTS_SAUCE = 5;
    const int WASTE_POINTS_BUN = 10;
    const float SAUCE_SQUIRT_BOTTLE_Y_POSITION = 6f;

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
    Coroutine _breadBinRoutine;
    float _burgerOffsetY = 0.6f;
    float _burgerSizeY = 4.4f;
    float _saucesYPosition;
    SauceType _selectedSauce;
    Vector2 _saucePlatformSize;
    bool _squirting = false;
    Vector2 _movementVector = Vector2.zero;

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

        // assign callbacks for when the selection hand enters a trigger, and when an action is complete
        Chef.AddItemSelectionCallbacks(TriggerEntered_, TriggerExited_);
        Chef.AssignActionCallback(UpdateAction_);

        // store position of the sauce bottles
        _saucesYPosition = Chef.SauceBottles[0].localPosition.y;
        _saucePlatformSize = Chef.SaucePlatform.transform.localScale;
    }

    /// <summary>
    /// Function that can be called from other scripts once an action is complete
    /// </summary>
    /// <param name="newAction"></param>
    void UpdateAction_(ChefAction newAction)
    {
        _action = newAction;

        // does anything need done with this new state?
        switch (_action)
        {
            case ChefAction.SauceReturn:
                // move bottles back to position
                StartCoroutine(MoveSauceDown_());
                break;
        }
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
        bool valid = true;

        _currentItem.Insert(0, cso);

        // check that this item can be selected
        switch (_currentItem[0].ObjectType)
        {
            case SelectionType.BreadBin:
                valid = _burgerItemIndex == 0 && !Chef.BreadOptions[0].isActiveAndEnabled;
                if (_breadBinRoutine != null)
                {
                    StopCoroutine(_breadBinRoutine);
                }
                _breadBinRoutine = StartCoroutine(OpenBreadBin_());
                break;
            case SelectionType.Lettuce:
            case SelectionType.Tomato:
            case SelectionType.Pickle:
                valid = _burgerItemIndex > 0 && Chef.TopBun.gameObject.activeInHierarchy;
                break;

            case SelectionType.Sauce:
                valid = (_burgerItemIndex > 0) && (Chef.SquirtBottle.SauceImage.transform.localScale.x == 0) && Chef.TopBun.gameObject.activeInHierarchy; ;
                break;
        }

        // don't do anything if the action is not appropriate

        if (!valid) return;

        // if entered a grill zone, keep track of it
        if (_currentItem[0].ObjectType == SelectionType.GrillZone)
        {
            _selectedPattyIndex = _currentItem[0].Index;
        }

        SelectFirstItem_();
        UpdateHelp_();
    }

    /// <summary>
    /// Opens the bread bin
    /// </summary>
    private IEnumerator OpenBreadBin_()
    {
        for (int i = 0; i < Chef.BreadBinSprites.Length; i++)
        {
            Chef.BreadBin.sprite = Chef.BreadBinSprites[i];
            yield return new WaitForSeconds(0.05f);
        }
    }

    /// <summary>
    /// Closes the bread bin
    /// </summary>
    private IEnumerator CloseBreadBin_()
    {
        for(int i = Chef.BreadBinSprites.Length - 1; i >=0; i--)
        {
            Chef.BreadBin.sprite = Chef.BreadBinSprites[i];
            yield return new WaitForSeconds(0.05f);
        }
    }

    /// <summary>
    /// Selects the first item
    /// </summary>
    private void SelectFirstItem_()
    {
        if (_currentItem.Count > 0)
        {
            _currentItem[0].Selected();

            if (_currentItem[0].ObjectType == SelectionType.GrillZone)
            {
                // make burger glow instead
                var burgerIndex = _currentItem[0].Index;
                if (Chef.Burgers[burgerIndex].isActiveAndEnabled)
                {
                    _currentItem[0].RendererGlow.gameObject.SetActive(false);
                    Chef.Burgers[burgerIndex].Glow.SetActive(true);
                }
            }
        }
    }

    /// <summary>
    /// Called when a trigger is left
    /// </summary>
    /// <param name="cso">The selection object that was collided with</param>
    void TriggerExited_(CookingSelectionObject cso)
    {
        _currentItem.Remove(cso);
        cso.Unselected();

        switch (cso.ObjectType)
        {
            case SelectionType.BreadBin:
            {
                if (_breadBinRoutine != null)
                {
                    StopCoroutine(_breadBinRoutine);
                }
                _breadBinRoutine = StartCoroutine(CloseBreadBin_());
                break;
            }
            case SelectionType.GrillZone:
            {
                if (_currentItem.Count == 0)
                {
                    Chef.Burgers[_selectedPattyIndex].Glow.SetActive(false);
                    _selectedPattyIndex = -1;
                }
                break;
            }
        }
        SelectFirstItem_();
        UpdateHelp_();
    }

    /// <summary>
    /// Shows the appropriate help
    /// </summary>
    private void UpdateHelp_()
    {
        // hide all
        Chef.Help_BlankBurger.SetActive(false);
        Chef.Help_Sauce.SetActive(false);
        Chef.Help_Burger.SetActive(false);
        Chef.Help_SelectItem.SetActive(false);
        Chef.Help_AddToBurger.SetActive(false);
        Chef.Help_Plate.SetActive(false);
        Chef.Help_Chop.SetActive(false);
        Chef.Help_Bread.SetActive(false);
        Chef.Help_GrillLeft.SetActive(false);
        Chef.Help_BoardRight.SetActive(false);

        GameObject helpToShow = null;

        switch (_action)
        {
            case ChefAction.FacingBoard:
            {
                if (_currentItem.Count > 0)
                {
                    switch (_currentItem[0].ObjectType)
                    {
                        // player is selecting something
                        case SelectionType.BreadBin:
                        case SelectionType.BriocheBun:
                        case SelectionType.BrownBun:
                        case SelectionType.SesameBun:
                        case SelectionType.Lettuce:
                        case SelectionType.Tomato:
                        case SelectionType.Pickle:
                        case SelectionType.Sauce:
                            helpToShow = Chef.Help_SelectItem;
                            break;
                        // player is on the plate
                        case SelectionType.Plate:
                            helpToShow = Chef.Help_Plate;
                            break;
                        case SelectionType.BriocheBunTop:
                        case SelectionType.BrownBunTop:
                        case SelectionType.SesameBunTop:
                            helpToShow = Chef.Help_AddToBurger;
                            break;
                    }
                }
                Chef.Help_GrillLeft.SetActive(true);
                break;
            }
            case ChefAction.FacingGrill:
            {
                if (_currentItem.Count > 0)
                {
                    switch (_currentItem[0].ObjectType)
                    {
                        // player is on a burger zone
                        case SelectionType.GrillZone:
                            helpToShow = Chef.Burgers[_selectedPattyIndex].isActiveAndEnabled ? Chef.Help_Burger : Chef.Help_SelectItem;
                            break;
                    }
                }

                Chef.Help_BoardRight.SetActive(true);

                break;
            }
            case ChefAction.SelectingBread:
            {
                if (_currentItem.Count > 0)
                {
                    switch (_currentItem[0].ObjectType)
                    {
                        // player is over the top of the bun
                        case SelectionType.BriocheBunTop:
                        case SelectionType.SesameBunTop:
                        case SelectionType.BrownBunTop:
                            helpToShow = Chef.Help_AddToBurger;
                            break;
                    }
                }
                break;
            }
            case ChefAction.ChoppingBread:
            {
                helpToShow = Chef.Help_Bread;
                break;
            }
            case ChefAction.ChoppingPickle:
            case ChefAction.ChoppingTomato:
            {
                helpToShow = Chef.Help_Chop;
                break;
            }
        }
        helpToShow?.SetActive(true);
    }

    /// <summary>
    /// Cancels the veg chopping process
    /// </summary>
    /// <param name="veg">The chopping to cancel</param>
    private void CancelVegChop_(ChopItem veg)
    {
        if (veg.CanCancel())
        {
            veg.ResetItem();
            _action = ChefAction.FacingBoard;
            Chef.SelectionHand.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Flicks the spatula up, then flips the burger
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    IEnumerator DoFlip_(Action action)
    {
        const int ROTATION_ANGLE = 40;

        // flick spatula up
        for(int i = 0; i < ROTATION_ANGLE; i+=8)
        {
            Chef.SelectionSpatula.transform.eulerAngles = new Vector3(i, 0, 0);
            yield return new WaitForSeconds(0.001f);
        }

        // return spatula down
        for (float i = ROTATION_ANGLE; i >= 0; i-=8)
        {
            Chef.SelectionSpatula.transform.eulerAngles = new Vector3(i, 0, 0);
            yield return new WaitForSeconds(0.001f);
        }

        // flip burger
        Chef.Burgers[_selectedPattyIndex].Flip(action);
    }

    /// <summary>
    /// When the spacebar is pressed
    /// </summary>
    private void CrossPressed_()
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
                            StartCoroutine(DoFlip_(() => { StartCoroutine(Chef.Burgers[_flippedPattyIndex].StartNewSide()); }));
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
                            StartVegChop_(BurgerVegType.Lettuce);
                            break;
                        case SelectionType.Tomato:
                            StartVegChop_(BurgerVegType.Tomato);
                            break;
                        case SelectionType.Pickle:
                            StartVegChop_(BurgerVegType.Pickle);
                            break;
                        case SelectionType.Sauce:
                            DoSauce_(_currentItem[0].Index);
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
                        case SelectionType.BrownBun:
                            ChopBread_();
                            break;
                    }
                }
                break;
            }
            case ChefAction.Confirmation:
            {
                _confirmCallback?.Invoke();
                Chef.ConfirmPopup.SetActive(false);
                break;
            }
            case ChefAction.SelectingBurger:
            {
                DropBurger(1);
                break;
            }
        }
    }

    /// <summary>
    /// Begins chopping an item
    /// </summary>
    private void StartVegChop_(BurgerVegType veg)
    {
        bool doChop = false;

        ChopItem choppingItem = null;

        switch (veg)
        {
            case BurgerVegType.Lettuce:
                SpawnVeg_(BurgerVegType.Lettuce);
                break;
            case BurgerVegType.Tomato:
                doChop = true;
                choppingItem = Chef.ChoppingTomato;
                _action = ChefAction.ChoppingTomato;
                break;
            case BurgerVegType.Pickle:
                doChop = true;
                choppingItem = Chef.ChoppingPickle;
                _action = ChefAction.ChoppingPickle;
                break;
        }

        if (doChop)
        {
            Chef.SelectionHand.gameObject.SetActive(false);
            choppingItem.Initialise(veg, VegChopComplete_);
            choppingItem.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Callback when chopping veg is complete
    /// </summary>
    void VegChopComplete_(BurgerVegType veg)
    {
        SpawnVeg_(veg);
        Chef.SelectionHand.gameObject.SetActive(true);
        _action = ChefAction.FacingBoard;
    }

    /// <summary>
    /// Handles the selection and squirting of sauce
    /// </summary>
    /// <param name="index">The index of the selected sauce</param>
    private void DoSauce_(int index)
    {
        // move selected bottle up (change state)
        Chef.SelectionHand.gameObject.SetActive(false);
        _action = ChefAction.SauceIntoPosition;
        StartCoroutine(MoveSauceUp_(index));
    }

    /// <summary>
    /// Moves the selected bottle up, and the squirt bottle down
    /// </summary>
    /// <param name="index">The bottle to move</param>
    private IEnumerator MoveSauceUp_(int index)
    {
        _selectedSauce = (SauceType)index;

        var bottle = Chef.SauceBottles[index];

        // move bottle up
        while (bottle.localPosition.y < 8)
        {
            bottle.Translate(new Vector3(0, 14 * Time.deltaTime, 0));
            yield return new WaitForSeconds(0.01f);
        }

        // change sauce appearance
        Chef.SquirtBottle.BottleImage.sprite = LicenseToGrillController.Instance.SauceImages[index];
        Chef.SquirtBottle.SauceImage.color = ColourFetcher.GetSauceColour(index);

        // change particle colour
        var particleMain = Chef.SquirtBottle.SquirtParticle.main;
        particleMain.startColor = ColourFetcher.GetSauceColour(index);

        // move big bottle down
        while (Chef.SquirtBottle.transform.localPosition.y > SAUCE_SQUIRT_BOTTLE_Y_POSITION)
        {
            Chef.SquirtBottle.transform.Translate(new Vector3(0, -14 * Time.deltaTime, 0));
            yield return new WaitForSeconds(0.01f);
        }

        _action = ChefAction.SquirtingSauce;
        Chef.SaucePlatform.gameObject.SetActive(true);
        Chef.Help_Sauce.SetActive(true);
    }

    /// <summary>
    /// Moves the selected bottle down, and the squirt bottle up
    /// </summary>
    /// <param name="index">The bottle to move</param>
    private IEnumerator MoveSauceDown_(bool cancelled = false)
    {
        Chef.Help_Sauce.SetActive(false);

        if (!cancelled)
        {
            yield return new WaitForSeconds(1f);

            _burgerSelections.Add(new BurgerSauce(_selectedSauce, Chef.SquirtBottle.SauceImage.transform.localScale.x));
            _burgerElements.Add(null);
            _burgerItemIndex++;
        }

        // the highest bottle is the one to use
        var bottle = Chef.SauceBottles.Where(b => b.localPosition.y >= Chef.SauceBottles.Max(c => c.localPosition.y)).First();

        // move big bottle up
        while (Chef.SquirtBottle.transform.localPosition.y < 11)
        {
            Chef.SquirtBottle.transform.Translate(new Vector3(0, 14 * Time.deltaTime, 0));
            yield return new WaitForSeconds(0.01f);
        }

        // move bottle down
        while (bottle.localPosition.y > _saucesYPosition)
        {
            bottle.Translate(new Vector3(0, -14 * Time.deltaTime, 0));
            yield return new WaitForSeconds(0.01f);
        }
        bottle.transform.localPosition = new Vector3(bottle.localPosition.x, _saucesYPosition, bottle.localPosition.z);

        // continue to next action
        _action = ChefAction.FacingBoard;
        Chef.SelectionHand.gameObject.SetActive(true);
        Chef.SaucePlatform.gameObject.SetActive(false);
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
        Chef.ErrorPopup.SetActive(false);
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

        // stop glowing the grill zone
        if (_currentItem[0].ObjectType == SelectionType.GrillZone)
        {
            burger.Glow.SetActive(true);
            _currentItem[0].RendererGlow.gameObject.SetActive(false);
        }

        // show burger
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
            UpdateHelp_();
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

        _burgerOffsetY = 0.6f;
        _burgerSizeY = 4.4f;

        // destroy all burger elements
        foreach (var item in _burgerElements)
        {
            if (item != null)
                Destroy(item.gameObject);
        }

        // no more items
        _burgerElements.Clear();
        _burgerSelections.Clear();

        // reset sauce
        Chef.SquirtBottle.ResetSauce();
        Chef.SaucePlatform.transform.localScale = _saucePlatformSize;

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
    void SpawnVeg_(BurgerVegType veg)
    {
        // check item is ok
        if (!Chef.TopBun.gameObject.activeInHierarchy) return;

        Sprite sprite = null;

        switch (veg)
        {
            // spawn tomato
            case BurgerVegType.Tomato:
                sprite = LicenseToGrillController.Instance.TomatoSlices;
                break;
            // spawn lettuce
            case BurgerVegType.Lettuce:
                sprite = LicenseToGrillController.Instance.LettuceSlice;
                break;
            // spawn pickle
            case BurgerVegType.Pickle:
                sprite = LicenseToGrillController.Instance.PickleSlices;
                break;
        }


        // spawn an item
        if (sprite != null)
            SpawnSomething_(LicenseToGrillController.Instance.FoodPlateItemPrefab, sprite, new BurgerVeg(veg));
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
            collider.offset = new Vector2(collider.offset.x, _burgerOffsetY);
            collider.size = new Vector2(collider.size.x, _burgerSizeY);

            // move to next position
            _burgerItemIndex++;

            _burgerElements.Add(spawned);
            _burgerSelections.Add(burgerItem);

            _burgerOffsetY -= GetOffsetY_(burgerItem);
            _burgerSizeY += GetSizeY_(burgerItem);

            // update sauce if not already placed
            if (Chef.SquirtBottle.SauceImage.transform.localScale.x == 0)
            {
                Chef.SaucePlatform.transform.localScale += 2 * new Vector3(0, GetOffsetY_(burgerItem));
                Chef.SquirtBottle.SauceImage.transform.Translate(new Vector3(0, GetOffsetY_(burgerItem), 0));
                Chef.SquirtBottle.SauceImage.transform.localPosition = new Vector3(0, Chef.SquirtBottle.SauceImage.transform.localPosition.y, -0.1f - (0.1f * _burgerItemIndex));
            }
        }

        return spawned;
    }

    /// <summary>
    /// Get the offset of the collider to use
    /// </summary>
    /// <param name="burgerItem">The item that is to be created</param>
    /// <returns>The offset to use</returns>
    private float GetOffsetY_(object burgerItem)
    {
        float offset = 0.325f;
        if (burgerItem is BurgerVeg)
            offset = 0.17f;
        if (burgerItem is BurgerBun && _burgerItemIndex > 0)
            offset *= 0.7f;

        return offset;
    }

    /// <summary>
    /// Get the size of the collider to use
    /// </summary>
    /// <param name="burgerItem">The item that is to be created</param>
    /// <returns>The size to use</returns>
    private float GetSizeY_(object burgerItem)
    {
        float size = 0.02f;
        if (burgerItem is BurgerVeg)
            size = 0.008f;
        if (burgerItem is BurgerBun && _burgerItemIndex > 0)
            size *= 0.7f;

        return size;
    }

    /// <summary>
    /// Controls knife movement
    /// </summary>
    /// <param name="x">X positino of the knife</param>
    private void CheckKnife_(float x)
    {
        const float KNIFE_MOVEMENT = 0.3f;

        // if right, and meant to be going right
        if (x > 0.9f && _knifeTarget > 0)
        {
            Chef.Knife.Translate(new Vector3(KNIFE_MOVEMENT / 5, 0, 0));
            _knifeTarget = -1;
        }
        // if left, and meant to be going left
        else if (x < -0.9f && _knifeTarget < 0)
        {
            Chef.BreadChopSound.Play();

            Chef.Knife.Translate(new Vector3(-KNIFE_MOVEMENT, 0, 0));
            _knifeTarget = 1;

            // once reached the end
            if (Chef.Knife.localPosition.x <= -3f)
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
                UpdateHelp_();
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

    #region Button inputs
    public override void OnL1()
    {
        if (PauseGameHandler.Instance.IsPaused() && IsHost())
        {
            PauseGameHandler.Instance.PreviousPage();
        }
        else
        {
            // move the camera based on the current action
            switch (_action)
            {
                case ChefAction.FacingBoard:
                    Chef.CameraLeft_();
                    _action = ChefAction.FacingGrill;
                    UpdateHelp_();
                    break;
            }
        }
    }

    public override void OnR1()
    {
        if (PauseGameHandler.Instance.IsPaused() && IsHost())
        {
            PauseGameHandler.Instance.NextPage();
        }
        else
        {
            // move the camera based on the current action
            switch (_action)
            {
                case ChefAction.FacingGrill:
                    Chef.CameraRight_();
                    _action = ChefAction.FacingBoard;
                    UpdateHelp_();
                    break;
            }
        }
    }

    /// <summary>
    /// Override function for the cross button
    /// </summary>
    public override void OnCross()
    {
        CrossPressed_();
    }

    /// <summary>
    /// Override function for the circle button
    /// </summary>
    public override void OnCircle()
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
                SelectFirstItem_();
                break;
            case ChefAction.SquirtingSauce:
                _action = ChefAction.SauceReturn;
                StartCoroutine(MoveSauceDown_(true));
                break;
            case ChefAction.ChoppingTomato:
                CancelVegChop_(Chef.ChoppingTomato);
                break;
            case ChefAction.ChoppingPickle:
                CancelVegChop_(Chef.ChoppingPickle);
                break;
            case ChefAction.Confirmation:
                _action = _actionCopy;
                Chef.ConfirmPopup.SetActive(false);
                break;
        }
    }

    /// <summary>
    /// Override function for the triangle button
    /// </summary>
    public override void OnTriangle()
    {
        switch (_action)
        {
            case ChefAction.FacingBoard:
            {
                // dispose of plate
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
                    // dispose of burger
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
            case ChefAction.SelectingBurger:
            {
                DropBurger(0);
                break;
            }
            case ChefAction.ChoppingTomato:
            {
                Chef.ChoppingTomato.Slice();
                break;
            }
            case ChefAction.ChoppingPickle:
            {
                Chef.ChoppingPickle.Slice();
                break;
            }
        }
    }

    /// <summary>
    /// Override function for the square button
    /// </summary>
    public override void OnSquare()
    {
        switch (_action)
        {
            case ChefAction.SelectingBurger:
            {
                DropBurger(2);
                break;
            }
            case ChefAction.FacingGrill:
            {
                if (_selectedPattyIndex > -1)
                {
                    // flip burger
                    if (Chef.TopBun.gameObject.activeInHierarchy)
                    {
                        StartCoroutine(DoFlip_(() =>
                        {
                            _flippedPattyIndex = _selectedPattyIndex;
                            SpawnBurger_();
                            Chef.Burgers[_flippedPattyIndex].gameObject.SetActive(false);
                            Chef.Burgers[_flippedPattyIndex].ResetBurger();
                        }));
                    }
                    else
                    {
                        ShowErrorMessage_("Can't serve burger without a bun on the plate");
                    }
                }
                break;
            }
        }
    }

    /// <summary>
    /// Override function for the touchpad button
    /// </summary>
    public override void OnTouchpad()
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

    /// <summary>
    /// Override function for the R2 button
    /// </summary>
    public override void OnR2(InputAction.CallbackContext ctx)
    {
        if (_action == ChefAction.SquirtingSauce)
        {
            // squirt sauce
            var val = ctx.ReadValue<float>();

            if (val > 0.8f && !_squirting)
            {
                _squirting = true;
                Chef.SquirtBottle.Squirt();
            }
            if (val < 0.2f && _squirting)
            {
                Chef.SquirtBottle.StopSquirt();
                _squirting = false;
            }
        }
    }

    /// <summary>left joystick
    /// Override function for the 
    /// </summary>
    public override void OnMove(InputAction.CallbackContext ctx, InputDevice device)
    {
        _movementVector = ctx.ReadValue<Vector2>();
    }

    private void Update()
    {
        // move the hand, spatula or bread
        switch (_action)
        {
            case ChefAction.FacingBoard:
            case ChefAction.SelectingBread:
                Chef.MoveHand(_movementVector.x, _movementVector.y);
                break;
            case ChefAction.FacingGrill:
                Chef.MoveSpatula(_movementVector.x, _movementVector.y);
                break;
            case ChefAction.ChoppingBread:
                CheckKnife_(_movementVector.x);
                break;
        }
    }
    #endregion
}