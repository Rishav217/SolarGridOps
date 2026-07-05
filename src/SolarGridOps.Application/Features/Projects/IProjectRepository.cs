using SolarGridOps.Domain.Entities;

namespace SolarGridOps.Application.Features.Projects;

public interface IProjectRepository
{
    Task<bool> CustomerExistsAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<bool> ProjectCodeExistsAsync(string projectCode, CancellationToken cancellationToken = default);
    Task AddAsync(Project project, CancellationToken cancellationToken = default);
    Task<Project?> GetByIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Project>> ListAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Project>> ListByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
}
