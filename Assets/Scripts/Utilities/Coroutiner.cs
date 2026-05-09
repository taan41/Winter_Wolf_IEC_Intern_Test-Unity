using System.Collections;
using UnityEngine;

public class Coroutiner : MonoBehaviour
{
	public static Coroutiner Instance { get; private set; }

	void Awake()
	{
		if (Instance == null) Instance = this;
		else Destroy(this);
	}

	public Coroutine Run(IEnumerator coroutine)
	{
		return StartCoroutine(coroutine);
	}
}