using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using ProductCatalog.Domain.Interfaces.UoW;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ProductCatalog.Domain.Commands.Base
{
    public abstract class CommandHandler
    {
        protected readonly ILogger _logger;
        protected ValidationResult ValidationResult;

        protected CommandHandler(ILogger logger)
        {
            _logger = logger;
            ValidationResult = new ValidationResult();
        }

        protected void AddError(string mensagem)
        {
            ValidationResult.Errors.Add(new ValidationFailure(string.Empty, mensagem));
        }

        protected async Task<ValidationResult> Commit(IUnitOfWork uow, string message = null)
        {
            try
            {
                await uow.Commit();
            }
            catch (Exception ex)
            {

                AddError(ex.Message);
            }

            return ValidationResult;
        }

        protected void LogInfo(string message)
        {
            try
            {
                Console.WriteLine(message);
                _logger.LogInformation(message);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error on logging process: {ex.Message}");
            }
        }
        protected void LogError(string errorMessage)
        {
            try
            {
                Console.WriteLine(errorMessage);
                _logger.LogError(errorMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error on logging process: {ex.Message}");
            }
        }
    }
}
