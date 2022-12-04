using DarkRift;
using DarkRift.Client.Unity;
using Network.Shared;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network.Client
{

    public class SMClientInput : MonoBehaviour
    {
        private UnityClient m_Client;
        private UnitInput m_Input;

        private void Start()
        {
            m_Client = GetComponent<UnityClient>();
            m_Input = PlayerManager.Instance.Unit.Input;

            m_Input.OnMovementChanged += OnMovementChanged;
        }

        private void OnMovementChanged(float movement)
        {
            FloatInputPacket movementInputPacket = new FloatInputPacket();
            movementInputPacket.fixedTime = Time.fixedTime;
            movementInputPacket.value = movement;

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(movementInputPacket);
                using (Message message = Message.Create((ushort)Tag.MovementInput, writer))
                {
                    m_Client.SendMessage(message, SendMode.Reliable);
                }
            }
        }
    }

}