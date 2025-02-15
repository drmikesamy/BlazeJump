﻿namespace BlazeJump.Common.Services.Notification
{
	public class NotificationService : INotificationService
	{
		public event EventHandler UpdateState;
		private bool _loading { get; set; } = false;
		public bool Loading
		{
			get
			{
				return _loading;
			}
			set
			{
				if (_loading != value)
				{
					_loading = value;
					UpdateState?.Invoke(this, EventArgs.Empty);
				}
			}
		}
		public void UpdateTheState()
		{
#if ANDROID
			MainThread.BeginInvokeOnMainThread(() =>
			{
				UpdateState?.Invoke(this, EventArgs.Empty);
			});
#else
			UpdateState?.Invoke(this, EventArgs.Empty);
#endif

		}
	}
}