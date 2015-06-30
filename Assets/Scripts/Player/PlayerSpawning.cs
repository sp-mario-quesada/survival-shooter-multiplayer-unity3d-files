﻿using UnityEngine;
using System.Collections;

public class PlayerSpawning : Photon.MonoBehaviour
{
	public float sinkSpeed = 1;
	public float respawnDelay = 5;

	PlayerMovement playerMovement;                              
	PlayerShooting playerShooting;
	SkinnedMeshRenderer[] playerRenderers;
	bool isSinking;
	RoomTimeManager roomTimeManager;

	void Awake ()
	{
		roomTimeManager = GameObject.FindGameObjectWithTag ("RoomTimeManager").GetComponent<RoomTimeManager> ();
		playerMovement = GetComponent <PlayerMovement> ();
		playerShooting = GetComponentInChildren <PlayerShooting> ();
		playerRenderers = GetComponentsInChildren<SkinnedMeshRenderer> ();
	}

	void Update ()
	{
		if (photonView.isMine && isSinking) {
			transform.Translate (-Vector3.up * sinkSpeed * Time.deltaTime);
		}
		if (transform.position.y < -1) {
			isSinking = false;
			SetPlayerVisibility (false);
		}
	}

	void OnPhotonInstantiate (PhotonMessageInfo	info)
	{
		if (roomTimeManager.isPauseState ()) {
			DestroyPlayer (true);
		} else {
			RespawnPlayer ();
		}
	}

	void OnPauseStarted ()
	{
		DestroyPlayer ();
	}

	void OnRoundStarted ()
	{
		RespawnPlayer ();
	}

	void RespawnPlayer ()
	{
		Debug.Log ("RespawnPlayer");
		if (roomTimeManager.isPauseState ()) {
			return;
		}

		if (photonView.isMine) {
			PositionHelper.RandomizeTransform (transform);
			SetPlayerControl (true);
		}
		Debug.Log (photonView.isMine);
		SetPlayerPhysics (true);
		SetPlayerVisibility (true);
		gameObject.BroadcastMessage ("OnPlayerRespawn", SendMessageOptions.DontRequireReceiver);
	}

	void DestroyPlayer (bool instantly = false)
	{
		if (instantly) {
			SetPlayerVisibility (false);
		} else if (photonView.isMine) {
			isSinking = true;
		}

		SetPlayerPhysics (false);
		SetPlayerControl (false);
	}

	// Called at the end of "Die" animation, must be public
	public void OnDeathAnimEnd ()
	{
		gameObject.BroadcastMessage ("OnPlayerDieAnimEnd", SendMessageOptions.DontRequireReceiver);
		DestroyPlayer ();
	}

	void SetPlayerPhysics (bool enabled)
	{
		GetComponent<CapsuleCollider> ().enabled = enabled;
		GetComponent<BoxCollider> ().enabled = enabled;
		GetComponent <Rigidbody> ().isKinematic = !enabled;
	}

	public void SetPlayerVisibility (bool enabled)
	{
		foreach (var item in playerRenderers) {
			item.enabled = enabled;
		}
	}

	void SetPlayerControl (bool enabled)
	{
		playerMovement.enabled = enabled;
		playerShooting.enabled = enabled;
	}

	void OnPlayerDead ()
	{
		SetPlayerControl (false);
		Invoke ("RespawnPlayer", respawnDelay);
	}
}
