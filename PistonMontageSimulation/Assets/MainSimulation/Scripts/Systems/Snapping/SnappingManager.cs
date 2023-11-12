using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PistonProject.Managers
{
	public class SnappingManager : Singleton<SnappingManager>
	{
		public float snapDistance = 0.5f;
		public GameObject[] snapPoints; // All the snap points

		private Transform selectedPart; // The part selected for potential disassembly

		public void TrySnap(Transform part, string partIdentifier)
		{
			float closestDistance = float.MaxValue;
			Transform targetSnapPoint = null;

			// Iterate over all snap points to find the closest one
			foreach (GameObject snapPointObj in snapPoints)
			{
				SnapPoint snapPoint = snapPointObj.GetComponent<SnapPoint>();
				if (snapPoint.snapIdentifier == partIdentifier)
				{
					float distance = Vector3.Distance(part.position, snapPoint.transform.position);
					if (distance < snapDistance && distance < closestDistance)
					{
						closestDistance = distance;
						targetSnapPoint = snapPoint.transform;
					}
				}
			}

			// If a valid snap point is found snap the part
			if (targetSnapPoint != null)
			{
				StartCoroutine(SnapPartToPosition(part, targetSnapPoint));
				part.SetParent(targetSnapPoint); // Set the parent to the snap point
				AssemblyManager.Instance.SetPartAssembled(partIdentifier, true);
			}
		}

		public void TryUnsnap(Transform part, string partIdentifier)
		{
			SnapPoint partSnapPoint = part.GetComponent<SnapPoint>();
			if (partSnapPoint != null && partSnapPoint.isSnapped)
			{
				StartCoroutine(UnsnapPartFromPosition(part, partSnapPoint));
				part.SetParent(null); // Remove the parent
				AssemblyManager.Instance.SetPartAssembled(partIdentifier, false);
			}
		}

		private IEnumerator SnapPartToPosition(Transform part, Transform snapPoint)
		{
			SnapPoint partSnapPoint = part.GetComponent<SnapPoint>();

			float time = 0f;
			Vector3 startPosition = part.position;
			Quaternion startRotation = part.rotation;
			float duration = 0.5f; 

			while (time < duration)
			{
				part.position = Vector3.Lerp(startPosition, snapPoint.position, time / duration);
				part.rotation = Quaternion.Lerp(startRotation, snapPoint.rotation, time / duration);
				time += Time.deltaTime;
				yield return null;
			}

			part.position = snapPoint.position;
			part.rotation = snapPoint.rotation;
			partSnapPoint.isSnapped = true;
		}

		private IEnumerator UnsnapPartFromPosition(Transform part, SnapPoint partSnapPoint)
		{
			float time = 0f;
			Vector3 endPosition = part.position + new Vector3(0.1f, 0, 0); 
			Quaternion endRotation = part.rotation;
			float duration = 0.5f; 

			while (time < duration)
			{
				part.position = Vector3.Lerp(part.position, endPosition, time / duration);
				part.rotation = Quaternion.Lerp(part.rotation, endRotation, time / duration);
				time += Time.deltaTime;
				yield return null;
			}

			
			partSnapPoint.isSnapped = false;
		}

		public void SelectPart()
		{
			
			Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit))
			{
				// Check if the hit object has the SnapPoint component
				SnapPoint snapPoint = hit.transform.GetComponent<SnapPoint>();
				if (snapPoint != null)
				{
					selectedPart = hit.transform;
				}
			}
		}
	}
}
