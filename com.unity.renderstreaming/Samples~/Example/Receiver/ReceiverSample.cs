using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Unity.RenderStreaming.Samples
{
    class ReceiverSample : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private SignalingManager renderStreaming;
        [SerializeField] private VideoStreamReceiver receiveVideoViewer;
        [SerializeField] private SingleConnection connection;
#pragma warning restore 0649

        [SerializeField] InputActionReference connectAction;

        bool isConnected;
        private string connectionId;
        private InputSender inputSender;

        void Awake()
        {
            inputSender = GetComponent<InputSender>();
        }

        void Start()
        {
            if (renderStreaming.runOnAwake)
                return;

            renderStreaming.Run();
        }


        void OnEnable()
        {
            if (connectAction != null && connectAction.action != null)
            {
                connectAction.action.Enable();
                connectAction.action.performed += OnConnectPressed;
            }
        }

        void OnDisable()
        {
            if (connectAction != null && connectAction.action != null)
            {
                connectAction.action.performed -= OnConnectPressed;
            }
        }


        public void OnConnectPressed(InputAction.CallbackContext ctx)
        {
            ToggleConnection();
        }


        public void ToggleConnection()
        {
            if (!isConnected)
            {
                if (string.IsNullOrEmpty(connectionId))
                {
                    connectionId = System.Guid.NewGuid().ToString("N");
                }

                connection.CreateConnection(connectionId);
                isConnected = true;
            }
            else
            {
                connection.DeleteConnection(connectionId);
                isConnected = false;
            }
        }

        private void Update()
        {
            if (Keyboard.current.kKey.wasPressedThisFrame)
            {
                ToggleConnection();
            }

            if (isConnected)
            {
                //RunHeightMap();
            }
        }
    }
}
