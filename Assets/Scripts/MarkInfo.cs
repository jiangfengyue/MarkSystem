using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// 21/12/2016 - TAG: Just a test

public class MarkInfo : MonoBehaviour {
	public Vector3 m_fSatPos;
	public Camera m_MainCam;

	Image m_Image;
	public float m_fOrder;

	public bool isRendering=false;  

	// Use this for initialization
	void Start () {
		if(m_MainCam==null)
			m_MainCam = GameObject.Find("Main Camera").GetComponent<Camera>();

	}
	
	// Update is called once per frame
	void Update () {



//		transform.position = m_fSatPos;

		//在Canvas为WorldSpace时调用
//		WorldSpaceUpdate();
//		ScreenSpaceUpdate();
	}


	void WorldSpaceUpdate()
	{
//		isRendering=curtTime!=lastTime?true:false;  
/*
//		if(isRendering)
		{
			Vector3 v = m_MainCam.transform.position-transform.position;
			transform.rotation = Quaternion.LookRotation(v, m_MainCam.transform.up);
//			transform.localScale = Vector3.one*v.magnitude/0.3F;
		}
		else
		{
			isRendering = isRendering;
		}*/
	}

	public void ScreenSpaceUpdate()
	{
		transform.position = m_MainCam.WorldToScreenPoint(m_fSatPos);
		m_fOrder = transform.position.z;
			Vector3 vPos = transform.position;
			if(Mathf.Abs(vPos.x)<Screen.width && Mathf.Abs(vPos.y)<Screen.height)
			{
				transform.gameObject.SetActive(true);
				Debug.Log(true);
			}
			else
			{
				transform.gameObject.SetActive(false);
				Debug.Log(false);
			}




//		float fScale = Mathf.Clamp(10000/m_fOrder,0.1F,1F);
//		m_Image.color = new Color(m_Image.color.r, m_Image.color.g, m_Image.color.b, fScale);

		transform.position=new Vector3(transform.position.x, transform.position.y, 0);
//		transform.localScale = Vector3.one * fScale;
	}
	void OnBecameVisible(){
	//可见状态下你要执行 的东西
		isRendering = true;
	}
	void OnBecameInvisible(){
	//不可见状态下你要执行的东西
		isRendering = false;
	}

}
