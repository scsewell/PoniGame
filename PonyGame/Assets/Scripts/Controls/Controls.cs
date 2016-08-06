using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using InputController;


// Actions needing a key binding.
public enum GameButton
{
    MoveLeft, MoveRight, MoveForward, MoveBackward, Run, RunToggle, Jump,
}

// Actions needing an axis binding.
public enum GameAxis
{
    LookX, LookY, MoveX, MoveY, Zoom,
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

    void Awake()
    {
        loadDefaultControls();
    }

    /*
     * Needs to run at the end of every FixedUpdate frame to handle the input buffers.
     */
    void FixedUpdate()
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
        
        //m_buttons.Add(GameButton.Menu,          new BufferedButton(new List<ButtonSource> { new KeyButton(KeyCode.Escape),        new JoystickButton(GamepadButton.Start)     }));
        m_buttons.Add(GameButton.MoveLeft,      new BufferedButton(new List<ButtonSource> { new KeyButton(KeyCode.A)                }));
        m_buttons.Add(GameButton.MoveRight,     new BufferedButton(new List<ButtonSource> { new KeyButton(KeyCode.D)                }));
        m_buttons.Add(GameButton.MoveForward,   new BufferedButton(new List<ButtonSource> { new KeyButton(KeyCode.W)                }));
        m_buttons.Add(GameButton.MoveBackward,  new BufferedButton(new List<ButtonSource> { new KeyButton(KeyCode.S)                }));
        m_buttons.Add(GameButton.Jump,          new BufferedButton(new List<ButtonSource> { new KeyButton(KeyCode.Space),           new JoystickButton(GamepadButton.A)             }));
        m_buttons.Add(GameButton.Run,           new BufferedButton(new List<ButtonSource> { new KeyButton(KeyCode.LeftShift),       new JoystickButton(GamepadButton.LStick)        }));
        m_buttons.Add(GameButton.RunToggle,     new BufferedButton(new List<ButtonSource> { new KeyButton(KeyCode.LeftControl),     new JoystickButton(GamepadButton.B)             }));

        m_axis = new Dictionary<GameAxis, BufferedAxis>();
        
        m_axis.Add(GameAxis.LookX,              new BufferedAxis(new List<AxisSource> {     new MouseAxis(MouseAxis.Axis.MouseX),                   new JoystickAxis(GamepadAxis.RStickX, 2.0f, 0.3f)     }));
        m_axis.Add(GameAxis.LookY,              new BufferedAxis(new List<AxisSource> {     new MouseAxis(MouseAxis.Axis.MouseY),                   new JoystickAxis(GamepadAxis.RStickY, 2.0f, 0.3f)     }));
        m_axis.Add(GameAxis.MoveX,              new BufferedAxis(new List<AxisSource> {     new JoystickAxis(GamepadAxis.LStickX, 1.0f, 1.0f)       }));
        m_axis.Add(GameAxis.MoveY,              new BufferedAxis(new List<AxisSource> {     new JoystickAxis(GamepadAxis.LStickY, 1.0f, 1.0f)       }));
        m_axis.Add(GameAxis.Zoom,               new BufferedAxis(new List<AxisSource> {     new MouseAxis(MouseAxis.Axis.ScrollWheel),              }));
    }

    /*
     * Returns true if any of the relevant keyboard or joystick buttons are held down.
     */
    public static bool IsDown(GameButton button)
    {
        return m_buttons[button].IsDown();
    }

    /*
     * Returns true if a relevant keyboard or joystick key was pressed since the last FixedUpdate.
     */
    public static bool JustDown(GameButton button)
    {
        return m_buttons[button].JustDown();
    }

    /*
     * Returns true if a relevant keyboard or joystick key was released since the last FixedUpdate.
     */
    public static bool JustUp(GameButton button)
    {
        return m_buttons[button].JustUp();
    }

    /*
     * Returns true if a relevant keyboard or joystick key was pressed this frame.
     */
    public static bool VisualJustDown(GameButton button)
    {
        return m_buttons[button].VisualJustDown();
    }

    /*
     * Returns true if a relevant keyboard or joystick key was released this frame.
     */
    public static bool VisualJustUp(GameButton button)
    {
        return m_buttons[button].VisualJustUp();
    }

    /*
     * Returns the average value of an axis from all Update frames since the last FixedUpdate.
     */
    public static float AverageValue(GameAxis axis)
    {
        return m_axis[axis].AverageValue();
    }

    /*
     * Returns the cumulative value of an axis from all Update frames since the last FixedUpdate.
     */
    public static float CumulativeValue(GameAxis axis)
    {
        return m_axis[axis].CumulativeValue();
    }
}