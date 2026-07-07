using SolarGridOps.Application.Common;

namespace SolarGridOps.Application.Features.Projects;

public interface IProjectService
{
    Task<Result<ProjectDto>> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken = default);
    Task<Result<ProjectDto>> GetByIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ProjectDto>>> ListAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ProjectDto>>> ListByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<Result<ProjectDto>> UpdatePhaseAsync(Guid projectId, UpdateProjectPhaseRequest request, CancellationToken cancellationToken = default);
}
