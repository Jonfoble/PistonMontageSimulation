using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
	public InputAction leftClickAction;
	public InputAction rightClickAction;
	public UnityEvent onBeginRotate = new UnityEvent();
	public UnityEvent onEndRotate = new UnityEvent();
	public UnityEvent onBeginDrag = new UnityEvent();
	public UnityEvent onEndDrag = new UnityEvent();

	private void Awake()
	{
		leftClickAction.Enable();
		rightClickAction.Enable();
	}

	private void OnEnable()
	{
		leftClickAction.performed += _ => onBeginDrag.Invoke();
		leftClickAction.canceled += _ => onEndDrag.Invoke();
		
		rightClickAction.performed += _ => onBeginRotate.Invoke();
		rightClickAction.canceled += _ => onEndRotate.Invoke();
	}

	private void OnDisable()
	{
		leftClickAction.Disable();
		leftClickAction.performed -= _ => onBeginDrag.Invoke();
		leftClickAction.canceled -= _ => onEndDrag.Invoke();

		rightClickAction.Disable();
		rightClickAction.performed -= _ => onBeginRotate.Invoke();
		rightClickAction.canceled -= _ => onEndRotate.Invoke();
	}
}
