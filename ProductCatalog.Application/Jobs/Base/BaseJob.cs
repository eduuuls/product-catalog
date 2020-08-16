using AutoMapper;
using Microsoft.Extensions.Logging;
using System;

namespace ProductCatalog.Application.Jobs.Base
{
    public class BaseJob
    {
        protected readonly IMapper _mapper;
        protected readonly ILogger _logger;
        public BaseJob(ILogger logger, IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;
        }

        protected void LogInfo(string message)
        {
            Console.WriteLine(message);
            _logger.LogInformation(message);
        }
        protected void LogError(string errorMessage)
        {
            Console.WriteLine(errorMessage);
            _logger.LogError(errorMessage);
        }
    }
}
