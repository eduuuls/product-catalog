using ProductCatalog.Domain.Interfaces.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProductCatalog.Domain.Interfaces.Repositories.Base
{
    public interface IRepository<T>
    {
        bool AsNoTracking { get; set; }
        void Add(T entidade);
        void Update(T entidade);
        void UpdateRange(T[] entities);
        void Delete(params int[] id);
        void Delete(T entidade);
        T Search(params int[] id);
        IQueryable<T> List();
    }
}
