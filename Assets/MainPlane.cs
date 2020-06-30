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

using AssemblyCSharp;

public class MainPlane : MonoBehaviour
{
	public float speed = 0.0f;
	public Camera FirstPersonCam;
	public bool cameraSwitch = false;
	public Canvas FirstPersonCanvas;
	Camera ThirdPersonCam;

	public static string apiKey = "9ce5f303a889540af8e432356913aac65f1d2eb9f79e07f2d48825e4eaecf2a5";
	public static string secretKey = "a00191f1588b190550a51814cfe35b1c957775c0114f2aa80c9f5749741e3a9c";
	public static string roomid = "426048572";
	public static string username;
	//Listener listen = new Listener();

	// Start is called before the first frame update
	void Start()
	{
		UnityEngine.Debug.Log("plane pilot script added to: " + gameObject.name);
		ThirdPersonCam = Camera.main;
		FirstPersonCanvas.gameObject.SetActive(false);
		ThirdPersonCam.enabled = true;
		FirstPersonCam.enabled = false;

		// Inicia comunicación con servidor
		/*WarpClient.initialize(apiKey, secretKey);
		WarpClient.GetInstance().AddConnectionRequestListener(listen);
		WarpClient.GetInstance().AddChatRequestListener(listen);
		WarpClient.GetInstance().AddUpdateRequestListener(listen);
		WarpClient.GetInstance().AddLobbyRequestListener(listen);
		WarpClient.GetInstance().AddNotificationListener(listen);
		WarpClient.GetInstance().AddRoomRequestListener(listen);
		WarpClient.GetInstance().AddZoneRequestListener(listen);
		WarpClient.GetInstance().AddTurnBasedRoomRequestListener(listen);
		username = System.DateTime.UtcNow.Ticks.ToString();
		WarpClient.GetInstance().Connect(username);*/
	}

	public float interval = 0.1f;
	float timer = 0;

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

		//Envío mensaje servidor
		timer -= Time.deltaTime;
		if (timer < 0)
		{
			string json = "{\"x\":\"" + Math.Round(transform.rotation.eulerAngles.x, 2) + "\",\"y\":\"" + Math.Round(transform.rotation.eulerAngles.y, 2) + "\",\"z\":\"" + Math.Round(transform.rotation.eulerAngles.z, 2) + "\"}";
			//listen.sendMsg(json);
			UnityEngine.Debug.Log(json);
			timer = interval;
		}
		//WarpClient.GetInstance().Update();
		//Termina envío servidor

		float terrainHeightWhereWeAre = Terrain.activeTerrain.SampleHeight(transform.position);
		if (terrainHeightWhereWeAre > transform.position.y)
		{
			transform.position = new Vector3(transform.position.x, terrainHeightWhereWeAre + 2, transform.position.z);
		}
	}
}
