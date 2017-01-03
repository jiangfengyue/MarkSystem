using UnityEngine;
using System.Collections;

// 21/12/2016 - TAG: Just a test

public class MarkInfo : MonoBehaviour {
	public Vector3 m_fSatPos;
	public Camera m_MainCam;

	public float m_fOrder;
	// Use this for initialization
	void Start () {
		if(m_MainCam==null)
			m_MainCam = GameObject.Find("Main Camera").GetComponent<Camera>();

		m_fSatPos = new Vector3(m_MainCam.GetComponent<CamRotate>().m_fMinDistance, m_MainCam.GetComponent<CamRotate>().m_fMinDistance, m_MainCam.GetComponent<CamRotate>().m_fMinDistance);
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = m_MainCam.WorldToScreenPoint(m_fSatPos);

		transform.position = new Vector3(transform.position.x, transform.position.y, m_fOrder);

	}
}
