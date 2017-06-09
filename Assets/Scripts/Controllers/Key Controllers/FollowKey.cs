﻿using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class FollowKey : KeySuperClass
{

	private bool beingHeld = false;
	private List<float> childrenSpawnTime;
	private bool childSpawned = false;
	private Transform ownerArrow;
	private ArrowController ownerArrowController;

	private void spawnChild () {
		int childSpawnNodeCounter = ownerArrowController.getTraveledNode ();
		Vector3 positionBeforeSpawnNode = transform.position;
		float totalDistanceFromParent = 0;
		float distanceToNextNode = Vector3.Distance (positionBeforeSpawnNode, ownerArrowController.arrow.Nodes [childSpawnNodeCounter].Position.vector3 ());
		float childSpawnDistance = (childrenSpawnTime [0] - spawnTime) * ownerArrowController.velocity;
		while (totalDistanceFromParent + distanceToNextNode < childSpawnDistance) {
			totalDistanceFromParent += distanceToNextNode;
			positionBeforeSpawnNode = ownerArrowController.arrow.Nodes [childSpawnNodeCounter].Position.vector3 ();
			++childSpawnNodeCounter;
			distanceToNextNode = Vector3.Distance (positionBeforeSpawnNode, ownerArrowController.arrow.Nodes [childSpawnNodeCounter].Position.vector3 ());
		}
		GameObject newChild = Instantiate (GameController.getInstance ().followKeyPrefab);
		FollowKey childScript = newChild.GetComponent <FollowKey> ();
		childScript.setSpawnTime (childrenSpawnTime [0]);
		if (childrenSpawnTime.Count > 0) {
 			childScript.setChildrenSpawnTime (childrenSpawnTime.GetRange (1, childrenSpawnTime.Count - 1));
		} else {
			childScript.setChildrenSpawnTime (new List<float> ());
		}
		childScript.setOwnerArrow (ownerArrow);
		newChild.transform.position = (childSpawnDistance - totalDistanceFromParent) * 
			Vector3.Normalize (ownerArrowController.arrow.Nodes [childSpawnNodeCounter].Position.vector3 () - positionBeforeSpawnNode) + positionBeforeSpawnNode;
		newChild.GetComponent <KeySuperClass> ().setSpawnTime (childrenSpawnTime [0]);
		childSpawned = true;
	}

	void FixedUpdate ()
	{
		if (childrenSpawnTime.Count > 0) {
			if (!childSpawned) {
				if (childrenSpawnTime [0] <= Time.time + GameController.getInstance ().getGuideLineLength () / ownerArrowController.velocity) {
					spawnChild ();
				}
			}
		}
	}

	public void setChildrenSpawnTime (List<float> newChildrenSpawnTime) {
		childrenSpawnTime = newChildrenSpawnTime;
	}

	public void setOwnerArrow (Transform newOwnerArrow) {
		ownerArrow = newOwnerArrow;
		ownerArrowController = ownerArrow.GetComponent <ArrowController> ();
	}


	override public void unHold () {
		beingHeld = false;
	}

	override public void tap () {
		//do nothing
	}

	override public void hold () {
		beingHeld = true;
	}

	protected override void OnTriggerExit (Collider collider) {
		if (!beingHeld) {
			Destroy (gameObject);
			Debug.Log ("Miss");
		}
	}

	protected override void OnTriggerStay (Collider collider)
	{
		if (!hit) {
			if (Time.time > spawnTime - 0.1) { 
				if (collider.transform.tag == "Arrow") {
					hit = true;
				}
			}
		} else {
			if (beingHeld) {
				Destroy (gameObject);
				Debug.Log ("Perfect");
			}
		}
	}
}


