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
		private Renderer objectRenderer;
		private Color originalColor;
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
			}
		}

		public void MoveObject()
		{
			if (objectToDrag != null)
			{
				Vector3 worldPosition = GetMouseWorldPosition() + offset;
				// Move the root object if part of an assembly to move all connected parts
				Transform objectToMove = objectToDrag.root == objectToDrag ? objectToDrag : objectToDrag.root;
				objectToMove.position = worldPosition;
			}
		}

		public void EndDrag()
		{
			if (objectToDrag != null)
			{
				objectRenderer.material.color = originalColor; // Restore the original color
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
			if (TryGetTarget(out RaycastHit hit, "Draggable"))
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
				// Rotate the root object if part of an assembly to rotate all connected parts
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
			if (partSnapPoint != null && partSnapPoint.isSnapped)
			{
				// If part is snapped and is not the root, then it's part of an assembly
				if (target.root != target)
				{
					// Select the root to move the entire assembly
					objectToDrag = target.root;
				}
				else
				{
					// If it's the root, we're moving a single part or the whole assembly
					objectToDrag = target;
				}
			}
			else if (partSnapPoint == null)
			{
				// If it doesn't have a SnapPoint, it's a free part
				objectToDrag = target;
			}

			if (objectToDrag != null)
			{
				offset = objectToDrag.position - GetMouseWorldPosition();
				objectRenderer = objectToDrag.GetComponent<Renderer>();
				originalColor = objectRenderer.material.color;
				objectRenderer.material.color = Color.yellow; // Highlight the object
				isDragging = true;
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

		#endregion
	}
}
