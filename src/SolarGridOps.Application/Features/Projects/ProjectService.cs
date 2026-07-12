using SolarGridOps.Application.Common;
using SolarGridOps.Domain.Entities;

namespace SolarGridOps.Application.Features.Projects;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;

    public ProjectService(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<Result<ProjectDto>> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        var customerExists = await _projectRepository.CustomerExistsAsync(request.CustomerId, cancellationToken);
        if (!customerExists)
        {
            return Result<ProjectDto>.Failure(Error.NotFound("Customer not found."));
        }

        var code = request.ProjectCode.Trim();
        var codeExists = await _projectRepository.ProjectCodeExistsAsync(code, cancellationToken);
        if (codeExists)
        {
            return Result<ProjectDto>.Failure(Error.Conflict("Project code already exists."));
        }

        var project = new Project
        {
            CustomerId = request.CustomerId,
            ProjectCode = code,
            CapacityKW = request.CapacityKW,
            CurrentPhase = request.CurrentPhase,
            InstallationStartDate = request.InstallationStartDate,
            InstallationEndDate = request.InstallationEndDate,
            SiteAddress = request.SiteAddress?.Trim(),
            Notes = request.Notes?.Trim()
        };

        await _projectRepository.AddAsync(project, cancellationToken);

        var created = await _projectRepository.GetByIdAsync(project.Id, cancellationToken);
        return Result<ProjectDto>.Success(Map(created!));
    }

    public async Task<Result<ProjectDto>> GetByIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);
        if (project is null)
        {
            return Result<ProjectDto>.Failure(Error.NotFound("Project not found."));
        }

        return Result<ProjectDto>.Success(Map(project));
    }

    public async Task<Result<IReadOnlyList<ProjectDto>>> ListAsync(CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.ListAsync(cancellationToken);
        return Result<IReadOnlyList<ProjectDto>>.Success(projects.Select(Map).ToList());
    }

    public async Task<Result<IReadOnlyList<ProjectDto>>> ListByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customerExists = await _projectRepository.CustomerExistsAsync(customerId, cancellationToken);
        if (!customerExists)
        {
            return Result<IReadOnlyList<ProjectDto>>.Failure(Error.NotFound("Customer not found."));
        }

        var projects = await _projectRepository.ListByCustomerAsync(customerId, cancellationToken);
        return Result<IReadOnlyList<ProjectDto>>.Success(projects.Select(Map).ToList());
    }

    private static ProjectDto Map(Project project)
    {
        return new ProjectDto
        {
            Id = project.Id,
            CustomerId = project.CustomerId,
            CustomerName = project.Customer.FullName,
            ProjectCode = project.ProjectCode,
            CapacityKW = project.CapacityKW,
            CurrentPhase = project.CurrentPhase,
            InstallationStartDate = project.InstallationStartDate,
            InstallationEndDate = project.InstallationEndDate,
            SiteAddress = project.SiteAddress,
            Notes = project.Notes
        };
    }
}
