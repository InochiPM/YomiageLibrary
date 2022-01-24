using Microsoft.WindowsAPICodePack.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Yomiage.GUI.Models;

namespace Yomiage.GUI.ViewModels
{
    class SettingSaveViewModel : ViewModelBase
    {
        public ReactiveCommand SelectFolderCommand { get; }
        public ReactiveCommand<string> TemplateCommand { get; }
        public ReactiveCommand<string> AddTemplateCommand { get; }

        public ReactivePropertySlim<string> Rule { get; }
        public ReactivePropertySlim<string> RuleFolderPath { get; }

        public SettingService SettingService { get; }
        public SettingSaveViewModel(SettingService settingService) : base()
        {
            this.SettingService = settingService;
            SelectFolderCommand = new ReactiveCommand().WithSubscribe(SelectFolderAction).AddTo(Disposables);
            TemplateCommand = new ReactiveCommand<string>().WithSubscribe(TemplateAction).AddTo(Disposables);
            AddTemplateCommand = new ReactiveCommand<string>().WithSubscribe(AddTemplateAction).AddTo(Disposables);
            this.Rule = new(settingService.Rule);
            this.Rule.Subscribe(v => settingService.Rule = v);
            this.RuleFolderPath = new(settingService.RuleFolderPath);
            this.RuleFolderPath.Subscribe(v => settingService.RuleFolderPath = v);
        }

        private void SelectFolderAction()
        {
            using var cofd = new CommonOpenFileDialog()
            {
                Title = "Select a folder to save the audio.",
                // フォルダ選択モードにする
                IsFolderPicker = true,
            };
            if (cofd.ShowDialog() != CommonFileDialogResult.Ok) { return; }
            RuleFolderPath.Value = cofd.FileName;
        }

        private void TemplateAction(string param)
        {
            if (!string.IsNullOrWhiteSpace(Rule.Value))
            {
                var result = MessageBox.Show("Write a new naming convention.", "Warning", MessageBoxButton.OKCancel);
                if(result != MessageBoxResult.OK) { return; }
            }
            switch (param)
            {
                case "Sequential number_input text":
                    Rule.Value = "{Number}_{Text}";
                    break;
                case "Date and time_sequential number_input text":
                    Rule.Value = "{yyMMdd_HHmmss}_{Number}_{Text}";
                    break;
                case "Date and time_sequential number_character name_input text":
                    Rule.Value = "{yyMMdd_HHmmss}_{Number}_{VoicePreset}_{Text}";
                    break;
                case "character name\\date_sequential number_input text":
                    Rule.Value = "{VoicePreset}\\{yyMMdd_HHmmss}_{Number}_{Text}";
                    break;
                case "Date and time\\sequential number_character name_input text":
                    Rule.Value = "{yyyyMMdd}\\{HHmmss}_{Number}_{VoicePreset}_{Text}";
                    break;
            }
        }

        private void AddTemplateAction(string param)
        {
            Rule.Value += "{" + param + "}";
        }
    }
}
