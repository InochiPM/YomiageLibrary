using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yomiage.GUI.Dialog.ViewModels
{
    class SettingShortcutViewModel : DialogViewModelBase
    {
        public override string Title => "Shortcuts";

        public ReactiveCollection<Item> Items { get; } = new ReactiveCollection<Item>();

        public SettingShortcutViewModel()
        {
            Items.Add(new Item() { Operation = "Play", Key = "F5", Target = "Text" });
            Items.Add(new Item() { Operation = "Stop", Key = "F6", Target = "Text" });

            Items.Add(new Item() { Operation = "Play Previous Line", Key = "Ctrl + Left", Target = "Text" });
            Items.Add(new Item() { Operation = "Play Next Line", Key = "Ctrl + Right", Target = "Text" });

            Items.Add(new Item() { Operation = "Save As", Key = "Ctrl + Shift + Alt + S", Target = "Text or Phrase Editor" });

            Items.Add(new Item() { Operation = "Stop or Play Phrases", Key = "Space", Target = "Phrase Editor" });

            Items.Add(new Item() { Operation = "Add Phrase", Key = "Shift + S", Target = "Phrase Editor" });
            Items.Add(new Item() { Operation = "Copy Phrase Editor Tab", Key = "Shift + C", Target = "Phrase Editor" });

            Items.Add(new Item() { Operation = "Show Innotation", Key = "1", Target = "Phrase Editor" });
            Items.Add(new Item() { Operation = "Show Parameter Screen 2", Key = "2", Target = "Phrase Editor" });
            Items.Add(new Item() { Operation = "Show Parameter Screen ３", Key = "3", Target = "Phrase Editor" });
            Items.Add(new Item() { Operation = "Show Parameter Screen ４", Key = "4", Target = "Phrase Editor" });
            Items.Add(new Item() { Operation = "Show Parameter Screen ５", Key = "5", Target = "Phrase Editor" });
            Items.Add(new Item() { Operation = "Show Parameter Screen ６", Key = "6", Target = "Phrase Editor" });
            Items.Add(new Item() { Operation = "Show Parameter Screen ７", Key = "7", Target = "Phrase Editor" });
            Items.Add(new Item() { Operation = "Show Parameter Screen ８", Key = "8", Target = "Phrase Editor" });
            Items.Add(new Item() { Operation = "Show Parameter Screen ９", Key = "9", Target = "Phrase Editor" });

            Items.Add(new Item() { Operation = "Change word end to ---", Key = "NumPad0", Target = "Phrase Editor" });
            Items.Add(new Item() { Operation = "Change to Normal", Key = "NumPad1", Target = "Phrase Editor" });
            Items.Add(new Item() { Operation = "Change to Happy", Key = "NumPad2", Target = "Phrase Editor" });
            Items.Add(new Item() { Operation = "Change to Question?", Key = "NumPad3", Target = "Phrase Editor" });
            Items.Add(new Item() { Operation = "Change to Exclamation!", Key = "NumPad4", Target = "Phrase Editor" });

            Items.Add(new Item() { Operation = "Switch the accent mark up or down", Key = "A", Target = "Phrase Editor (Phoneme)" });
            Items.Add(new Item() { Operation = "Switch to Silence", Key = "V", Target = "Phrase Editor (Phoneme)" });
            Items.Add(new Item() { Operation = "Merge or Split Accent Phrase", Key = "S", Target = "Phrase Editor (Phoneme)" });
            Items.Add(new Item() { Operation = "Remove Accent Phrase", Key = "Shift + D", Target = "Phrase Editor (Phoneme)" });
            Items.Add(new Item() { Operation = "Toggle in-text pause", Key = "P", Target = "Phrase Editor (Phoneme)" });
            Items.Add(new Item() { Operation = "Optional Pause Setting", Key = "Shift + P", Target = "Phrase Editor (Phoneme)" });
            Items.Add(new Item() { Operation = "read-write editing", Key = "Y", Target = "Phrase Editor (Phoneme)" });
            Items.Add(new Item() { Operation = "Increase the number of long tones (-)", Key = "M", Target = "Phrase Editor (Phoneme)" });
            Items.Add(new Item() { Operation = "Decrease the number of long tones (-)", Key = "Shift + M", Target = "Phrase Editor (Phoneme)" });
            Items.Add(new Item() { Operation = "Increase the number of stress sounds (-)", Key = "T", Target = "Phrase Editor (Phoneme)" });
            Items.Add(new Item() { Operation = "Decrease the number of stress sounds (-)", Key = "Shift + T", Target = "Phrase Editor (Phoneme)" });
            Items.Add(new Item() { Operation = "Toggle vowels and long vowels", Key = "B", Target = "Phrase Editor (Phoneme)" });
            Items.Add(new Item() { Operation = "Toggle normal and guttural", Key = "N", Target = "Phrase Editor (Phoneme)" });

        }

    }

    class Item
    {
        public string Operation { get; init; }
        public string Key { get; init; }
        public string Target { get; init; }
    }
}
