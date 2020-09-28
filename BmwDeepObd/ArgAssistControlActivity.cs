﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using EdiabasLib;

namespace BmwDeepObd
{
    [Android.App.Activity(Label = "@string/arg_assist_title",
        WindowSoftInputMode = SoftInput.StateAlwaysHidden,
        ConfigurationChanges = ActivityConfigChanges)]
    public class ArgAssistControlActivity : ArgAssistBaseActivity
    {
        public class InstanceData
        {
            public string Arguments { get; set; }
        }

        private InstanceData _instanceData = new InstanceData();
        protected bool _activityRecreated;

        private Spinner _spinnerArgument;
        private EdiabasToolActivity.ResultSelectListAdapter _spinnerArgumentAdapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (savedInstanceState != null)
            {
                _activityRecreated = true;
                _instanceData = GetInstanceState(savedInstanceState, _instanceData) as InstanceData;
            }

            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowCustomEnabled(true);
            SetContentView(Resource.Layout.arg_assist_control);

            InitBaseVariables();

            _spinnerArgument = FindViewById<Spinner>(Resource.Id.spinnerArgument);
            _spinnerArgumentAdapter = new EdiabasToolActivity.ResultSelectListAdapter(this);
            _spinnerArgument.Adapter = _spinnerArgumentAdapter;
            _spinnerArgument.SetOnTouchListener(this);

            if (!_activityRecreated && _instanceData != null)
            {
                _instanceData.Arguments = Intent.GetStringExtra(ExtraArguments);
            }

            _buttonApply.Click += (sender, args) =>
            {
                if (ArgsValid() && UpdateResult())
                {
                    Finish();
                }
            };

            _buttonExecute.Click += (sender, args) =>
            {
                if (ArgsValid() && UpdateResult(true))
                {
                    Finish();
                }
            };

            _radioButtonArgTypeArg.CheckedChange += (sender, args) =>
            {
                UpdateArgList();
            };

            _radioButtonArgTypeId.CheckedChange += (sender, args) =>
            {
                UpdateArgList();
            };

            UpdateDisplay();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            _instanceData.Arguments = GetArgString();
            StoreInstanceState(outState, _instanceData);
            base.OnSaveInstanceState(outState);
        }

        public override void OnBackPressed()
        {
            if (!StoreChangesRequest(accepted =>
            {
                if (accepted)
                {
                    UpdateResult();
                }

                base.OnBackPressed();
            }))
            {
                base.OnBackPressed();
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            HideKeyboard();
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    if (!StoreChangesRequest(accepted =>
                    {
                        if (accepted)
                        {
                            UpdateResult();
                        }

                        Finish();
                    }))
                    {
                        Finish();
                    }
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        private void UpdateDisplay()
        {
            if (_activityCommon == null)
            {
                return;
            }

            try
            {
                string selectArg = null;
                string argType = string.Empty;
                if (!string.IsNullOrEmpty(_instanceData.Arguments))
                {
                    string[] argArray = _instanceData.Arguments.Split(";");
                    if (argArray.Length > 0)
                    {
                        argType = argArray[0].Trim();
                    }

                    if (argArray.Length > 1)
                    {
                        selectArg = argArray[1].Trim();
                    }
                }

                switch (argType.ToUpperInvariant())
                {
                    case EdiabasToolActivity.ArgTypeID:
                        _radioButtonArgTypeId.Checked = true;
                        break;

                    default:
                        _radioButtonArgTypeArg.Checked = true;
                        break;
                }

                UpdateArgList(selectArg);
                UpdateButtonState();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void UpdateArgList(string selectArg = null)
        {
            try
            {
                bool argTypeId = _radioButtonArgTypeId.Checked;

                int selection = 0;
                int index = 0;
                _spinnerArgumentAdapter.Items.Clear();
                if (_serviceId >= 0)
                {
                    foreach (EdiabasToolActivity.SgFuncInfo funcInfo in _sgFuncInfoList.OrderBy(x => argTypeId ? x.Id : x.Arg))
                    {
                        if (funcInfo.ServiceList.Contains(_serviceId))
                        {
                            string name = argTypeId ? funcInfo.Id : funcInfo.Arg;
                            string info = funcInfo.InfoTrans ?? funcInfo.Info;
                            EdiabasToolActivity.ExtraInfo extraInfo = new EdiabasToolActivity.ExtraInfo(name, string.Empty, new List<string> { info });
                            if (selectArg != null)
                            {
                                if (string.Compare(name, selectArg, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    selection = index;
                                }
                            }
                            _spinnerArgumentAdapter.Items.Add(extraInfo);
                            index++;
                        }
                    }
                }

                _spinnerArgumentAdapter.NotifyDataSetChanged();
                _spinnerArgument.SetSelection(selection);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private bool StoreChangesRequest(AcceptDelegate handler)
        {
            if (!ArgsValid())
            {
                return false;
            }

            new AlertDialog.Builder(this)
                .SetPositiveButton(Resource.String.button_yes, (sender, args) =>
                {
                    handler(true);
                })
                .SetNegativeButton(Resource.String.button_no, (sender, args) =>
                {
                    handler(false);
                })
                .SetMessage(Resource.String.arg_assist_apply_args)
                .SetTitle(Resource.String.alert_title_question)
                .Show();

            return true;
        }

        private void UpdateButtonState()
        {
            bool enable = ArgsValid();
            _buttonApply.Enabled = enable;
            _buttonExecute.Enabled = enable && !_offline;
        }

        private bool ArgsValid()
        {
            try
            {
                int position = _spinnerArgument.SelectedItemPosition;
                if (position >= 0 && position < _spinnerArgumentAdapter.Items.Count)
                {
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string GetArgString()
        {
            try
            {
                string argType = EdiabasToolActivity.ArgTypeArg;
                if (_radioButtonArgTypeId.Checked)
                {
                    argType = EdiabasToolActivity.ArgTypeID;
                }

                StringBuilder sb = new StringBuilder();
                sb.Append(argType);

                int position = _spinnerArgument.SelectedItemPosition;
                if (position >= 0 && position < _spinnerArgumentAdapter.Items.Count)
                {
                    EdiabasToolActivity.ExtraInfo item = _spinnerArgumentAdapter.Items[position];
                    if (!string.IsNullOrEmpty(item.Name))
                    {
                        sb.Append(";");
                        sb.Append(item.Name);
                    }
                }

                return sb.ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private bool UpdateResult(bool execute = false)
        {
            try
            {
                Intent intent = new Intent();
                intent.PutExtra(ExtraArguments, GetArgString());
                intent.PutExtra(ExtraExecute, execute);
                SetResult(Android.App.Result.Ok, intent);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
