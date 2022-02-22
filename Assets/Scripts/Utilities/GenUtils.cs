/**
 * File Name: GenUtils.cs
 * Description: Script that contains general utility functions
 * 
 * Authors: Will Lacey
 * Date Created: October 14, 2020
 * 
 * Additional Comments: 
 *      File Line Length: 120
 *
 *      This script relates to MathUtils.cs
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;
using UnityEngine.EventSystems;

public static class GenUtils
{
    #region Mouse Raytracing Functions

    public static bool IsMouseOverGUI()
    {
        return EventSystem.current.IsPointerOverGameObject(); // is pointer with the given ID over EventSystem object?
    }

    public static Ray MouseScreenPointToRay()
    {
        return Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
    }

    #endregion
}