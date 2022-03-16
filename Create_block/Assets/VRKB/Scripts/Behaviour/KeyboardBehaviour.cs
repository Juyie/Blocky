/// \file
/// <summary>
/// Manages keyboard-wide settings.  Parses JSON config file and propagates
/// settings to individual keys (KeyBehaviour objects).
/// </summary>

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VRKB
{
    public class KeyboardBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Message shown in the input area before the user starts typing.
        /// </summary>
        public string PlaceholderText = "Enter text...";
        /// <summary>
        /// Length of time key must be pressed before it starts repeating.
        /// </summary>
        public float RepeatDelayInMilliseconds = 100.0f;
        /// <summary>
        /// The rate at which a key repeats when it is held in a pressed state.
        /// </summary>
        public float RepeatRatePerSecond = 3.0f;
        /// <summary>
        /// If true, allow two or more keys to be pressed simultaneously with the mallet.
        /// Setting this to false helps to reduce typing errors.
        /// </summary>
        public bool AllowSimultaneousKeyPresses = false;
        /// <summary>
        /// Show all input text as wildcard characters ("*").
        /// This is helpful if a user is entering a password and wants
        /// to keep it hidden from possible observers.
        /// </summary>
        public bool PasswordMode = false;
        /// <summary>
        /// The material use for a key that is the unpressed state.
        /// Change the referenced material to change the colors of
        /// the keyboard keys when they are in an unpressed state,
        /// as well as other visual properties.
        /// </summary>
        public Material UnpressedKeyMaterial;
        /// <summary>
        /// The material use for a key that is the pressed state.
        /// Change the referenced material to change the colors of
        /// the keyboard keys when they are in an pressed state,
        /// as well as other visual properties.
        /// </summary>
        public Material PressedKeyMaterial;
        /// <summary>
        /// JSON configuration file which controls the output mappings,
        /// character labels, and icons for the individual keys.
        /// </summary>
        public TextAsset KeyboardConfigFile;
        /// <summary>
        /// A list of callback methods that are invoked each time a key is pressed.
        /// This event is useful for triggering actions such as playing a
        /// key clicking sound or generating haptic feedback on the VR controller(s).
        /// <param name="KeyBehaviour">the key that was pressed</param>
        /// <param name="Collider">the collider that pressed the key (usually a mallet)</param>
        /// <param name="bool">true if this key press event was generated by autorepeat</param>
        /// </summary>
        public OnKeyPressEvent OnKeyPress;
        /// <summary>
        /// A list of callback methods that are invoked when the user presses the
        /// "cancel" key. (On the default keyboard, the red "X" key is the cancel key.)
        /// This event is useful for resetting various program state after the
        /// user cancels his/her input (e.g. hiding the keyboard, closing a dialog box).
        /// <param name="string">
        /// text that user has typed prior to hitting the cancel key
        /// </param>
        /// </summary>
        public OnCancelEvent OnCancel;
        /// <summary>
        /// A list of callback methods that are invoked when the user presses the
        /// "confirm" key. (On the default keyboard, the green checkmark key is
        /// the confirm key.) This event is useful for grabbing the input text
        /// from the keyboard after the user has confirmed that it is correct.
        /// <param name="string">
        /// text that user has typed prior to hitting the confirm key
        /// </param>
        /// </summary>
        public OnConfirmEvent OnConfirm;

        protected TMP_InputField _inputField;
        protected TextMeshProUGUI _placeholder;
        protected KeyboardConfig _config;
        protected string _layer;
        protected string _prevLayer;
        protected Action _prevAction;
        protected HashSet<KeyBehaviour> _pressedKeys;
        protected EventSystem _eventSystem;

        protected char _cursor;
        /// <summary>
        /// The current contents of the keyboard input area.
        /// These are the characters that the user has typed since
        /// he/she last pressed the cancel or confirm keys.
        /// </summary>
        public string Text
        {
            get {
                // append cursor char if not present
                if (_inputField.text.Length == 0)
                    _inputField.text += _cursor;

                // return text with trailing cursor char removed
                return _inputField.text.Substring(0,
                    _inputField.text.Length - 1);
            }
            set {
                if (value == null) {
                    _inputField.text = null;
                } else {
                    // add trailing cursor char
                    _inputField.text = value + _cursor;
                }
            }
        }

        public void Start()
        {
            _pressedKeys = new HashSet<KeyBehaviour>();

            if (EventSystem.current == null) {
                _eventSystem = new GameObject("EventSystem",
                    typeof(EventSystem), typeof(StandaloneInputModule))
                    .GetComponent<EventSystem>();
            } else {
                _eventSystem = EventSystem.current;
            }

            _inputField = GetComponentInChildren<TMP_InputField>();
            _eventSystem.SetSelectedGameObject(_inputField.gameObject);
            _inputField.ActivateInputField();
            _inputField.Select();

            // Make the caret/cursor for the input text field invisible.
            // As of TextMeshPro version 1.3.0, updating the caret
            // position by setting `TargetInputField.caretPosition`
            // doesn't work, and so the caret remains stuck at the
            // beginning of the input text.
            //
            // To address this, I make my cursor by adding an extra
            // character (e.g. an underscore) to the end of the text.

            _inputField.caretWidth = 0;

            _placeholder = _inputField.placeholder as TextMeshProUGUI;

            _cursor = '_';

            // action taken in response to previous key press
            _prevAction = new Action(ActionType.None);

            LoadConfig();
            Update();
        }

        public void LoadConfig()
        {
            _config = new KeyboardConfig(KeyboardConfigFile.text);
            _layer = _config.DefaultLayerName();
            LoadLayer(_config, _layer);
        }

        public void LoadLayer(KeyboardConfig keyboardConfig, string layerName)
        {
            foreach(var key in GetComponentsInChildren<KeyBehaviour>()) {
#if UNITY_EDITOR
                Undo.RecordObject(key, "apply key config from JSON file");
#endif
                key.Config = keyboardConfig.GetKeyConfig(layerName, key.name);
#if UNITY_EDITOR
                PrefabUtility.RecordPrefabInstancePropertyModifications(key);
#endif
            }
        }

        public void AutogenerateKeyNames()
        {
            KeyBehaviour[] keys = GetComponentsInChildren<KeyBehaviour>();
            for (int i = 0; i < keys.Length; ++i) {
#if UNITY_EDITOR
                Undo.RecordObject(keys[i], "autogenerate key name");
#endif
                keys[i].name = "key" + i;
#if UNITY_EDITOR
                PrefabUtility.RecordPrefabInstancePropertyModifications(keys[i]);
#endif
            }
        }

        public void ClearText()
        {
            Text = null;
        }

        public void Backspace()
        {
            if (Text.Length == 0)
                return;

            Text = Text.Substring(0, Text.Length - 1);
        }

        public void TypeChars(string text)
        {
            if (text == null)
                return;

            // split on backspace chars, so we can handle them specially

            string[] fields = text.Split('\b');
            for (int i = 0; i < fields.Length; ++i) {
                Text += fields[i];
                if (i < fields.Length - 1)
                    Backspace();
            }
        }

        public bool IsPressed(KeyBehaviour key)
        {
            return _pressedKeys.Contains(key);
        }

        public int NumKeysPressed()
        {
            return _pressedKeys.Count;
        }

        public bool PressKey(KeyBehaviour key, Collider collider, bool autoRepeat=false)
        {
            if (!AllowSimultaneousKeyPresses && _pressedKeys.Count > 0 && !autoRepeat)
                return false;

            _pressedKeys.Add(key);

            // invoke user-specified callback(s) for key press events
            OnKeyPress.Invoke(key, collider, autoRepeat);

            Action action = key.Config.Action;
            switch(action.Type)
            {
                case ActionType.Output:
                    // send character(s) to output area
                    TypeChars(action.Arg);
                    // if current keyboard layer is only active for
                    // one key press (e.g. Shift key), revert
                    // to previous layer
                    if (_prevAction.Type == ActionType.EnableLayerForNextKey) {
                        _layer = (_prevLayer != null)
                            ? _prevLayer : _config.DefaultLayerName();
                        LoadLayer(_config, _layer);
                    }
                    break;
                case ActionType.EnableLayer:
                case ActionType.EnableLayerForNextKey:
                    _prevLayer = _layer;
                    _layer = action.Arg;
                    LoadLayer(_config, _layer);
                    break;
                case ActionType.Cancel:
                    // invoke user-specified callback(s) for
                    // cancelling keyboard input
                    OnCancel.Invoke(Text);
                    break;
                case ActionType.Confirm:
                    // invoke user-specified callback(s) for
                    // confirming keyboard input
                    OnConfirm.Invoke(Text);
                    break;
                case ActionType.None:
                default:
                    break;

            }
            _prevAction = action;

            return true;
        }

        public void ReleaseKey(KeyBehaviour key, Collider collider)
        {
            _pressedKeys.Remove(key);
        }

        public void Update()
        {
            if (_placeholder != null)
                _placeholder.text = PlaceholderText;
            if (_inputField != null && PasswordMode)
                _inputField.inputType = TMP_InputField.InputType.Password;
        }
    }

}
