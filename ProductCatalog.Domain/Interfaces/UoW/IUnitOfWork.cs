using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ProductCatalog.Domain.Interfaces.UoW
{
    public interface IUnitOfWork
    {
        Task<bool> Commit();

        Task<bool> Commit(bool autoDetectChangesOff);
    }
}
