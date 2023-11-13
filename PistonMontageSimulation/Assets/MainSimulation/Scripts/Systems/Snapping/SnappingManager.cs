using EPOOutline;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PistonProject.Managers
{
	public class SnappingManager : Singleton<SnappingManager>
	{

		public float snapDistance = 0.5f;
		public GameObject[] snapPoints; // All the snap points
		[SerializeField] private float errorDisplayTime = 0.5f; // How long to display the error color
		private Coroutine errorFeedbackCoroutine;


		private Transform selectedPart; // The part selected for potential disassembly
		private string partIdentifierOfSelectedPart;

		public void TrySnap(Transform part, string partIdentifier)
		{
			float closestDistance = float.MaxValue;
			Transform targetSnapPoint = null;
			Outlinable closestOutlinable = null;

			// Iterate over all snap points to find the closest one
			foreach (GameObject snapPointObj in snapPoints)
			{
				SnapPoint snapPoint = snapPointObj.GetComponent<SnapPoint>();
				Outlinable outlinable = snapPointObj.GetComponent<Outlinable>();

				if (snapPoint.snapIdentifier == partIdentifier)
				{
					float distance = Vector3.Distance(part.position, snapPoint.transform.position);
					if (distance < snapDistance && distance < closestDistance)
					{
						closestDistance = distance;
						closestOutlinable = outlinable;
						targetSnapPoint = snapPoint.transform;
					}
				}
			}

			// Only enable the silhouette if the part is eligible for assembly
			if (AssemblyManager.Instance.CanPartBeAssembled(partIdentifier))
			{
				if (closestOutlinable != null && targetSnapPoint != null)
				{
					closestOutlinable.enabled = true;
					StartCoroutine(SnapPartToPosition(part, targetSnapPoint, closestOutlinable));
					part.SetParent(targetSnapPoint); // Set the parent to the snap point
					AssemblyManager.Instance.SetPartAssembled(partIdentifier, true);
				}
			}
			else
			{
				if (closestOutlinable != null)
				{
					// Part is close enough but assembly is forbidden, so do not enable silhouette
					closestOutlinable.enabled = false;
				}
			}
		}
		public void TryUnSnap()
		{
			TryUnsnap(selectedPart, partIdentifierOfSelectedPart);
		}
		public void TryUnsnap(Transform part, string partIdentifier)
		{
			SnapPoint partSnapPoint = part.GetComponent<SnapPoint>();
			if (partSnapPoint != null && partSnapPoint.isSnapped)
			{
				// Check if the part can be disassembled
				if (!AssemblyManager.Instance.CanPartBeDisassembled(partIdentifier))
				{
					errorFeedbackCoroutine = StartCoroutine(ShowErrorFeedback(part));
					return; // Cannot unsnap this part due to a forbidden condition
				}
				StartCoroutine(UnsnapPartFromPosition(part, partSnapPoint));
				part.SetParent(null); // Remove the parent
				AssemblyManager.Instance.SetPartAssembled(partIdentifier, false);

			}
		}
		private IEnumerator SnapPartToPosition(Transform part, Transform snapPoint, Outlinable outlinable)
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
			if (outlinable != null) outlinable.enabled = false;

			MeshRenderer snapPointRenderer = snapPoint.GetComponent<MeshRenderer>();
			if (snapPointRenderer != null)
			{
				snapPointRenderer.enabled = false;
			}
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
				SnapPoint snapPoint = hit.transform.GetComponent<SnapPoint>();
				if (snapPoint != null)
				{
					partIdentifierOfSelectedPart = snapPoint.snapIdentifier;
					selectedPart = hit.transform;
				}
			}
		}
		public void UpdateSnapPointOutlines(Transform part, string partIdentifier)
		{
			float closestDistance = float.MaxValue;
			GameObject closestSnapPoint = null;

			// Iterate over all snap points to find the closest one and their Outlinable component
			foreach (GameObject snapPointObj in snapPoints)
			{
				SnapPoint snapPoint = snapPointObj.GetComponent<SnapPoint>();
				Outlinable outlinable = snapPointObj.GetComponent<Outlinable>();
				MeshRenderer meshRenderer = snapPointObj.GetComponent<MeshRenderer>(); // Ensure the snap points have MeshRenderers

				if (snapPoint.snapIdentifier == partIdentifier)
				{
					float distance = Vector3.Distance(part.position, snapPoint.transform.position);
					if (distance < snapDistance)
					{
						if (distance < closestDistance)
						{
							closestDistance = distance;
							if (closestSnapPoint != null)
							{
								// Disable the previous closest silhouette and renderer
								closestSnapPoint.GetComponent<Outlinable>().enabled = false;
								closestSnapPoint.GetComponent<MeshRenderer>().enabled = false;
							}
							closestSnapPoint = snapPointObj;
						}
					}
					else
					{
						// Disable silhouette
						if (outlinable != null && meshRenderer != null)
						{
							outlinable.enabled = false;
							meshRenderer.enabled = false;
						}
					}
				}
			}

			// Enable the silhouette
			if (closestSnapPoint != null)
			{
				closestSnapPoint.GetComponent<Outlinable>().enabled = true;
				closestSnapPoint.GetComponent<MeshRenderer>().enabled = true;
			}
		}
		private IEnumerator ShowErrorFeedback(Transform part)
		{
			Outlinable outlinable = part.GetComponent<Outlinable>();
			if (outlinable != null)
			{
				if (errorFeedbackCoroutine != null)
				{
					StopCoroutine(errorFeedbackCoroutine);
				}
				// Enable the outline effect
				outlinable.enabled = true;

				yield return new WaitForSeconds(errorDisplayTime);

				// Disable the outline effect 
				outlinable.enabled = false;
				errorFeedbackCoroutine = null;
			}
		}
	}
}
