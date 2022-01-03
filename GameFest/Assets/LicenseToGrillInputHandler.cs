using UnityEngine;

enum ChefAction { FacingBoard, FacingGrill };

public class LicenseToGrillInputHandler : GenericInputHandler
{
    // constants
    const uint MAX_BURGER_ITEMS = 7;
    const float SPAWN_POINT_OFFSET = 18f;

    // components
    ChefScript Chef;

    // status variables
    uint _burgerItemIndex = 0;
    CookingSelectionObject _currentItem = null;
    ChefAction _action = ChefAction.FacingBoard;

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
    }

    /// <summary>
    /// Called when a trigger is left
    /// </summary>
    /// <param name="cso">The selection object that was collided with</param>
    void TriggerExited_(CookingSelectionObject cso)
    {
        _currentItem = null;
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
            // move the camera based on the current action
            switch (_action)
            {
                case ChefAction.FacingGrill:
                    Chef.Burgers[0].Flip();
                    break;
                case ChefAction.FacingBoard:
                    if (_currentItem != null)
                        SpawnBunOrVeg_();
                    break;
            }
        }

        // TEMP:
        CheckMove_();
    }

    /// <summary>
    /// Spawn bread or vegetables
    /// </summary>
    void SpawnBunOrVeg_()
    {
        if (_currentItem == null) return;

        Sprite sprite = null;

        switch (_currentItem.ObjectType)
        {
            // spawn bread
            case SelectionType.BreadBin:
                sprite = LicenseToGrillController.Instance.BreadBottoms[0];
                break;
            // spawn tomato
            case SelectionType.Tomato:
                sprite = LicenseToGrillController.Instance.TomatoSlices;
                break;
            // spawn lettuce
            case SelectionType.Lettuce:
                sprite = LicenseToGrillController.Instance.LettuceSlice;
                break;
            // spawn sauce
            case SelectionType.Sauce:
                // TODO: implement this
                break;
        }

        // spawn an item
        if (sprite != null)
            SpawnSomething_(sprite, new Color(1, 1, 1));
    }

    /// <summary>
    /// Spawn a patty
    /// </summary>
    void SpawnBurger_()
    {
        Sprite sprite = LicenseToGrillController.Instance.Burgers[0];
        // TODO: get type of burger from the one that was selected
        // TODO: change colour based on the one that was selected
        SpawnSomething_(sprite, new Color(1, 1, 1));
    }

    /// <summary>
    /// Spawns an item
    /// </summary>
    /// <param name="sprite">The sprite to use</param>
    /// <param name="colour">The colour to set the sprite as</param>
    void SpawnSomething_(Sprite sprite, Color colour)
    {
        // limit to certain number items
        if (_burgerItemIndex > MAX_BURGER_ITEMS)
        {
            // TODO: display message
        }
        else
        {
            // spawn an item
            var food = Instantiate(LicenseToGrillController.Instance.FoodPlateItemPrefab, new Vector3(0, 0, 0), Quaternion.identity, Chef.Plate);
            food.localPosition = new Vector3(0, SPAWN_POINT_OFFSET, -0.1f - (0.1f * _burgerItemIndex));
            food.localScale = new Vector3(1, 1, 1);
            food.GetComponent<SpriteRenderer>().sprite = sprite;

            // adjust collider
            var collider = food.GetComponent<BoxCollider2D>();
            collider.offset = new Vector2(collider.offset.x, 0.6f - (_burgerItemIndex * 0.4f));
            collider.size = new Vector2(collider.size.x, 4.44f + (_burgerItemIndex * 0.025f));

            // move to next position
            _burgerItemIndex++;
        }
    }

    /// <summary>
    /// Check if the player is moving
    /// </summary>
    private void CheckMove_()
    {
        float x = 0f, y = 0f;

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
                Chef.MoveHand(x, y);
                break;
        }
    }
}
