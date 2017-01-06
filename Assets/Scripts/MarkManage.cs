using UnityEngine;
using Mono.Xml;
using System.Security;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MarkManage : MonoBehaviour {
	private struct MarkStyle{
		public string sId;
		public Color clrLabel;
		public float fLableScale;
		public float fIconScale;
		public string sIcon;
	}

	private struct SatelliteInfo{
		public string sGroup;
		public string sCompanyName;
		public string sId;
		public string sName;
		public string sStyleId;
		public string sAltitudeMode;
		public Vector3 vPos;
		public GameObject goSat;

	}
//	private List<MarkStyle> m_listMarkStyle = new List<MarkStyle>();
	private Hashtable m_htMarkStyle = new Hashtable();
	private List<SatelliteInfo> m_listSatInfo = new List<SatelliteInfo>();
	private const float m_fEarthRadius = 6371.004F;	//以千米为单位
	public Camera m_MainCam;
	public GameObject m_SatPrefab = null;
	private int m_iSatNum = 0;
	public float m_fScale = 30.0F;	//在世界坐标模式下，对标注的缩放
	private float m_fMaxMark2Earth = 0;	//卫星到地球的最远距离
	Canvas m_Canvas;
	struct MarkDis{
		public GameObject go;
		public float fDis;
	}
	private List<List<MarkDis>> m_listMarkLevels = new List<List<MarkDis>>();
	private List<MarkDis> m_listVisibleMark = new List<MarkDis>();
	public int m_iMarkLevel = 3;
	void LoadData()
	{
		SecurityParser SP = new SecurityParser();

		// Unity用Resources读取资源不需要后缀名，且文件夹斜杠不需要双写
		string xmlPath = "data/Satellites";
		string ss = Resources.Load( xmlPath ).ToString();
		SP.LoadXml(ss);
		
		SecurityElement seKml = SP.ToXml();	// kml
		SecurityElement seDoc = (SecurityElement)seKml.Children[0];
		byte r = 0;
		byte g = 0;
        byte b = 0;
		byte a = 0;
		string sGroupName = null;
		string sCompanyName = null;
		foreach (SecurityElement child in seDoc.Children)
		{
			switch(child.Tag)
			{
				case "Style":
					MarkStyle ms = new MarkStyle();
					ms.sId = child.Attribute("id");
					foreach (SecurityElement seStyle in child.Children)
					{
						if(seStyle.Tag=="LabelStyle")
							foreach (SecurityElement seContent in seStyle.Children)
							{
								if(seContent.Tag == "color")
								{
									byte.TryParse(seContent.Text.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, null, out a);
									byte.TryParse(seContent.Text.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, null, out r);
									byte.TryParse(seContent.Text.Substring(4, 2), System.Globalization.NumberStyles.HexNumber, null, out g);
									byte.TryParse(seContent.Text.Substring(6, 2), System.Globalization.NumberStyles.HexNumber, null, out b);
									ms.clrLabel = new Color(r, g, b, a);
								}
								else
									ms.fLableScale = float.Parse( seContent.Text );
							}
						else
							foreach (SecurityElement seContent in seStyle.Children)
							{
								if(seContent.Tag == "Icon")
								{
									SecurityElement seHref = (SecurityElement)seContent.Children[0];
									ms.sIcon = seHref.Text;
								}
								else
									ms.fIconScale = float.Parse( seContent.Text );
							}
					}
					m_htMarkStyle.Add(ms.sId, ms);
					break;
				case "Folder":
					{
						foreach (SecurityElement seFolderSubContent in child.Children)
						{
							switch(seFolderSubContent.Tag)
							{
								case "name":
									sGroupName = seFolderSubContent.Text;
									break;
								case "Folder":
									foreach (SecurityElement seFolderSub2 in seFolderSubContent.Children)
									{
										switch(seFolderSub2.Tag)
										{
											case "name":
												sCompanyName = seFolderSub2.Text;
												break;
											case "Placemark":
												SatelliteInfo si = new SatelliteInfo();
												si.sId = seFolderSub2.Attribute("id");
												si.sGroup = sGroupName;
												si.sCompanyName = sCompanyName;

												foreach (SecurityElement sePlacemarkChild in seFolderSub2.Children)
												{
													switch(sePlacemarkChild.Tag)
													{
														case "name":
															si.sName = sePlacemarkChild.Text;
															break;
														case "styleUrl":
															si.sStyleId = sePlacemarkChild.Text.Substring(1,sePlacemarkChild.Text.Length-1);
															break;
														case "Point":
															foreach (SecurityElement sePointChild in sePlacemarkChild.Children)
															{
																if(sePointChild.Tag=="altitudeMode")
																	si.sAltitudeMode = sePointChild.Text;
																else
																{
																	string sCor = sePointChild.Text;
																	int i1 = sCor.IndexOf(',');
																	int i2 = sCor.IndexOf(',',i1+1);
																	float x = float.Parse(sCor.Substring(0,i1));
																	float y = float.Parse(sCor.Substring(i1+1, i2-i1-1));
																	float radius = float.Parse(sCor.Substring(i2+1, sCor.Length-i2-1));	//获取的距地高度以米为单位

																	//本程序以km为单位，故要进行量纲变化,再加上地球半径
																	radius = radius/1000F + m_fEarthRadius;
																	float arc = CamRotate.Angle2Arc(y);
																	y = Mathf.Sin(arc)*radius;
																	float xz = radius*Mathf.Cos(arc);
																	arc = CamRotate.Angle2Arc(x);
																	x = xz*Mathf.Cos(arc);
																	float z =  xz*Mathf.Sin(arc);
																	si.vPos.Set(x, y,z);
																}
															}
															break;
														default:
															break;
													}
												}
												si.goSat = AddMark(si);
												m_listSatInfo.Add(si);
												break;
											default:
												break;											
										}
									}
									break;
								default:
									break;
							}
						}
					}
					break;		
				default:
					break;
			}
		}
		m_iSatNum = m_listSatInfo.Count;
	}
	// Use this for initialization
	void Start () {
		LoadData();
		if(m_MainCam==null)
			m_MainCam = GameObject.Find("Main Camera").GetComponent<Camera>();
		m_Canvas = transform.parent.GetComponent<Canvas>();
	}
	
	bool IsInRange(float _f, float _fMin, float _fMax)
	{
		return _f>_fMin && _f<_fMax;
	}

	// Update is called once per frame
	void Update () {
/*		Transform t = transform.GetChild(0);
		MarkInfo mi = t.GetComponent<MarkInfo>();

			Vector3 vPos = m_MainCam.WorldToScreenPoint(mi.m_fSatPos);
			Vector3 vCam2Sat = mi.m_fSatPos - m_MainCam.transform.position;
			float fDis = vCam2Sat.magnitude;
			Vector2 vP = new Vector2();
			if(IsInRange(vPos.x,0,Screen.width) && IsInRange(vPos.y, 0, Screen.height) && vPos.z>0)	//判断是否在视域内
			{
				if(!IsInRange(Vector3.Angle(m_MainCam.transform.position,mi.m_fSatPos), 90, 270) )
				{
					t.gameObject.SetActive(true);
					t.position=new Vector3(vPos.x, vPos.y, 0);
					vP.Set(Vector3.Angle(-m_MainCam.transform.position, vCam2Sat),Mathf.Asin(700/m_MainCam.transform.position.magnitude)*180F/Mathf.PI);;
				}
				else if(Vector3.Angle(-m_MainCam.transform.position, vCam2Sat)<Mathf.Asin(700/m_MainCam.transform.position.magnitude)*180F/Mathf.PI)
					t.gameObject.SetActive(false);
				else{
					t.gameObject.SetActive(true);
					t.position=new Vector3(vPos.x, vPos.y, 0);
					vP.Set(Vector3.Angle(-m_MainCam.transform.position, vCam2Sat),Mathf.Asin(700/m_MainCam.transform.position.magnitude)*180F/Mathf.PI);;
				}
			}
			else
			{
				t.gameObject.SetActive(false);
				Debug.Log(false);
			}
			*/
		switch(m_Canvas.renderMode)
		{
			case RenderMode.ScreenSpaceOverlay:
				ScreenSpaceUpdate();
				break;
			case RenderMode.WorldSpace:
				WorldSpaceUpdate();
				break;
		}
	}
	

	void ScreenSpaceUpdate()
	{
		//初始化清空层次列表
		for(int i=0; i<m_iMarkLevel; i++)
		{
			m_listMarkLevels.Add(new List<MarkDis>());
		}

		Vector3 vScreenPos;
		float fDis, fScale, fCamDis = m_MainCam.transform.position.magnitude;
		Vector3 vCam2Sat;

		for(int i=0; i<m_iSatNum; i++)
		{
			vScreenPos = m_MainCam.WorldToScreenPoint(m_listSatInfo[i].vPos);	//转换的屏幕坐标范围从0到Screen.width；z值表示深度，约等于摄像机与目标的距离，若为负，则表示在摄像机后面
			vCam2Sat = m_listSatInfo[i].vPos - m_MainCam.transform.position;
			fDis = vScreenPos.z;
			vScreenPos.Set(vScreenPos.x, vScreenPos.y, 0);
			if(IsInRange(vScreenPos.x,0,Screen.width) && IsInRange(vScreenPos.y, 0, Screen.height) && fDis>0)
			{
				if(fCamDis<18000F)		//摄像机距地球>18000时，地球完全被卫星遮住，不需要判断是否地球遮挡卫星
				{
					if(!IsInRange(Vector3.Angle(m_MainCam.transform.position,m_listSatInfo[i].vPos), 90, 270) )	//判断标注矢量与摄像机矢量之间的夹角是否大于90
					{
						m_listSatInfo[i].goSat.SetActive(true);
						m_listSatInfo[i].goSat.transform.position = vScreenPos;
						fScale = Mathf.Clamp(10000/fDis,0.1F,1F);
						m_listSatInfo[i].goSat.transform.localScale = Vector3.one * fScale;
					}
					else if(Vector3.Angle(-m_MainCam.transform.position, vCam2Sat)<Mathf.Asin(750/m_MainCam.transform.position.magnitude)*180F/Mathf.PI)  //判断摄像机到标注的矢量与摄像机到球心的矢量夹角是否小于地球的半径角
						m_listSatInfo[i].goSat.SetActive(false);
					else
					{
						m_listSatInfo[i].goSat.SetActive(true);
						m_listSatInfo[i].goSat.transform.position = vScreenPos;
						fScale = Mathf.Clamp(10000/fDis,0.1F,1F);
						m_listSatInfo[i].goSat.transform.localScale = Vector3.one * fScale;				
					}
				}
				else
				{
					m_listSatInfo[i].goSat.SetActive(true);
					m_listSatInfo[i].goSat.transform.position = vScreenPos;
					fScale = Mathf.Clamp(10000/fDis,0.1F,1F);
					m_listSatInfo[i].goSat.transform.localScale = Vector3.one * fScale;					
				}

			}
			else
				m_listSatInfo[i].goSat.SetActive(false);
			
			if(m_listSatInfo[i].goSat.activeSelf)
			{
				MarkDis md = new MarkDis();
				md.go = m_listSatInfo[i].goSat;
				md.fDis = fDis;
				m_listVisibleMark.Add(md);
			}
		}

		SortMarks();
	}

	void SortMarks()
	{
		int iDealMaxLevel = 3;//m_listMarkLevels.Count;
		int iVisibleNum = m_listVisibleMark.Count;
		int i, iLevel;
		float fDisStep = (m_MainCam.transform.position.magnitude + m_fMaxMark2Earth)/m_iMarkLevel;
		for(i=0; i<iVisibleNum; i++)
		{
			iLevel = (int)(m_listVisibleMark[i].fDis/fDisStep);
			if(iLevel<=iDealMaxLevel)
			{
				InsertMarksAt(iLevel, m_listVisibleMark[i]);
			}
			else
				m_listMarkLevels[iLevel].Add(m_listVisibleMark[i]);
		}

		m_listVisibleMark.Clear();
		ReOrderInSystem();

		if(m_listMarkLevels.Count>0)
		{
			for(i=0; i<m_listMarkLevels.Count; i++)
				m_listMarkLevels[i].Clear();
		}
		m_listMarkLevels.Clear();
	}

	void InsertMarksAt(int _iLevel, MarkDis _md)
	{
		int iCount = m_listMarkLevels[_iLevel].Count;
		if(iCount==0)
		{
			m_listMarkLevels[_iLevel].Add(_md);
			return;
		}
/*		//冒泡遍历排序
		int i = 0;
		
		
		while(i<iCount)
		{
			if(_md.fDis>m_listMarkLevels[_iLevel][i].fDis)
				break;
			i++;
		}
		if(i==iCount)
			m_listMarkLevels[_iLevel].Add(_md);
		else
			m_listMarkLevels[_iLevel].Insert(i, _md);	
*/
		//二分法排序
		int iLow = 0;
		int iHigh = iCount-1;
		int iMid = (iLow+iHigh)/2;

		while(iMid!=iLow && iMid!=iHigh)
		{
			if(_md.fDis>=m_listMarkLevels[_iLevel][iMid].fDis)
				iHigh = iMid;
			else
				iLow = iMid;

			iMid = (iLow+iHigh)/2;
		}

		if(_md.fDis>=m_listMarkLevels[_iLevel][iMid].fDis)
		{
			m_listMarkLevels[_iLevel].Insert(iMid, _md);
		}
		else
		{
			if(iMid==iCount-1)
				m_listMarkLevels[_iLevel].Add(_md);
			else
				m_listMarkLevels[_iLevel].Insert(iMid+1, _md);
		}
	}

	void ReOrderInSystem()
	{
		int i, j;
		int iLevelCount = m_listMarkLevels.Count;
		int iMarkNum;
		for(i=iLevelCount-1; i>-1; i--)
		{
			iMarkNum = m_listMarkLevels[i].Count;
			for(j=0; j<iMarkNum; j++)
			{
				m_listMarkLevels[i][j].go.transform.SetAsLastSibling();
			}
		}
	}
	void WorldSpaceUpdate()
	{

		Vector3 vPos;
		float fDis;

		for(int i=0; i<m_iSatNum; i++)
		{
			vPos = m_MainCam.WorldToScreenPoint(m_listSatInfo[i].vPos); //每个卫星在屏幕上的位置
			fDis = vPos.z;
			if(IsInRange(vPos.x,0,Screen.width) && IsInRange(vPos.y, 0, Screen.height) && fDis>0)
			{
				m_listSatInfo[i].goSat.SetActive(true);
				m_listSatInfo[i].goSat.transform.position = m_listSatInfo[i].vPos;
				vPos = m_MainCam.transform.position-m_listSatInfo[i].vPos;
				m_listSatInfo[i].goSat.transform.rotation = Quaternion.LookRotation(vPos, Vector3.Cross(vPos, Vector3.Cross(m_MainCam.transform.up,vPos)));
				m_listSatInfo[i].goSat.transform.localScale = Vector3.one * m_fScale;

			}
			else
				m_listSatInfo[i].goSat.SetActive(false);
		}

	}
	GameObject AddMark(SatelliteInfo _si)
	{
		GameObject go = GameObject.Instantiate(m_SatPrefab);
		MarkInfo info = go.GetComponent<MarkInfo>();
		Image img = go.GetComponent<Image>();
		img.color = ((MarkStyle)m_htMarkStyle[_si.sStyleId]).clrLabel;
		info.m_fSatPos = _si.vPos;
		go.transform.SetParent(transform);
		go.transform.position = _si.vPos;
		if(_si.vPos.magnitude>m_fMaxMark2Earth)
			m_fMaxMark2Earth = _si.vPos.magnitude;
		Transform tr = go.transform.GetChild(0);	//Text
		Text text = tr.GetComponent<Text>();
		text.text = _si.sId;
		return go;
	}
}
