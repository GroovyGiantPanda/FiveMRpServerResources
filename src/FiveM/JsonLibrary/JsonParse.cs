using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfJsonLibrary
{
//    class TargetModel
//    {
//        private int myFirstInt;
//        private bool myFirstBool;
//    }

//    class JsonStuffer
//    {

//    }

//    class JsonParser<T>
//    {
//        private int currentPosition = 0;
//        private string jsonInput;
//        private int currentCharacter = -1;
//        private int captureStart = -1;
//        private object rootObject = null;
//        private object currentObject = null;
//        private bool isFirstObject = true;

//        public T Parse(string json)
//        {
//            jsonInput = json;
//            read();

//        }

//        private void read()
//        {
//            currentPosition++;
//            if (currentPosition == jsonInput.Length)
//            {
//                currentCharacter = -1;
//                return;
//            }
//            currentCharacter = jsonInput[currentPosition];
//        }

//        public void readValue()
//        {
//            switch (currentCharacter)
//            {
//                case 'n':
//                    readNull();
//                    break;
//                case 't':
//                    readTrue();
//                    break;
//                case 'f':
//                    readFalse();
//                    break;
//                case '"':
//                    readString();
//                    break;
//                case '[':
//                    readArray();
//                    break;
//                case '{':
//                    readObject();
//                    break;
//                case var c when "-0123456789".Contains(c.ToString()):
//                    readNumber();
//                    break;
//                default:
//                    throw new InvalidDataException($"Unexpected character '{currentCharacter.ToString()}'");
//            }
//        }

//        private bool readNumber()
//        {
//            if (isFirstObject && !typeof(T).IsNumericType()) throw new InvalidDataException();
//            isFirstObject = false;
//            if (!isDigit())
//            {
//                return false;
//            }
//            read();
//            return true;
//        }

//        private void readObject()
//        {
//            if (isFirstObject && typeof(T).IsGenericType &&
//                typeof(T).GetGenericTypeDefinition() == typeof(Dictionary<,>)) throw new InvalidDataException();
//            isFirstObject = false;
//        }

//        private void readArray()
//        {
//            if (isFirstObject && typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>))
//                throw new InvalidDataException();
//            isFirstObject = false;
//        }

//        private void readString()
//        {
//            if (isFirstObject && typeof(T) != typeof(string)) throw new InvalidDataException();
//            isFirstObject = false;
//        }

//        private bool readFalse()
//        {
//            if (isFirstObject && typeof(T) != typeof(bool)) throw new InvalidDataException();
//            isFirstObject = false;
//            readRequiredChar('a');
//            readRequiredChar('l');
//            readRequiredChar('s');
//            readRequiredChar('e');
//            return true;
//        }

//        private bool readTrue()
//        {
//            if (isFirstObject && typeof(T) != typeof(bool)) throw new InvalidDataException();
//            isFirstObject = false;
//            readRequiredChar('r');
//            readRequiredChar('u');
//            readRequiredChar('e');
//            return true;
//        }

//        private bool readNull()
//        {
//            if (isFirstObject && Nullable.GetUnderlyingType(typeof(T)) != null) throw new InvalidDataException();
//            isFirstObject = false;
//            readRequiredChar('u');
//            readRequiredChar('l');
//            readRequiredChar('l');
//            return true;
//        }

//        private bool readChar(char c)
//        {
//            if (currentCharacter != c) return false;
//            read();
//            return true;
//        }
//        private bool readRequiredChar(char c)
//        {
//            if(!readChar(c)) throw new InvalidDataException();
//        }

//        private bool readExponent()
//        {
//            if (!readRequiredChar('e') && !readRequiredChar('E'))
//            {
//                return false;
//            }
//            if (!readRequiredChar('+'))
//            {
//                readRequiredChar('-');
//            }
//            if (!readDigit())
//            {
//                throw new InvalidDataException();
//            }
//            while (readDigit())
//            {
//            }
//            return true;
//        }

//        private bool readDigit()
//        {
//            if (!isDigit())
//            {
//                return false;
//            }
//            read();
//            return true;
//        }

//        public void consumeWhitespace()
//        {

//        }

//        private bool isWhitespace() => " \t\n\r".Contains($"{currentCharacter}");
//        private bool isDigit() => "0123456789".Contains($"{currentCharacter}");
//        private bool isHexDigit() => "0123456789abcdefABCDEF".Contains($"{currentCharacter}");
//        private bool isEndOfText() => currentCharacter == -1;
//        private int consumeOneLetter()
//        {

//        }
//        private int peekOneLetter()
//        {

//        }
//    }
}
