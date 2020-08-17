using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using System.Globalization;

public class MainPlane : MonoBehaviour
{
	public float speed = 0.0f;
	public Camera FirstPersonCam;
	public bool cameraSwitch = false;
	public Canvas FirstPersonCanvas;
	Camera ThirdPersonCam;

	// URL para el POST y GET
	//readonly string getURL = "http://ec2-3-17-189-111.us-east-2.compute.amazonaws.com/GETSimulador.php";
	readonly string postURL = "http://ec2-3-17-189-111.us-east-2.compute.amazonaws.com/POSTSimulador.php";

	// Start is called before the first frame update
	void Start()
	{
		UnityEngine.Debug.Log("plane pilot script added to: " + gameObject.name);
		ThirdPersonCam = Camera.main;
		FirstPersonCanvas.gameObject.SetActive(false);
		ThirdPersonCam.enabled = true;
		FirstPersonCam.enabled = false;
	}

	float elapsed = 0f;

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.V))
		{
			if (FirstPersonCam.enabled)
			{
				ThirdPersonCam.enabled = true;
				FirstPersonCam.enabled = false;
				FirstPersonCanvas.gameObject.SetActive(false);
			}
			else if (!FirstPersonCam.enabled)
			{
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

		if (Input.GetAxis("Speed") != 0.0f)
		{
			speed += Input.GetAxis("Speed") * Time.deltaTime * 20.0f;
		}

		if (speed < 0.0f) {
			speed = 0.0f;
		}

		if (speed > 200.0f)
		{
			speed = 200.0f;
		}

		transform.Rotate(0.2f * Input.GetAxis("Vertical"), 0.2f * Input.GetAxis("Direction"), 0.2f * -Input.GetAxis("Horizontal"));

		//Envío mensaje servidor
		elapsed += Time.deltaTime;
		if (elapsed >= 1f)
		{
			// Se hace el parseo del JSON con los angulos en este momento de tiempo
			//string json = "{\"x\":\"" + Math.Round(transform.rotation.eulerAngles.x, 2) + "\",\"y\":\"" + Math.Round(transform.rotation.eulerAngles.y, 2) + "\",\"z\":\"" + Math.Round(transform.rotation.eulerAngles.z, 2) + "\"}";

			double xValue = Math.Round(transform.rotation.eulerAngles.x, 2);
			double yValue = Math.Round(transform.rotation.eulerAngles.y, 2);
			double zValue = Math.Round(transform.rotation.eulerAngles.z, 2);

			string xAngle = xValue.ToString("G", CultureInfo.InvariantCulture);
			string yAngle = yValue.ToString("G", CultureInfo.InvariantCulture);
			string zAngle = zValue.ToString("G", CultureInfo.InvariantCulture);

			StartCoroutine(SimplePostRequest(xAngle, yAngle, zAngle));

			UnityEngine.Debug.Log("x:" + xAngle + " y:" + yAngle + " z:" + zAngle);
			elapsed %= 1f;
		}

		float terrainHeightWhereWeAre = Terrain.activeTerrain.SampleHeight(transform.position);
		if (terrainHeightWhereWeAre > transform.position.y)
		{
			transform.position = new Vector3(transform.position.x, terrainHeightWhereWeAre + 2, transform.position.z);
		}
	}

	IEnumerator SimplePostRequest(string xAngle, string yAngle, string zAngle)
	{
		List<IMultipartFormSection> wwwForm = new List<IMultipartFormSection>();
		wwwForm.Add(new MultipartFormDataSection("xValue", xAngle));
		wwwForm.Add(new MultipartFormDataSection("yValue", yAngle));
		wwwForm.Add(new MultipartFormDataSection("zValue", zAngle));

		UnityWebRequest www = UnityWebRequest.Post(postURL, wwwForm);

		yield return www.SendWebRequest();

		if (www.isNetworkError || www.isHttpError)
		{
			UnityEngine.Debug.Log(www.error);
		}
	}
}
