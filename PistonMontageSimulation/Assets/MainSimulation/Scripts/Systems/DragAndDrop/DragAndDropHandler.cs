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
				isDragging = true;
			}
		}

		public void MoveObject()
		{
			if (isRotating)
			{
				return;
			}
			if (objectToDrag != null)
			{
				Vector3 worldPosition = GetMouseWorldPosition() + offset;
				Transform objectToMove = objectToDrag.root == objectToDrag ? objectToDrag : objectToDrag.root;
				objectToMove.position = worldPosition;
			}
		}

		public void EndDrag()
		{
			if (objectToDrag != null)
			{
				RevertColorOfRoot(objectToDrag); // Restore the original color
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
		private void RevertColorOfRoot(Transform root)
		{
			Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
			foreach (Renderer rend in renderers)
			{
				// Revert to original color
				rend.material.color = originalColor; // Adjust this line to use the stored original colors
			}
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

				// Change color of all renderers in the root object
				ChangeColorOfRoot(objectToDrag, Color.blue);

				isDragging = true;
			}
		}

		private void ChangeColorOfRoot(Transform root, Color color)
		{
			// Retrieve all renderers from the root object and its children
			Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
			foreach (Renderer rend in renderers)
			{
				// Store the original color in a dictionary if you want to revert back later
				originalColor = rend.material.color; // You need to adjust this if you have multiple renderers
				rend.material.color = color; // Change color to highlight
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
