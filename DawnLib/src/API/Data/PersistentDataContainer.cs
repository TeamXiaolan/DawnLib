using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Dawn.Internal;
using Newtonsoft.Json;

namespace Dawn;

public class PersistentDataContainer : DataContainer
{
    private string _filePath;
    private bool _autoSave = true;

    internal static List<PersistentDataContainer> HasCorruptedData { get; private set; } = [];
    
    public class EditContext : IDisposable
    {
        private PersistentDataContainer _container;

        public EditContext(PersistentDataContainer container)
        {
            _container = container;
            _container._autoSave = false;
        }

        public void Dispose()
        {
            _container._autoSave = true;
            Task.Run(_container.SaveAsync);
        }
    }

    public PersistentDataContainer(string filePath)
    {
        Debuggers.PersistentDataContainer?.Log($"new PersistentDataContainer: {Path.GetFileName(filePath)}");
        _filePath = filePath;
        if (File.Exists(filePath))
        {
            Debuggers.PersistentDataContainer?.Log("loading existing file");
            try
            {
                dictionary = JsonConvert.DeserializeObject<Dictionary<NamespacedKey, object>>(File.ReadAllText(_filePath), DawnLib.JSONSettings);
            }
            catch (Exception exception)
            {
                DawnPlugin.Logger.LogFatal($"Exception when loading from persistent data container ({Path.GetFileName(_filePath)}):\n{exception}");
                HasCorruptedData.Add(this);
            }
            Debuggers.PersistentDataContainer?.Log($"loaded {dictionary.Count} entries.");
        }
    }

    public override void Set<T>(NamespacedKey key, T value)
    {
        base.Set(key, value);
        if (_autoSave)
            Task.Run(SaveAsync);
    }

    public override void Clear()
    {
        base.Clear();
        if (_autoSave)
            Task.Run(SaveAsync);
    }

    public override void Remove(NamespacedKey key)
    {
        base.Remove(key);
        if (_autoSave)
            Task.Run(SaveAsync);
    }

    public IDisposable LargeEdit()
    {
        return new EditContext(this);
    }

    private async Task SaveAsync()
    {
        Debuggers.PersistentDataContainer?.Log($"saving ({Path.GetFileName(_filePath)})");

        try
        {
            await using FileStream stream = File.Open(_filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
            await writer.WriteAsync(JsonConvert.SerializeObject(dictionary, DawnLib.JSONSettings));
            Debuggers.PersistentDataContainer?.Log($"saved ({Path.GetFileName(_filePath)})");
        }
        catch (Exception e)
        {
            DawnPlugin.Logger.LogError($"Error happened while trying to save PersistentDataContainer ({Path.GetFileName(_filePath)}):\n{e}");
        }
    }

    internal void DeleteFile()
    {
        File.Delete(_filePath);
    }
    
    public string FileName => Path.GetFileName(_filePath);
}