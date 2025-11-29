using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dawn.Internal;
using Newtonsoft.Json;

namespace Dawn;

public class PersistentDataContainer : DataContainer
{
    private string _filePath;
    public bool AutoSave { get; private set; } = true;

    private readonly SemaphoreSlim _saveLock = new(1, 1);

    internal static List<PersistentDataContainer> HasCorruptedData { get; private set; } = [];

    public class EditContext : IDisposable
    {
        private PersistentDataContainer _container;
        private bool _prevAutoSave; // incase the persistent data container gets passed around, we don't want to accidentally re-enable auto-saving

        public EditContext(PersistentDataContainer container)
        {
            _container = container;
            _prevAutoSave = _container.AutoSave;
            _container.AutoSave = false;
        }

        public void Dispose()
        {
            _container.AutoSave = _prevAutoSave;
            _container.MarkDirty();
        }
    }
    
    public PersistentDataContainer(string filePath)
    {
        Debuggers.PersistentDataContainer?.Log($"new PersistentDataContainer: {Path.GetFileName(filePath)}");
        _filePath = filePath;
        if (!File.Exists(filePath)) 
            return;
        
        Debuggers.PersistentDataContainer?.Log("loading existing file");
        try
        {
            dictionary = JsonConvert.DeserializeObject<Dictionary<NamespacedKey, object>>(File.ReadAllText(_filePath), DawnLib.JSONSettings)!;
        }
        catch (Exception exception)
        {
            DawnPlugin.Logger.LogFatal($"Exception when loading from persistent data container ({Path.GetFileName(_filePath)}):\n{exception}");
            HasCorruptedData.Add(this);
            return;
        }

        /*if (dictionary == null)
        {
            DawnPlugin.Logger.LogFatal($"Failure when loading from persistent data container ({Path.GetFileName(_filePath)}), file likely corrupted, please delete.");
            HasCorruptedData.Add(this);
            return;
        }*/

        foreach (object dictionaryValue in dictionary.Values)
        {
            if (dictionaryValue is ChildPersistentDataContainer child)
            {
                Debuggers.PersistentDataContainer?.Log($"updated parent for a loaded persistent data container. count = {child.Count}: {string.Join(", ", child.Keys.Select(it => it.ToString()))}");
                
                child.Internal_SetParent(this);
            }
        }
        
        Debuggers.PersistentDataContainer?.Log($"loaded {dictionary.Count} entries.");
    }

    public override void Set<T>(NamespacedKey key, T value)
    {
        if (value is IDataContainer && value is not ChildPersistentDataContainer)
        {
            throw new NotSupportedException($"{key} is a {value.GetType().Name}, which is not supported by persistent data container. Only ChildPersistentDataContainer is supported.");
        }

        if (value is ChildPersistentDataContainer child && child.Parent != this)
        {
            throw new NotSupportedException($"{key} is a child persistent data container being added to '{FileName}' when it belongs to '{child.Parent.FileName}'.");
        }
        
        base.Set(key, value);
        if (AutoSave)
        {
            Task.Run(SaveAsync);
        }
    }

    public override void Clear()
    {
        base.Clear();
        if (AutoSave)
        {
            MarkDirty();
        }
    }

    public override void Remove(NamespacedKey key)
    {
        base.Remove(key);
        if (AutoSave)
        {
            MarkDirty();
        }
    }

    [Obsolete("Use CreateEditContext()")]
    public IDisposable LargeEdit() => CreateEditContext();

    public override IDisposable CreateEditContext()
    {
        return new EditContext(this);
    }

    public override void MarkDirty()
    {
        Task.Run(SaveAsync);
    }

    private async Task SaveAsync()
    {
        Debuggers.PersistentDataContainer?.Log($"saving ({Path.GetFileName(_filePath)})");

        await _saveLock.WaitAsync().ConfigureAwait(false);
        try
        {
            await using FileStream stream = new FileStream(_filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);

            using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
            string payload = JsonConvert.SerializeObject(dictionary, DawnLib.JSONSettings);
            await writer.WriteAsync(payload).ConfigureAwait(false);
            await writer.FlushAsync().ConfigureAwait(false);
            await stream.FlushAsync().ConfigureAwait(false);
            Debuggers.PersistentDataContainer?.Log($"saved ({Path.GetFileName(_filePath)})");
        }
        catch (Exception e)
        {
            DawnPlugin.Logger.LogError($"Error happened while trying to save PersistentDataContainer ({Path.GetFileName(_filePath)}):\n{e}");
        }
        finally
        {
            _saveLock.Release();
        }
    }

    internal void DeleteFile()
    {
        File.Delete(_filePath);
    }

    public string FileName => Path.GetFileName(_filePath);
}

public class ChildPersistentDataContainer : DataContainer
{
    public PersistentDataContainer Parent { get; private set; }

    internal ChildPersistentDataContainer() { }

    public ChildPersistentDataContainer(PersistentDataContainer parent)
    {
        Parent = parent;
    }
    
    public override void MarkDirty()
    {
        if (Parent.AutoSave)
        {
            Parent.MarkDirty();
        }
    }

    public override IDisposable CreateEditContext() => Parent.CreateEditContext();

    internal void Internal_SetParent(PersistentDataContainer parent) => Parent = parent;
}