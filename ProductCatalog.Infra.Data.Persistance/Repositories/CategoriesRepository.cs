using Microsoft.EntityFrameworkCore;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;
using ProductCatalog.Domain.Interfaces.Repositories;
using ProductCatalog.Domain.Interfaces.UoW;
using ProductCatalog.Infra.Data.Persistance.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductCatalog.Infra.Data.Persistance.Repositories
{
    public class CategoriesRepository : Repository<Category>, ICategoriesRepository
    {
        public IUnitOfWork UnitOfWork => _unitOfWork;
        public CategoriesRepository(ProductsCatalogDbContext context, IUnitOfWork unitOfWork) : base(context, unitOfWork)
        {
            
        }

        public async Task<Category> GetByKey(string name, DataProvider dataProvider)
        {
            var category = await List().Include(x=> x.Links)
                                            .FirstOrDefaultAsync(c => c.Name.ToLower() == name.Trim().ToLower()
                                                                    && c.DataProvider == dataProvider);

            return category;
        }

        public async Task<List<Category>> GetByProvider(DataProvider dataProvider, bool onlyActive)
        {
            return await List().Where(c => c.DataProvider == dataProvider 
                                                && (c.IsActive == onlyActive || !onlyActive)).ToListAsync();
        }

        public async Task<Category> GetById(Guid id)
        {
            return await List().Include(x => x.Links).FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}
