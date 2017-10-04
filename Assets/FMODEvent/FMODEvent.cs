using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Tools {

[System.Serializable]
public struct FMODEventParameter {
	public string name;
	public float value;
}

public class FMODEvent : MonoBehaviour {
	
	const int PARAM_ARRAY_SIZE = 5;
	
	// -- //
	
	[Header("Initialization")]
	[FMODUnity.EventRef]
	public string _FMODEvent;
	public string _startEvent;
	public string _stopEvent;
	public bool _playOneShot;
	
	[Header("When event triggered")]
	public bool _cumulatePlayOneShot;
	public FMOD.Studio.STOP_MODE _stopMode;
	
	[Header("Sound position settings")]
	public bool _playOnThisObject;
	public Transform _customTarget;
	public string _customPositionInEventPayload = "Position";
	
	[Header("Additionals parameters")]
	public FMODEventParameter[] _parameters;
	
	// -- //
	
	FMOD.Studio.EventInstance FMODEventInstance;
	FMOD.ATTRIBUTES_3D spacePosition;
	
	bool playing;
	EventPayload currentPayload;
	
	string[] floatParameters = new string[PARAM_ARRAY_SIZE];
	string[] boolParameters  = new string[PARAM_ARRAY_SIZE];
	
	// -- //
	
	void Start() {
		if(_startEvent.Length > 0) {
			EventManager.AddListener(_startEvent, OnStartEvent);
			
			if(!_playOneShot) {
				FMODEventInstance = FMODUnity.RuntimeManager.CreateInstance(_FMODEvent);
				FMODEventInstance.set3DAttributes(spacePosition);
				
				if(_stopEvent.Length > 0) {
					EventManager.AddListener(_stopEvent, OnStopEvent);
				}
			}
		}
	}
	
	// -- //
	
	void Update() {
		if(playing) {
			FeedParameters(currentPayload);
		}
	}
	
	// -- //
	
	void PlayOneShot(string eventName, EventPayload data) {
		FMODEventInstance = FMODUnity.RuntimeManager.CreateInstance(eventName);
		
		SetupSpacePosition(data.Has(_customPositionInEventPayload), data.Get<Vector3>(_customPositionInEventPayload));
		FMODEventInstance.start();
		FMODEventInstance.release();
	}
	
	void SetupSpacePosition(bool playAtCustomPosition, Vector3 customPosition) {
		Vector3 position = customPosition;
		Vector3 up       = Vector3.zero;
		Vector3 forward  = Vector3.zero;
		
		if(_playOnThisObject || !playAtCustomPosition) {
			Transform target = _playOnThisObject || _customTarget == null ? transform : _customTarget;
			position = target.position;
			up       = target.up;
			forward  = target.forward;
		}
		
		spacePosition.position = FMODUnity.RuntimeUtils.ToFMODVector(position);
		spacePosition.up       = FMODUnity.RuntimeUtils.ToFMODVector(up);
		spacePosition.forward  = FMODUnity.RuntimeUtils.ToFMODVector(forward);
		
		FMODEventInstance.set3DAttributes(spacePosition);
	}
	
	void FeedParameters(EventPayload data) {
		int floatLoop = data.GetParametersOfType<float>(ref floatParameters);
		int boolLoop  = data.GetParametersOfType<bool>(ref boolParameters);
		
		for(int i = 0; i < _parameters.Length; ++ i) {
			FMODEventInstance.setParameterValue(_parameters[i].name, _parameters[i].value);
		}
		
		for(int i = 0; i < floatLoop; ++ i) {
			FMODEventInstance.setParameterValue(floatParameters[i], data.Get<float>(floatParameters[i]));
		}
		
		for(int i = 0; i < boolLoop; ++ i) {
			// All bool are converted to float. True = 1.0f, False = 0.0f
			FMODEventInstance.setParameterValue(boolParameters[i], data.Get<bool>(boolParameters[i]) ? 1.0f : 0.0f);
		}
		
		SetupSpacePosition(data.Has(_customPositionInEventPayload), data.Get<Vector3>(_customPositionInEventPayload));
	}
	
	// -- LISTENERS -- //
	
	void OnStartEvent(EventPayload data) {
		if(_playOneShot) {
			if(!_cumulatePlayOneShot) {
				FMODEventInstance.stop(_stopMode);
			}
			
			PlayOneShot(_FMODEvent, data);
			return;
		}
		
		playing        = true;
		currentPayload = data;
		
		FeedParameters(data);
		FMODEventInstance.start();
	}
	
	void OnStopEvent(EventPayload data) {
		FMODEventInstance.stop(_stopMode);
		playing        = false;
		currentPayload = null;
	}
	
}

}