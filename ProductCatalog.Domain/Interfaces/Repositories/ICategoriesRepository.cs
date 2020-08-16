using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;
using ProductCatalog.Domain.Interfaces.Repositories.Base;
using ProductCatalog.Domain.Interfaces.UoW;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ProductCatalog.Domain.Interfaces.Repositories
{
    public interface ICategoriesRepository : IRepository<Category>
    {
        IUnitOfWork UnitOfWork { get; }
        Task<Category> GetByKey(string name, DataProvider dataProvider);
        Task<List<Category>> GetByProvider(DataProvider dataProvider, bool onlyActive);
        Task<Category> GetById(Guid id);
    }
}
