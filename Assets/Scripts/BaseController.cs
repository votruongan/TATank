using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseController : MonoBehaviour {
        ////////////////////////////////////////////////FIXEDUPDATE IMPL////////////////////////////////////////////////////////
    //Holds actions received from another Thread. Will be coped to actionCopiedQueueFixedUpdateFunc then executed from there
    private static List<System.Action> actionQueuesFixedUpdateFunc = new List<Action>();

    //holds Actions copied from actionQueuesFixedUpdateFunc to be executed
    List<System.Action> actionCopiedQueueFixedUpdateFunc = new List<System.Action>();

    // Used to know if whe have new Action function to execute. This prevents the use of the lock keyword every frame
    private volatile static bool noActionQueueToExecuteFixedUpdateFunc = true;

    public int PendingActionCount(){
        return actionQueuesFixedUpdateFunc.Count;
    }

    public static void executeInFixedUpdate(System.Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException("action");
        }

        lock (actionQueuesFixedUpdateFunc)
        {
            actionQueuesFixedUpdateFunc.Add(action);
            noActionQueueToExecuteFixedUpdateFunc = false;
        }
    }

    public virtual void FixedUpdate()
    {
        if (noActionQueueToExecuteFixedUpdateFunc)
        {
            return;
        }

        //Clear the old actions from the actionCopiedQueueFixedUpdateFunc queue
        actionCopiedQueueFixedUpdateFunc.Clear();
        lock (actionQueuesFixedUpdateFunc)
        {
            //Copy actionQueuesFixedUpdateFunc to the actionCopiedQueueFixedUpdateFunc variable
            actionCopiedQueueFixedUpdateFunc.AddRange(actionQueuesFixedUpdateFunc);
            //Now clear the actionQueuesFixedUpdateFunc since we've done copying it
            actionQueuesFixedUpdateFunc.Clear();
            noActionQueueToExecuteFixedUpdateFunc = true;
        }

        // Loop and execute the functions from the actionCopiedQueueFixedUpdateFunc
        for (int i = 0; i < actionCopiedQueueFixedUpdateFunc.Count; i++)
        {
            actionCopiedQueueFixedUpdateFunc[i].Invoke();
        }
    }

}