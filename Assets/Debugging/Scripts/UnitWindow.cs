using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Debugging
{
    public class UnitWindow : BaseWindow
    {
        private enum Tab
        {
            State,
            Input,
            Data
        }

        private Unit unit;

        [SerializeField]
        private TabGroup tabGroup;
        private Tab selectedTab = Tab.State;

        private string currentStateText;
        private string lastStateText;
        private string[] previousStateText;
        private float currentStateActivationTime;

        protected override void OnAwake()
        {
            tabGroup.OnTabSelected += OnTabSelected;
        }

        protected override void OnShow()
        {
            unit = targetComponent as Unit;
            SetupState();
            SetupData();
            tabGroup.SetTabIndex((int)selectedTab);
        }

        protected override void OnHide()
        {
            if (unit == null) { return; }
            unit.StateMachine.OnStateChanged -= OnStateChanged;
        }

        private void OnTabSelected(int index)
        {
            selectedTab = (Tab)index;
            ClearText();
        }

        private void Update()
        {
            UpdateState();

            switch (selectedTab)
            {
                case Tab.State:
                    DrawState();
                    break;
                case Tab.Input:
                    DrawInput();
                    break;
                case Tab.Data:
                    DrawData();
                    break;
            }
        }

        private string AddSpacer(string input, int space)
        {
            while (input.Length < space)
            {
                input += " ";
            }
            input += "| ";
            return input;
        }

        private string DashDuration(float duration)
        {
            const int dashesPerSecond = 5;
            string dashes = "";
            for (int i = 0; i < duration * dashesPerSecond; ++i)
            {
                dashes += "-";
            }
            return dashes;
        }

        #region State

        private void SetupState()
        {
            unit.StateMachine.OnStateChanged += OnStateChanged;
            previousStateText = new string[4];
            currentStateActivationTime = Time.unscaledDeltaTime;
        }

        private void UpdateState()
        {
            lastStateText = currentStateText;

            // Get state name
            currentStateText = unit.StateMachine.CurrentState.ToString();

            // Add spaces until set index
            currentStateText = AddSpacer(currentStateText, 10);

            // Add dashes to represent time spent in state
            currentStateText += DashDuration(Time.unscaledTime - currentStateActivationTime);
        }

        private void DrawState()
        {
            SetText(0, currentStateText);
            for (int i = 0; i < previousStateText.Length; ++i)
            {
                if (previousStateText[i] == null) { return; }
                SetText(i + 1, previousStateText[i]);
            }
        }

        private void OnStateChanged(UnitState newState)
        {
            currentStateActivationTime = Time.unscaledTime;

            // Move all slots down
            for (int i = previousStateText.Length - 1; i > 0; --i)
            {
                previousStateText[i] = previousStateText[i - 1];
            }

            // Place current state text in first slot
            previousStateText[0] = lastStateText;
        }

        #endregion

        #region Input

        private void DrawInput()
        {
            SetText(0, AddSpacer("Movement", 10) + unit.Input.Movement);
            SetText(1, AddSpacer("Jumping", 10) + unit.Input.Jumping);
            SetText(2, AddSpacer("Crawling", 10) + unit.Input.Crawling);
        }

        #endregion

        #region Data

        private float facingRightChangedTime;
        private bool lastFacingRight = true;

        private void SetupData()
        {
            facingRightChangedTime = Time.unscaledTime;
        }

        private void DrawData()
        {
            // VELOCITY
            string velocity = AddSpacer("Velocity", 10);
            velocity += unit.Physics.velocity;
            SetText(0, velocity);

            // FACING
            string facing = AddSpacer("Facing", 10);
            facing += (unit.FacingRight ? "Right" : "Left");
            facing = AddSpacer(facing, 18);
            if (unit.FacingRight != lastFacingRight) { facingRightChangedTime = Time.unscaledTime; }
            facing += DashDuration(Time.unscaledTime - facingRightChangedTime);
            SetText(1, facing);
            lastFacingRight = unit.FacingRight;

        }

        #endregion
    }
}