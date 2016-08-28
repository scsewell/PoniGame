using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using InputController;


// Actions needing a key binding.
public enum GameButton
{
    Menu, Walk, WalkToggle, Jump, Lock, Primary, Secondary, TK, SwitchCharacter, ConsiderSuicide,
}

// Actions needing an axis binding.
public enum GameAxis
{
    LookX, LookY, LockX, LockY, MoveX, MoveY, Zoom, TKDistance, TKRotateX, TKRotateY
}

[RequireComponent(typeof(ControlsEarlyUpdate))]

/*
 * Stores and maintains user constrols.
 */
public class Controls : MonoBehaviour
{
    private static Dictionary<GameButton, BufferedButton> m_buttons;
    public static Dictionary<GameButton, BufferedButton> Buttons
    {
        get { return m_buttons; }
    }

    private static Dictionary<GameAxis, BufferedAxis> m_axis;
    public static Dictionary<GameAxis, BufferedAxis> Axis
    {
        get { return m_axis; }
    }

    private static bool m_isMuted = false;
    public static bool IsMuted
    {
        get { return m_isMuted; }
        set { m_isMuted = value; }
    }

    void Awake()
    {
        loadDefaultControls();
    }

    /*
     * Needs to run at the end of every FixedUpdate frame to handle the input buffers.
     */
    private void FixedUpdate()
    {
        foreach (BufferedButton button in m_buttons.Values)
        {
            button.RecordFixedUpdateState();
        }
        foreach (BufferedAxis axis in m_axis.Values)
        {
            axis.RecordFixedUpdateState();
        }
    }

    /*
     * Needs to run at the start of every Update frame to buffer new inputs.
     */
    public void EarlyUpdate()
    {
        foreach (BufferedButton button in m_buttons.Values)
        {
            button.RecordUpdateState();
        }
        foreach (BufferedAxis axis in m_axis.Values)
        {
            axis.RecordUpdateState();
        }
    }

    /*
     * Clears the current controls and replaces them with the default set.
     */
    public static void loadDefaultControls()
    {
        m_buttons = new Dictionary<GameButton, BufferedButton>();
        
        m_buttons.Add(GameButton.Menu, new BufferedButton(false, new List<ButtonSource>
        {
            new KeyButton(KeyCode.Escape),
            new JoystickButton(GamepadButton.Start)
        }));
        m_buttons.Add(GameButton.Walk, new BufferedButton(true, new List<ButtonSource>
        {
            new KeyButton(KeyCode.LeftShift),
        }));
        m_buttons.Add(GameButton.WalkToggle, new BufferedButton(true, new List<ButtonSource>
        {
            new KeyButton(KeyCode.LeftControl),
        }));
        m_buttons.Add(GameButton.Jump, new BufferedButton(true, new List<ButtonSource>
        {
            new KeyButton(KeyCode.Space),
            new JoystickButton(GamepadButton.A)
        }));
        m_buttons.Add(GameButton.Lock, new BufferedButton(true, new List<ButtonSource>
        {
            new KeyButton(KeyCode.Q),
            new JoystickButton(GamepadButton.RStick)
        }));
        m_buttons.Add(GameButton.Primary, new BufferedButton(true, new List<ButtonSource>
        {
            new KeyButton(KeyCode.Mouse0),
            new JoystickButton(GamepadButton.RTrigger)
        }));
        m_buttons.Add(GameButton.Secondary, new BufferedButton(true, new List<ButtonSource>
        {
            new KeyButton(KeyCode.Mouse1),
            new JoystickButton(GamepadButton.LTrigger)
        }));
        m_buttons.Add(GameButton.TK, new BufferedButton(true, new List<ButtonSource>
        {
            new KeyButton(KeyCode.Z),
            new JoystickButton(GamepadButton.Y)
        }));
        m_buttons.Add(GameButton.SwitchCharacter, new BufferedButton(true, new List<ButtonSource>
        {
            new KeyButton(KeyCode.Tab),
            new JoystickButton(GamepadButton.Back)
        }));
        m_buttons.Add(GameButton.ConsiderSuicide, new BufferedButton(true, new List<ButtonSource>
        {
            new KeyButton(KeyCode.K),
        }));

        m_axis = new Dictionary<GameAxis, BufferedAxis>();
        
        m_axis.Add(GameAxis.LookX, new BufferedAxis(new List<AxisSource>
        {
            new MouseAxis(MouseAxis.Axis.MouseX),
            new JoystickAxis(GamepadAxis.RStickX, 2.0f, 0.2f)
        }));
        m_axis.Add(GameAxis.LookY,new BufferedAxis(new List<AxisSource>
        {
            new MouseAxis(MouseAxis.Axis.MouseY),
            new JoystickAxis(GamepadAxis.RStickY, 2.0f, 0.2f)
        }));
        m_axis.Add(GameAxis.LockX, new BufferedAxis(new List<AxisSource>
        {
            new MouseAxis(MouseAxis.Axis.MouseX, 2.0f),
            new KeyAxis(KeyCode.RightArrow, KeyCode.LeftArrow),
            new JoystickAxis(GamepadAxis.RStickX, 1.0f, 0.3f)
        }));
        m_axis.Add(GameAxis.LockY, new BufferedAxis(new List<AxisSource>
        {
            new MouseAxis(MouseAxis.Axis.MouseY, 2.0f),
            new KeyAxis(KeyCode.UpArrow, KeyCode.DownArrow),
            new JoystickAxis(GamepadAxis.RStickY, 1.0f, 0.3f)
        }));
        m_axis.Add(GameAxis.MoveX, new BufferedAxis(new List<AxisSource>
        {
            new KeyAxis(KeyCode.D, KeyCode.A),
            new JoystickAxis(GamepadAxis.LStickX, 1.0f, 1.0f)
        }));
        m_axis.Add(GameAxis.MoveY, new BufferedAxis(new List<AxisSource>
        {
            new KeyAxis(KeyCode.W, KeyCode.S),
            new JoystickAxis(GamepadAxis.LStickY, 1.0f, 1.0f)
        }));
        m_axis.Add(GameAxis.Zoom, new BufferedAxis(new List<AxisSource>
        {
            new KeyAxis(KeyCode.Equals, KeyCode.Minus),
            new KeyAxis(KeyCode.KeypadPlus, KeyCode.KeypadMinus),
        }));
        m_axis.Add(GameAxis.TKDistance, new BufferedAxis(new List<AxisSource>
        {
            new MouseAxis(MouseAxis.Axis.ScrollWheel),
            new JoystickButtonAxis(GamepadButton.RShoulder, GamepadButton.LShoulder, 0.1f)
        }));
        m_axis.Add(GameAxis.TKRotateX, new BufferedAxis(new List<AxisSource>
        {
            new MouseAxis(MouseAxis.Axis.MouseX),
            new JoystickAxis(GamepadAxis.RStickX, 2.0f, 1.0f)
        }));
        m_axis.Add(GameAxis.TKRotateY, new BufferedAxis(new List<AxisSource>
        {
            new MouseAxis(MouseAxis.Axis.MouseY),
            new JoystickAxis(GamepadAxis.RStickY, 2.0f, 1.0f)
        }));
    }

    /*
     * Returns true if any of the relevant keyboard or joystick buttons are held down.
     */
    public static bool IsDown(GameButton button)
    {
        BufferedButton bufferedButton = m_buttons[button];
        return !(m_isMuted && bufferedButton.CanBeMuted) && bufferedButton.IsDown();
    }

    /*
     * Returns true if a relevant keyboard or joystick key was pressed since the last appropriate update.
     */
    public static bool JustDown(GameButton button)
    {
        BufferedButton bufferedButton = m_buttons[button];
        bool isFixed = (Time.deltaTime == Time.fixedDeltaTime);
        return !(m_isMuted && bufferedButton.CanBeMuted) && (isFixed ? bufferedButton.JustDown() : bufferedButton.VisualJustDown());
    }

    /*
     * Returns true if a relevant keyboard or joystick key was released since the last appropriate update.
     */
    public static bool JustUp(GameButton button)
    {
        BufferedButton bufferedButton = m_buttons[button];
        bool isFixed = (Time.deltaTime == Time.fixedDeltaTime);
        return !(m_isMuted && bufferedButton.CanBeMuted) && (isFixed ? bufferedButton.JustUp() : bufferedButton.VisualJustUp());
    }

    /*
     * Returns the average value of an axis from all Update frames since the last FixedUpdate.
     */
    public static float AverageValue(GameAxis axis)
    {
        return m_isMuted ? 0 : m_axis[axis].AverageValue();
    }

    /*
     * Returns the cumulative value of an axis from all Update frames since the last FixedUpdate.
     */
    public static float CumulativeValue(GameAxis axis)
    {
        return m_isMuted ? 0 : m_axis[axis].CumulativeValue();
    }
}