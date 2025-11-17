using System;
using System.ComponentModel;
using System.Globalization;

namespace IndustrySystem.Presentation.Wpf.Resources;

public class LocalizationProvider : INotifyPropertyChanged
{
 // Instance used by code-behind when needed
 public static LocalizationProvider Instance { get; } = new LocalizationProvider();

 public CultureInfo CurrentCulture { get; private set; } = CultureInfo.CurrentUICulture;

 public string this[string key]
 {
 get
 {
 var s = Strings.ResourceManager.GetString(key, Strings.Culture);
 return s ?? $"[{key}]";
 }
 }

 public void SetCulture(string cultureName)
 {
 var ci = CultureInfo.GetCultureInfo(cultureName);
 Strings.Culture = ci; // switch .resx culture
 CurrentCulture = ci;
 OnPropertyChanged("Item[]"); // notify indexer change
 }

 public event PropertyChangedEventHandler? PropertyChanged;
 private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
