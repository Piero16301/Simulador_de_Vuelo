using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.IO.Ports;
using System;

using com.shephertz.app42.gaming.multiplayer.client;
using com.shephertz.app42.gaming.multiplayer.client.events;
using com.shephertz.app42.gaming.multiplayer.client.listener;
using com.shephertz.app42.gaming.multiplayer.client.command;
using com.shephertz.app42.gaming.multiplayer.client.message;
using com.shephertz.app42.gaming.multiplayer.client.transformer;

//using AssemblyCSharp;

public class MainPlane : MonoBehaviour
{
	public float speed = 0.0f;
	public Camera FirstPersonCam;
	public bool cameraSwitch = false;
	public Canvas FirstPersonCanvas;
	Camera ThirdPersonCam;

	public static string apiKey = "1b424fc63486fcc47ad6974aef4d3bd877c93b878f645250244eb8dec91bcfa8";
	public static string secretKey = "cbc5b0c85a1f331878e0703abe76749859c8d7f798f284a914f26622874655d2";

	SerialPort arduinoPort = new SerialPort("COM8");

	private void Awake()
	{
		arduinoPort.BaudRate = 9600;
		arduinoPort.Parity = Parity.None;
		arduinoPort.StopBits = StopBits.One;
		arduinoPort.DataBits = 8;
		arduinoPort.Handshake = Handshake.None;
	}

	// Start is called before the first frame update
	void Start()
	{
		UnityEngine.Debug.Log("plane pilot script added to: " + gameObject.name);
		ThirdPersonCam = Camera.main;
		FirstPersonCanvas.gameObject.SetActive(false);
		ThirdPersonCam.enabled = true;
		FirstPersonCam.enabled = false;
	}

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

		transform.Rotate(0.15f * Input.GetAxis("Vertical"), 0.1f * Input.GetAxis("Direction"), 0.2f * -Input.GetAxis("Horizontal"));

		UnityEngine.Debug.Log("X:" + Math.Round(transform.rotation.eulerAngles.x, 2) + " Y:" + Math.Round(transform.rotation.eulerAngles.y, 2) + " Z:" + Math.Round(transform.rotation.eulerAngles.z, 2));
		
		//SendMessageToArduino("X:" + Math.Round(transform.rotation.eulerAngles.x, 2) + " Y:" + Math.Round(transform.rotation.eulerAngles.y, 2) + " Z:" + Math.Round(transform.rotation.eulerAngles.z, 2));

		float terrainHeightWhereWeAre = Terrain.activeTerrain.SampleHeight(transform.position);
		if (terrainHeightWhereWeAre > transform.position.y)
		{
			transform.position = new Vector3(transform.position.x, terrainHeightWhereWeAre + 2, transform.position.z);
		}
	}

	public void SendMessageToArduino(string message)
	{
		arduinoPort.WriteLine(message);
	}

	public void ClosePort()
	{
		arduinoPort.Close();
	}
}
