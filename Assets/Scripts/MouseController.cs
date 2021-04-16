using Plugins.GeometricVision;
using Plugins.GeometricVision.EntityScripts.FromUnity;
using Plugins.GeometricVision.Utilities;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 
/// </summary>
public class MouseController : MonoBehaviour
{
    private Vector3 mousePos = Vector3.zero;
    private Transform mouseTargetingSystem;
    private Ray ray = new Ray();
    private RaycastHit hitInfo = new RaycastHit();
    [SerializeField] private Transform targetingIndicator = null;
    Camera mainPlayerCamera = null;
    [SerializeField] float snapDistance = 1;
    [SerializeField] float gunAngleSensitivity = 0;
    [SerializeField] float gunAngleDistanceThreshold = 0;
    private GeometryVision geoVision;

    private Transform player = null;
    [SerializeField] private Vector3 playerLookAtHeight = Vector3.zero;
    [SerializeField] private bool stabilizeRotation = false;
    private Transform gunHolder = null;
    [SerializeField] private GameObject gunPrefab = null;
    private Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(  GameObject.Find(gunPrefab.name) );
        if (GameObject.Find(gunPrefab.name) == null)
        {
            gunPrefab = Instantiate(gunPrefab);
        }
        Debug.Log(  GameObject.Find(gunPrefab.name) );
        mouseTargetingSystem = this.transform;
        mouseTargetingSystem.gameObject.AddComponent<GeometryVision>();
        Debug.Log( "geovis"+ GetComponent<GeometryVision>());

        geoVision = GetComponent<GeometryVision>();
        //Default field of view is too narrow for this purpose. Lets change it.
        geoVision.FieldOfView = 145f;
        geoVision.GameObjectBasedProcessing.Value = false;
        geoVision.EntityProcessing.Value = true;
        mouseTargetingSystem.transform.rotation = Quaternion.LookRotation(Vector3.down);
        player = GameObject.Find("Player(Clone)").transform;
        gunHolder = GameObject.Find("Weapon").transform;
        mainPlayerCamera = Camera.main;
        player.GetComponent<SimplePlayerController>().GeoVision =GetComponent<GeometryVision>();
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        
        //This is needed since the targeting system needs its own start to initialize. Otherwise it give null ref on the build version
        if (geoVision.TargetingInstructions != null && geoVision.TargetingInstructions[0].EntityFilterComponent == null)
        {
            geoVision.TargetingInstructions[0].EntityFilterComponent = typeof(RotationSpeed_SpawnAndRemove); 
        }
        
        
        MoveMouseTargetingSystemWithTheMouse();
        HandleTargetingIndicatorPositioning();
        mouseTargetingSystem.LookAt(targetingIndicator.position);
        HandleGunHandAngle();

        void MoveMouseTargetingSystemWithTheMouse()
        {
            mousePos = Input.mousePosition;
            mousePos.z = mainPlayerCamera.nearClipPlane;
            mouseTargetingSystem.transform.position = mainPlayerCamera.ScreenToWorldPoint(mousePos);
        }

        void HandleTargetingIndicatorPositioning()
        {
            hitInfo = new RaycastHit();
            ray = this.mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hitInfo, 500))
            {
                targetingIndicator.position = hitInfo.point;
            }
        }

        void HandleGunHandAngle()
        {
            this.gunHolder.localRotation = Quaternion.Euler(
                Mathf.Clamp(
                    -Vector3.Distance(hitInfo.point, player.position) * this.gunAngleSensitivity +
                    gunAngleDistanceThreshold, -90, 0),
                0, 0);
        }
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            return;
        }

        player.LookAt(targetingIndicator.position + playerLookAtHeight);

        if (this.stabilizeRotation)
        {
            stabilizeRotation(player);
        }

        gunPrefab.transform.position = gunHolder.position;
        gunPrefab.transform.rotation = gunHolder.rotation;

        void stabilizeRotation(Transform transformRotationToStabilise)
        {
            var rot = transformRotationToStabilise.rotation;
            var newRot = rot.eulerAngles;
            newRot.x = 0;
            newRot.z = 0;
            transformRotationToStabilise.rotation = Quaternion.Euler(newRot);
        }
    }
}