using UnityEngine;
using System.Collections;

// 21/12/2016 - TAG: Just a test

public class MarkInfo : MonoBehaviour {
	public Vector3 m_fSatPos;
	public Camera m_MainCam;

	// Use this for initialization
	void Start () {
		if(m_MainCam==null)
			m_MainCam = GameObject.Find("Main Camera").GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = m_MainCam.WorldToScreenPoint(m_fSatPos);
	}
}
