﻿using Prism.Ioc;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using Yomiage.GUI.ViewModels;
using Yomiage.SDK.Common;

namespace Yomiage.GUI.Models
{
    public class ScriptService : IDisposable
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        public void Dispose() => _disposables.Dispose();

        private readonly ObservableCollection<MainTextViewModel> scripts = new ObservableCollection<MainTextViewModel>();
        public ReadOnlyObservableCollection<MainTextViewModel> Scripts { get; }
        public ReactivePropertySlim<MainTextViewModel> ActiveScript { get; } = new ReactivePropertySlim<MainTextViewModel>();

        private IContainerExtension container;

        public ScriptService(IContainerExtension container)
        {
            this.container = container;
            Scripts = new ReadOnlyObservableCollection<MainTextViewModel>(scripts);
        }

        public void Add(MainTextViewModel script) => scripts.Add(script);
        public void AddNew()
        {
            var script = container.Resolve<MainTextViewModel>();
            Add(script);
            ActiveScript.Value = script;
        }
        public void AddWithFocus(MainTextViewModel script)
        {
            Add(script);
            ActiveScript.Value = script;
        }

        public void AddOpen(string filePath)
        {
            if (scripts.Any(s => s.FilePath.Value == filePath)) {
                ActiveScript.Value = scripts.First(s => s.FilePath.Value == filePath);
                return;
            }
            var script = container.Resolve<MainTextViewModel>();
            script.FilePath.Value = filePath;
            script.Title.Value = Path.GetFileNameWithoutExtension(filePath);
            try
            {
                script.Content.Value = File.ReadAllText(filePath);
            }
            catch
            {
                // ファイルの読み込みに失敗しました。
                return;
            }
            script.ResetDirty();
            this.AddWithFocus(script);

        }

        public void Remove(MainTextViewModel script)
        {
            if (scripts.Contains(script))
            {
                scripts.Remove(script);
            }
            if (scripts.Count == 0)
            {
                AddNew();
            }
        }

        public void SaveScripts()
        {
            var dict = scripts.Select(s =>
                new Dictionary<string, string>()
                {
                    {"Title" , s.Title.Value},
                    {"FilePath" , s.FilePath.Value},
                    {"IsDirty" , s.IsDirty.Value.ToString()},
                    {"IsActive" , (s == ActiveScript.Value).ToString()},
                    {"Content" , s.IsDirty.Value ? s.GetContent() : ""},
                });
            JsonUtil.Serialize(dict, "scripts.json");
        }
        public void LoadScripts()
        {
            if (!File.Exists("scripts.json")) { AddNew(); return; }
            try
            {
                var dict = JsonUtil.Deserialize<Dictionary<string,string>[]>("scripts.json");
                foreach(var s in dict)
                {
                    try
                    {
                        var script = container.Resolve<MainTextViewModel>();
                        script.Title.Value = s["Title"];
                        script.FilePath.Value = s["FilePath"];
                        var isDirty = (s["IsDirty"] == "True");
                        if (isDirty)
                        {
                            script.Content.Value = s["Content"];
                        }
                        else
                        {
                            if (File.Exists(script.FilePath.Value))
                            {
                                try
                                {
                                    script.Content.Value = File.ReadAllText(script.FilePath.Value);
                                }
                                catch
                                {
                                    continue;
                                }
                            }
                        }
                        script.IsDirty.Value = isDirty;
                        if (s["IsActive"] == "True")
                        {
                            this.AddWithFocus(script);
                        }
                        else
                        {
                            this.Add(script);
                        }
                    }
                    catch
                    {

                    }
                }
            }
            catch
            {

            }
            if(this.scripts.Count == 0) { AddNew(); }
        }
    }
}