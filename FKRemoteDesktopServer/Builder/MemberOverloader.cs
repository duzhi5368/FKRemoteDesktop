using FKRemoteDesktop.Utilities;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Text;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Builder
{
    public class MemberOverloader
    {
        private bool DoRandom { get; set; }
        private int StartingLength { get; set; }
        private readonly Dictionary<string, string> _renamedMembers = new Dictionary<string, string>();
        private readonly char[] _charMap;
        private readonly SafeRandom _random = new SafeRandom();
        private int[] _indices;

        public MemberOverloader(int startingLength, bool doRandom = true)
                : this(startingLength, doRandom, "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToLower().ToCharArray())
        {
        }

        private MemberOverloader(int startingLength, bool doRandom, char[] chars)
        {
            _charMap = chars;
            DoRandom = doRandom;
            StartingLength = startingLength;
            _indices = new int[startingLength];
        }

        public void GiveName(MemberReference member)
        {
            string currentName = GetCurrentName();
            string originalName = member.ToString();
            member.Name = currentName;
            while (_renamedMembers.ContainsValue(member.ToString()))
            {
                member.Name = GetCurrentName();
            }
            _renamedMembers.Add(originalName, member.ToString());
        }

        private string GetCurrentName()
        {
            return DoRandom ? GetRandomName() : GetOverloadedName();
        }

        private string GetRandomName()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < StartingLength; i++)
            {
                builder.Append((char)_random.Next(int.MinValue, int.MaxValue));
            }
            return builder.ToString();
        }

        private string GetOverloadedName()
        {
            IncrementIndices();
            char[] chars = new char[_indices.Length];
            for (int i = 0; i < _indices.Length; i++)
                chars[i] = _charMap[_indices[i]];
            return new string(chars);
        }

        private void IncrementIndices()
        {
            for (int i = _indices.Length - 1; i >= 0; i--)
            {
                _indices[i]++;
                if (_indices[i] >= _charMap.Length)
                {
                    if (i == 0)
                        Array.Resize(ref _indices, _indices.Length + 1);
                    _indices[i] = 0;
                }
                else
                    break;
            }
        }
    }
}
