using ProductCatalog.Domain.Commands.Base;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Enums;
using System;
using System.Collections.Generic;

namespace ProductCatalog.Domain.Commands
{
    public abstract class CategoryCommand: Command
    {
        public CategoryCommand(MessageDestination destination, string messageType)
            : base(destination, messageType)

        {

        }

        public Guid Id { get; protected set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int NumberOfProducts { get; set; }
        public DataProvider DataProvider { get; set; }
        public IEnumerable<CategoryLink> Links { get; set; }
    }
}
