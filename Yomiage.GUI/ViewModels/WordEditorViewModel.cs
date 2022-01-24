﻿using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Yomiage.Core.Models;
using Yomiage.GUI.EventMessages;
using Yomiage.GUI.Models;
using Yomiage.SDK.Talk;

namespace Yomiage.GUI.ViewModels
{
    class WordEditorViewModel : ViewModelBase
    {
        public ReactivePropertySlim<string> OriginalText { get; } = new("");
        public ReactivePropertySlim<TalkScript> Phrase { get; } = new();
        public ReactivePropertySlim<string> Priority { get; } = new("3.標準");
        public ReactiveProperty<bool> CanRegister { get; } = new ReactiveProperty<bool>(false);
        public ReactiveProperty<bool> CanUnRegister { get; } = new ReactiveProperty<bool>(false);

        public ReactiveCommand<string> UpdateCommand { get; }
        public AsyncReactiveCommand PlayCommand { get; }
        public AsyncReactiveCommand StopCommand { get; }
        public ReactiveCommand RegisterCommand { get; }
        public ReactiveCommand UnRegisterCommand { get; }
        public ReactiveCommand ClearCommand { get; }

        VoicePlayerService voicePlayerService;
        WordDictionaryService wordDictionaryService;
        VoicePresetService voicePresetService;
        TextService textService;

        public WordEditorViewModel(
            VoicePlayerService voicePlayerService,
            WordDictionaryService wordDictionaryService,
            VoicePresetService voicePresetService,
            TextService textService,
            IMessageBroker messageBroker,
            IDialogService _dialogService
            ) : base(_dialogService)
        {
            this.voicePlayerService = voicePlayerService;
            this.wordDictionaryService = wordDictionaryService;
            this.voicePresetService = voicePresetService;
            this.textService = textService;

            UpdateCommand = new ReactiveCommand<string>().WithSubscribe(UpdateAction).AddTo(Disposables);
            PlayCommand = this.voicePlayerService.CanSave.ToAsyncReactiveCommand().WithSubscribe(PlayAction).AddTo(Disposables);
            StopCommand = new AsyncReactiveCommand().WithSubscribe(voicePlayerService.Stop).AddTo(Disposables);
            RegisterCommand = this.CanRegister.ToReactiveCommand().WithSubscribe(RegisterAction).AddTo(Disposables);
            UnRegisterCommand = this.CanUnRegister.ToReactiveCommand().WithSubscribe(UnRegisterAction).AddTo(Disposables);
            ClearCommand = new ReactiveCommand().WithSubscribe(ClearAction).AddTo(Disposables);
            OriginalText.Subscribe(t => { MakeScript(t); UpdateState(); }).AddTo(Disposables);
            Priority.Subscribe(_ => UpdateState()).AddTo(Disposables);

            messageBroker.Subscribe<EditWord>(word =>
            {
                OriginalText.Value = word.OriginalText;
                if(word.Phrase != null)
                {
                    Phrase.Value = word.Phrase;
                }
                Priority.Value = word.Priority;
            });
        }

        private void MakeScript(string text)
        {
            var scripts = this.textService.Parse(
                text,
                PromptStringEnable: false,
                WordDictionarys: this.wordDictionaryService.WordDictionarys);
            if(scripts.Length > 0)
            {
                this.Phrase.Value = scripts.First();
            }
            else
            {
                this.Phrase.Value = new TalkScript();
            }
        }

        private async Task PlayAction()
        {
            this.Phrase.Value.OriginalText = OriginalText.Value;
            await this.voicePlayerService.Play(this.Phrase.Value);
        }

        private void RegisterAction()
        {
            this.Phrase.Value.OriginalText = this.OriginalText.Value;
            this.wordDictionaryService.RegisterDictionary(
                this.Phrase.Value,
                this.Priority.Value
                );
            UpdateState();
        }

        private void UnRegisterAction()
        {
            var result = MessageBox.Show("Are you sure you want to remove\nthe word you are editing\nfrom the dictionary?", "Warning", MessageBoxButton.OKCancel);
            if (result != MessageBoxResult.OK) { return; }
            this.wordDictionaryService.UnRegiserDictionary(OriginalText.Value);
            UpdateState();
        }

        private void UpdateState()
        {
            if (string.IsNullOrWhiteSpace(OriginalText.Value))
            {
                CanRegister.Value = false;
                CanUnRegister.Value = false;
                return;
            }
            var registerd = this.wordDictionaryService.IsRegisterd(OriginalText.Value, Phrase.Value);
            CanRegister.Value = registerd != true;
            CanUnRegister.Value = registerd != false;
        }

        private void ClearAction()
        {
            this.OriginalText.Value = null;
            this.Phrase.Value = new TalkScript();
            this.Priority.Value = "3.Mid";
        }

        private void UpdateAction(string param)
        {
            UpdateState();
        }
    }
}
