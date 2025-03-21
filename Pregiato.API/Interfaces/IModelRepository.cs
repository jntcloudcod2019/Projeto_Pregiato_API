﻿using Pregiato.API.Models;
using Pregiato.API.Requests;

namespace Pregiato.API.Interface
{
    public interface IModelRepository
    {
        Task<IEnumerable<Model>> GetAllModelAsync();
        Task<Model> GetByIdModelAsync(Guid id);
        Task AddModelAsync(Model model);
        Task UpdateModelAsync(Model model);
        Task DeleteModelAsync(Guid id);
        Task<Model?> GetModelByCriteriaAsync(string query);
        Task<Model> GetModelAllAsync(string? idModel, string? cpf, string? rg);
        Task<Model> ModelExistsAsync(CreateModelRequest inputModel);
    }
}
