using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Exists just to have an easy way of getting and changing the location of where cards are sent when discarded.
/// </summary>

public class DiscardLocation : MonoBehaviour {

	[SerializeField] float cardLayer = 2f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public Vector3 GetPosition()
	{
		return new Vector3(transform.position.x, transform.position.y, -cardLayer);
	}
}
