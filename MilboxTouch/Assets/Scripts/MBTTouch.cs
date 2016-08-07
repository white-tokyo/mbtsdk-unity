using UnityEngine;

public class MBTTouch
{
	public	static int touchCount
	{
		get;
		private set;
	}

	public	static Vector2 position
	{
		get;
		private set;
	}

	public	static Vector2 deltaPosition
	{
		get;
		private set;
	}

	public	static TouchPhase phase
	{
		get;
		private set;
	}


	private	static int		_nowFrameCount	= -1;

#if UNITY_EDITOR
	private	static Vector2	_prePos = Vector2.zero;
#else

#endif


	public static void Update ()
	{
		if (_nowFrameCount==Time.frameCount)
		{
			return;
		}

		_nowFrameCount = Time.frameCount;

#if UNITY_EDITOR

		if (Input.GetMouseButton(0))
		{
			touchCount	= 1;
			position	= Input.mousePosition;
			
			if (Input.GetMouseButtonDown(0))
			{
				phase			= TouchPhase.Began;
				deltaPosition	= Vector2.zero;
			}
			else
			{
				phase			= TouchPhase.Moved;
				deltaPosition	= _prePos - position;
			}
			_prePos		= position;
		}
		else if (Input.GetMouseButtonUp(0))
		{
			touchCount	= 1;
			phase		= TouchPhase.Ended;
			position	= Input.mousePosition;
			deltaPosition= _prePos - position;
		}
		else
		{
			touchCount	= 0;
			phase		= TouchPhase.Ended;
		}

#else

		touchCount = Input.touchCount;

		if( 0<touchCount )
		{
		Touch nowTouch	= Input.GetTouch(0);

		phase			= nowTouch.phase;
		position		= nowTouch.position;
		deltaPosition	= nowTouch.deltaPosition;
		}
		else
		{
		phase			= TouchPhase.Ended;
		}
#endif
	}
}
