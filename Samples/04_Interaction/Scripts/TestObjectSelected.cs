using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestObjectSelected : MonoBehaviour
{

    protected int _currentSelection = 0;

    public virtual void ObjectSelectedAction()
    {
        Color color;
        if (_currentSelection == 0)
        {
            color = Color.cyan;
        }
        else if (_currentSelection == 1)
        {
            color = Color.magenta;
        }
        else
        {
            color = Color.yellow;
        }

        GetComponent<Renderer>().material.color = color;

        _currentSelection = (_currentSelection + 1) % 3;
    }

}