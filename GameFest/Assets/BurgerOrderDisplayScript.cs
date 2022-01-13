using UnityEngine;
using UnityEngine.UI;

public class BurgerOrderDisplayScript : MonoBehaviour
{
    public Text TxtCustomer;
    public Image[] Images;

    /// <summary>
    /// Initialise the display
    /// </summary>
    /// <param name="elements">Elements to display</param>
    /// <param name="customerName">The name of the customer</param>
    public void Initialise(BurgerConstruction elements, string customerName)
    {
        // display name
        TxtCustomer.text = customerName;

        int index = 0;
        foreach (var element in elements.GetItems())
        {
            var state = true;
            Sprite sprite = GetSprite_(element, index, ref state);

            // set appearance of the image
            Images[index].gameObject.SetActive(state);
            Images[index].sprite = sprite;

            index++;
        }

        // hide unused images
        for (; index < Images.Length; index++)
        {
            Images[index].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Gets the sprite to use for the burger element
    /// </summary>
    /// <param name="element">The element to display</param>
    /// <param name="index">The index of the element in the burger</param>
    /// <param name="state">The state to set the gameobject as</param>
    /// <returns>The sprite to use for the element</returns>
    private Sprite GetSprite_(object element, int index, ref bool state)
    {
        Sprite sprite = null;

        // pattys
        if (element is BurgerPatty)
        {
            // get correct image
            switch ((element as BurgerPatty).GetBurgerType())
            {
                case BurgerType.Beef: sprite = LicenseToGrillController.Instance.Burgers[0]; break;
                case BurgerType.Chicken: sprite = LicenseToGrillController.Instance.Burgers[1]; break;
                case BurgerType.Veggie: sprite = LicenseToGrillController.Instance.Burgers[2]; break;
            }
        }
        // vegetables
        else if (element is BurgerVeg)
        {
            // get correct image
            switch ((element as BurgerVeg).GetVegType())
            {
                case BurgerVegType.Lettuce: sprite = LicenseToGrillController.Instance.LettuceSlice; break;
                case BurgerVegType.Tomato: sprite = LicenseToGrillController.Instance.TomatoSlices; break;
                case BurgerVegType.Pickle: sprite = LicenseToGrillController.Instance.PickleSlices; break;
            }
        }
        // sauces
        else if (element is BurgerSauce)
        {
            // get correct image
            switch ((element as BurgerSauce).GetSauceType())
            {
                case SauceType.Ketchup: sprite = LicenseToGrillController.Instance.Sauces[0]; break;
                case SauceType.BBQ: sprite = LicenseToGrillController.Instance.Sauces[1]; break;
                case SauceType.Mustard: sprite = LicenseToGrillController.Instance.Sauces[2]; break;
            }
        }
        // buns
        else if (element is BurgerBun)
        {
            if (index < 1)
            {
                // get correct image
                switch ((element as BurgerBun).GetBunType())
                {
                    case BunType.Brioche: sprite = LicenseToGrillController.Instance.BreadTop[0]; break;
                    case BunType.Sesame: sprite = LicenseToGrillController.Instance.BreadTop[1]; break;
                }
            }
            else
            {
                // only show the first bun
                state = false;
            }
        }

        return sprite;
    }
}