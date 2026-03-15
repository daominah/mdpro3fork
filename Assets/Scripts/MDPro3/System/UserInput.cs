using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.UI;

#if (!UNITY_ANDROID && !UNITY_IOS && !UNITY_STANDALONE_LINUX) || UNITY_EDITOR
using UnityEngine.InputSystem.Switch;
using UnityEngine.InputSystem.UI;
#endif

namespace MDPro3
{
    public class UserInput : MonoBehaviour
    {
        public delegate void UserInputAction();

        public static UserInput instance;
        public static PlayerInput PlayerInput;
        public static string KeyboardSchemeName = "Keyboard&Mouse";
        public static string GamepadSchemeName = "Gamepad";
        public static string TouchSchemeName = "Touch";
        public static bool NextSelectionIsAxis;
        public static GameObject HoverObject;

        public static event UserInputAction OnDragStart;
        public static event UserInputAction OnDragEnd;
        private static bool m_Draging;
        public static bool Draging 
        {
            get => m_Draging;
            set
            {
                m_Draging = value;
                if (m_Draging)
                    OnDragStart.Invoke();
                else
                    OnDragEnd.Invoke();
            }
        }

        public static event UserInputAction OnMouseMovedAction;
        public static event UserInputAction OnMouseCursorHide;

        public delegate void ControlDeviceChange(string scheme);
        public static event ControlDeviceChange OnControlDeviceChange;

        public enum GamepadType
        {
            None,
            Xbox,
            PlayStation,
            Nintendo
        }
        public static GamepadType gamepadType = GamepadType.None;

        public static Vector2 MoveInput;
        public static Vector2 MousePos;
        public static Vector2 LeftScrollWheel;
        public static Vector2 RightScrollWheel;

        public static bool WasCancelPressed;
        public static bool WasSubmitPressed;
        public static bool WasLeftPressed;
        public static bool WasRightPressed;
        public static bool WasUpPressed;
        public static bool WasDownPressed;
        public static bool WasGamepadButtonWestPressed;
        public static bool WasGamepadButtonNorthPressed;
        public static bool WasLeftStickPressed;
        public static bool WasRightStickPressed;
        public static bool WasLeftShoulderPressed;
        public static bool WasRightShoulderPressed;
        public static bool WasLeftShoulderPressing;
        public static bool WasRightShoulderPressing;
        public static bool WasLeftTriggerPressed;
        public static bool WasRightTriggerPressed;
        public static bool WasGamepadSelectPressed;
        public static bool WasGamepadStartPressed;

        public static bool MouseLeftDown;
        public static bool MouseRightDown;
        public static bool MouseMiddleDown;
        public static bool MouseLeftPressing;
        public static bool MouseRightPressing;
        public static bool MouseMiddlePressing;
        public static bool MouseLeftUp;
        public static bool MouseRightUp;
        public static bool MouseMiddleUp;

        private InputAction moveAction;
        private InputAction cancelAction;
        private InputAction submitAction;
        private InputAction mouseAction;
        private InputAction leftClickAction;
        private InputAction rightClickAction;
        private InputAction middleClickAction;
        private InputAction leftScrollAction;
        private InputAction rightScrollAction;
        private InputAction gamepadButtonWestAction;
        private InputAction gamepadButtonNorthAction;
        private InputAction leftStickAction;
        private InputAction rightStickAction;
        private InputAction leftShoulderAction;
        private InputAction rightShoulderAction;
        private InputAction leftTriggerAction;
        private InputAction rightTriggerAction;
        private InputAction gamepadSelectAction;
        private InputAction gamepadStartAction;

        private Vector2 lastMousePos;
        private Vector2 cursorRestorePos;
        private bool hasCursorRestorePos;

        private Gamepad pad;
        private Coroutine stopRumbleAfterTimeCoroutine;
        public static string gamePadName;

        private float leftPressingTime;
        private float rightPressingTime;
        private float upPressingTime;
        private float downPressingTime;
        private const float moveRepeatDelay = 0.4f;
        private const float moveRepeatRate = 0.2f;
        private const float moveInputDeadzone = 0.35f;

        private void Awake()
        {
            instance = this;
            PlayerInput = GetComponent<PlayerInput>();
            PlayerInput.onControlsChanged += OnControlsChanged;

            moveAction = PlayerInput.actions["Navigate"];
            cancelAction = PlayerInput.actions["Cancel"];
            submitAction = PlayerInput.actions["Submit"];
            mouseAction = PlayerInput.actions["MousePos"];
            leftClickAction = PlayerInput.actions["Click"];
            rightClickAction = PlayerInput.actions["RightClick"];
            middleClickAction = PlayerInput.actions["MiddleClick"];
            leftScrollAction = PlayerInput.actions["LeftScrollWheel"];
            rightScrollAction = PlayerInput.actions["RightScrollWheel"];

            gamepadButtonWestAction = PlayerInput.actions["GamepadButtonWest"];
            gamepadButtonNorthAction = PlayerInput.actions["GamepadButtonNorth"];
            leftStickAction = PlayerInput.actions["LeftStickPress"];
            rightStickAction = PlayerInput.actions["RightStickPress"];
            leftShoulderAction = PlayerInput.actions["LeftShoulderPress"];
            rightShoulderAction = PlayerInput.actions["RightShoulderPress"];
            leftTriggerAction = PlayerInput.actions["LeftTriggerPress"];
            rightTriggerAction = PlayerInput.actions["RightTriggerPress"];
            gamepadSelectAction = PlayerInput.actions["GamepadSelect"];
            gamepadStartAction = PlayerInput.actions["GamepadStart"];

            OnMouseMovedAction += ShowCursor;
        }

        private void Update()
        {
            var hasTouchInput = TryGetTouchState(out var touchPosition, out var touchPressed, out var touchPressing, out var touchReleased);
            if (hasTouchInput)
            {
                EnsureTouchControlScheme();
                if (Cursor.lockState == CursorLockMode.Locked)
                    ShowCursorForTouch();
            }

            MoveInput = ApplyMoveDeadzone(moveAction.ReadValue<Vector2>());
            MousePos = hasTouchInput ? touchPosition : mouseAction.ReadValue<Vector2>();
            LeftScrollWheel = leftScrollAction.ReadValue<Vector2>();
            RightScrollWheel = rightScrollAction.ReadValue<Vector2>();

            if (MousePos != lastMousePos || touchPressed)
            {
                MouseMovedEvent();
            }

            if (MoveInput != Vector2.zero && !hasTouchInput && !InputFieldActivating())
            {
                if (Cursor.lockState == CursorLockMode.None)
                {
                    HideCursor();
                }
            }

            WasCancelPressed = cancelAction.WasPressedThisFrame();

            WasSubmitPressed = submitAction.WasPressedThisFrame();
            WasGamepadButtonWestPressed = gamepadButtonWestAction.WasPressedThisFrame();
            WasGamepadButtonNorthPressed = gamepadButtonNorthAction.WasPressedThisFrame();
            WasLeftStickPressed = leftStickAction.WasPressedThisFrame();
            WasRightStickPressed = rightStickAction.WasPressedThisFrame();
            WasLeftShoulderPressed = leftShoulderAction.WasPressedThisFrame();
            WasRightShoulderPressed = rightShoulderAction.WasPressedThisFrame();
            WasLeftShoulderPressing = leftShoulderAction.IsPressed();
            WasRightShoulderPressing = rightShoulderAction.IsPressed();
            WasLeftTriggerPressed = leftTriggerAction.WasPressedThisFrame();
            WasRightTriggerPressed = rightTriggerAction.WasPressedThisFrame();
            WasGamepadSelectPressed = gamepadSelectAction.WasPressedThisFrame();
            WasGamepadStartPressed = gamepadStartAction.WasPressedThisFrame();

            #region Mouse

            MouseLeftDown = leftClickAction.WasPressedThisFrame() || touchPressed;
            MouseRightDown = rightClickAction.WasPressedThisFrame();
            MouseMiddleDown = middleClickAction.WasPressedThisFrame();
            MouseLeftPressing = leftClickAction.IsPressed() || touchPressing;
            MouseMiddlePressing = middleClickAction.IsPressed();
            MouseRightPressing = rightClickAction.IsPressed();
            MouseLeftUp = leftClickAction.WasReleasedThisFrame() || touchReleased;
            MouseRightUp = rightClickAction.WasReleasedThisFrame();
            MouseMiddleUp = middleClickAction.WasReleasedThisFrame();

            lastMousePos = MousePos;

            #endregion

            #region Navigation

            if (MoveInput.x > 0f)
            {
                if(rightPressingTime == 0f)
                    WasRightPressed = true;
                else
                    WasRightPressed = false;

                rightPressingTime += Time.unscaledDeltaTime;
            }
            else
            {
                rightPressingTime = 0f;
                WasRightPressed = false;
            }
            if (rightPressingTime > 0f && !WasRightPressed)
            {
                if(rightPressingTime > moveRepeatDelay)
                {
                    var overDelay = rightPressingTime - moveRepeatDelay;
                    if(overDelay > moveRepeatRate)
                    {
                        WasRightPressed = true;
                        rightPressingTime -= moveRepeatRate;
                    }
                }
            }

            if (MoveInput.x < 0f)
            {
                if (leftPressingTime == 0f)
                    WasLeftPressed = true;
                else
                    WasLeftPressed = false;

                leftPressingTime += Time.unscaledDeltaTime;
            }
            else
            {
                leftPressingTime = 0f;
                WasLeftPressed = false;
            }
            if (leftPressingTime > 0f && !WasLeftPressed)
            {
                if (leftPressingTime > moveRepeatDelay)
                {
                    var overDelay = leftPressingTime - moveRepeatDelay;
                    if (overDelay > moveRepeatRate)
                    {
                        WasLeftPressed = true;
                        leftPressingTime -= moveRepeatRate;
                    }
                }
            }

            if (MoveInput.y > 0f)
            {
                if (upPressingTime == 0f)
                    WasUpPressed = true;
                else
                    WasUpPressed = false;

                upPressingTime += Time.unscaledDeltaTime;
            }
            else
            {
                upPressingTime = 0f;
                WasUpPressed = false;
            }
            if (upPressingTime > 0f && !WasUpPressed)
            {
                if (upPressingTime > moveRepeatDelay)
                {
                    var overDelay = upPressingTime - moveRepeatDelay;
                    if (overDelay > moveRepeatRate)
                    {
                        WasUpPressed = true;
                        upPressingTime -= moveRepeatRate;
                    }
                }
            }

            if (MoveInput.y < 0f)
            {
                if (downPressingTime == 0f)
                    WasDownPressed = true;
                else
                    WasDownPressed = false;

                downPressingTime += Time.unscaledDeltaTime;
            }
            else
            {
                downPressingTime = 0f;
                WasDownPressed = false;
            }
            if (downPressingTime > 0f && !WasDownPressed)
            {
                if (downPressingTime > moveRepeatDelay)
                {
                    var overDelay = downPressingTime - moveRepeatDelay;
                    if (overDelay > moveRepeatRate)
                    {
                        WasDownPressed = true;
                        downPressingTime -= moveRepeatRate;
                    }
                }
            }

            #endregion

            #region Hover Object

            HoverObject = null;
            if (Program.instance.camera_.cameraMain.gameObject.activeInHierarchy
                && !EventSystem.current.IsPointerOverGameObject())
            {
                Ray ray = Program.instance.camera_.cameraMain.ScreenPointToRay(MousePos);
                if (Physics.Raycast(ray, out var hit))
                    HoverObject = hit.collider.gameObject;
            }

            #endregion

        }

        private static Vector2 ApplyMoveDeadzone(Vector2 input)
        {
            input.x = ApplyMoveDeadzone(input.x);
            input.y = ApplyMoveDeadzone(input.y);
            return input;
        }

        private static float ApplyMoveDeadzone(float input)
        {
            if (Mathf.Abs(input) < moveInputDeadzone)
                return 0f;

            return Mathf.Sign(input);
        }

        private static bool TryGetTouchState(out Vector2 touchPosition, out bool touchPressed, out bool touchPressing, out bool touchReleased)
        {
            touchPosition = default;
            touchPressed = false;
            touchPressing = false;
            touchReleased = false;

            var touchscreen = Touchscreen.current;
            if (touchscreen == null)
                return false;

            var touch = touchscreen.primaryTouch;
            touchPressed = touch.press.wasPressedThisFrame;
            touchPressing = touch.press.isPressed;
            touchReleased = touch.press.wasReleasedThisFrame;
            if (!touchPressed && !touchPressing && !touchReleased)
                return false;

            touchPosition = touch.position.ReadValue();
            return true;
        }

        private static bool TouchInputActive()
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen == null)
                return false;

            var touch = touchscreen.primaryTouch;
            return touch.press.wasPressedThisFrame || touch.press.isPressed || touch.press.wasReleasedThisFrame;
        }

        private void EnsureTouchControlScheme()
        {
            if (PlayerInput == null || Touchscreen.current == null || PlayerInput.currentControlScheme == TouchSchemeName)
                return;

            try
            {
                PlayerInput.SwitchCurrentControlScheme(TouchSchemeName, Touchscreen.current);
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void MouseMovedEvent()
        {
            if (PlayerInput.currentControlScheme != GamepadSchemeName || TouchInputActive())
                OnMouseMovedAction?.Invoke();
        }

        private void OnControlsChanged(PlayerInput input)
        {
            StartCoroutine(OnControlsChangedAsync(input));
        }

        private IEnumerator OnControlsChangedAsync(PlayerInput input)
        {
            yield return null;
            gamepadType = GamepadType.None;
            if (PlayerInput.currentControlScheme == GamepadSchemeName)
            {
                gamepadType = GamepadType.Xbox;

                if (Gamepad.current is DualShockGamepad)
                    gamepadType = GamepadType.PlayStation;
#if (!UNITY_ANDROID && !UNITY_IOS && !UNITY_STANDALONE_LINUX) || UNITY_EDITOR
                else if (Gamepad.current is SwitchProControllerHID)
                    gamepadType = GamepadType.Nintendo;
#endif
            }
            OnControlDeviceChange?.Invoke(input.currentControlScheme);
        }

        public static bool NeedDefaultSelect()
        {
            if (TouchInputActive())
                return false;
            if (PlayerInput.currentControlScheme == GamepadSchemeName)
                return true;
            else if (Cursor.lockState == CursorLockMode.Locked)
                return true;

            return false;
        }

        public static void SetMoveRepeatRate(float rate)
        {
#if (!UNITY_ANDROID && !UNITY_IOS && !UNITY_STANDALONE_LINUX) || UNITY_EDITOR
            var module = instance.GetComponent<InputSystemUIInputModule>();
            module.moveRepeatRate = rate;
#endif
        }

        #region Rumble

        public void Rumble(float lowFrequency, float highFrequence, float duration)
        {
            if (!Config.GetBool("Rumble", true))
                return;
            if (PlayerInput.currentControlScheme != GamepadSchemeName)
                return;

            pad = Gamepad.current;
            if (pad == null)
                return;

            Gamepad.current.SetMotorSpeeds(lowFrequency, highFrequence);
            if(stopRumbleAfterTimeCoroutine != null)
                StopCoroutine(stopRumbleAfterTimeCoroutine);
            stopRumbleAfterTimeCoroutine = StartCoroutine(StopRumble(duration, pad));
        }

        private IEnumerator StopRumble(float duration, Gamepad pad)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            pad?.SetMotorSpeeds(0f, 0f);
            stopRumbleAfterTimeCoroutine = null;
        }

        public static void RumbleForUp()
        {
            instance.Rumble(0.1f, 1f, 0.1f);
        }

        public static void RumbleForDown()
        {
            instance.Rumble(1f, 0.1f, 0.1f);
        }

        #endregion

        #region CursorVisibility

        private bool ignoreNextCursorMove;

        private void ShowCursor()
        {
            if (ignoreNextCursorMove)
            {
                ignoreNextCursorMove = false;
                return;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (hasCursorRestorePos && Mouse.current != null)
            {
                Mouse.current.WarpCursorPosition(cursorRestorePos);
                MousePos = cursorRestorePos;
                lastMousePos = cursorRestorePos;
                hasCursorRestorePos = false;
                ignoreNextCursorMove = true;
            }
        }

        private void ShowCursorForTouch()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            hasCursorRestorePos = false;
            ignoreNextCursorMove = false;
        }

        private void HideCursor()
        {
            cursorRestorePos = MousePos;
            hasCursorRestorePos = true;
            ignoreNextCursorMove = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            OnMouseCursorHide?.Invoke();
        }

        #endregion

        #region State

        public static bool InputFieldActivating()
        {
            var current = EventSystem.current.currentSelectedGameObject;
            if (current == null) return false;
            if (!current.TryGetComponent<Selectable>(out var selectable))
                return false;
            if (selectable is TMP_InputField tmpInputField)
                return tmpInputField.isFocused;
            if (selectable is InputField legacyInputField)
                return legacyInputField.isFocused;
            return false;
        }

        #endregion
    }
}
