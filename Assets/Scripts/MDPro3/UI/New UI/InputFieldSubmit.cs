using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MDPro3.UI
{
    [Serializable]
    public class StringUnityEvent : UnityEvent<string>
    { }

    [RequireComponent(typeof(InputField))]
    public class InputFieldSubmit : MonoBehaviour
    {
        public StringUnityEvent onSubmit;

        private InputField inputField;

        void Awake()
        {
            inputField = GetComponent<InputField>();
            inputField.lineType = InputField.LineType.MultiLineNewline;
        }

        void OnEnable()
        {
            inputField.onValidateInput += CheckForEnter;
        }

        void OnDisable()
        {
            inputField.onValidateInput -= CheckForEnter;
        }

        private char CheckForEnter(string text, int charIndex, char addedChar)
        {
            if (addedChar == '\n' && onSubmit != null)
            {
                onSubmit.Invoke(text);
                return '\0';
            }
            else
                return addedChar;
        }
    }
}
