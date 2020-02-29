﻿using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Views;

namespace BmwDeepObd
{
    public class BaseActivity : AppCompatActivity
    {
        public class InstanceDataBase
        {
            public bool ActionBarVisibilitySet
            {
                get => _actionBarVisibilitySet;
                set => _actionBarVisibilitySet = value;
            }

            public bool ActionBarVisible
            {
                get => _actionBarVisible;
                set
                {
                    _actionBarVisible = value;
                    _actionBarVisibilitySet = true;
                }
            }

            private bool _actionBarVisibilitySet;
            private bool _actionBarVisible;
        }

        public const string InstanceDataKeyDefault = "InstanceData";
        public const string InstanceDataKeyBase = "InstanceDataBase";
        protected InstanceDataBase _instanceDataBase = new InstanceDataBase();
        private GestureDetectorCompat _gestureDetector;
        protected bool _allowTitleHiding = true;
        protected bool _touchShowTitle = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (savedInstanceState != null)
            {
                _instanceDataBase = GetInstanceState(savedInstanceState, _instanceDataBase, InstanceDataKeyBase) as InstanceDataBase;
            }

            ResetTitle();

            GestureListener gestureListener = new GestureListener(this);
            _gestureDetector = new GestureDetectorCompat(this, gestureListener);

            if (_instanceDataBase != null)
            {
                if (_instanceDataBase.ActionBarVisibilitySet)
                {
                    if (_instanceDataBase.ActionBarVisible)
                    {
                        SupportActionBar.Show();
                    }
                    else
                    {
                        SupportActionBar.Hide();
                    }
                }
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            StoreInstanceState(outState, _instanceDataBase, InstanceDataKeyBase);
            base.OnSaveInstanceState(outState);
        }

        protected override void OnStart()
        {
            base.OnStart();

            if (!_instanceDataBase.ActionBarVisibilitySet)
            {
                _instanceDataBase.ActionBarVisible = true;
                if (ActivityCommon.SuppressTitleBar)
                {
                    if (SupportActionBar.CustomView == null && _allowTitleHiding)
                    {
                        SupportActionBar.Hide();
                        _instanceDataBase.ActionBarVisible = false;
                    }
                }
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (!ActivityCommon.AutoHideTitleBar && !ActivityCommon.SuppressTitleBar)
            {
                SupportActionBar.Show();
                _instanceDataBase.ActionBarVisible = true;
            }
        }

        public override bool DispatchTouchEvent(MotionEvent ev)
        {
            _gestureDetector.OnTouchEvent(ev);
            return base.DispatchTouchEvent(ev);
        }

        protected override void AttachBaseContext(Context @base)
        {
            base.AttachBaseContext(SetLocale(@base, ActivityMain.GetLocaleSetting()));
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            SetLocale(this, ActivityMain.GetLocaleSetting());
        }

        public void ResetTitle()
        {
            try
            {
                int? label = PackageManager?.GetActivityInfo(ComponentName, PackageInfoFlags.MetaData)?.LabelRes;
                if (label.HasValue && label != 0)
                {
                    SetTitle(label.Value);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public static Context SetLocale(Context context, string language)
        {
            try
            {
                Java.Util.Locale locale = null;
                if (string.IsNullOrEmpty(language))
                {
                    Android.Support.V4.OS.LocaleListCompat localeList =
                        Android.Support.V4.OS.ConfigurationCompat.GetLocales(Resources.System.Configuration);
                    if (localeList != null && localeList.Size() > 0)
                    {
                        locale = localeList.Get(0);
                    }
                }

                if (locale == null)
                {
                    locale = new Java.Util.Locale(!string.IsNullOrEmpty(language) ? language : "en");
                }

                Java.Util.Locale.Default = locale;

                Resources resources = context.Resources;
                Configuration configuration = resources.Configuration;
                if (Build.VERSION.SdkInt < BuildVersionCodes.JellyBeanMr1)
                {
                    configuration.Locale = locale;
                }
                else
                {
                    configuration.SetLocale(locale);
                }

                if (Build.VERSION.SdkInt < BuildVersionCodes.JellyBeanMr1)
                {
#pragma warning disable 618
                    resources.UpdateConfiguration(configuration, resources.DisplayMetrics);
#pragma warning restore 618
                    return context;
                }

                return context.CreateConfigurationContext(configuration);
            }
            catch (Exception)
            {
                return context;
            }
        }

        public static object GetInstanceState(Bundle savedInstanceState, object lastInstanceData, string key = InstanceDataKeyDefault)
        {
            if (savedInstanceState != null)
            {
                try
                {
                    string xml = savedInstanceState.GetString(key, string.Empty);
                    if (!string.IsNullOrEmpty(xml))
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(lastInstanceData.GetType());
                        using (StringReader sr = new StringReader(xml))
                        {
                            object instanceData = xmlSerializer.Deserialize(sr);
                            if (instanceData.GetType() == lastInstanceData.GetType())
                            {
                                return instanceData;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            return lastInstanceData;
        }

        public static bool StoreInstanceState(Bundle outState, object instanceData, string key = InstanceDataKeyDefault)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(instanceData.GetType());
                using (StringWriter sw = new StringWriter())
                {
                    using (XmlWriter writer = XmlWriter.Create(sw))
                    {
                        xmlSerializer.Serialize(writer, instanceData);
                        string xml = sw.ToString();
                        outState.PutString(key, xml);
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
            return false;
        }

        private class GestureListener : GestureDetector.SimpleOnGestureListener
        {
            private const int flingMinDiff = 100;
            private const int flingMinVel = 100;
            private readonly BaseActivity _activity;
            private readonly View _contentView;
            private readonly int _topBorder;

            public GestureListener(BaseActivity activity)
            {
                _activity = activity;
                _contentView = _activity?.FindViewById<View>(Android.Resource.Id.Content);
                _topBorder = 200;
                if (activity != null)
                {
                    float yDpi = activity.Resources.DisplayMetrics.Ydpi;
                    _topBorder = (int)yDpi / 2;
                }
            }

            public override bool OnDown(MotionEvent e)
            {
                return true;
            }

            public override void OnLongPress(MotionEvent e)
            {
                base.OnLongPress(e);

                if (!ActivityCommon.AutoHideTitleBar && !ActivityCommon.SuppressTitleBar)
                {
                    return;
                }

                if (_activity._touchShowTitle && !_activity.SupportActionBar.IsShowing)
                {
                    _activity.SupportActionBar.Show();
                    _activity._instanceDataBase.ActionBarVisible = true;
                }
            }

            public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
            {
                if (!ActivityCommon.AutoHideTitleBar && !ActivityCommon.SuppressTitleBar)
                {
                    return true;
                }

                if (_contentView != null && e1 != null && e2 != null)
                {
                    int[] location = new int[2];
                    _contentView.GetLocationOnScreen(location);
                    int top = location[1];
                    float y1 = e1.RawY - top;
                    float y2 = e2.RawY - top;

                    if (y1 < _topBorder || y2 < _topBorder)
                    {
                        float diffX = e2.RawX - e1.RawX;
                        float diffY = e2.RawY - e1.RawY;
                        if (Math.Abs(diffX) < Math.Abs(diffY))
                        {
                            if (Math.Abs(diffY) > flingMinDiff && Math.Abs(velocityY) > flingMinVel)
                            {
                                if (diffY > 0)
                                {
                                    _activity.SupportActionBar.Show();
                                    _activity._instanceDataBase.ActionBarVisible = true;
                                }
                                else
                                {
                                    _activity.SupportActionBar.Hide();
                                    _activity._instanceDataBase.ActionBarVisible = false;
                                }
                            }
                        }
                    }
                }

                return true;
            }
        }
    }
}
