/**
 * File Name: MathUtils.cs
 * Description: Script that contains various mathematical utility functions
 * 
 * Authors: Will Lacey
 * Date Created: January 15, 2022
 * 
 * Additional Comments: 
 *      File Line Length: 120
 *      
 *      This script has also been created in Project-Fort; although it has been adapted to better fit this package
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtils
{
    /************************************************************/
    #region Fields

    static Texture2D _noiseSource;

    #endregion
    /************************************************************/
    #region Properties

    public static float noiseSampleScale { get; set; } = 0.2f;
    public static float maxHorizontalNoiseDelta { get; set; } = 0.2f;
    public static float maxVerticalNoiseDelta { get; set; } = 0.003f;

    public static Texture2D noiseSource
    {
        get
        {
            if (_noiseSource == null) _noiseSource = Resources.Load<Texture2D>("Noise Perlin");
            return _noiseSource;
        }
        set => _noiseSource = value;
    }

    #endregion
    /************************************************************/
    #region Functions

    #region Noise & Randomness Functions

    public static Vector3 Perturb(Vector3 point, bool perturbHorizontal = true, bool perturbVertical = true)
    {
        return Perturb(point, maxHorizontalNoiseDelta, maxVerticalNoiseDelta, perturbHorizontal, perturbVertical);
    }

    public static Vector3 Perturb(
        Vector3 point, float maxNoiseDelta,
        bool perturbHorizontal = true, bool perturbVertical = true)
    {
        return Perturb(point, maxNoiseDelta, maxNoiseDelta, perturbHorizontal, perturbVertical);
    }

    public static Vector3 Perturb(
        Vector3 point,
        float maxHorizontalNoiseDelta, float maxVerticalNoiseDelta, bool perturbHorizontal, bool perturbVertical)
    {
        // samples the noise source for randomness using a given point, yields a random value between 0 and 1
        Vector4 sample = noiseSource.GetPixelBilinear(point.x * noiseSampleScale, point.z * noiseSampleScale);

        // convert the sample to a value between -1 and 1, then multiply it by it corresponding noise strength
        if (perturbHorizontal)
        {
            point.x += (sample.x * 2f - 1f) * maxHorizontalNoiseDelta;
            point.z += (sample.z * 2f - 1f) * maxHorizontalNoiseDelta;
        }
        if (perturbVertical)
        {
            point.y += (sample.y * 2f - 1f) * maxVerticalNoiseDelta;
        }

        return point;
    }

    public static Vector3 GetRandomVector3(float minInclusive, float maxInclusive)
    {
        return new Vector3
        {
            x = Random.Range(minInclusive, maxInclusive),
            y = Random.Range(minInclusive, maxInclusive),
            z = Random.Range(minInclusive, maxInclusive)
        };
    }

    public static Color GetRandomColor()
    {
        return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
    }

    #endregion

    #region Other Math Functions

    public static float Normalize(float value, float min, float max)
    {
        return (value - min) / (max - min);
    }

    public static int RoundUp(float value)
    {
        return (int) Mathf.Ceil(value);
    }

    public static float Remap(float value, float fromRangeMin, float fromRangeMax, float toRangeMin, float toRangeMax)
    {
        value = Mathf.Clamp(value, fromRangeMin, fromRangeMax); // just in case the value is outside the bounds
        return (value - fromRangeMin) / (fromRangeMax - fromRangeMin) * (toRangeMax - toRangeMin) + toRangeMin;
    }

    #endregion

    #endregion
    /************************************************************/
}
