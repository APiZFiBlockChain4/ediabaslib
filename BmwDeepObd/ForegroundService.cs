﻿using System;
using System.Reflection;
using Android.Support.V4.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.Content;

namespace BmwDeepObd
{
    [Android.App.Service(Label = "@string/app_name")]
    public class ForegroundService : Android.App.Service
    {
#if DEBUG
        static readonly string Tag = typeof(ForegroundService).FullName;
#endif
        public const int ServiceRunningNotificationId = 10000;
        public const string BroadcastMessageKey = "broadcast_message";
        public const string BroadcastStopComm = "stop_communication";
        public const string NotificationBroadcastAction = ActivityCommon.AppNameSpace + ".Notification.Action";
        public const string ActionBroadcastCommand = ActivityCommon.AppNameSpace + ".Action.Command";

        public const string ActionStartService = "ForegroundService.action.START_SERVICE";
        public const string ActionStopService = "ForegroundService.action.STOP_SERVICE";
        public const string ActionStopCommunication = "ForegroundService.action.STOP_COMM";
        public const string ActionMainActivity = "ForegroundService.action.MAIN_ACTIVITY";

        bool _isStarted;
        private ActivityCommon _activityCommon;

        public override void OnCreate()
        {
            base.OnCreate();
#if DEBUG
            Android.Util.Log.Info(Tag, "OnCreate: the service is initializing.");
#endif
            _activityCommon = new ActivityCommon(this, null, BroadcastReceived);
            _activityCommon.SetLock(ActivityCommon.LockType.Cpu);
            lock (ActivityCommon.GlobalLockObject)
            {
                EdiabasThread ediabasThread = ActivityCommon.EdiabasThread;
                if (ediabasThread != null)
                {
                    ediabasThread.ActiveContext = this;
                }
            }
        }

        public override Android.App.StartCommandResult OnStartCommand(Intent intent, Android.App.StartCommandFlags flags, int startId)
        {
            if (intent?.Action == null)
            {
                return Android.App.StartCommandResult.Sticky;
            }
            switch (intent.Action)
            {
                case ActionStartService:
                    if (_isStarted)
                    {
#if DEBUG
                        Android.Util.Log.Info(Tag, "OnStartCommand: The service is already running.");
#endif
                    }
                    else
                    {
#if DEBUG
                        Android.Util.Log.Info(Tag, "OnStartCommand: The service is starting.");
#endif
                        RegisterForegroundService();
                        _isStarted = true;
                    }
                    break;

                case ActionStopCommunication:
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, "OnStartCommand: Stop communication");
#endif
                    Intent startIntent = new Intent(this, typeof(ActivityMain));
                    startIntent.SetAction(ActionMainActivity);
                    startIntent.SetFlags(ActivityFlags.NewTask);
                    startIntent.PutExtra(ActivityMain.ExtraStopComm, true);
                    StartActivity(startIntent);

                    SendStopCommBroadcast();
                    StopEdiabasThread();
                    break;
                }

                case ActionStopService:
                {
#if DEBUG
                    Android.Util.Log.Info(Tag, "OnStartCommand: The service is stopping.");
#endif
                    SendStopCommBroadcast();
                    StopEdiabasThread();

                    StopForeground(true);
                    StopSelf();
                    _isStarted = false;
                    break;
                }
            }

            // This tells Android not to restart the service if it is killed to reclaim resources.
            return Android.App.StartCommandResult.Sticky;
        }

        public override IBinder OnBind(Intent intent)
        {
            // Return null because this is a pure started service. A hybrid service would return a binder.
            return null;
        }

        public override void OnDestroy()
        {
            // We need to shut things down.
            //Log.Info(Tag, "OnDestroy: The started service is shutting down.");

            // Remove the notification from the status bar.
            NotificationManagerCompat notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Cancel(ServiceRunningNotificationId);
            _activityCommon.SetLock(ActivityCommon.LockType.None);
            lock (ActivityCommon.GlobalLockObject)
            {
                EdiabasThread ediabasThread = ActivityCommon.EdiabasThread;
                if (ediabasThread != null)
                {
                    ediabasThread.ActiveContext = null;
                }
            }

            _activityCommon.Dispose();
            _activityCommon = null;
            _isStarted = false;
            base.OnDestroy();
        }

        private void RegisterForegroundService()
        {
            var notification = new NotificationCompat.Builder(this)
                .SetContentTitle(Resources.GetString(Resource.String.app_name))
                .SetContentText(Resources.GetString(Resource.String.service_notification))
                .SetSmallIcon(Resource.Drawable.ic_stat_obd)
                .SetContentIntent(BuildIntentToShowMainActivity())
                .SetOngoing(true)
                //.AddAction(BuildStopCommAction())
                .AddAction(BuildStopServiceAction())
                .Build();

            // Enlist this instance of the service as a foreground service
            StartForeground(ServiceRunningNotificationId, notification);
        }

        private void SendStopCommBroadcast()
        {
            Intent broadcastIntent = new Intent(NotificationBroadcastAction);
            broadcastIntent.PutExtra(BroadcastMessageKey, BroadcastStopComm);
            LocalBroadcastManager.GetInstance(this).SendBroadcast(broadcastIntent);
        }

        private void StopEdiabasThread()
        {
            lock (ActivityCommon.GlobalLockObject)
            {
                if (ActivityCommon.EdiabasThread != null)
                {
                    if (!ActivityCommon.EdiabasThread.ThreadStopping())
                    {
                        ActivityCommon.EdiabasThread.StopThread(true);
                        ActivityCommon.EdiabasThread.Dispose();
                        ActivityCommon.EdiabasThread = null;
                    }
                }
            }
        }

        /// <summary>
        /// Builds a PendingIntent that will display the main activity of the app. This is used when the 
        /// user taps on the notification; it will take them to the main activity of the app.
        /// </summary>
        /// <returns>The content intent.</returns>
        private Android.App.PendingIntent BuildIntentToShowMainActivity()
        {
            var notificationIntent = new Intent(this, typeof(ActivityMain));
            notificationIntent.SetAction(ActionMainActivity);
            //notificationIntent.SetFlags(ActivityFlags.SingleTop /*| ActivityFlags.ClearTask*/);
            notificationIntent.SetFlags(ActivityFlags.NewTask);
            notificationIntent.PutExtra(ActivityMain.ExtraStopComm, false);

            var pendingIntent = Android.App.PendingIntent.GetActivity(this, 0, notificationIntent, Android.App.PendingIntentFlags.UpdateCurrent);
            return pendingIntent;
        }

        /// <summary>
        /// Builds the Notification.Action that will allow the user to stop the service via the
        /// notification in the status bar
        /// </summary>
        /// <returns>The stop service action.</returns>
        private NotificationCompat.Action BuildStopServiceAction()
        {
            var stopServiceIntent = new Intent(this, GetType());
            stopServiceIntent.SetAction(ActionStopService);
            var stopServicePendingIntent = Android.App.PendingIntent.GetService(this, 0, stopServiceIntent, 0);

            var builder = new NotificationCompat.Action.Builder(Resource.Drawable.ic_stat_cancel,
                GetText(Resource.String.service_stop_comm),
                stopServicePendingIntent);
            return builder.Build();
        }

        /// <summary>
        /// Builds the Notification.Action that will allow the user to stop the service via the
        /// notification in the status bar
        /// </summary>
        /// <returns>The stop service action.</returns>
        // ReSharper disable once UnusedMember.Local
        private NotificationCompat.Action BuildStopCommAction()
        {
            var stopServiceIntent = new Intent(this, GetType());
            stopServiceIntent.SetAction(ActionStopCommunication);
            var stopServicePendingIntent = Android.App.PendingIntent.GetService(this, 0, stopServiceIntent, 0);

            var builder = new NotificationCompat.Action.Builder(Resource.Drawable.ic_stat_cancel,
                GetText(Resource.String.service_stop_comm_app),
                stopServicePendingIntent);
            return builder.Build();
        }

        private void BroadcastReceived(Context context, Intent intent)
        {
            if (intent == null)
            {
                return;
            }
            string action = intent.Action;
            switch (action)
            {
                case ActionBroadcastCommand:
                {
                    HandleActionBroadcast(intent);
                    HandleCustomBroadcast(context, intent);
                    break;
                }
            }
        }

        private void HandleActionBroadcast(Intent intent)
        {
            string request = intent.GetStringExtra("action");
            if (string.IsNullOrEmpty(request))
            {
                return;
            }
            string[] requestList = request.Split(':');
            if (requestList.Length < 1)
            {
                return;
            }
            if (string.Compare(requestList[0], "new_page", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (requestList.Length < 2)
                {
                    return;
                }
                JobReader.PageInfo pageInfoSel = null;
                foreach (JobReader.PageInfo pageInfo in ActivityCommon.JobReader.PageList)
                {
                    if (string.Compare(pageInfo.Name, requestList[1], StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        pageInfoSel = pageInfo;
                        break;
                    }
                }
                if (pageInfoSel == null)
                {
                    return;
                }
                if (!ActivityCommon.CommActive)
                {
                    return;
                }
                EdiabasThread ediabasThread = ActivityCommon.EdiabasThread;
                if (ediabasThread == null)
                {
                    return;
                }
                if (ediabasThread.JobPageInfo != pageInfoSel)
                {
                    ActivityCommon.EdiabasThread.CommActive = true;
                    ediabasThread.JobPageInfo = pageInfoSel;
                }
            }
        }

        private void HandleCustomBroadcast(Context context, Intent intent)
        {
            try
            {
                EdiabasThread ediabasThread = ActivityCommon.EdiabasThread;
                // ReSharper disable once UseNullPropagation
                if (ediabasThread == null)
                {
                    return;
                }
                JobReader.PageInfo pageInfo = ediabasThread.JobPageInfo;
                if (pageInfo.ClassObject == null)
                {
                    return;
                }
                Type pageType = pageInfo.ClassObject.GetType();
                MethodInfo broadcastReceived = pageType.GetMethod("BroadcastReceived", new[] { typeof(JobReader.PageInfo), typeof(Context), typeof(Intent) });
                if (broadcastReceived == null)
                {
                    return;
                }
                object[] args = { pageInfo, context, intent };
                broadcastReceived.Invoke(pageInfo.ClassObject, args);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
