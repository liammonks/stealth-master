using DarkRift;
using DarkRift.Client.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network.Client
{

    public class ClientInput : SingletonBehaviour<ClientInput>
    {

        protected override void Awake()
        {
            PlayerManager.Instance.OnUnitSpawned += RegisterUnitInput;
            if (PlayerManager.Instance.Unit != null)
            {
                RegisterUnitInput(PlayerManager.Instance.Unit);
            }
        }

        private void RegisterUnitInput(Unit unit)
        {
            unit.Input.OnMovementChanged += OnMovementChanged;
            unit.Input.OnRunningChanged += OnRunningChanged;
            unit.Input.OnJumpingChanged += OnJumpingChanged;
        }

        private void OnMovementChanged(float movement)
        {

        }

        private void OnRunningChanged(bool running)
        {

        }

        private void OnJumpingChanged(bool jumping)
        {

        }
    }

}