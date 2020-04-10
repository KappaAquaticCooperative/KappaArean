using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable {
	Unit OnInteractiveStart (Unit user);
	void OnInteractiveExit(Unit user);
}
