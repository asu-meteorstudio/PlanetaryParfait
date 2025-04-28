using UnityEngine;
using Unity.Netcode;

public class MeshSetup : NetworkBehaviour
{
    #region FIELDS
    
    // Player meshes
    [SerializeField] private GameObject xrMesh;
    [SerializeField] private GameObject desktopMesh;

    /// <summary>
    /// Assigns number to vr or desktop users.
    /// 0 is for unassigned
    /// 1 is for VR
    /// 2 is for Desktop
    /// </summary>
     [SerializeField] private NetworkVariable<int> isVRMode = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    #endregion
    
    #region MONO
    
    public void Start()
    {
        ToggleOtherMeshes(); //called for current joining client to set mesh for all clients already in scene
    }
    #endregion
    
    #region METHODS
    /// <summary>
    /// When a new player spawns, it checks to see whether or not it is in VR or desktop & turns off the appropriate gameobjects. Sets isVRMode variable accordingly.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        //delegate that checks when isVRMode is set
        isVRMode.OnValueChanged += OnVRModeChanged;

        if(IsLocalPlayer)
        {
            if (GameState.IsVR)
            {
                // User is in VR platform
                desktopMesh.SetActive(false);
                isVRMode.Value = 1;
                foreach (MeshRenderer mr in gameObject.GetComponentsInChildren<MeshRenderer>())
                {
                    mr.enabled = false;
                }
            }
            else
            {
                // User is in Desktop platform
                xrMesh.SetActive(false);
                isVRMode.Value = 2;
                gameObject.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
            }
        }
    }

    /// <summary>
    /// Delegate that executes on all players once isVRMode value is updated for current joining client
    /// </summary>
    /// <param name="previousValue">Previous isVRMode value</param>
    /// <param name="newValue">New isVRMode value</param>
    private void OnVRModeChanged(int previousValue, int newValue)
    {
        ToggleOtherMeshes();
    }

    /// <summary>
    /// Updates player's mesh on all clients
    /// </summary>
    public void ToggleOtherMeshes()
    {
        // if NOT local player, turn off corresponding 
        if (!IsLocalPlayer)
        {
            // other connected clients
            if (isVRMode.Value == 1)
            {
                // turn off desktop mesh
                desktopMesh.SetActive(false);
            }
            else if(isVRMode.Value == 2)
            {
                // turn off vr mesh
                xrMesh.SetActive(false);
            }
            else
            {
                // late joining clients will see this message at least once, as late start is called before AND after VR mode value is set
                Debug.LogWarning("Could not assign rig.");
            }
        }

    }
    #endregion
}
