﻿using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Yomiage.Core.Types;
using Yomiage.SDK.Common;
using Yomiage.SDK.Settings;

namespace Yomiage.GUI.Dialog.ViewModels
{
    class SettingsEngineViewModel : DialogViewModelBase
    {
        public override string Title => "Engine Settings";

        public Engine Engine { get; set; }

        public ReactiveCollection<ISetting> Settings { get; } = new ();

        public ReactivePropertySlim<string> Name { get; } = new ();
        public ReactivePropertySlim<string> Description { get; } = new ();
        public ReactivePropertySlim<string[]> LibraryFormat { get; } = new ();
        public ReactivePropertySlim<string> OS { get; } = new();
        public ReactivePropertySlim<string> Language { get; } = new();
        public ReactivePropertySlim<string> ActivationKey { get; } = new();
        public ReactivePropertySlim<bool> Activated { get; } = new ();
        public ReadOnlyReactivePropertySlim<bool> NotActivated { get; }
        public ReactivePropertySlim<string> LastState { get; } = new();
        public ReactivePropertySlim<int> MajorVersion { get; } = new();
        public ReactivePropertySlim<int> MinorVersion { get; } = new();

        public ReactivePropertySlim<EngineSettings> SettingsEdit { get; } = new();

        public ReactiveCommand OpenLicenseCommand { get; }
        public AsyncReactiveCommand<string> ActivationCommand { get; }
        public ReactiveCommand DefaultCommand { get; }
        public ReactiveCommand ApplyCommand { get; }
        public ReactiveCommand OpenFolderCommand { get; }

        private ReactiveTimer timer = new(new TimeSpan(0, 0, 1));

        private IDialogService dialogService;

        public SettingsEngineViewModel(IDialogService dialogService)
        {
            this.dialogService = dialogService;
            OpenLicenseCommand = new ReactiveCommand().WithSubscribe(OpenLicenseAction).AddTo(Disposables);
            ActivationCommand = new AsyncReactiveCommand<string>().WithSubscribe(ActivationAction).AddTo(Disposables);
            DefaultCommand = new ReactiveCommand().WithSubscribe(DefaultAction).AddTo(Disposables);
            ApplyCommand = new ReactiveCommand().WithSubscribe(ApplyAction).AddTo(Disposables);
            OpenFolderCommand = new ReactiveCommand().WithSubscribe(OpenFolderAction).AddTo(Disposables);
            NotActivated = Activated.Select(x => !x).ToReadOnlyReactivePropertySlim();
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.ContainsKey("Engine"))
            {
                Engine = parameters.GetValue<Engine>("Engine");
            }
            if(Engine == null)
            {
                CancelAction();
                return;
            }
            Initialize();
        }

        private void OpenFolderAction()
        {
            Process.Start("EXPLORER.EXE", this.Engine.ConfigDirectory);
        }

        private void Initialize()
        {
            var config = Engine.EngineConfig;
            Name.Value = config.Name;
            Description.Value = config.Description;
            LibraryFormat.Value = config.LibraryFormat;
            OS.Value = config.OS;
            Language.Value = config.Language;
            Activated.Value = Engine.VoiceEngine.IsActivated;
            MajorVersion.Value = Engine.VoiceEngine.MajorVersion;
            MinorVersion.Value = Engine.VoiceEngine.MinorVersion;
            timer.Subscribe(_ => LastState.Value = Engine.VoiceEngine.StateText);
            timer.Start();
            SetSettingList();
        }

        private void SetSettingList()
        {
            var list = new List<ISetting>();
            var settings = JsonUtil.DeepClone(Engine.VoiceEngine.Settings);
            SettingsEdit.Value = settings;
            settings?.Bools?
                .Where(s => !s.Hide)?
                .ToList()?
                .ForEach(s => list.Add(s));
            settings?.Ints?
                .Where(s => !s.Hide)?
                .ToList()?
                .ForEach(s => list.Add(s));
            settings?.Doubles?
                .Where(s => !s.Hide)?
                .ToList()?
                .ForEach(s => list.Add(s));
            settings?.Strings?
                .Where(s => !s.Hide)?
                .ToList()?
                .ForEach(s => list.Add(s));
            Settings.Clear();
            list.OrderBy(s => s.Order)
                .ToList()
                .ForEach(s => Settings.Add(s));
        }

        private void OpenLicenseAction()
        {
            var path = Path.Combine(Engine.ConfigDirectory, "license.md");
            if (File.Exists(path))
            {
                OpenText(path);
                return;
            }
            path = Path.Combine(Engine.ConfigDirectory, "license.txt");
            if (File.Exists(path))
            {
                OpenText(path);
            }
        }
        private void OpenText(string path)
        {
            IDialogParameters param = new DialogParameters();
            param.Add("FileName", path);
            this.dialogService.ShowDialog("TextDialog", param, _ => { });
        }
        private async Task ActivationAction(string param)
        {
            switch (param)
            {
                case "Activate":
                    await Engine.VoiceEngine.Activate(ActivationKey.Value);
                    break;
                case "DeActivate":
                    await Engine.VoiceEngine.DeActivate();
                    break;
            }
            Activated.Value = Engine.VoiceEngine.IsActivated;
        }
        private void DefaultAction()
        {
            if(MessageBox.Show("Are you sure you want to reset\nthe values back to defaults?", "Warning", MessageBoxButton.OKCancel) != MessageBoxResult.OK) { return; }
            foreach(var s in Settings)
            {
                if (s is BoolSetting sb) { sb.Value = sb.DefaultValue; }
                if (s is IntSetting si) { si.Value = si.DefaultValue; }
                if (s is DoubleSetting sd) { sd.Value = sd.DefaultValue; }
                if (s is StringSetting ss) { ss.Value = ss.DefaultValue; }
            }
            SetSettingList();
            ActivationKey.Value = string.Empty;
        }
        private void ApplyAction()
        {
            if (SettingsEdit.Value != null)
            {
                JsonUtil.Serialize(SettingsEdit.Value, Engine.SettingPath);
            }
            Engine.VoiceEngine.Settings = JsonUtil.DeepClone(SettingsEdit.Value);
        }
        protected override void OkAction()
        {
            ApplyAction();
            base.OkAction();
        }
    }
}
