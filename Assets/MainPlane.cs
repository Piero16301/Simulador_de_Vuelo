using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainPlane : MonoBehaviour
{
	public float speed = 0.0f;
	public Camera FirstPersonCam;
	public bool cameraSwitch = false;
	public Canvas FirstPersonCanvas;
	Camera ThirdPersonCam;
	
	// Start is called before the first frame update
	void Start()
	{
		Debug.Log("plane pilot script added to: " + gameObject.name);
		ThirdPersonCam = Camera.main;
		FirstPersonCanvas.gameObject.SetActive(false);
		ThirdPersonCam.enabled = true;
		FirstPersonCam.enabled = false;
	}
	
	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.V)) {
			if (FirstPersonCam.enabled) {
				ThirdPersonCam.enabled = true;
				FirstPersonCam.enabled = false;
				FirstPersonCanvas.gameObject.SetActive(false);
			}
			else if (!FirstPersonCam.enabled) {
				ThirdPersonCam.enabled = false;
				FirstPersonCam.enabled = true;
				FirstPersonCanvas.gameObject.SetActive(true);
			}
		}

		Vector3 moveCamTo = transform.position - transform.forward * 80.0f + Vector3.up * 30.0f;
		float bias = 0.96f;
		ThirdPersonCam.transform.position = ThirdPersonCam.transform.position * bias +
			moveCamTo * (1.0f - bias);
		ThirdPersonCam.transform.LookAt(transform.position + transform.forward * 50.0f);
		
		transform.position += transform.forward * Time.deltaTime * speed;
		
		speed -= transform.forward.y * Time.deltaTime * 50.0f;

		if (Input.GetAxis ("Speed") != 0.0f) {
			speed += Input.GetAxis ("Speed") * Time.deltaTime * 20.0f;
		}

		/*if (speed < 0.0f) {
			speed = 0.0f;
		}*/
		
		if (speed > 100.0f) {
			speed = 100.0f;
		}
		
		transform.Rotate(0.15f * Input.GetAxis("Vertical"), 0.1f * Input.GetAxis("Direction"), 0.2f * -Input.GetAxis("Horizontal"));
		
		float terrainHeightWhereWeAre = Terrain.activeTerrain.SampleHeight(transform.position);
		if (terrainHeightWhereWeAre > transform.position.y) {
			transform.position = new Vector3(transform.position.x,
			                                 terrainHeightWhereWeAre + 2,
			                                 transform.position.z);
		}
	}
}
