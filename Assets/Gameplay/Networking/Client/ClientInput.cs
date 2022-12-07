using DarkRift;
using DarkRift.Client.Unity;
using Network.Shared;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network.Client
{

    public class ClientInput
    {
        private Client m_Client;

        public ClientInput(Client client)
        {
            m_Client = client;

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
            FloatInputPacket movementInputPacket = new FloatInputPacket();
            movementInputPacket.fixedTime = Time.fixedTimeAsDouble;
            movementInputPacket.value = movement;

            m_Client.MessageSender.QueueMessage(ClientTag.MovementInput, movementInputPacket);
        }

        private void OnRunningChanged(bool running)
        {
            BoolInputPacket runningInputPacket = new BoolInputPacket();
            runningInputPacket.fixedTime = Time.fixedTimeAsDouble;
            runningInputPacket.value = running;

            m_Client.MessageSender.QueueMessage(ClientTag.RunningInput, runningInputPacket);
        }

        private void OnJumpingChanged(bool jumping)
        {
            BoolInputPacket jumpingInputPacket = new BoolInputPacket();
            jumpingInputPacket.fixedTime = Time.fixedTimeAsDouble;
            jumpingInputPacket.value = jumping;

            m_Client.MessageSender.QueueMessage(ClientTag.JumpingInput, jumpingInputPacket);
        }
    }

}