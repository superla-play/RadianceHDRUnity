using System;
using System.Collections.Generic;
using UnityEngine;

namespace Superla.RadianceHDR
{
    public enum RGBEErrorCode
    {
        READ_ERROR,
        WRITE_ERROR,
        FORMAT_ERROR,
        MEMORY_ERROR
    }

    public static class RGBEError
    {
        public static RGBEReturnCode LogError(RGBEErrorCode code, string message = null)
        {
            switch (code)
            {
                default:
                    Debug.LogError($"{code.ToString()}, {message}");
                    break;
            }
            return RGBEReturnCode.RGBE_RETURN_FAILURE;
        }
    }
}
