using FromTargetingSystem2._0;
using FromTargetingSystem2._0.EntityComponents;
using Plugins.GeometricVision;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class SimplePlayerController : MonoBehaviour
{
    private GeometryVision geoVision = null;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject explosion = null;
    [SerializeField] private GameObject remains = null;
    [SerializeField] private float speed = 0;
    [SerializeField] private float rotationSpeed = 0;

    private Entity entityExplosionToSpawn= Entity.Null;
    private Entity entityRemainsToSpawn = Entity.Null;
    private MissileSystem missileSystem = null;
    private BlobAssetStore bStore = null;
        
    [SerializeField] private Transform gun = null;
    public GeometryVision GeoVision
    {
        get { return geoVision; }
        set { geoVision = value; }
    }

    private void Start()
    {
        if (bStore == null)
        {
            bStore = new BlobAssetStore();
        }
        missileSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<MissileSystem>();
        
        GameObjectConversionSettings gS =
            GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, bStore);
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            
        //Explosion
        entityExplosionToSpawn = GameObjectConversionUtility.ConvertGameObjectHierarchy(explosion, gS);
        SetPrefabData(entityExplosionToSpawn, false);

        //Remains
        entityRemainsToSpawn = GameObjectConversionUtility.ConvertGameObjectHierarchy(remains, gS);
        SetPrefabData(entityRemainsToSpawn, true);
        
        
        //Sets prefab data for entity archetype.
        void SetPrefabData(Entity entity, bool setRenderMesh)
        {
            entityManager.SetComponentData(entity, new Translation {Value = new Vector3(0, 0, 0f)});
            entityManager.AddComponent<Explosion>(entity);
            entityManager.SetComponentData(entity, new Explosion()
            {
                position = new Vector3(0f, 0, 0f),
                force = 10,
            });

            if (setRenderMesh)
            {                
                entityManager.SetSharedComponentData(entity, new RenderMesh
                {
                    mesh = remains.GetComponent<MeshFilter>().sharedMesh,
                    material = remains.GetComponent<MeshRenderer>().sharedMaterial
                });
            }
        }
    }

    private void OnDestroy()
    {
        if (bStore != null)
        {
            bStore.Dispose(); 
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleHorizontalMovement();
        HandleMissileFiring();
        missileSystem.Update();
        
        //////
        //Local function
        /////


        //Simple function to make reading default keys and also to move the player
        void HandleHorizontalMovement()
        {
            Vector3 input = Vector3.zero;
            
            if (Input.GetKey(KeyCode.A))
            {
                input.x = -1;
            }

            if (Input.GetKey(KeyCode.W))
            {
                input.z = 1;
            }

            if (Input.GetKey(KeyCode.D))
            {
                input.x = 1;
            }

            if (Input.GetKey(KeyCode.S))
            {
                input.z = -1;
            }
            
            var position = this.transform.position;
            this.transform.position = Vector3.MoveTowards(position, position + input,
                Time.deltaTime * 5);
        }
        
        void HandleMissileFiring()
        {
            if (Input.GetMouseButtonDown(0))
            {
                missileSystem.GeoVision = geoVision;
                missileSystem.Explosion = entityExplosionToSpawn;
                missileSystem.EnemyRemains = entityRemainsToSpawn;
                missileSystem.spawnMissileAndUpdate(geoVision, gun.transform.position, this.gun.rotation, speed,
                    rotationSpeed);
            }
        }
    }
}