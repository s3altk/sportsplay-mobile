using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Mobile
{
    public static class PasswordChecker
    {
        private static bool HasLength(string password)
        {
            return password.Length >= 6 ? true : false;
        }

        private static bool ContainsNumber(string password)
        {
            foreach (var item in password)
            {
                if (Char.IsNumber(item))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsSymbolOrPunctuation(string password)
        {
            foreach (var item in password)
            {
                if (Char.IsSymbol(item) || Char.IsPunctuation(item))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsEnglishLowerLetter(string password)
        {
            foreach (var item in password)
            {
                if (item >= 'a' && item <= 'z')
                {
                    if (Char.IsLetter(item) && Char.IsLower(item))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool ContainsEnglishUpperLetter(string password)
        {
            foreach (var item in password)
            {
                if (item >= 'A' && item <= 'Z')
                {
                    if (Char.IsLetter(item) && Char.IsUpper(item))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool IsPasswordCorrect(string password)
        {
            bool len, num, sym, low, up;

            len = HasLength(password);

            if (len)
            {
                low = ContainsEnglishLowerLetter(password);

                if (low)
                {
                    up = ContainsEnglishUpperLetter(password);

                    if (up)
                    {
                        num = ContainsNumber(password);      
 
                        if (num)
                        {
                            sym = ContainsSymbolOrPunctuation(password);

                            if (sym)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}