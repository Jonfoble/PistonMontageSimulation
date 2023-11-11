using System.Collections;
using UnityEngine;

namespace PistonProject.Managers
{
	public class SnappingManager : Singleton<SnappingManager>
	{
		public float snapDistance = 0.5f;
		public GameObject[] snapPoints; // Array that is holding all the snap points

		private Transform partToSnap; // The current part being dragged
		private Transform targetSnapPoint; // The snap point the current part will snap to

		public void TrySnap(Transform part, string partIdentifier)
		{
			float closestDistance = float.MaxValue;

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
						partToSnap = part;
					}
				}
			}
			if (partToSnap != null && targetSnapPoint != null)
			{
				StartCoroutine(SnapPartToPosition(partToSnap, targetSnapPoint));
			}
		}
		private IEnumerator SnapPartToPosition(Transform part, Transform snapPoint)
		{
			SnapPoint partSnapPoint = part.GetComponent<SnapPoint>();

			float time = 0f;
			Vector3 startPosition = part.position;
			Quaternion startRotation = part.rotation;
			float duration = 0.5f; // Time in seconds for the snap to occur

			while (time < duration && !partSnapPoint.isSnapped)
			{
				part.position = Vector3.Lerp(startPosition, snapPoint.position, time / duration);
				part.rotation = Quaternion.Lerp(startRotation, snapPoint.rotation, time / duration);
				time += Time.deltaTime;
				yield return null;
			}
			part.position = snapPoint.position;
			part.rotation = snapPoint.rotation; 
			if (partSnapPoint != null)
			{
				partSnapPoint.isSnapped = true;
			}
		}
	}
}