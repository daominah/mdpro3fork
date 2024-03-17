using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace MDPro3.UI
{
    public class InputValidation : MonoBehaviour
    {
        public enum ValidationType
        {
            None,
            Path,
            NoSpace
        }

        InputField m_InputFied;

        public ValidationType type = ValidationType.Path;

        private static readonly List<char> InvalidPathChars = new List<char>()
    {
        '\\', '/', ':', '*', '?', '\"', '<', '>', '|'
    };

        private void Awake()
        {
            m_InputFied = GetComponent<InputField>();
            m_InputFied.onValueChanged.AddListener(OnInputFieldValueChange);
        }

        void OnInputFieldValueChange(string inputInfo)
        {
            if (type == ValidationType.Path)
            {
                foreach (var c in inputInfo)
                    if (InvalidPathChars.Contains(c))
                        m_InputFied.text = m_InputFied.text.Replace(c.ToString(), "");
            }
            else if (type == ValidationType.NoSpace)
            {
                if (inputInfo.Length > 0 && inputInfo.Contains(" "))
                    m_InputFied.text = m_InputFied.text.Replace(" ", "");
            }
        }
    }
}
