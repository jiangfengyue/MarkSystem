using UnityEngine;
using Mono.Xml;
using System.Security;
using System.Collections.Generic;

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

	}
	private List<MarkStyle> m_listMarkStyle = new List<MarkStyle>();
	private List<SatelliteInfo> m_listSatInfo = new List<SatelliteInfo>();
	private const float m_fEarthRadius = 6371.004F;	//以千米为单位

	public GameObject m_SatPrefab = null;
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
					m_listMarkStyle.Add(ms);
					break;
				case "Folder":
//					foreach (SecurityElement seFolder in child.Children)
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
												m_listSatInfo.Add(si);
												AddMark(si);
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
	}
	// Use this for initialization
	void Start () {
		LoadData();
/*		SecurityParser SP = new SecurityParser();
		

		// Unity用Resources读取资源不需要后缀名，且文件夹斜杠不需要双写
//		string xmlPath = "data/Satellites";

		string ss = Resources.Load( "data/22" ).ToString();
		SP.LoadXml(ss);
		Debug.Log(ss);
		SecurityElement seKml = SP.ToXml();	// kml
		SecurityElement seDoc = (SecurityElement)seKml.Children[0];
		byte r = 0;
		byte g = 0;
        byte b = 0;
		byte a = 0;
		string sGroupName = null;
		string sCompanyName = null;
*/

		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void AddMark(SatelliteInfo _si)
	{
		GameObject go = GameObject.Instantiate(m_SatPrefab);
		MarkInfo info = go.GetComponent<MarkInfo>();
//		Image img = go.GetComponent<Image>();
		info.m_fSatPos = _si.vPos;
		go.transform.SetParent(transform);
	}
}
