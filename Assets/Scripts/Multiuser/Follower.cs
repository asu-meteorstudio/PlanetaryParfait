using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using XRControls;
using Unity.Collections;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
using XRController = XRControls.XRController;

namespace Multiuser
{
    /// <summary>
    /// This script is attached to the Player Prefab, which ONLY contains the player mesh and animator. 
    /// It updates the location and animation of the networked mesh to the player that is local to each user's device. 
    /// This is to ensure that no cameras or movement controls are mixed up over the network.
    /// </summary>
    public class Follower : NetworkBehaviour
    {
        #region FIELDS
        
        public string networkTag; // label that tells us what GameObject we are tracking (ex: head, left, right, XRRig, DesktopRig) 
        public Transform localTransform = null;

        // animation variables
        private Animator _animator;
        private int _animIDSpeed;
        private int _animIDMotionSpeed;
        private float _animationBlend;

        private Transform spawnPoint; // where the user spawns when they join an experience

        [SerializeField] private GameObject localPlayer; // refers to the avatar that the user controls while the game runs locally on their machine

        #endregion

        #region MONO
        
        private void Start()
        {
            if (networkTag.Contains("desktop_rig"))
            {
                _animator = GetComponent<Animator>();
                AssignAnimationIDs();
            }
            
            localPlayer = GameObject.FindGameObjectWithTag("Player");
                
            /*spawnPoint = GameObject.FindGameObjectWithTag("UserSpawnpoint").transform;
            transform.position = spawnPoint.position;*/
        }

        private void Update()
        {
            if(IsLocalPlayer) // only run on the local player's machine
            {
                if(localTransform == null)
                {
                    // Match the local avatar tags to the one that is spawned across the network -- this will ensure things are synced up across multiplayer
                    if (Leader.pairs.TryGetValue(networkTag, out localTransform))
                    {
                        Debug.Log("Success! Pair Tag = " + networkTag);
                    }
                    else
                    {
                        // There was an error pairing the tags. If this happens, users will run into issues in multiplayer
                        Debug.LogError("Could not pair " + networkTag + " onto game object = " + this.gameObject.name);
                    }
                }
                else
                {
                    transform.position = localTransform.position;
                    transform.rotation = localTransform.rotation;
                    transform.localScale = localTransform.localScale;
                    if(!GameState.IsVR) UpdateAnimationSpeed();
                }
            }

        }
        #endregion

        #region METHODS
        
        /// <summary>
        /// Assigns animation variables to IDs in the animator
        /// </summary>
        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        /// <summary>
        /// Updates animations for networked character mesh
        /// </summary>
        private void UpdateAnimationSpeed()
        {
            _animator = this.GetComponent<Animator>();

            _animationBlend = Mathf.Lerp(_animationBlend, localPlayer.GetComponent<FirstPersonController>().GetSpeed(), Time.deltaTime * 10.0f);
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, 1f);
            
        }
        #endregion
    }
}