using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PSS;
using static System.Diagnostics.Debug;
using static MainThread;
using System.Diagnostics;
using System.Collections.Concurrent;
using PSS.MultiThreading;
using System.Threading.Tasks;

public class RuntimeAsyncQueue : MonoBehaviour
{
    [Header("Settings")]
    public int MaxOperationsPerFrame = 20;

    [Header("Info")]
    public int itemsInQueue = 1;

    volatile private bool runningTasks = false;

    private void Start()
    {
        MainThread.LoadScript();
    }

    private void Update()
    {
        if (!runningTasks && MainThread.Queue.Count > 0)
        {
            StartCoroutine("RunQueueObjects");
        }
    }

    private IEnumerator RunQueueObjects()
    {
        runningTasks = true;

        while (MainThread.Queue.Count > 0)
        {
            itemsInQueue = MainThread.Queue.Count;
            for (int i = 0; i < MaxOperationsPerFrame; i++)
            {
                MainThread.RunNextOperation();
            }
            yield return null;
        }

        runningTasks = false;
    }
}

/// <summary>
/// The static Class that handles async functions that should be executed on the main thread
/// </summary>
public static class MainThread
{
    public static RuntimeAsyncQueue queueScript { get; private set; }

    /// <summary>
    /// The <see cref="MainThreadTask"/>s waiting to be ran on the main thread.
    /// </summary>
    public static ConcurrentQueue<MainThreadOperation> Queue { get; set; } = new ConcurrentQueue<MainThreadOperation>();

    private static Stopwatch Watch { get; set; } = new Stopwatch();

    /// <summary>
    /// A delagate representing any lamda task to be ran on the main thread.
    /// </summary>
    public delegate void MainThreadTask();

    /// <summary>
    /// Loads the script reference for the RuntimeWorker
    /// </summary>
    /// <returns></returns>
    public static bool LoadScript()
    {
        if (queueScript is null)
        {
            queueScript = GameObject.FindObjectOfType<RuntimeAsyncQueue>();
            if (queueScript is null)
            {
                return false;
            }
            return true;
        }
        else
        {
            return true;
        }
    }

    public static MainThreadOperation Add(MainThreadTask function)
    {
        if (LoadScript())
        {
            MainThreadOperation newOperation = new MainThreadOperation()
            {
                Status = TaskStatus.Created,
                function = function
            };
            Queue.Enqueue(newOperation);
            return newOperation;
        }
        return null;
    }

    /// <summary>
    /// Runs the first operation in the stack;
    /// </summary>
    public static void RunNextOperation()
    {
        // Make sure the watch is running
        if (!Watch.IsRunning)
        {
            Watch.Start();
        }

        if (Queue.TryDequeue(out MainThreadOperation operation))
        {
            if (operation is null)
            {
                return;
            }

            if (operation.Status != TaskStatus.Created)
            {
                return;
            }

            if (operation.function != null)
            {
                operation.function += () =>
                {
                    operation.Status = TaskStatus.RanToCompletion;
                    operation.TimeOperationFinished = Watch.ElapsedMilliseconds;
                };
                operation.TimeOperationRan = Watch.ElapsedMilliseconds;
                operation.Status = TaskStatus.Running;
                operation.function.Invoke();
            }
        }
    }

    [System.Serializable]
    public class MainThreadOperation : ITask
    {
        public MainThreadTask function { get; set; }
        public TaskStatus Status { get; set; }
        public long TimeOperationRan { get; set; } = 0;
        public long TimeOperationFinished { get; set; } = 0;
        public long TimeElapsedMilliseconds => TimeOperationFinished - TimeOperationRan;
    }
}