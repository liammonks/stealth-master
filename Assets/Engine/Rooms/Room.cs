// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class Room : MonoBehaviour
// {
//     [SerializeField] private GameObject front;
//     [SerializeField] private GameObject exterior;
//     [SerializeField] private Entrance[] entrances;

//     private List<Collider2D> colliders = new List<Collider2D>();
//     public List<MeshRenderer> exteriorRenderers = new List<MeshRenderer>();
//     private List<Unit_OLD> units = new List<Unit_OLD>();

//     private void Awake() {
//         // Initialise entrances,
//         // When a unit uses an entrance attatched to this room, execute RoomEntered
//         foreach(Entrance entrance in entrances)
//         {
//             entrance.Initialise(this);
//         }
//         // Get all colliders and disable them by default
//         GetColliders(transform);
//         EnableColliders(false);
//         // Get exterior renderers so that their shadows can be disabled when entering
//         GetRenderers(exterior.transform);
//     }

//     private void GetColliders(Transform target)
//     {
//         // Add colliders from target transform to colliders list
//         foreach(Collider2D collider in target.GetComponents<Collider2D>())
//         {
//             if (collider.isTrigger) { continue; }
//             colliders.Add(collider);
//         }
//         // Repeat for each child of target transform
//         foreach(Transform child in target)
//         {
//             GetColliders(child);
//         }
//     }

//     private void GetRenderers(Transform target)
//     {
//         // Add renderers from target transform to exteriorRenderers list
//         foreach (MeshRenderer renderer in target.GetComponents<MeshRenderer>())
//         {
//             // If the renderer does not cast shadows by default we can ignore it
//             if (renderer.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.Off) { continue; }
//             exteriorRenderers.Add(renderer);
//         }
//         // Repeat for each child of target transform
//         foreach (Transform child in target)
//         {
//             GetRenderers(child);
//         }
//     }
    
//     private void EnableColliders(bool enable)
//     {
//         foreach (Collider2D collider in colliders)
//         {
//             collider.enabled = enable;
//         }
//     }

//     private void EnableExteriorShadows(bool enable)
//     {
//         UnityEngine.Rendering.ShadowCastingMode shadowMode = enable ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
//         foreach (MeshRenderer renderer in exteriorRenderers)
//         {
//             renderer.shadowCastingMode = shadowMode;
//         }
//     }

//     public bool ContainsUnit(Unit_OLD unit)
//     {
//         return units.Contains(unit);
//     }

//     public void OnUnitEnteredRoom(Unit_OLD enteringUnit)
//     {
//         units.Add(enteringUnit);
//         // Disable the front of the room when player enters and enable colliders
//         if(enteringUnit is Player_OLD)
//         {
//             front.SetActive(false);
//             EnableColliders(true);
//             EnableExteriorShadows(false);
//         }
//     }

//     public void OnUnitExitedRoom(Unit_OLD exitingUnit)
//     {
//         units.Remove(exitingUnit);
//         // Enable the front of the room when player exits and distable colliders
//         if (exitingUnit is Player_OLD)
//         {
//             front.SetActive(true);
//             EnableColliders(false);
//             EnableExteriorShadows(true);
//         }
//     }
// }
