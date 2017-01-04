using UnityEngine;
public class CamRotate:MonoBehaviour {

	[SerializeField] protected Transform m_Target; // The target object to follow
    [Range(0f, 10f)] [SerializeField] private float m_TurnSpeed = 1.5f;   // How fast the rig will rotate from user input.
    [SerializeField] private float m_TurnSmoothing = 0.0f;                // How much smoothing to apply to the turn input, to reduce mouse-turn jerkiness

	private float m_LookAngle;                    // The rig's y axis rotation.
	private float m_TiltAngle; // The pivot's x axis rotation.
    private float k_LookDistance = 100f;    // How far in front of the pivot the character's look target is.
	private Vector3 m_vDir; 

	public float m_fMinDistance; 		//与物体的最短距离，为物体半径的1.5倍

	// Use this for initialization
	void Start () {
	
	}
	
    protected void Awake() {
		m_vDir = transform.position - m_Target.position; 
		k_LookDistance = m_vDir.magnitude; 
		transform.LookAt(m_Target); 	
		m_TiltAngle = 90 - Vector3.Angle(m_vDir, Vector3.up);
		m_LookAngle = Vector3.Angle(Vector3.Cross(Vector3.up, Vector3.Cross(m_vDir, Vector3.up)) ,Vector3.forward);
		MeshFilter mf = m_Target.GetComponent < MeshFilter > (); 
		m_fMinDistance = mf.mesh.bounds.size.x * 1.5f/2f; 
}

	// Update is called once per frame
	void Update () {
		HandleCamera(); 
	}

	private void HandleCamera() {
		if (Time.timeScale < float.Epsilon)
			return; 

		var z = Input.GetAxis("Mouse ScrollWheel")/20 + 1; 

		m_vDir *= z; 

		if (Input.GetMouseButton(1)) {
			Rotate(); 
		}

		if (m_TurnSmoothing > 0)
			transform.position = Vector3.Lerp(transform.position, m_vDir, m_TurnSmoothing * Time.deltaTime); 
		else
			transform.position = m_vDir; 

		transform.LookAt(m_Target); 	
		m_vDir = transform.position;
		
	}

	private void Rotate_Euler() {
			// Read the user input
			var x = Input.GetAxis("Mouse X") * m_TurnSpeed; 
			var y = Input.GetAxis("Mouse Y") * m_TurnSpeed; 


			m_vDir = Quaternion.Euler(y, x, 0) * m_vDir; 
			m_vDir = m_vDir.normalized * m_vDir.magnitude; 
	}

	private void Rotate_Quaternion() {
			// Read the user input
			var x = Input.GetAxis("Mouse X") * m_TurnSpeed; 
			var y = Input.GetAxis("Mouse Y") * m_TurnSpeed; 	
			float fDis = m_vDir.magnitude; 

			Quaternion q1 = Quaternion.AngleAxis(x, Vector3.up); 
			
			m_vDir = Quaternion.AngleAxis(y, transform.right) * m_vDir;

			m_vDir = q1 * m_vDir; 
			m_vDir = m_vDir.normalized * fDis; 			
	}

	//利用三角函数计算空间位置
	private void Rotate() {
			var x = Input.GetAxis("Mouse X") * m_TurnSpeed; 
			var y = Input.GetAxis("Mouse Y") * m_TurnSpeed; 

			m_LookAngle += x;
			m_TiltAngle += y;

			float fArcTilt = Angle2Arc(m_TiltAngle);
			float fArcLook = Angle2Arc(m_LookAngle);
			float fPosY = Mathf.Sin(fArcTilt);
			float fPosXZ = Mathf.Cos(fArcTilt);
			float fPosZ = Mathf.Cos(fArcLook);
			float fPosX = Mathf.Sin(fArcLook);

			m_vDir = new Vector3(fPosX, fPosY, fPosZ).normalized * m_vDir.magnitude;
	}

	float Angle2Arc(float _fAngle)
	{
		return _fAngle*Mathf.PI/180;
	}
}
