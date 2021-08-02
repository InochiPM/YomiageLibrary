﻿using AvalonDock.Layout.Serialization;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using Reactive.Bindings;
using Reactive.Bindings.Notifiers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Yomiage.Core.Models;
using Yomiage.GUI.EventMessages;
using Yomiage.GUI.Models;

namespace Yomiage.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private double PresetWidth = Settings.Default.PresetWidth;
        private double CharacterWidth = Settings.Default.CharacterWidth;
        private double TuningHeight = Settings.Default.TuningHeight;

        private SettingService SettingService;
        private ScriptService ScriptService;
        private PhraseService PhraseService;
        private ConfigService ConfigService;
        private VoiceEngineService voiceEngineService;
        private VoiceLibraryService voiceLibraryService;
        private VoicePresetService voicePresetService;
        private VoicePlayerService voicePlayerService;
        private PhraseDictionaryService phraseDictionaryService;
        private IMessageBroker messageBroker;

        public MainWindow(
            LayoutService layoutService,
            SettingService settingService,
            ScriptService scriptService,
            PhraseService phraseService,
            PhraseDictionaryService phraseDictionaryService,
            ConfigService configService,
            VoiceEngineService voiceEngineService,
            VoiceLibraryService voiceLibraryService,
            VoicePresetService voicePresetService,
            VoicePlayerService voicePlayerService,
            IMessageBroker messageBroker)
        {
            this.SettingService = settingService;
            this.ScriptService = scriptService;
            this.PhraseService = phraseService;
            this.phraseDictionaryService = phraseDictionaryService;
            this.ConfigService = configService;
            this.voiceEngineService = voiceEngineService;
            this.voiceLibraryService = voiceLibraryService;
            this.voicePresetService = voicePresetService;
            this.voicePlayerService = voicePlayerService;
            this.messageBroker = messageBroker;
            InitializeComponent();
            RecoverWindowBounds();

            layoutService.PresetVisible.Subscribe(visible =>
            {
                if (visible)
                {
                    this.Column1.Width = new GridLength(PresetWidth);
                    this.Column2.Width = new GridLength(4);
                }
                else
                {
                    PresetWidth = this.Column1.Width.Value;
                    this.Column1.Width = new GridLength(0);
                    this.Column2.Width = new GridLength(0);
                }
            });

            layoutService.CharacterVisible.Subscribe(visible =>
            {
                if (visible)
                {
                    this.Column4.Width = new GridLength(4);
                    this.Column5.Width = new GridLength(CharacterWidth);
                }
                else
                {
                    CharacterWidth = this.Column5.Width.Value;
                    this.Column4.Width = new GridLength(0);
                    this.Column5.Width = new GridLength(0);
                }
            });

            layoutService.TuningVisible.Subscribe(visible =>
            {
                if (visible)
                {
                    this.Row2.Height = new GridLength(4);
                    this.Row3.Height = new GridLength(TuningHeight);
                }
                else
                {
                    TuningHeight = this.Row3.Height.Value;
                    this.Row2.Height = new GridLength(0);
                    this.Row3.Height = new GridLength(0);
                }
            });

            layoutService.InitializeCommand.Subscribe(_ =>
            {
                layoutService.PresetVisible.Value = true;
                layoutService.CharacterVisible.Value = true;
                layoutService.TuningVisible.Value = true;
                layoutService.IsCharacterMaximized.Value = false;
                layoutService.IsLineNumberVisible.Value = false;

                this.Column1.Width = new GridLength(290);
                this.Column2.Width = new GridLength(4);
                this.Column4.Width = new GridLength(4);
                this.Column5.Width = new GridLength(190);
                this.Row2.Height = new GridLength(4);
                this.Row3.Height = new GridLength(330);

                this.Width = 1010;
                this.Height = 700;
                this.Left = 100;
                this.Top = 100;
                this.WindowState = WindowState.Normal;
            });

            layoutService.ShowTabCommand.Subscribe(tab =>
            {
                switch (tab)
                {
                    case TabType.UserTab:
                        PresetDocking.ActiveContent = UserTab;
                        break;
                }

            });


            if (voicePresetService.UserPreset.Contains(voicePresetService.SelectedPreset.Value))
            {
                PresetDocking.ActiveContent = this.UserTab;
            }
            if (voicePresetService.ExternalPreset.Contains(voicePresetService.SelectedPreset.Value))
            {
                // 最後に選択されていたのが、外部トークソフトの場合は外部ソフトのタブを初期表示にする。
                // PresetDocking.ActiveContent = this.ExternalTab;
            }

            this.TuningDocking.ActiveContent =
            settingService.TuneTabIndex switch
            {
                2 => TuneTab2,
                3 => TuneTab3,
                4 => TuneTab4,
                _ => TuneTab1,
            };

            messageBroker.Subscribe<ChangeTuningTab>(message =>
            {
                this.TuningDocking.ActiveContent =
                message.TabIndex switch
                {
                    2 => TuneTab2,
                    3 => TuneTab3,
                    4 => TuneTab4,
                    _ => TuneTab1,
                };
            });
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.PresetWidth = this.Column1.Width.Value;
            Settings.Default.CharacterWidth = this.Column5.Width.Value;
            Settings.Default.TuningHeight = this.Row3.Height.Value;
            Settings.Default.Save();
            SaveWindowBounds();
            SettingService.SaveMaster();
        }


        void SaveWindowBounds()
        {
            var settings = Settings.Default;
            settings.WindowMaximized = WindowState == WindowState.Maximized;
            settings.WindowLeft = Left;
            settings.WindowTop = Top;
            settings.WindowWidth = Width;
            settings.WindowHeight = Height;
            settings.Save();
        }

        /// <summary>
        /// ウィンドウの位置・サイズを復元します。
        /// </summary>
        void RecoverWindowBounds()
        {
            var settings = Settings.Default;
            // 左
            if (settings.WindowLeft >= 0 &&
                (settings.WindowLeft + settings.WindowWidth) < SystemParameters.VirtualScreenWidth)
            { Left = settings.WindowLeft; }
            // 上
            if (settings.WindowTop >= 0 &&
                (settings.WindowTop + settings.WindowHeight) < SystemParameters.VirtualScreenHeight)
            { Top = settings.WindowTop; }
            // 幅
            if (settings.WindowWidth > 0 &&
                settings.WindowWidth <= SystemParameters.WorkArea.Width)
            { Width = settings.WindowWidth; }
            // 高さ
            if (settings.WindowHeight > 0 &&
                settings.WindowHeight <= SystemParameters.WorkArea.Height)
            { Height = settings.WindowHeight; }
            // 最大化
            if (settings.WindowMaximized)
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowState = WindowState.Normal;
            }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.ScriptService.LoadScripts();
            this.PhraseService.LoadEditors();

            var splash = Application.Current.Properties["Splash"];
            if(splash is Splash window)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        this.ConfigService.Init(window.SetProgress);
                    }
                    catch(Exception e)
                    {

                    }
                    await Task.Delay(1000);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        window.Close();
                    });
                });
            }
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            this.ScriptService.SaveScripts();
            this.PhraseService.SaveEditors();
            foreach(var p in this.voicePresetService.AllPresets)
            {
                p.ResetEffect();
            }
            this.ConfigService.SavePresets();
            this.voiceEngineService.Dispose();
            this.voiceLibraryService.Dispose();
            this.voicePlayerService.Stop();
        }

        private void TuningDocking_ActiveContentChanged(object sender, EventArgs e)
        {
            if (this.TuningDocking.ActiveContent == this.TuneTab1) { SettingService.TuneTabIndex = 1; }
            if (this.TuningDocking.ActiveContent == this.TuneTab2) { SettingService.TuneTabIndex = 2; }
            if (this.TuningDocking.ActiveContent == this.TuneTab3) { SettingService.TuneTabIndex = 3; }
            if (this.TuningDocking.ActiveContent == this.TuneTab4) { SettingService.TuneTabIndex = 4; }
        }

        private void MetroWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.messageBroker.Publish(new Wakeup());
        }
    }
}