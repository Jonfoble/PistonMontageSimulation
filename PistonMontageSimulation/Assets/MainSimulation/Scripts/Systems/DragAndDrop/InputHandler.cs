using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
	public InputAction leftClickAction;
	public InputAction middleClickAction;
	public InputAction rightClickAction;
	public UnityEvent onBeginRotate = new UnityEvent();
	public UnityEvent onEndRotate = new UnityEvent();
	public UnityEvent onBeginDrag = new UnityEvent();
	public UnityEvent onEndDrag = new UnityEvent();
	public UnityEvent onBeginDisassemble = new UnityEvent();
	public UnityEvent onEndDisassemble = new UnityEvent();

	private void Awake()
	{
		leftClickAction.Enable();
		rightClickAction.Enable();
		middleClickAction.Enable();
	}

	private void OnEnable()
	{
		leftClickAction.performed += _ => onBeginDrag.Invoke();
		leftClickAction.canceled += _ => onEndDrag.Invoke();

		rightClickAction.performed += _ => onBeginDisassemble.Invoke();
		rightClickAction.canceled += _ => onEndDisassemble.Invoke();

		middleClickAction.performed += _ => onBeginRotate.Invoke();
		middleClickAction.canceled += _ => onEndRotate.Invoke();
	}

	private void OnDisable()
	{
		leftClickAction.Disable();
		leftClickAction.performed -= _ => onBeginDrag.Invoke();
		leftClickAction.canceled -= _ => onEndDrag.Invoke();

		rightClickAction.Disable();
		rightClickAction.performed -= _ => onBeginDisassemble.Invoke();
		rightClickAction.canceled -= _ => onEndDisassemble.Invoke();

		middleClickAction.Disable();
		middleClickAction.performed -= _ => onBeginRotate.Invoke();
		middleClickAction.canceled -= _ => onEndRotate.Invoke();
	}
}
