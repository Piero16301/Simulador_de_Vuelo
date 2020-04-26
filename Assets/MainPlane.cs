using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainPlane : MonoBehaviour
{
	public float speed = 0.0f;
	
	// Start is called before the first frame update
	void Start()
	{
		Debug.Log("plane pilot script added to: " + gameObject.name);
	}
	
	// Update is called once per frame
	void Update()
	{
		Vector3 moveCamTo = transform.position - transform.forward * 80.0f + Vector3.up * 30.0f;
		float bias = 0.96f;
		Camera.main.transform.position = Camera.main.transform.position * bias +
			moveCamTo * (1.0f - bias);
		Camera.main.transform.LookAt(transform.position + transform.forward * 50.0f);
		
		transform.position += transform.forward * Time.deltaTime * speed;
		
		speed -= transform.forward.y * Time.deltaTime * 50.0f;
		
		if (Input.GetKey(KeyCode.W)) {
			speed += Time.deltaTime * 20.0f;
		}
		
		if (Input.GetKey(KeyCode.S)) {
			speed -= Time.deltaTime * 20.0f;
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
