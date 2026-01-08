using System;
using UnityEngine;

namespace Platformer
{
    public class Teleporter : Interactable
    {
        public GameObject targetLocation;
        [SerializeField] private float timeToTeleport = 1;
        private float teleportTimer;
        private PlayerController player;

        private void Awake()
        {
            player = FindAnyObjectByType<PlayerController>();
        }

        public void StartTeleport()
        {
            player.isTeleporting = true;

            teleportTimer = timeToTeleport;
        }

        public override void Interact()
        {
            if (teleportTimer <= 0)
            {
                Debug.Log("Starting Teleport");
                StartTeleport();
            }
        }

        private void FixedUpdate()
        {
            if (teleportTimer > 0)
            {
                teleportTimer -= Time.fixedDeltaTime;

                if (teleportTimer <= 0)
                {
                    TeleportPlayer();
                }
            }
        }

        public void TeleportPlayer()
        {
            player.transform.position = targetLocation.transform.position;
            player.isTeleporting = false;
        }
    }
}
