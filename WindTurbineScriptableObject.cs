// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using Microsoft.Geospatial;
using UnityEngine;

/// <summary>
/// Scriptable Object for Wind Turbine data received from ADT
/// </summary>
[CreateAssetMenu(fileName = "TurbineData", menuName = "Scriptable Objects/Turbine Data/Turbine Data")]
public class WindTurbineScriptableObject : ScriptableObject
{
    /// <summary>
    /// onDataUpdated action
    /// </summary>
    public Action onDataUpdated;

    /// <summary>
    /// Wind turbine data
    /// </summary>
    public WindTurbineData windTurbineData;

    /// <summary>
    /// Wind Turbine Meta Data
    /// </summary>
    public WindTurbineMetaData windTurbineMetaData;

    /// <summary>
    /// Update scriptable object data and invoke defined action.
    /// </summary>
    /// <param name="newWindTurbineData"></param>
    public void UpdateData(WindTurbineData newWindTurbineData)
    {
        windTurbineData = newWindTurbineData;
        onDataUpdated?.Invoke();
    }

    public void UpdateTemp(double temp)
    {
        windTurbineData.MangOHTemperature = temp;
        windTurbineData.MangOHTemperatureF = temp * 1.8 + 32.0;
        onDataUpdated?.Invoke();
    }

    public void UpdateLight(double light)
    {
        windTurbineData.MangOHLight = light;
        onDataUpdated?.Invoke();
    }

    public void UpdateVibration(bool isVibrated)
    {
        if (isVibrated)
        {
            windTurbineData.MangOHVibration = "YES";
            windTurbineData.MangOHVibrationAsFloat = 1.00f;
        }
        else
        {
            windTurbineData.MangOHVibration = "NO";
            windTurbineData.MangOHVibrationAsFloat = 0.0001f;
        }
        
        onDataUpdated?.Invoke();
    }



    /// <summary>
    /// Stores the Wind Turbines user placed location in the scene
    /// </summary>
    public LatLon CurrentLocation { get; set; }
}
