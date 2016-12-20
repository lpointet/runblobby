using UnityEngine;
using System.Collections.Generic;

/* Thanks to lordofduct
 * http://forum.unity3d.com/threads/looking-for-tips-about-implementation-of-mediator-design-pattern-in-c.299863/
 */

public class Command {}

public class TouchLeft : Command
{
	public int leftId;
	public Vector2 leftTouchPosition;
}
public class TouchRight : Command
{
	public int rightId;
	public Vector2 rightTouchPosition;
}
public class EndTouch : Command { public int fingerId; }
public class TouchClickable : Command {
	public Vector2 touchPosition;
	public int objectId;
}
public class TouchPlayer : Command { }
public class TouchWaterLevel2 : Command {
	public Vector2 touchPosition;
	public int objectId;
}

public delegate void MediatorCallback<T>(T c) where T : Command;

public class Mediator : MonoBehaviour
{
	public static Mediator current;
	private Dictionary<System.Type, System.Delegate> subscribers = new Dictionary<System.Type, System.Delegate>();

	public void Subscribe<T>(MediatorCallback<T> callback) where T : Command
	{
		if(callback == null) throw new System.ArgumentNullException("callback");
		var tp = typeof(T);
		if(subscribers.ContainsKey(tp))
			subscribers[tp] = System.Delegate.Combine(subscribers[tp], callback);
		else
			subscribers.Add(tp, callback);
	}

	public void DeleteSubscriber<T>(MediatorCallback<T> callback) where T : Command
	{
		if(callback == null) throw new System.ArgumentNullException("callback");
		var tp = typeof(T);
		if(subscribers.ContainsKey(tp))
		{
			var d = subscribers[tp];
			d = System.Delegate.Remove(d, callback);
			if(d == null) subscribers.Remove(tp);
			else subscribers[tp] = d;
		}
	}

	public void Publish<T>(T c) where T : Command
	{
		var tp = typeof(T);
		if(subscribers.ContainsKey(tp))
		{
			subscribers[tp].DynamicInvoke(c);
		}
	}

	void Awake () {
		current = this;
	}
}