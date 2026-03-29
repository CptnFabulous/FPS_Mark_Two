using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RadialMenuInputInteraction : IInputInteraction
{
    bool wasPressed = false;

    public float pressThreshold => InputSystem.settings.defaultButtonPressPoint;
    public float holdDuration => InputSystem.settings.defaultHoldTime;

    public void Process(ref InputInteractionContext context)
    {
        bool isPressed = context.ReadValue<float>() > pressThreshold;

        switch (context.phase)
        {
            case InputActionPhase.Waiting:

                //Debug.Log("Checking to start");
                // If button has been pressed, start check
                if (isPressed)
                {
                    //Debug.Log("Started");
                    //wasPressed = isPressed;
                    context.Started();
                    context.SetTimeout(holdDuration);
                }

                break;

            case InputActionPhase.Started:

                //Debug.Log($"Checking for success, {context.time}, {context.timerHasExpired}");

                // Input is waiting for success/failure (is the button held down long enough to trigger)
                if (!wasPressed)
                {
                    // If not pressed previously, check if timer has expired and button is still held.
                    if (context.timerHasExpired)
                    {
                        // If so, perform and stay started, and set wasPressed to true
                        //Debug.Log("Hold complete");
                        wasPressed = true;
                        context.PerformedAndStayStarted();
                    }
                    else if (!isPressed)
                    {
                        // If button was released before wasPressed was set to true, that means we've cancelled.
                        //Debug.Log("Cancelled");
                        wasPressed = false;
                        context.Canceled();
                    }
                }
                else
                {
                    // If wasPressed is true and isPressed is false, that means we pressed successfully but are now releasing.
                    if (!isPressed)
                    {
                        //Debug.Log("Released");
                        wasPressed = false;
                        context.Performed();
                    }
                }

                break;
        }

    }

    public void Reset() => wasPressed = false;
}