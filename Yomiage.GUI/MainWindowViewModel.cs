﻿using Microsoft.Win32;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Yomiage.Core.Models;
using Yomiage.Core.Types;
using Yomiage.GUI.Data;
using Yomiage.GUI.EventMessages;
using Yomiage.GUI.Graph;
using Yomiage.GUI.Models;
using Yomiage.GUI.Util;
using Yomiage.GUI.ViewModels;
using Yomiage.SDK;

namespace Yomiage.GUI
{
    class MainWindowViewModel : ViewModelBase
    {

        // ------  表示系  ------

        public ReactivePropertySlim<bool> PresetVisible { get; }

        public ReactivePropertySlim<bool> TuningVisible { get; }
        public ReactivePropertySlim<bool> CharacterVisible { get; }

        public ReactivePropertySlim<bool> IsCharacterMaximized { get; }
        public ReactivePropertySlim<bool> IsLineNumberVisible { get; }

        public ReadOnlyReactivePropertySlim<string> StatusText { get; }

        public ReactivePropertySlim<int> TunerSpan { get; } = new ReactivePropertySlim<int>(5);
        public ReactivePropertySlim<int> CharacterSpan { get; } = new ReactivePropertySlim<int>(1);

        /// <summary>
        /// テキストの行、列、文字数
        /// </summary>
        public ReactivePropertySlim<int> Row { get; } = new ReactivePropertySlim<int>(0);
        public ReactivePropertySlim<int> Col { get; } = new ReactivePropertySlim<int>(0);
        public ReactivePropertySlim<int> Num { get; } = new ReactivePropertySlim<int>(0);

        public ReactiveCommand InitializeLayoutCommand { get; }
        public ReactiveCommand InitializeSettingCommand { get; }

        public ReactiveCommand<string> ScriptCommand { get; }
        public ReactiveCommand<string> MasterCommand { get; }
        public ReactiveCommand<string> VoiceCommand { get; }
        public ReactiveCommand<string> MenuCommand { get; }
        public ReactiveCommand HelpCommand { get; }
        public ReactiveCommand LogCommand { get; }

        public ScriptService ScriptService { get; }
        public PhraseService PhraseService { get; }
        private readonly SettingService SettingService;
        private readonly LayoutService LayoutService;
        private readonly VoicePresetService VoicePresetService;
        // private readonly VoicePlayerService VoicePlayerService;
        // private readonly ConfigService ConfigService;
        readonly IMessageBroker messageBroker;
        readonly PauseDictionaryService pauseDictionaryService;
        private readonly VoiceEngineService voiceEngineService;


        public MainWindowViewModel(
            LayoutService layoutService,
            SettingService settingService,
            ScriptService scriptService,
            PhraseService phraseService,
            VoicePresetService voicePresetService,
            VoiceEngineService voiceEngineService,
            //ConfigService configService,
            //VoicePlayerService voicePlayerService,
            //ApiService apiService, // apiサーバを立てるために呼ぶ
            PauseDictionaryService pauseDictionaryService,
            IMessageBroker messageBroker,
            IDialogService dialogService) : base(dialogService)
        {
            if (Application.Current is PrismApplication app)
            {
                app.Container.Resolve<ApiService>();
            }
            this.SettingService = settingService;
            this.LayoutService = layoutService;
            this.ScriptService = scriptService;
            this.PhraseService = phraseService;
            this.VoicePresetService = voicePresetService;
            // this.VoicePlayerService = voicePlayerService;
            // this.ConfigService = configService;
            this.pauseDictionaryService = pauseDictionaryService;
            this.messageBroker = messageBroker;
            this.voiceEngineService = voiceEngineService;
            PhraseGraph.DialogService = dialogService;

            StatusText = Status.StatusText.ToReadOnlyReactivePropertySlim();

            TunerSpan = new ReactivePropertySlim<int>(5).AddTo(Disposables);
            CharacterSpan = new ReactivePropertySlim<int>(1).AddTo(Disposables);


            InitializeLayoutCommand = layoutService.InitializeCommand;
            PresetVisible = layoutService.PresetVisible;
            TuningVisible = layoutService.TuningVisible;
            CharacterVisible = layoutService.CharacterVisible;
            IsCharacterMaximized = layoutService.IsCharacterMaximized;
            IsCharacterMaximized.Subscribe(isChecked =>
            {
                if (isChecked)
                {
                    TunerSpan.Value = 3;
                    CharacterSpan.Value = 3;
                }
                else
                {
                    TunerSpan.Value = 5;
                    CharacterSpan.Value = 1;
                }
            }).AddTo(Disposables);
            IsLineNumberVisible = layoutService.IsLineNumberVisible;

            MasterCommand = new ReactiveCommand<string>().WithSubscribe(MasterAction).AddTo(Disposables);
            ScriptCommand = new ReactiveCommand<string>().WithSubscribe(ScriptAction).AddTo(Disposables);
            VoiceCommand = new ReactiveCommand<string>().WithSubscribe(VoiceAction).AddTo(Disposables);
            MenuCommand = new ReactiveCommand<string>().WithSubscribe(MenuAction).AddTo(Disposables);
            InitializeSettingCommand = new ReactiveCommand().WithSubscribe(InitializeSettingAction).AddTo(Disposables);
            HelpCommand = new ReactiveCommand().WithSubscribe(HelpAction).AddTo(Disposables);
            LogCommand = new ReactiveCommand().WithSubscribe(LogAction).AddTo(Disposables);

            MessageBroker.Default.Subscribe<TextCursorPosition>(value =>
            {
                this.Row.Value = value.Row;
                this.Col.Value = value.Col;
                this.Num.Value = value.Num;
            });
        }

        private void HelpAction()
        {
            ProcessStartInfo pi = new()
            {
                FileName = "https://sites.google.com/view/unicoe/%E3%83%9B%E3%83%BC%E3%83%A0",
                UseShellExecute = true,
            };
            Process.Start(pi);
        }

        private void LogAction()
        {
            Process.Start("EXPLORER.EXE", AppLog.LogDirectory);
        }

        private void MasterAction(string param)
        {
            switch (param)
            {
                case "Initialize":
                    this.SettingService.ResetMaster();
                    break;
                case "CreatePause":
                    this.pauseDictionaryService.Create();
                    break;
            }
        }

        private void ScriptAction(string param)
        {
            switch (param)
            {
                case "New":
                    this.ScriptService.AddNew();
                    break;
                case "Open":
                    List<(string, IVoiceEngine)> filters = new();
                    foreach(var engine in this.voiceEngineService.AllEngines)
                    {
                        var list = engine.VoiceEngine.FileConverter?.ImportFilterList;
                        if (list != null)
                        {
                            foreach(var filter in list)
                            {
                                if (filter.Count(c => c == '|') == 1 &&
                                    !filter.StartsWith("|") &&
                                    !filter.EndsWith("|"))
                                {
                                    filters.Add((filter, engine.VoiceEngine));
                                }
                            }
                        }
                    }
                    var filterString = "Text File| *.txt";
                    filters.ForEach(f => filterString += "|" + f.Item1);
                    filterString += "|All Files|*.*";
                    var ofd = new OpenFileDialog() { Filter = filterString };
                    if (ofd.ShowDialog() != true) { return; }
                    (string, IVoiceEngine) selectedFilter = (null, null);
                    if(ofd.FilterIndex >= 2 && ofd.FilterIndex < filters.Count + 2)
                    {
                        selectedFilter = filters[ofd.FilterIndex - 2];
                    }
                    this.ScriptService.AddOpen(ofd.FileName, selectedFilter);
                    break;
                case "Save":
                    this.ScriptService.ActiveScript.Value?.Save();
                    break;
                case "SaveAs":
                    this.ScriptService.ActiveScript.Value?.SaveAs();
                    break;
                case "Play":
                    this.ScriptService.ActiveScript.Value.PlayCommand.Execute();
                    break;
                case "SaveVoice":
                    this.ScriptService.ActiveScript.Value.SaveCommand.Execute();
                    break;
            }
        }

        private void VoiceAction(string param)
        {
            switch (param)
            {
                case "SelectPresetTab":
                    messageBroker.Publish(new ChangeTuningTab() { TabIndex = 2 });
                    break;
                case "Copy":
                    if (this.VoicePresetService.SelectedPreset.Value != null)
                    {
                        var result = MessageBox.Show(this.VoicePresetService.SelectedPreset.Value.Name + " のコピーを作成してよろしいですか？", "確認", MessageBoxButton.YesNo);
                        if (result == MessageBoxResult.Yes)
                        {
                            this.VoicePresetService.Copy(this.VoicePresetService.SelectedPreset.Value);
                            LayoutService.ShowTabCommand.Execute(TabType.UserTab);
                        }
                    }
                    break;
                case "Delete":
                    if (this.VoicePresetService.SelectedPreset.Value != null &&
                        this.VoicePresetService.SelectedPreset.Value.Type == PresetType.User)
                    {
                        var result = MessageBox.Show(this.VoicePresetService.SelectedPreset.Value.Name + " を削除してよろしいですか？", "確認", MessageBoxButton.YesNo);
                        if (result == MessageBoxResult.Yes)
                        {
                            this.VoicePresetService.Remove(this.VoicePresetService.SelectedPreset.Value);
                        }
                    }
                    break;
            }
        }

        private void MenuAction(string param)
        {
            switch (param)
            {
                case "SaveSettings":
                    {
                        var sfd = new SaveFileDialog()
                        {
                            Filter = "Setting File(*.ysettings)|*.ysettings",
                        };
                        if (sfd.ShowDialog() == true)
                        {
                            this.SettingService.Save(sfd.FileName);
                        }
                    }
                    break;
                case "LoadSettings":
                    {
                        var ofd = new OpenFileDialog()
                        {
                            Filter = "Settings File(*.ysettings)|*.ysettings",
                        };
                        if (ofd.ShowDialog() == true)
                        {
                            this.SettingService.Load(ofd.FileName);
                            LayoutService.InitializeCommand.Execute();
                        }
                    }
                    break;
            }
        }

        private void InitializeSettingAction()
        {
            var result = System.Windows.MessageBox.Show(
                "Contents to reset\n・Preferences\n・Project settings (some excluded)\n・Audio storage settings\n・Layout (excluding some parts)\n・Master Control\n\nContents not reset\n・Phrase, word, and symbol dictionary\n・Voice Presets\n・Engine, voice library",
                "Reset all settings.",
                System.Windows.MessageBoxButton.OKCancel);

            if (result == System.Windows.MessageBoxResult.OK)
            {
                SettingService.Reset();
                LayoutService.InitializeCommand.Execute();
            }
        }
    }


}
