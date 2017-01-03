using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
public class CamRotate : MonoBehaviour {

	[SerializeField] protected Transform m_Target;            // The target object to follow
    [SerializeField] private float m_MoveSpeed = 1f;                      // How fast the rig will move to keep up with the target's position.
    [Range(0f, 10f)] [SerializeField] private float m_TurnSpeed = 1.5f;   // How fast the rig will rotate from user input.
    [SerializeField] private float m_TurnSmoothing = 0.0f;                // How much smoothing to apply to the turn input, to reduce mouse-turn jerkiness
    [SerializeField] private float m_TiltMax = 75f;                       // The maximum value of the x axis rotation of the pivot.
    [SerializeField] private float m_TiltMin = 45f;                       // The minimum value of the x axis rotation of the pivot.

	private float m_LookAngle;                    // The rig's y axis rotation.
    private float m_TiltAngle;                    // The pivot's x axis rotation.
    private float k_LookDistance = 100f;    // How far in front of the pivot the character's look target is.
	private Vector3 m_PivotEulers;
	private Vector3 m_vDir;

	public float m_fMinDistance;		//与物体的最短距离，为物体半径的1.5倍

	// Use this for initialization
	void Start () {
	
	}
	
    protected void Awake()
    {
		m_vDir = transform.position - m_Target.position;
		k_LookDistance = m_vDir.magnitude;
		transform.LookAt(m_Target);  		     
		m_PivotEulers = Quaternion.Inverse(transform.rotation).eulerAngles ;
		MeshFilter mf = m_Target.GetComponent<MeshFilter>();
		m_fMinDistance = mf.mesh.bounds.size.x*1.5f/2f;
    }

	// Update is called once per frame
	void Update () {
		HandleCamera();
	}

	private void HandleCamera()
	{
		if(Time.timeScale < float.Epsilon)
			return;

		var z = Input.GetAxis("Mouse ScrollWheel")/20+1;

		m_vDir *= z;

		if(Input.GetMouseButton(1))
		{
			// Read the user input
			var x = Input.GetAxis("Mouse X")*m_TurnSpeed;
			var y = Input.GetAxis("Mouse Y")*m_TurnSpeed;


			// Adjust the look angle by an amount proportional to the turn speed and horizontal input.
			m_LookAngle = m_PivotEulers.y + 0;
			// on platforms with a mouse, we adjust the current angle based on Y mouse input and turn speed
			m_TiltAngle = m_PivotEulers.x - 0;
			// and make sure the new value is within the tilt range
			m_TiltAngle = Mathf.Clamp(m_TiltAngle, -m_TiltMin, m_TiltMax);


			m_vDir = Quaternion.Euler(y, x, 0) * m_vDir;
			m_vDir = m_vDir.normalized * m_vDir.magnitude;
		}

		if (m_TurnSmoothing > 0)
			transform.position = Vector3.Lerp(transform.position, m_vDir, m_TurnSmoothing * Time.deltaTime);
		else
			transform.position = m_vDir;

		transform.LookAt(m_Target);	
		
	}
}
