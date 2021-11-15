using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CartAttackInputHandler : MonoBehaviour
{
    CarControllerScript _carController;

    private void Awake()
    {
        _carController = GetComponent<CarControllerScript>();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        var x = 0;
        var y = 0;

        if (Input.GetKey(KeyCode.A))
            x = -1;
        if (Input.GetKey(KeyCode.D))
            x = 1;
        if (Input.GetKey(KeyCode.W))
            y = 1;
        if (Input.GetKey(KeyCode.S))
            y = -1;

        if (Input.GetKey(KeyCode.Space))
            _carController.Boost();

        _carController.SetAccelerationValue(y);
        _carController.SetSteeringValue(x);
    }
}
