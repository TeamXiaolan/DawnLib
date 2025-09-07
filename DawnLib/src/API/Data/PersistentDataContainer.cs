using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Dawn;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Dusk;

public class PersistentDataContainer : DataContainer
{
    private string _filePath;
    private bool _autoSave;

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
        _filePath = filePath;
        if (File.Exists(filePath))
        {
            dictionary = JsonConvert.DeserializeObject<Dictionary<NamespacedKey, object>>(File.ReadAllText(_filePath), DawnLib.JSONSettings);
        }
    }

    public override void Set<T>(NamespacedKey key, T value)
    {
        base.Set(key, value);
        if (_autoSave)
            Task.Run(SaveAsync);
    }

    public IDisposable LargeEdit()
    {
        return new EditContext(this);
    }

    private async Task SaveAsync()
    {
        await File.WriteAllTextAsync(_filePath, JsonConvert.SerializeObject(dictionary, DawnLib.JSONSettings));
    }
}