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
			Vector3 worldPosition = GetMouseWorldPosition() + offset;
			objectToDrag.position = worldPosition;
			TrySnapObject(objectToDrag);
		}

		public void EndDrag()
		{
			if (objectToDrag != null)
			{
				objectRenderer.material.color = originalColor; // Restore the original color
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

			Vector2 mouseDelta = Mouse.current.delta.ReadValue();
			ApplyRotation(-mouseDelta.x, mouseDelta.y);
			TrySnapObject(objectToDrag);
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
			if (partSnapPoint == null || partSnapPoint.isSnapped) return;

			objectToDrag = target;
			offset = objectToDrag.position - GetMouseWorldPosition();
			objectRenderer = objectToDrag.GetComponent<Renderer>();
			originalColor = objectRenderer.material.color;
			objectRenderer.material.color = Color.yellow; // Highlight the object
			isDragging = true;
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

		private void ApplyRotation(float rotationX, float rotationY)
		{
			rotationX *= rotationSpeed * Time.deltaTime;
			rotationY *= rotationSpeed * Time.deltaTime;
			objectToDrag.Rotate(mainCamera.transform.up, rotationX, Space.World);
			objectToDrag.Rotate(mainCamera.transform.right, rotationY, Space.World);
		}

		private void TrySnapObject(Transform objectToTrySnap)
		{
			SnapPoint snapPoint = objectToTrySnap.GetComponent<SnapPoint>();
			if (snapPoint != null)
			{
				SnappingManager.Instance.TrySnap(objectToTrySnap, snapPoint.snapIdentifier);
			}
		}
		#endregion
	}
}
