// SPDX-License-Identifier: GPL-3.0-or-later

namespace Vistava.Service.Common;

public class FilePathComparer : IComparer<string>
{
    private const int StackSize = 1024 * 32;
    private readonly char[] xStack = new char[StackSize];
    private readonly char[] yStack = new char[StackSize];

    public int Compare(string? x, string? y)
    {
        x ??= string.Empty;
        y ??= string.Empty;

        if (x.Length > xStack.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(x));
        }
        if (y.Length > yStack.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(y));
        }

        int xIndex = 0, yIndex = 0;
        int xStackIndex = 0, yStackIndex = 0;
        char xChar = (char)0, yChar = (char)0;
        bool xStackIsNumeric = false, yStackIsNumeric = false;
        bool xCharNumeric = false, yCharNumeric = false;
        bool xStackComplete = false, yStackComplete = false;
        int comparisonResult;

        while (true)
        {
            if (xIndex < x.Length)
            {
                xChar = x[xIndex];
                xCharNumeric = char.IsNumber(xChar);
                if (xStackIndex == 0)
                {
                    xStackIsNumeric = xCharNumeric;
                }
            }
            else
            {
                xStackComplete = true;
            }
            if (yIndex < y.Length)
            {
                yChar = y[yIndex];
                yCharNumeric = char.IsNumber(yChar);
                if (yStackIndex == 0)
                {
                    yStackIsNumeric = yCharNumeric;
                }
            }
            else
            {
                yStackComplete = true;
            }

            if (xStackComplete && yStackComplete)
            {
                comparisonResult = Compare(xStack, xStackIndex, xStackIsNumeric,
                    yStack, yStackIndex, yStackIsNumeric);
                if (comparisonResult != 0)
                {
                    return comparisonResult;
                }

                if (xIndex < x.Length)
                {
                    xStackComplete = false;
                    xStackIndex = 0;
                }
                if (yIndex < y.Length)
                {
                    yStackComplete = false;
                    yStackIndex = 0;
                }
                if (xStackComplete && yStackComplete)
                {
                    return 0;
                }
            }
            else if (!xStackComplete || !yStackComplete)
            {
                if (xStackIsNumeric == xCharNumeric && xStackIndex < xStack.Length &&
                    xIndex < x.Length && !xStackComplete)
                {
                    xStack[xStackIndex++] = xChar;
                    xIndex++;
                }
                else
                {
                    xStackComplete = true;
                }

                if (yStackIsNumeric == yCharNumeric && yStackIndex < yStack.Length &&
                    yIndex < y.Length && !yStackComplete)
                {
                    yStack[yStackIndex++] = yChar;
                    yIndex++;
                }
                else
                {
                    yStackComplete = true;
                }
            }
            else
            {
                return 0;
            }
        }
    }

    private static int Compare(char[] x, int xLength, bool xNumeric, char[] y, int yLength, bool yNumeric)
    {
        if (xLength == 0 || yLength == 0)
        {
            return xLength.CompareTo(yLength);
        }
        else
        {
            if (xNumeric && yNumeric)
            {
                // Start the comparison at the first encountered number from 1-9.
                int xIndex = 0, yIndex = 0;
                for (int i = 0; i < xLength; i++)
                {
                    char c = x[i];
                    if (c >= 49 && c <= 57)
                    {
                        xIndex = i;
                        break;
                    }
                }
                for (int i = 0; i < yLength; i++)
                {
                    char c = y[i];
                    if (c >= 49 && c <= 57)
                    {
                        yIndex = i;
                        break;
                    }
                }

                // If the actual number count is different, the number with more numbers is bigger.
                int xDecimals = xLength - xIndex;
                int yDecimals = yLength - yIndex;
                if (xDecimals > yDecimals)
                {
                    return 1;
                }
                else if (xDecimals < yDecimals)
                {
                    return -1;
                }
                else
                {
                    // Compare each individual decimal from left to right and stop when one decimal
                    // is bigger than the other one.
                    while (xIndex < xLength && yIndex < yLength)
                    {
                        char xChar = x[xIndex++];
                        char yChar = y[yIndex++];
                        if (xChar > yChar)
                        {
                            return 1;
                        }
                        else if (xChar < yChar)
                        {
                            return -1;
                        }
                    }
                    return 0;
                }
            }
            else
            {
                int length = Math.Min(xLength, yLength);

                for (int i = 0; i < length; i++)
                {
                    char xChar = char.ToLowerInvariant(x[i]);
                    char yChar = char.ToLowerInvariant(y[i]);
                    if (xChar < yChar)
                    {
                        return -1;
                    }
                    else if (xChar > yChar)
                    {
                        return 1;
                    }
                }

                if (xLength < yLength)
                {
                    return -1;
                }
                else if (xLength > yLength)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
