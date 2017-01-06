using UnityEngine;
using System.Collections;

public class FPSCaculate : MonoBehaviour {

	private float m_LastUpdateShowTime=0f;    //上一次更新帧率的时间;
	
	private float m_UpdateShowDeltaTime=0.01f;//更新帧率的时间间隔;
	
	private int m_FrameUpdate=0;//帧数;
	
	private float m_FPS=0;
	
	TextAccess m_taOutput = new TextAccess();
	string m_sFPSRecord;
	void Awake()
	{
		Application.targetFrameRate=100;
	}
	
	// Use this for initialization
	void Start ()
	{
		m_LastUpdateShowTime=Time.realtimeSinceStartup;
		m_taOutput.Open(Application.persistentDataPath, "FPS.txt");		
		m_taOutput.Delete();
	}
	
	// Update is called once per frame
	void Update ()
	{
		m_FrameUpdate++;
		float f = Time.realtimeSinceStartup-m_LastUpdateShowTime;
		if(f>=m_UpdateShowDeltaTime)
		{
			m_FPS=m_FrameUpdate/f;
			m_FrameUpdate=0;
			m_LastUpdateShowTime=Time.realtimeSinceStartup;
		}		
		m_sFPSRecord += m_FPS.ToString() + "\r\n";
	}
	
	void OnGUI()
	{
		GUI.Label(new Rect(Screen.width/2,0,100,100),"FPS: "+m_FPS);
	}

	/// <summary>
	/// This function is called when the MonoBehaviour will be destroyed.
	/// </summary>
	void OnDestroy()
	{
		m_taOutput.Write(m_sFPSRecord);
		m_taOutput.Close();
	}
}
