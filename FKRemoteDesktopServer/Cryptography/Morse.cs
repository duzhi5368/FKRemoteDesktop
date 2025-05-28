using System.Collections.Generic;
using System.Text;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Cryptography
{
    public static class Morse
    {
        private static Dictionary<char, string> _morseAlphabetDictionary;

        public static void InitializeDictionary()
        {
            _morseAlphabetDictionary = new Dictionary<char, string>()
            {
                {'a',".-"},{'A',"^.-"},{'b',"-..."},{'B',"^-..."},{'c',"-.-."},{'C',"^-.-."},{'d',"-.."},{'D',"^-.."},
                {'e',"."},{'E',"^."},{'f',"..-."},{'F',"^..-."},{'g',"--."},{'G',"^--."},{'h',"...."},{'H',"^...."},
                {'i',".."},{'I',"^.."},{'j',".---"},{'J',"^.---"},{'k',"-.-"},{'K',"^-.-"},{'l',".-.."},{'L',"^.-.."},
                {'m',"--"},{'M',"^--"},{'n',"-."},{'N',"^-."},{'o',"---"},{'O',"^---"},{'p',".--."},{'P',"^.--."},
                {'q',"--.-"},{'Q',"^--.-"},{'r',".-."},{'R',"^.-."},{'s',"..."},{'S',"^..."},{'t',"-"},{'T',"^-"},
                {'u',"..-"},{'U',"^..-"},{'v',"...-"},{'V',"^...-"},{'w',".--"},{'W',"^.--"},{'x',"-..-"},{'X',"^-..-"},
                {'y',"-.--"},{'Y',"^-.--"},{'z',"--.."},{'Z',"^--.."},{'0',"-----"},{'1',".----"},{'2',"..---"},
                {'3',"...--"},{'4',"....-"},{'5',"....."},{'6',"-...."},{'7',"--..."},{'8',"---.."},{'9',"----."},
                {'/',"/"},{'=',"...^-"},{'+',"^.^"},{'!',"^..^"},
            };
        }

        public static string Send(string input)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (char character in input)
            {
                if (_morseAlphabetDictionary.ContainsKey(character))
                {
                    stringBuilder.Append(_morseAlphabetDictionary[character] + " ");
                }
                else
                {
                    stringBuilder.Append(character + " ");
                }
            }
            return stringBuilder.ToString();
        }

        public static string Receive(string input)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string[] codes = input.Split(' ');
            foreach (var code in codes)
            {
                foreach (char keyVar in _morseAlphabetDictionary.Keys)
                {
                    if (_morseAlphabetDictionary[keyVar] == code)
                    {
                        stringBuilder.Append(keyVar);
                    }
                }
            }
            return stringBuilder.ToString();
        }
    }
}
