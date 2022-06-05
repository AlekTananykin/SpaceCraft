using System;
using Main;
using Mechanics;
using Network;
using UI;
using UnityEngine;
using Mirror;

namespace Characters
{
    public class ShipController : NetworkMovableObject
    {
        [SerializeField] private Transform _cameraAttach;
        private CameraOrbit _cameraOrbit;

        
        private PlayerLabel playerLabel;

        private float _shipSpeed;
        private Rigidbody _rigidbody;

        [SyncVar]
        private string _playerName;

        private Vector3 currentPositionSmoothVelocity;

        protected override float speed => _shipSpeed;

        public string PlayerName
        {
            get => _playerName;
            set
            {
                _playerName = value;
            }
        }

        private void OnGUI()
        {
            if (_cameraOrbit == null)            
                return;

            _cameraOrbit.ShowPlayerLabels(playerLabel);
        }

        public override void OnStartAuthority()
        {
            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody == null)            
                return;
            
            gameObject.name = _playerName;
            _cameraOrbit = FindObjectOfType<CameraOrbit>();
            _cameraOrbit.Initiate(_cameraAttach == null ? transform : _cameraAttach);
            playerLabel = GetComponentInChildren<PlayerLabel>();
            base.OnStartAuthority();
        }

        protected override void HasAuthorityMovement()
        {
            var spaceShipSettings = SettingsContainer.Instance?.SpaceShipSettings;
            if (spaceShipSettings == null)            
                return;            

            var isFaster = Input.GetKey(KeyCode.LeftShift);
            var speed = spaceShipSettings.ShipSpeed;
            var faster = isFaster ? spaceShipSettings.Faster : 1.0f;

            _shipSpeed = Mathf.Lerp(_shipSpeed, speed * faster, spaceShipSettings.Acceleration);

            var currentFov = isFaster ? spaceShipSettings.FasterFov : spaceShipSettings.NormalFov;
            _cameraOrbit.SetFov(currentFov, spaceShipSettings.ChangeFovSpeed);

            var velocity = _cameraOrbit.transform.TransformDirection(Vector3.forward) * _shipSpeed;
            _rigidbody.velocity = velocity * (_updatePhase == UpdatePhase.FixedUpdate ? Time.fixedDeltaTime : Time.deltaTime);

            if (!Input.GetKey(KeyCode.C))
            {
                var targetRotation = Quaternion.LookRotation(Quaternion.AngleAxis(_cameraOrbit.LookAngle, -transform.right) * velocity);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);
            }

            if (isServer)
            {
                SendToClients();
            }
            else
            {
                CmdSendTransform(transform.position, transform.rotation.eulerAngles);
            }
        }

        protected override void FromOwnerUpdate()
        {
            transform.position = Vector3.SmoothDamp(transform.position, serverPosition, 
                ref currentPositionSmoothVelocity, speed);
            transform.rotation = Quaternion.Euler(serverEulers);                       
        }

        protected override void SendToClients()
        {
            serverPosition = transform.position;
            serverEulers = transform.eulerAngles;
        }

        [Command]
        private void CmdSendTransform(Vector3 position, Vector3 eulers)
        {
            serverPosition = position;
            serverEulers = eulers;
        }

        [ClientCallback]
        private void LateUpdate()
        {
            _cameraOrbit?.CameraMovement();
        }

        [Client]
        private void SetPlayerName()
        {
           GameObject[] huds =  GameObject.FindGameObjectsWithTag("HUD");

            if (0 == huds.Length)
                return;

            GameObject hud = huds[0];
            if (null == hud)
                return;

            TMPro.TMP_InputField field = 
                hud.GetComponentInChildren<TMPro.TMP_InputField>();

            if (null == field)
                return;

            if (null != playerLabel)
                playerLabel.name = field.text;

            CmdSetPlayerName(field.text);
        }

        [Command]
        private void CmdSetPlayerName(string name)
        {
            _playerName = name;

            if (null != playerLabel)
                playerLabel.name = name;
        }

        protected override void Initiate(UpdatePhase updatePhase = UpdatePhase.Update)
        {
            base.Initiate(updatePhase);

            if (hasAuthority)
            {
                SetPlayerName();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            CmdCollide(gameObject, other.gameObject);
        }

        [Command]
        void CmdCollide(GameObject obj1, GameObject obj2)
        {
            TryDestroyShip(obj1);
            TryDestroyShip(obj2);
        }

        [Server]
        private void TryDestroyShip(GameObject target)
        {
            if (!target.TryGetComponent(out ShipController _))
                return;
            
            if (target.TryGetComponent(out NetworkConnection connection))
                connection.Disconnect();

            
        }
    }
}
