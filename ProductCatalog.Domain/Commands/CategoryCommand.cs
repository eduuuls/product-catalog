using ProductCatalog.Domain.Commands.Base;
using ProductCatalog.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductCatalog.Domain.Commands
{
    public abstract class CategoryCommand: Command
    {
        public CategoryCommand(MessageDestination destination, string messageType)
            : base(destination, messageType)

        {

        }

        public Guid Id { get; protected set; }
        public string Name { get; protected set; }
        public string Description { get; protected set; }
        public string Url { get; protected set; }
        public string ImageUrl { get; protected set; }
        public bool IsActive { get; protected set; }
        public int NumberOfProducts { get; set; }
        public DataProvider DataProvider { get; protected set; }
    }
}
