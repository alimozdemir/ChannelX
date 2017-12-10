using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace ChannelX.Models.Trackers
{
    //Common interface of a object tracker, Designed as singleton. Keep tracking with events 
    public interface ITracker<T, E>
    {
        void Add(T context, E entity);
        E Remove(T context);
        Task<IEnumerable<E>> All();
        Task<IEnumerable<E>> All(string key);
        Task Update(E val);
        Task<E> Find(string key);

    }
}