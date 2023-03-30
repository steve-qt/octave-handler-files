using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunctionTimer{

	private Action action;
	private float timer;

	public FunctionTimer(Action action, float timer) {
		this.action = action;
		this.timer = timer;
	}

	public void Update(){
		//if (isDestroyed) return;
		//Debug.Log("timer: " + timer);
		timer -= Time.deltaTime;
		if (timer < 0)
		{
			action();
			timer += 10;
		}
	}

}