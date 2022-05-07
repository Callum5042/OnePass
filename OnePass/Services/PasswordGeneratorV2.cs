﻿using System;
using System.Linq;
using System.Text;

namespace OnePass.Services
{
    public class PasswordGeneratorV2
    {
        private const string _lowerCase = "abcdefghijklmnopqrstuvwxyz";
        private const string _upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string _numbers = "0123456789";
        private const string _symbols = "[]{};:@#~<>/!?";

        public bool HasLowercase { get; set; }

        public bool HasUppercase { get; set; }

        public bool HasNumbers { get; set; }

        public bool HasSymbols { get; set; }

        public int MaxLength { get; set; }

        public int MinLength { get; set; }

        public PasswordGeneratorV2()
        {
            HasLowercase = true;
            HasUppercase = true;
            HasNumbers = true;
            HasSymbols = true;

            MinLength = 10;
            MaxLength = 20;
        }

        public string Generate()
        {
            if (MinLength > MaxLength)
            {
                throw new InvalidOperationException($"{nameof(MinLength)} cannot be bigger than {nameof(MaxLength)}");
            }

            var random = new Random();
            var builder = new StringBuilder();

            // Fit criteria
            if (HasLowercase)
            {
                builder.Append(_lowerCase[random.Next(0, _lowerCase.Length - 1)]);
            }

            if (HasUppercase)
            {
                builder.Append(_upperCase[random.Next(0, _upperCase.Length - 1)]);
            }

            if (HasNumbers)
            {
                builder.Append(_numbers[random.Next(0, _numbers.Length - 1)]);
            }

            if (HasSymbols)
            {
                builder.Append(_symbols[random.Next(0, _symbols.Length - 1)]);
            }

            // Fill out rest with random characters
            var minLength = MinLength - builder.Length;
            var maxLength = MaxLength - builder.Length;
            var length = random.Next(minLength, Math.Max(maxLength, minLength));

            var chars = _lowerCase + _upperCase + _numbers + _symbols;
            for (int i = 0; i < length; i++)
            {
                builder.Append(chars[random.Next(0, chars.Length - 1)]);
            }

            // Randomise the string
            var list = builder.ToString().ToArray();
            KnuthShuffle(list);

            return new string(list);
        }

        public static void KnuthShuffle<T>(T[] array)
        {
            var random = new Random();
            for (var i = 0; i < array.Length; i++)
            {
                var j = random.Next(i, array.Length);
                (array[j], array[i]) = (array[i], array[j]);
            }
        }
    }
}
