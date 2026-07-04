// SPDX-License-Identifier: GPL-3.0-or-later

namespace Vistava.Service.Utils;

public class KeyedGeneratorBundle<TKey, TOut>(Func<TKey, TOut> Generator) where TKey : notnull
{
    private readonly Dictionary<TKey, Task<TOut>> runningTasks = new();

    public async Task<TOut> GenerateAsync(TKey key, CancellationToken stoppingToken)
    {
        Task<TOut>? task;

        lock (runningTasks)
        {
            if (!runningTasks.TryGetValue(key, out task))
            {
                task = Task.Run(TaskAction, CancellationToken.None);
                runningTasks.Add(key, task);
            }
        }

        await task.WaitAsync(stoppingToken);
        return task.Result;

        TOut TaskAction()
        {
            try
            {
                return Generator(key);
            }
            finally
            {
                lock (runningTasks)
                {
                    runningTasks.Remove(key);
                }
            }
        }
    }
}