using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Tools;

namespace Game {

public class TriggerEvent : MonoBehaviour {
	
	public float _volume    = 1f;
	public bool _jumpToCoin = false;
	public Vector3 _soundPosition;
	
	// -- //
	
	EventPayload payload = new EventPayload();
	
	// -- //
	
	void Start() {
		payload.Add("Volume", _volume);
		payload.Add("JumpToCoin", _jumpToCoin);
		payload.Add("Position", _soundPosition);
	}
	
	void Update() {
		if(Input.GetKeyDown(KeyCode.T)) {
			EventManager.Trigger("OnStartFMODSound", payload);
		} else if(Input.GetKeyDown(KeyCode.Y)) {
			EventManager.Trigger("OnStopFMODSound", payload);
		} else if(Input.GetKeyDown(KeyCode.U)) {
			payload.Set("Volume", _volume);
			payload.Set("JumpToCoin", _jumpToCoin);
			payload.Set("Position", _soundPosition);
		}
	}
	
}

}