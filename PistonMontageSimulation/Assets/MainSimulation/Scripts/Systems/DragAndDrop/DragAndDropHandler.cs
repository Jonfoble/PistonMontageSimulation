using EPOOutline;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PistonProject.Managers
{
	public class DragAndDropHandler : MonoBehaviour
	{
		[SerializeField] private float rotationSpeed = 100f; // Rotation Speed

		private Camera mainCamera;
		private Vector3 offset;
		private Transform objectToDrag;
		private bool isDragging;
		private bool isRotating;

		private void Awake()
		{
			mainCamera = Camera.main;
		}

		private void Update()
		{
			if (isDragging) MoveObject();
			else if (isRotating) RotateObject();
		}

		#region Drag And Drop Mechanics

		public void BeginDrag()
		{
			if (TryGetTarget(out RaycastHit hit, "Draggable"))
			{
				SetDraggableObject(hit.transform);
				isDragging = true;
			}
		}

		public void MoveObject()
		{
			if (isRotating || objectToDrag == null)
			{
				return;
			}

			Vector3 worldPosition = GetMouseWorldPosition() + offset;
			Transform objectToMove = objectToDrag.root == objectToDrag ? objectToDrag : objectToDrag.root;
			objectToMove.position = worldPosition;

			// Check proximity and enable/disable silhouettes
			UpdateSilhouetteDisplay(objectToDrag);
		}

		public void EndDrag()
		{
			if (objectToDrag != null)
			{
				SetOutlineEffect(objectToDrag, false); // Close Outliner
													   // Try to snap it into place when released
				SnapPoint snapPoint = objectToDrag.GetComponent<SnapPoint>();
				if (snapPoint != null)
				{
					SnappingManager.Instance.TrySnap(objectToDrag, snapPoint.snapIdentifier);
				}
				objectToDrag = null;
			}
			isDragging = false;
		}

		#endregion

		#region Rotation Mechanics

		public void BeginRotate()
		{
			if (TryGetTarget(out RaycastHit hit, "Draggable") && !isDragging)
			{
				objectToDrag = hit.transform;
				isRotating = true;
			}
		}

		public void RotateObject()
		{
			if (isDragging) return;

			if (objectToDrag != null)
			{
				Vector2 mouseDelta = Mouse.current.delta.ReadValue();
				// rotate all connected parts
				Transform objectToRotate = objectToDrag.root == objectToDrag ? objectToDrag : objectToDrag.root;
				ApplyRotation(objectToRotate, -mouseDelta.x, mouseDelta.y);
			}
		}

		public void EndRotate()
		{
			isRotating = false;
		}

		#endregion

		#region Private Helper Methods

		private void SetDraggableObject(Transform target)
		{
			SnapPoint partSnapPoint = target.GetComponent<SnapPoint>();
			if (partSnapPoint != null)
			{
				objectToDrag = target.root != target ? target.root : target;
			}
			else
			{
				// If it doesn't have a SnapPoint, it's a free part
				objectToDrag = target;
			}

			if (objectToDrag != null)
			{
				offset = objectToDrag.position - GetMouseWorldPosition();

				// Open Outliner
				SetOutlineEffect(objectToDrag, true);

				isDragging = true;
			}
		}
		private void SetOutlineEffect(Transform root, bool shouldOutline)
		{
			// Retrieve the Outlinable component from the root object
			Outlinable outlinable = root.GetComponent<Outlinable>();
			if (outlinable != null)
			{
				foreach (var childOutlinable in root.GetComponentsInChildren<Outlinable>())
				{
					childOutlinable.enabled = shouldOutline;
				}
			}
		}
		private bool TryGetTarget(out RaycastHit hit, string tag)
		{
			Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
			if (Physics.Raycast(ray, out hit) && hit.transform.CompareTag(tag))
			{
				return true;
			}
			hit = new RaycastHit();
			return false;
		}

		private Vector3 GetMouseWorldPosition()
		{
			Vector3 mousePosition = Mouse.current.position.ReadValue();
			mousePosition.z = mainCamera.WorldToScreenPoint(objectToDrag.position).z;
			return mainCamera.ScreenToWorldPoint(mousePosition);
		}

		private void ApplyRotation(Transform objectToRotate, float rotationX, float rotationY)
		{
			rotationX *= rotationSpeed * Time.deltaTime;
			rotationY *= rotationSpeed * Time.deltaTime;
			objectToRotate.Rotate(mainCamera.transform.up, rotationX, Space.World);
			objectToRotate.Rotate(mainCamera.transform.right, rotationY, Space.World);
		}
		private void UpdateSilhouetteDisplay(Transform part)
		{
			float closestDistance = float.MaxValue;
			Outlinable closestOutlinable = null;
			MeshRenderer closestMeshRenderer = null; // Add a reference to the MeshRenderer
			string partIdentifier = part.GetComponent<SnapPoint>()?.snapIdentifier;

			foreach (GameObject snapPointObj in SnappingManager.Instance.snapPoints)
			{
				SnapPoint snapPoint = snapPointObj.GetComponent<SnapPoint>();
				Outlinable outlinable = snapPointObj.GetComponent<Outlinable>();
				MeshRenderer meshRenderer = snapPointObj.GetComponentInChildren<MeshRenderer>();

				if (snapPoint.snapIdentifier == partIdentifier)
				{
					float distance = Vector3.Distance(part.position, snapPoint.transform.position);

					bool withinRange = distance < SnappingManager.Instance.snapDistance;
					bool canAssemble = AssemblyManager.Instance.CanPartBeAssembled(partIdentifier);

					if (withinRange && canAssemble)
					{
						if (closestOutlinable == null || distance < closestDistance)
						{
							closestDistance = distance;
							closestOutlinable = outlinable;
							closestMeshRenderer = meshRenderer;
						}
					}

					// Reset silhouettes for snap points that are not the closest
					if (outlinable != null && outlinable != closestOutlinable)
					{
						outlinable.enabled = false;
					}
					if (meshRenderer != null && meshRenderer != closestMeshRenderer)
					{
						meshRenderer.enabled = false;
					}
				}
			}

			// Enable the silhouette of the closest snap point
			if (closestOutlinable != null && closestMeshRenderer != null)
			{
				closestOutlinable.enabled = true;
				closestMeshRenderer.enabled = true;
			}
		}


		#endregion


	}
}
