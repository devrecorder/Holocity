﻿using System;
using System.Collections.Concurrent;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public ConcurrentQueue<Action> Tasks;
    public delegate void WorldGridTask(WorldGrid wGrid);

    private void Start()
    {
        Tasks = new ConcurrentQueue<Action>();
    }

    void Update ()
    {
        if (Tasks == null) Start();
		if(Tasks.Count != 0)
        {
            Action task;
            if(Tasks.TryDequeue(out task))
            {
                task.Invoke();
            }
        }
	}
}
