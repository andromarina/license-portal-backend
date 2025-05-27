using System;
using System.Text;

public static class ByteArrayColorConverter
{
    /// <summary>
    /// Converts magenta pixels to white in a BMP byte array
    /// </summary>
    /// <param name="hexString">The hexadecimal string representing the BMP file</param>
    /// <returns>Modified hexadecimal string with magenta converted to white</returns>
    public static string ConvertMagentaToWhite(string hexString)
    {
        // Convert hex string to byte array
        byte[] bytes = new byte[hexString.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
        }
        
        // Find the pixel data offset (typically 54 bytes for BMP)
        int pixelDataOffset = BitConverter.ToInt32(bytes, 10);
        
        // Get the bit depth
        int bitDepth = BitConverter.ToInt16(bytes, 28);
        
        // For 32-bit BMP (BGRA format)
        if (bitDepth == 32)
        {
            // Process each pixel (4 bytes per pixel: B,G,R,A)
            for (int i = pixelDataOffset; i < bytes.Length; i += 4)
            {
                // Check if we have enough bytes for a full pixel
                if (i + 2 < bytes.Length)
                {
                    // Check for magenta (high R, low G, high B)
                    byte b = bytes[i];      // Blue
                    byte g = bytes[i + 1];  // Green
                    byte r = bytes[i + 2];  // Red
                    
                    int threshold = 100;
                    if (r > (255 - threshold) && g < threshold && b > (255 - threshold))
                    {
                        // Change to white (255,255,255)
                        bytes[i] = 255;     // Blue
                        bytes[i + 1] = 255; // Green
                        bytes[i + 2] = 255; // Red
                        // Alpha channel (i+3) remains unchanged
                    }
                }
            }
        }
        
        // Convert byte array back to hex string
        StringBuilder result = new StringBuilder(bytes.Length * 2);
        foreach (byte b in bytes)
        {
            result.Append(b.ToString("X2"));
        }
        
        return result.ToString();
    }
    
    /// <summary>
    /// Converts a hexadecimal string to a base64 string
    /// </summary>
    /// <param name="hexString">The hexadecimal string to convert</param>
    /// <returns>Base64 encoded string</returns>
    public static string HexToBase64(string hexString)
    {
        // Remove any non-hex characters
        hexString = System.Text.RegularExpressions.Regex.Replace(hexString, "[^0-9A-Fa-f]", "");
        
        // Convert hex to byte array
        byte[] bytes = new byte[hexString.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
        }
        
        // Convert to base64
        return Convert.ToBase64String(bytes);
    }
    
    /// <summary>
    /// Converts magenta to white in a BMP hex string and returns the result as base64
    /// </summary>
    /// <param name="hexString">The hexadecimal string representing the BMP file</param>
    /// <returns>Base64 encoded string with magenta converted to white</returns>
    public static string ConvertMagentaToWhiteAndToBase64(string hexString)
    {
        // First convert magenta to white
        string modifiedHex = ConvertMagentaToWhite(hexString);
        
        // Then convert to base64
        return HexToBase64(modifiedHex);
    }
}
