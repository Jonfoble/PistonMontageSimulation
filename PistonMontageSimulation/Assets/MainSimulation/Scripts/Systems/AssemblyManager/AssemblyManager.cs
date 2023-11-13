using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace PistonProject.Managers
{
	public class AssemblyManager : Singleton<AssemblyManager>
	{
		[SerializeField] private List<AssemblyStep> assemblySteps;
		public UnityEvent onAssemblyComplete;

		private void Awake()
		{
			InitializeAssemblySteps();
		}
		private void InitializeAssemblySteps()
		{
			assemblySteps = new List<AssemblyStep>();
			SetupAssemblySteps();
		}
		private void SetupAssemblySteps()
		{
			// Create all steps
			AssemblyStep piston = new AssemblyStep("piston", true);
			AssemblyStep rod = new AssemblyStep("rod", false);
			AssemblyStep wrist_pin = new AssemblyStep("wrist_pin", false);
			AssemblyStep pin_clip_1 = new AssemblyStep("pin_clip_1", false);
			AssemblyStep pin_clip_2 = new AssemblyStep("pin_clip_2", false);
			AssemblyStep rod_bearing_cap_side = new AssemblyStep("rod_bearing_cap_side", false);
			AssemblyStep rod_bearing_rod_side = new AssemblyStep("rod_bearing_rod_side", false);
			AssemblyStep rod_bolt_1 = new AssemblyStep("rod_bolt_1", false);
			AssemblyStep rod_bolt_2 = new AssemblyStep("rod_bolt_2", false);
			AssemblyStep rod_cap = new AssemblyStep("rod_cap", false);

			// Define mandatory and forbidden assemblies
			rod.AddForbiddenAssembly(wrist_pin);
			// Define forbidden disassemblies: rod cannot be disassembled if wrist_pin is assembled
			rod.AddForbiddenDisassembly(wrist_pin);

			// Define forbidden assemblies and disassemblies for wrist_pin
			wrist_pin.AddForbiddenAssembly(pin_clip_1);
			wrist_pin.AddForbiddenAssembly(pin_clip_2);
			// wrist_pin cannot be disassembled if pin_clip_1 or pin_clip_2 are assembled
			wrist_pin.AddForbiddenDisassembly(pin_clip_1);
			wrist_pin.AddForbiddenDisassembly(pin_clip_2);

			// Define mandatory assemblies for pin_clips
			pin_clip_1.AddMandatoryAssembly(wrist_pin);
			pin_clip_2.AddMandatoryAssembly(wrist_pin);

			// Define forbidden assemblies and disassemblies for rod_cap
			rod_cap.AddForbiddenAssembly(rod_bolt_1);
			rod_cap.AddForbiddenAssembly(rod_bolt_2);

			// rod_cap cannot be disassembled if rod_bolt_1 or rod_bolt_2 are assembled
			rod_cap.AddForbiddenDisassembly(rod_bolt_1);
			rod_cap.AddForbiddenDisassembly(rod_bolt_2);

			// Add the steps to the list
			assemblySteps.Add(piston);
			assemblySteps.Add(rod);
			assemblySteps.Add(wrist_pin);
			assemblySteps.Add(pin_clip_1);
			assemblySteps.Add(pin_clip_2);
			assemblySteps.Add(rod_bearing_rod_side);
			assemblySteps.Add(rod_bearing_cap_side);
			assemblySteps.Add(rod_bolt_1);
			assemblySteps.Add(rod_bolt_2);
			assemblySteps.Add(rod_cap);
		}

		public void SetPartAssembled(string partIdentifier, bool isAssembled)
		{
			var step = assemblySteps.Find(x => x.PartIdentifier == partIdentifier);
			if (step != null)
			{
				step.IsCompleted = isAssembled;

				// After setting the part as assembled, check if the assembly is complete
				CheckAssemblyComplete();
			}
		}
		public bool IsPartAssembled(string partIdentifier)
		{
			var step = assemblySteps.Find(x => x.PartIdentifier == partIdentifier);
			return step != null && step.IsCompleted;
		}
		public bool CanPartBeAssembled(string partIdentifier)
		{
			var step = assemblySteps.Find(x => x.PartIdentifier == partIdentifier);
			if (step != null)
			{
				// Check if all mandatory parts are assembled
				foreach (var mandatory in step.AssembliesMandatory)
				{
					if (!IsPartAssembled(mandatory.PartIdentifier))
					{
						return false; // A mandatory part is not assembled
					}
				}
				// Check if any forbidden parts are assembled
				foreach (var forbidden in step.AssembliesForbidden)
				{
					if (IsPartAssembled(forbidden.PartIdentifier))
					{
						return false; // A forbidden part is assembled
					}
				}
				return true; // All checks passed, can assemble the part
			}
			return false; // Part not found
		}
		public bool CanPartBeDisassembled(string partIdentifier)
		{
			// Find the assembly step for the part we want to disassemble.
			var stepToDisassemble = assemblySteps.Find(x => x.PartIdentifier == partIdentifier);
			if (stepToDisassemble != null && stepToDisassemble.IsCompleted)
			{
				// Check if disassembling this part is forbidden due to other parts being assembled.
				foreach (var forbiddenDisassembly in stepToDisassemble.DisassembliesForbidden)
				{
					if (IsPartAssembled(forbiddenDisassembly.PartIdentifier))
					{
						// If a part that forbids disassembly of this part is assembled, then disassembly is not allowed.
						return false;
					}
				}

				// If none of the disassembly conditions are violated, allow disassembly.
				return true;
			}

			// If the part is not found or not assembled, it can't be disassembled.
			return false;
		}
		private void CheckAssemblyComplete()
		{
			Debug.Log("CheckAssemblyComplete");
			// If all parts are assembled, invoke the onAssemblyComplete event
			if (assemblySteps.All(step => step.IsCompleted))
			{
				onAssemblyComplete?.Invoke();
			}
		}
	}
}
