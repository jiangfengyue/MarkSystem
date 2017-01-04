using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// 21/12/2016 - TAG: Just a test

public class MarkInfo : MonoBehaviour {
	public Vector3 m_fSatPos;
	public Camera m_MainCam;

	Image m_Image;
	public float m_fOrder;
	// Use this for initialization
	void Start () {
		if(m_MainCam==null)
			m_MainCam = GameObject.Find("Main Camera").GetComponent<Camera>();
		m_Image = transform.GetComponent<Image>();
//		m_fSatPos = new Vector3(m_MainCam.GetComponent<CamRotate>().m_fMinDistance, m_MainCam.GetComponent<CamRotate>().m_fMinDistance, m_MainCam.GetComponent<CamRotate>().m_fMinDistance);
	}
	
	// Update is called once per frame
	void Update () {
/*
		transform.position = m_MainCam.WorldToScreenPoint(m_fSatPos);
		m_fOrder = transform.position.z;
		float fScale = Mathf.Clamp(10000/m_fOrder,0.1F,1F);
//		m_Image.color = new Color(m_Image.color.r, m_Image.color.g, m_Image.color.b, fScale);

		transform.position=new Vector3(transform.position.x, transform.position.y, 0);
		transform.localScale = Vector3.one * fScale;
*/
//		transform.position = m_fSatPos;
	}
}
