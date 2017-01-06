using UnityEngine; 
using System.Collections; 
using System.IO; 
using System.Collections.Generic; 
using System; 

public class TextAccess{

	//文本中每行的内容
//	ArrayList infoall = null; 
	//皮肤资源，这里用于显示中文
	public GUISkin skin; 

	string m_sFileName;
	string m_sFilePath;
	StreamWriter m_swWriter = null; 
	StreamReader m_swReader = null;

	
	public TextAccess () {
		//删除文件
/*		DeleteFile(Application.persistentDataPath, "FileName.txt"); 

		//创建文件，共写入3次数据
		CreateFile(Application.persistentDataPath, "FileName.txt", "宣雨松MOMO"); 
		CreateFile(Application.persistentDataPath, "FileName.txt", "宣雨松MOMO"); 
		CreateFile(Application.persistentDataPath, "FileName.txt", "宣雨松MOMO"); 
		//得到文本中每一行的内容
		infoall = LoadFile(Application.persistentDataPath, "FileName.txt"); 
*/
	}

	public void Open(string _sPath, string _sName)
	{
		m_sFilePath = _sPath;
		m_sFileName = _sName;
	}

	public void Write(string _sContent)
	{
		if(m_swWriter == null)
		{
			FileInfo t = new FileInfo(m_sFilePath + "//"+ m_sFileName);
			if ( ! t.Exists) {
				//如果此文件不存在则创建
				m_swWriter = t.CreateText(); 
			}
			else {
				//如果此文件存在则打开
				m_swWriter = t.AppendText(); 
			}
		}

		m_swWriter.WriteLine(_sContent); 
	}

	public void Close()
	{
		if(m_swWriter!=null)
		{
			//关闭流
			m_swWriter.Close(); 
			//销毁流
			m_swWriter.Dispose(); 		
			m_swWriter = null;
		}
		if(m_swReader!=null)
		{
			//关闭流
			m_swReader.Close(); 
			//销毁流
			m_swReader.Dispose(); 		
			m_swReader = null;
		}
		m_sFilePath = "";
		m_sFileName = "";
	}

	public void Delete(string _sPath, string _sName)
	{
		File.Delete(_sPath + "//"+ _sName);
	}

	public void Delete()
	{
		if(m_sFilePath!="" && m_sFileName!="")
			File.Delete(m_sFilePath + "//"+ m_sFileName);
	}

	public ArrayList Read()
	{
		//使用流的形式读取
		if(m_swReader==null)
		{
			try {
				m_swReader = File.OpenText(m_sFilePath + "//"+ m_sFileName);
			}catch(Exception e) {
				//路径与名称未找到文件则直接返回空
				Debug.Log(e);
				return null; 
			}
		}
		
		string line; 
		ArrayList arrlist = new ArrayList(); 
		while ((line = m_swReader.ReadLine()) != null) {
				//一行一行的读取
				//将每一行的内容存入数组链表容器中
				arrlist.Add(line); 
			}

		//将数组链表容器返回
		return arrlist; 		
	}

	/**
	* path：文件创建目录
	* name：文件的名称
	*  info：写入的内容
	*/
	static void CreateFile(string path, string name, string info) {
		//文件流信息
		StreamWriter sw; 
		FileInfo t = new FileInfo(path + "//"+ name);
		if ( ! t.Exists) {
			//如果此文件不存在则创建
			sw = t.CreateText(); 
		}
		else {
			//如果此文件存在则打开
			sw = t.AppendText(); 
		}
		//以行的形式写入信息
		sw.WriteLine(info); 
		//关闭流
		sw.Close(); 
		//销毁流
		sw.Dispose(); 
	}
	
	/**
	* path：读取文件的路径
	* name：读取文件的名称
	*/
	static ArrayList LoadFile(string path, string name) {
		//使用流的形式读取
		StreamReader sr = null; 
		try {
			sr = File.OpenText(path + "//"+ name);
		}catch(Exception e) {
			Debug.Log(e);
			//路径与名称未找到文件则直接返回空
			return null; 
		}
		string line; 
		ArrayList arrlist = new ArrayList(); 
		while ((line = sr.ReadLine()) != null) {
				//一行一行的读取
				//将每一行的内容存入数组链表容器中
				arrlist.Add(line); 
			}
			//关闭流
			sr.Close(); 
			//销毁流
			sr.Dispose(); 
			//将数组链表容器返回
			return arrlist; 
	}
	
	/**
	* path：删除文件的路径
	* name：删除文件的名称
	*/
	
	static void DeleteFile(string path, string name) {
			File.Delete(path + "//"+ name);
	
	}

}

