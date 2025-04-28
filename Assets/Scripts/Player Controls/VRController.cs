using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using XRControls;

[System.Serializable]
public class MapTransform
{
    #region FIELDS
    public Transform vrTarget;
    public Transform IKTarget;
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;
    #endregion

    #region SPECIFIC
    public void MapVRAvatar()
    {
        IKTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
        IKTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
    }
    #endregion

}

public class VRController : MonoBehaviour
{
    #region FIELDS
    [SerializeField] private GameObject XRrig;
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private float characterHeightDifference = 1.67f; // represents height of 3D character model -- this variable is positioned at the model's feet.
 
    [SerializeField] private MapTransform head;
    [SerializeField] private MapTransform leftHand;
    [SerializeField] private MapTransform rightHand;

    [SerializeField] private float turnSmoothness;

    [SerializeField] private Transform IKHead;

    [SerializeField] private Vector3 headBodyOffset;
 //   [SerializeField] private GameObject XRController_script;
   //[SerializeField] private SkinnedMeshRenderer meshRenderer;
    //[SerializeField] private Material M_Armature_Body; // assigned in Inspector, same name as Asset name
    //[SerializeField] private Material M_Armature_Arms;
    //[SerializeField] private Material M_Armature_Legs;

   // [SerializeField] private Material[] VRmats; // VR materials -- only the body and arms 
    #endregion

    #region MONO
    private void Start()
    {
        // Assign variables
        XRrig = GameObject.Find("XRRig");
        mainCamera = GameObject.Find("Main Camera");
        //head.vrTarget = mainCamera.transform; // sets head to mainCamera on start
        //leftHand.vrTarget = GameObject.Find("LeftHand Controller").transform;
        //rightHand.vrTarget = GameObject.Find("RightHand Controller").transform;

        // Go through children of XRRig, find MainCamera and controllers
        //  FindXRRigChildrenByTag("MainCamera", head);
        //  FindXRRigChildrenByName("LeftHand Controller", leftHand);
        //  FindXRRigChildrenByName("RightHand Controller", rightHand);

        // Get SkinnedMeshRender component, used to render player based on if they are in VR or not
       // meshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();

        /*
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            if (gameObject.transform.GetChild(i).name == "Geometry")
            {
                var geometry = gameObject.transform.GetChild(i);

                for (int j = 0; j < geometry.transform.childCount; j++)
                {
                    // Get armature
                    if (geometry.transform.GetChild(0).name == "Armature_Mesh")
                    {
                        // We found correct SkinnedMeshRenderer component
                        meshRenderer = geometry.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>();
                    }
                }
            }
        }
        */

        // Assign materials based on if player is in VR or not
        //VRmats = new Material[2];
        //// fill array
        //VRmats[0] = M_Armature_Body;

        //if (GameState.singleton.isVR)
        //{
        //    // assign new materials to player
        //    meshRenderer.materials = VRmats;
        //}
    }

    private void Update()
    {
        // Rotate PlayerMesh with XRrig
       transform.rotation = XRrig.transform.rotation;
    }

    private void LateUpdate()
    {
        // transform.position = IKHead.position + headBodyOffset;
        transform.position = new Vector3(IKHead.position.x + headBodyOffset.x, mainCamera.transform.position.y - characterHeightDifference, (IKHead.position.z + headBodyOffset.z)); 

        transform.forward = Vector3.Lerp(transform.forward, Vector3.ProjectOnPlane(IKHead.forward, Vector3.up).normalized, Time.deltaTime * turnSmoothness);
        head.MapVRAvatar();
        leftHand.MapVRAvatar();
        rightHand.MapVRAvatar();
    }
    #endregion
    /*
    #region SPECIFIC

    /// <summary>
    /// Finds XR Rig Children by tag 
    /// </summary>
    private void FindXRRigChildrenByTag(string tagName, MapTransform targetObj)
    {
        var camOffset = XRrig.transform.GetChild(0); // CameraOffset child on XRrig

        for (int i = 0; i < camOffset.childCount; i++)
        {
            if (camOffset.transform.GetChild(i).tag == tagName)
            {
                targetObj.vrTarget = camOffset.transform.GetChild(i);
            }
        }
    }

    /// <summary>
    /// Finds XR Rig Children by name 
    /// </summary>
    private void FindXRRigChildrenByName(string name, MapTransform targetObj)
    {
        print("name " + name);
        var camOffset = XRrig.transform.GetChild(0); // CameraOffset child on XRrig

        for (int i = 0; i < camOffset.childCount; i++)
        {
            if (camOffset.transform.GetChild(i).name == name)
            {
                targetObj.vrTarget = camOffset.transform.GetChild(i);
            }
        }
    }

    #endregion
    */
}